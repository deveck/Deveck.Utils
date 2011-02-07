using System;
using System.Collections.Generic;
using System.Text;
using Deveck.Utils.StringUtils;
using System.Reflection;
using Deveck.Utils.Factory;
using Deveck.Utils.SimpleComm;
using System.Threading;
using System.Windows.Forms;
using System.Collections;
using Deveck.Utils.Devices.Telecom;

namespace Deveck.Utils.Samples
{
    public class Program:ApplicationContext
    {
   		static Random r = new Random();
		
		public static void Main(string[] args)
        {
            Application.Run(new Program());
        } 
        
        public Program()
        {
            throw new Exception("No Example has been selected! Comment out this line of code and comment in one or more Examples");
            //Examples_SimpleFormatter();
            //Examples_ClassIdentifierFactory();

            //Example_SimpleComm_HID();
            //Example_SimpleComm_RS232();
            //Example_SimpleComm_Network();

            //Example_Telecom_ATZ();
            //Example_Telecom_Capi();
            
        }

		#region SimpleFormatter examples
		private void Examples_SimpleFormatter()
		{
			Console.WriteLine(@"These samples show you how to use the SimpleFormatter from deveck.net");
            Console.WriteLine();

            SimpleFormatter f = new SimpleFormatter();
			Console.WriteLine("\tUse simple formatter like string.format: {0}", f.Format("{0} {1}", "Hello", "World"));
			Console.WriteLine("\tWith padding: {0}",
			                  f.Format("{0:LF:10:-}{1:R:10:*}", "Hello", "World"));
			Console.WriteLine("\tCentered to 40 chars: {0}", f.Format("{0:M:40:-}", "Hello World"));
			
			Console.WriteLine();
			Console.WriteLine("Registering macro \"hello\"");
			f.DefineTextMacro("hello", "HELLO");
			Console.WriteLine("Registering macro \"world\"");
			f.DefineTextMacro("world", "WoRlD");
			Console.WriteLine();
			
			Console.WriteLine("\tPadded output using macros: {0}", f.Format("{[hello]:LF:10:-}{[world]:R:10:*}"));
			Console.WriteLine("\tTruncating output:");
			Console.WriteLine(f.Format("\t\t{[hello]:L:2}"));
			Console.WriteLine(f.Format("\t\t{[hello]:L:3}"));
			Console.WriteLine(f.Format("\t\t{[hello]:L:4}"));
			Console.WriteLine();
			
			Console.WriteLine("\tPadded output using direct text input: {0}", f.Format("{\"hello\":LF:10:-}{\"world\":R:10:*}"));
			Console.WriteLine();
			Console.WriteLine();
			
			Console.WriteLine("\tNow we will reference a macro called \"universe\" several times\n\tthat has never been defined");
			f.OnGetParameter += OnGetParameter;
			Console.Write("\t");
			for(int i = 0; i< 10; i++)
			{
				if(i > 0)
					Console.Write(", ");
				Console.Write(f.Format("{[universe]}"));
			}
		}
		
        private static string OnGetParameter (string parameterName)
        {
			StringBuilder universe = new StringBuilder();
			
			foreach(char c in "universe")
			{
				bool lowerCase = r.NextDouble() > 0.5;
				
				universe.Append(lowerCase?c.ToString(): c.ToString().ToUpper());
			}
			
			return universe.ToString();
        }
		#endregion
		
		#region ClassIdentifier factory examples
		private void Examples_ClassIdentifierFactory()
		{
		
			Console.WriteLine(@"These samples show you how to use the ClassIdentifierFactory from deveck.net");
            Console.WriteLine();
			
			Console.WriteLine("We are now creating 4 objects:");
			Console.WriteLine("\t1. Object of CIClass1 using its identifier c1");
			Console.WriteLine("\t2. Object of CIClass2 using its identifier c2");
			Console.WriteLine("\t3. Object of CIClass3 using its class name");
			Console.WriteLine("\t4. Object of identifier i1.....oh this identifier does not exists, so no instance");
			Console.WriteLine();
			CIBase o1 = GenericClassIdentifierFactory.CreateFromClassIdentifierOrType<CIBase>("c1", "class number 1");
			CIBase o2 = GenericClassIdentifierFactory.CreateFromClassIdentifierOrType<CIBase>("c2", "class number 2");
			CIBase o3 = GenericClassIdentifierFactory.CreateFromClassIdentifierOrType<CIBase>("Deveck.Utils.Samples.Program+CIClass3, Samples", "class number 3");
			CIBase o4 = GenericClassIdentifierFactory.CreateFromClassIdentifierOrType<CIBase>("i1", "invalid class");
			
			Console.WriteLine("Who is o1?: Hello from o1: '{0}'", o1);
			Console.WriteLine("Who is o2?: Hello from o2: '{0}'", o2);
			Console.WriteLine("Who is o3?: Hello from o3: '{0}'", o3);
			Console.WriteLine("Is there o4? {0}", o4==null?"NO":"Yes, there it is: '" + o4.ToString() + "', have you created a class with identifier i1?");
			
			
		}
		
