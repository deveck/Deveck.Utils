using System;
using System.Collections.Generic;
using System.Text;
using Wiffzack.Telecom;
using System.Reflection;
using System.Xml;
using System.IO;

namespace Deveck.Utils.Devices.Telecom
{
    /// <summary>
    /// Verwaltet alle angeschlossenen Telecom Geräte
    /// </summary>
    public class TelecomManager:IDisposable
    {
        /// <summary>
        /// Beinhaltet alle angeschlossenen Telecomgeräte
        /// </summary>
        private List<TelecomDeviceInfo> _telecomDevices = new List<TelecomDeviceInfo>();

        /// <summary>
        /// Lokale Datei in der die Anrufe zwischengespeichert werden
        /// </summary>
        private FileInfo _localFileCache = null;

        /// <summary>
        /// Zugriff auf Link, usw...
        /// </summary>
        private DisplayLinkManager _link;

        /// <summary>
        /// Telecom Inteface Konfiguration
        /// </summary>
        private MyUnitConfig.TelecomInterfacesConfig _config;

        public TelecomManager(DisplayLinkManager link, MyUnitConfig.TelecomInterfacesConfig config)
        {
            _link = link;
            _config = config;

            _localFileCache = new FileInfo(link.Unit.FileSysTool.TempFileUri("DisplayUnit_TelecomDevices_CachedCalls.xml").AbsolutePath);

           
        }

        /// <summary>
        /// Lädt alle vorhandenen Telecommunications Geräte
        /// </summary>
        public void LoadTelecomInterfaces()
        {
            if (_config == null ||
                   _config.Interface == null)
                return;

            Assembly assembly = typeof(DisplayLinkManager).Assembly;

            foreach (MyUnitConfig.TelecomInterfacesConfig.InterfaceConfig interfaceConfig
                in _config.Interface)
            {
                try
                {
                    XmlDocument doc = new XmlDocument();

                    ICommunication commInstance = null;

                    if (interfaceConfig.CommunicationType != null && !interfaceConfig.CommunicationType.Equals(string.Empty))
                    {
                        commInstance = CommFactory.CreateCommunication(interfaceConfig.CommunicationType);

                        doc.LoadXml(interfaceConfig.CommunicationSettings);
                        commInstance.SetupCommunication(doc.DocumentElement);

                        //HACK
                        if (commInstance is SerialComm)
                            (commInstance as SerialComm).SetLines(true, true);
                    }


                    Type type = assembly.GetType(interfaceConfig.Type, true);
                    ITelecom telecomInstance = (ITelecom)Activator.CreateInstance(type);
                    doc = new XmlDocument();
                    doc.LoadXml(interfaceConfig.Settings);
                    telecomInstance.Initialize(commInstance, doc.DocumentElement, interfaceConfig.Endpoint);
                    telecomInstance.IncomingCall += AsyncIncomingCall;

                    if (_link.HubRegisterEndpoint(Wiffzack.Services.Hub.HubClient.EndPointType.display, interfaceConfig.Endpoint))
                        _telecomDevices.Add(new TelecomDeviceInfo(telecomInstance, interfaceConfig));
                    else
                        throw new ArgumentException("Error registering endpoint");
                    
                }
                catch (Exception ex)
                {
                    _logger.NonFatal("Error starting telecommunicationdevice ({0} \n Stack: {1})",
                        ex.Message, ex.StackTrace);
                }

            }

            LoadLocalFileCache();
        }

        /// <summary>
        /// Alle TelecomGeräte deaktivieren
        /// </summary>
        public void StopTelecomInterfaces()
        {
            foreach (TelecomDeviceInfo telecomInfo in _telecomDevices)
                telecomInfo.TelecomDevice.Dispose();
        }