		public abstract class CIBase
		{
			private string _name;
			
			public CIBase(string name)
			{
				_name = name;
			}
			
			public override string ToString ()
			{
				return _name;
			}
		}
		
		[ClassIdentifier("c1")]
		public class CIClass1 : CIBase
		{
			public CIClass1(string name)
				:base(name)
			{
			}
		}
		
		[ClassIdentifier("c2")]
		public class CIClass2 : CIBase
		{
			public CIClass2(string name)
				:base(name)
			{
			}
		}
		
		public class CIClass3 : CIBase
		{
			public CIClass3(string name)
				:base(name)
			{
			}
		}
		#endregion

		
		#region SimpleComm examples
		/// <summary>
		/// Shows how to use the HIDComm class to capture keyboard input 
		/// </summary>
		private void Example_SimpleComm_HID()
		{
            ICommunication hidComm =
                GenericClassIdentifierFactory.CreateFromClassIdentifierOrType<ICommunication>("simplecomm/win/hid");

            hidComm.SetupCommunication(null);

            Console.WriteLine("Found {0}-HID devices:", ((HIDComm)hidComm).Devices.Length);
            foreach (InputDevice.DeviceInfo dI in ((HIDComm)hidComm).Devices)
                Console.WriteLine("Device: \n\tname={0} \n\tdescription={1}\n\ttype={2}", dI.deviceName, dI.Name, dI.deviceType);

            Console.WriteLine("Setting up Catch-All HIDComm");
            hidComm.OnDataReceived += (OnDataReceivedDelegate)delegate(byte[] data, int length)
            {
                Console.WriteLine("HIDComm [captureall]: {0}", Encoding.Default.GetString(data, 0, length));
            };

            Console.WriteLine("Setting up HIDComm for first HID: {0}", ((HIDComm)hidComm).Devices[0].deviceName);
            Hashtable ht = new Hashtable();
            ht.Add("lock_to", ((HIDComm)hidComm).Devices[0].deviceName);

            ICommunication hidComm2 =
                GenericClassIdentifierFactory.CreateFromClassIdentifierOrType<ICommunication>("simplecomm/win/hid");

            hidComm2.SetupCommunication(ht);

            hidComm2.OnDataReceived += (OnDataReceivedDelegate)delegate(byte[] data, int length)
            {
                Console.WriteLine("HIDComm2: {0}", Encoding.Default.GetString(data, 0, length));
            };
		}

        /// <summary>
        /// Shows how to use the simplecomm/general/rs232
        /// to simulate multiple, interconnected serial ports you can use http://com0com.sourceforge.net/
        /// </summary>
        private void Example_SimpleComm_RS232()
        {
            string portSrc = "COM7";
            string portSink = "COM8";

            IDictionary configSrc = new Hashtable();
            configSrc.Add("port_name", portSrc);
            configSrc.Add("baud_rate", 57600);

            IDictionary configSink = new Hashtable();
            configSink.Add("port_name", portSink);
            configSink.Add("baud_rate", 57600);

            ICommunication source = GenericClassIdentifierFactory.CreateFromClassIdentifierOrType<ICommunication>("simplecomm/general/rs232");

            try
            {
                source.SetupCommunication(configSrc);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error setting up source port: {0}", portSrc);
                return;
            }


            ICommunication sink = GenericClassIdentifierFactory.CreateFromClassIdentifierOrType<ICommunication>("simplecomm/general/rs232");

            try
            {
                sink.SetupCommunication(configSink);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error setting up sink port: {0}", portSink);
                return;
            }

            SimpleComm_Transfer(source, sink, "simplecomm/general/rs232");

        }

        /// <summary>
        /// Shows how to use the simplecomm/general/tcp
        /// </summary>
        private void Example_SimpleComm_Network()
        {
            int port = 1234;
            

            IDictionary configListen = new Hashtable();
            configListen.Add("port", port);
            
            IDictionary configConnect = new Hashtable();
            configConnect.Add("remote_ip", "127.0.0.1");
            configConnect.Add("port", port);
            configConnect.Add("listen", false);

            ICommunication source = GenericClassIdentifierFactory.CreateFromClassIdentifierOrType<ICommunication>("simplecomm/general/tcp");
            source.SetupCommunication(configListen);

            ICommunication sink = GenericClassIdentifierFactory.CreateFromClassIdentifierOrType<ICommunication>("simplecomm/general/tcp");
            sink.SetupCommunication(configConnect);

            SimpleComm_Transfer(source, sink, "simplecomm/general/tcp");
        }

        private void SimpleComm_Transfer(ICommunication source, ICommunication sink, string identifier)
        {

            AutoResetEvent syncSrc = new AutoResetEvent(false);
            AutoResetEvent syncSink = new AutoResetEvent(false);

            source.OnDataReceived += (OnDataReceivedDelegate)delegate(byte[] data, int length)
            {
                Console.WriteLine("{0} SOURCE received: {1}", identifier, Encoding.Default.GetString(data, 0, length));
            };

            sink.OnDataReceived += (OnDataReceivedDelegate)delegate(byte[] data, int length)
            {
                Console.WriteLine("{0} SINK received: {1}", identifier, Encoding.Default.GetString(data, 0, length));
            };

            //Src thread
            ThreadPool.QueueUserWorkItem(
                (WaitCallback)delegate(object state)
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        byte[] b = Encoding.ASCII.GetBytes(string.Format("Hello from source: {0}", i));
                        source.SendData(b, 0, b.Length);
                        Thread.Sleep(20);
                    }

                    syncSrc.Set();
                });

            //Sink thread
            ThreadPool.QueueUserWorkItem(
                (WaitCallback)delegate(object state)
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        byte[] b = Encoding.ASCII.GetBytes(string.Format("Hello from sink: {0}", i));
                        sink.SendData(b, 0, b.Length);
                        Thread.Sleep(30);
                    }

                    syncSink.Set();
                });

            syncSrc.WaitOne();
            syncSink.WaitOne();
        }
		#endregion


        #region Telecom examples
        /// <summary>
        /// Tests the AnalogATZModem provider (implements ITelecom)
        /// </summary>
        private void Example_Telecom_ATZ()
        {
            ICommunication serialComm = GenericClassIdentifierFactory.CreateFromClassIdentifierOrType<ICommunication>("simplecomm/general/rs232");
            IDictionary config = new Hashtable();
            config.Add("port_name", "COM8");
            config.Add("baud_rate", 57600);
            serialComm.SetupCommunication(config);

            ITelecom t = GenericClassIdentifierFactory.CreateFromClassIdentifierOrType<ITelecom>("telecom/general/atz");
            t.IncomingCall += new TelecomIncomingCallDelegate(_IncomingCall);

            //Initialize the incoming-call provider with its default parameters
            IDictionary modemConfig = new Hashtable();
            modemConfig.Add("TransmitCommandTimeout", 5000);
            t.Initialize(serialComm, modemConfig);

        }

         /// <summary>
        /// Tests the AnalogATZModem provider (implements ITelecom)
        /// </summary>
        private void Example_Telecom_Capi()
        {
            ITelecom t = GenericClassIdentifierFactory.CreateFromClassIdentifierOrType<ITelecom>("telecom/win/capi");
            t.IncomingCall += new TelecomIncomingCallDelegate(_IncomingCall);
            t.Initialize(null, new Hashtable());
        }

        private void _IncomingCall(ITelecom sender, TelecomIncomingInfo info)
        {
            Console.WriteLine("Incoming Call from '{0}' detected", info.Identifier);
        }
        #endregion

    }
}