        /// <summary>
        /// Behandelt ein TelecomRequest von einem Client
        /// und sendet die gespeicherten Nummern zurück
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="path"></param>
        /// <param name="request"></param>
        public void HandleRequest(string clientId, string path, TelecomRequest request)
        {
            foreach (TelecomDeviceInfo telecomInfo in _telecomDevices)
            {
                if (telecomInfo.CanHandle(path))
                {
                    TelecomResponse response = new TelecomResponse(telecomInfo.IncomingCallInfos, request.ClientSideJobID);
                    using (DataOutputStream sink = new BinaryOutputStream(new MemoryStream(), true))
                    {
                        response.Write(sink);

                        sink.Flush();

                        ((MemoryStream)sink.BaseStream).Seek(0, SeekOrigin.Begin);

                        _link.HubSendStream(clientId, "/telecom", sink.BaseStream);
                    }
                }
            }
        }

        /// <summary>
        /// Ein eingehender Anruf wurde entdeckt
        /// </summary>
        /// <param name="obj"></param>
        private void AsyncIncomingCall(ITelecom sender, TelecomIncomingInfo incoming)
        {
            _logger.Info("Incoming Call: identifier='{0}' suppressed='{1}'", incoming.Identifier, incoming.Suppressed);

            foreach (TelecomDeviceInfo deviceInfo in _telecomDevices)
            {
                if (sender == deviceInfo.TelecomDevice)
                {
                    deviceInfo.AddIncomingCall(incoming);
                    break;
                }
            }

            lock (_localFileCache)
            {
                try
                {

                    XmlDocument doc = new XmlDocument();
                    doc.AppendChild(doc.CreateElement("LocalFileCache"));

                    foreach (TelecomDeviceInfo deviceInfo in _telecomDevices)
                    {
                        XmlElement deviceRootNode = doc.CreateElement("Device");
                        doc.DocumentElement.AppendChild(deviceRootNode);
                        XmlAttribute deviceNameAttribute = doc.CreateAttribute("Name");
                        deviceNameAttribute.Value = deviceInfo.InterfaceConfig.Endpoint;
                        deviceRootNode.Attributes.Append(deviceNameAttribute);

                        foreach (TelecomIncomingInfo incomingInfo in deviceInfo.IncomingCallInfos)
                        {
                            XmlElement incomingRootNode = doc.CreateElement("IncomingCall");
                            deviceRootNode.AppendChild(incomingRootNode);

                            incomingInfo.Write(incomingRootNode);
                        }
                    }

                    doc.Save(_localFileCache.FullName);
                }
                catch (Exception ex)
                {
                    _logger.NonFatal("Error saving local  cache ({0} \n StackTrace: {1})", ex.Message, ex.StackTrace);
                }
            }
        }

        /// <summary>
        /// Lädt den Lokalen Cache
        /// </summary>
        private void LoadLocalFileCache()
        {
            lock (_localFileCache)
            {
                try
                {
                    if (_localFileCache.Exists == false)
                        return;

                    XmlDocument doc = new XmlDocument();
                    doc.Load(_localFileCache.FullName);

                    foreach (XmlElement deviceNode in doc.DocumentElement.SelectNodes("Device"))
                    {
                        XmlAttribute nameAttribute = (XmlAttribute)deviceNode.Attributes.GetNamedItem("Name");

                        //Zugehöriges TelecomObjekt suchen
                        TelecomDeviceInfo currentDevice = null;
                        foreach (TelecomDeviceInfo deviceInfo in _telecomDevices)
                        {
                            if (deviceInfo.InterfaceConfig.Endpoint == nameAttribute.Value)
                            {
                                currentDevice = deviceInfo;
                                break;
                            }
                        }

                        if (currentDevice != null)
                        {
                            foreach (XmlElement incomingCallNode in deviceNode.SelectNodes("IncomingCall"))
                            {
                                try
                                {
                                    TelecomIncomingInfo incomingInfo = new TelecomIncomingInfo(incomingCallNode);
                                    currentDevice.AddIncomingCall(incomingInfo);
                                }
                                catch (Exception ex)
                                {
                                    _logger.NonFatal("IncomingCall entry could not be loaded! ({0} StackTrace: {1})", ex.Message, ex.StackTrace);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.NonFatal("Error loading local cache! ({0} \n StackTrace: {1})", ex.Message, ex.StackTrace);
                }
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            StopTelecomInterfaces();
            _telecomDevices.Clear();
        }

        #endregion
    }
}
