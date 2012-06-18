using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using System.Reflection;

namespace DevEck.Common.ByteUtils
{
    /// <summary>
    /// Helpers for packing and unpacking data to byte arrays
    /// </summary>  
    /// <remarks>
    /// All WriteXXX methods convert the results in big endian.
    /// All ReadXXX methods convert the results back to the processor specific endian format
    /// </remarks>
    public static class ByteHelpers
    {        
        #region Constants for length calculation
        public const int BooleanLength = 1;
        public const int Int16Length   = 2;
        public const int Int32Length   = 4;
        public const int Int64Length   = 8;
        public const int UInt16Length  = 2;
        public const int UInt32Length  = 4;
        public const int UInt64Length  = 8;
        public const int FloatLength   = 4;
        public const int DoubleLength  = 8;
        public const int DecimalLength = 4 * Int32Length;
        public const int MaxPackedDecimalLength = DecimalLength + 1;
        public const int MaxPackedInt32Length = 5;
        public const int MaxPackedInt64Length = 9;

        #endregion

        #region Big/Little Endian 
        /// <summary>
        ///  Packs data for network transfer
        /// </summary>
        /// <param name="target"></param>
        /// <param name="offset"></param>
        /// <param name="src_bits"></param>
        /// <param name="src_length"></param>        
        private static void PackBits(byte[] target, int offset, byte[] src_bits, int length)
        {   
#if DEBUG
            if (src_bits.Length != length)
                throw new ArgumentException("src_bits.Length != length");
#endif
            Array.Copy(src_bits, 0, target, offset, length);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(target, offset, length);
        }

        /// <summary>
        /// ... and do the unpack magic
        /// </summary>
        /// <param name="target"></param>
        /// <param name="offset"></param>
        /// <param name="src_bits"></param>
        /// <param name="src_length"></param>
        private static byte[] UnpackBits(byte[] src_bits, int offset, int length)
        {
            byte[] cpu_bits = new byte[length];

            Array.Copy(src_bits, offset, cpu_bits, 0, length);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(cpu_bits, 0, length);

            return cpu_bits;
        }
        #endregion

        #region Short (16bit), int (32bit) and long (64bit)
        public static void WriteInt16(byte[] target, int offset, short value)
        {
            PackBits(target, offset, BitConverter.GetBytes(value), Int16Length);
        }

        public static void WriteInt32(byte[] target, int offset, int value)
        {
            PackBits(target, offset, BitConverter.GetBytes(value), Int32Length);            
        }

        public static void WriteInt64(byte[] target, int offset, long value)
        {
            PackBits(target, offset, BitConverter.GetBytes(value), Int64Length);
        }

        public static short ReadInt16(byte[] target, int offset)
        {
            return BitConverter.ToInt16(UnpackBits(target, offset, Int16Length), 0);
        }

        public static int ReadInt32(byte[] target, int offset)
        {
            return BitConverter.ToInt32(UnpackBits(target, offset, Int32Length), 0);
        }

        public static long ReadInt64(byte[] target, int offset)
        {
            return BitConverter.ToInt64(UnpackBits(target, offset, Int64Length), 0);
        }
        #endregion

        #region ushort (16bit), uint (32bit), and ulong (64bit)
        public static void WriteUInt16(byte[] target, int offset, ushort value)
        {
            PackBits(target, offset, BitConverter.GetBytes(value), Int16Length);
        }

        public static void WriteUInt32(byte[] target, int offset, uint value)
        {
            PackBits(target, offset, BitConverter.GetBytes(value), Int32Length);            
        }

        public static void WriteUInt64(byte[] target, int offset, ulong value)
        {
            PackBits(target, offset, BitConverter.GetBytes(value), Int64Length);
        }

        public static ushort ReadUInt16(byte[] target, int offset)
        {
            return BitConverter.ToUInt16(UnpackBits(target, offset, Int16Length), 0);
        }

        public static uint ReadUInt32(byte[] target, int offset)
        {
            return BitConverter.ToUInt32(UnpackBits(target, offset, Int32Length), 0);
        }

        public static ulong ReadUInt64(byte[] target, int offset)
        {
            return BitConverter.ToUInt64(UnpackBits(target, offset, Int64Length), 0);
        }
        #endregion

        #region float, double and decimal
        public static void WriteFloat(byte[] target, int offset, float value)
        {
            PackBits(target, offset, BitConverter.GetBytes(value), FloatLength);
        }

        public static void WriteDouble(byte[] target, int offset, double value)
        {
            PackBits(target, offset, BitConverter.GetBytes(value), DoubleLength);
        }

        public static void WriteDecimal(byte[] target, int offset, decimal value)
        {
            int[] parts = decimal.GetBits(value);
            for (int n = 0; n < 4; ++n)
                WriteInt32(target, offset + n*4, parts[n]);
        }

        public static float ReadFloat(byte[] target, int offset)
        {
            return BitConverter.ToSingle(UnpackBits(target, offset, FloatLength), 0);
        }

        public static double ReadDouble(byte[] target, int offset)
        {
            return BitConverter.ToDouble(UnpackBits(target, offset, DoubleLength), 0);
        }

        public static decimal ReadDecimal(byte[] target, int offset)
        {
            int[] parts = new int[4];
            for (int n = 0; n < 4; ++n)
                parts[n] = ReadInt32(target, offset + n * 4);

            return new decimal(parts);
        }
        #endregion


        #region packed decimal
        /// <summary>
        ///  Writes decimal values in packed form
        /// </summary>
        /// <param name="target"></param>
        /// <param name="offset"></param>
        /// <returns>Count of used bytes</returns>
        /// <remarks>
    ///  This method of saving decimal values, is based on the obfuscation that in most cases
    ///  some of the four int-components are 0
        ///  
    ///  To not save this useless 0-components, the decimal value is seperated in 8 groups, each group 2 bytes.
    ///  Before the actual data, their is a single byte which tells what groups are available and what groups are not
        ///  
    ///  The worst case is that the packed decimal value takes one byte more than unpacked, but in practice this does not happen very often
        /// </remarks>
        public static void WritePackedDecimal(out int num_bytes, byte[] target, int offset, decimal value)
        {
            byte[] unpacked_bytes = new byte[DecimalLength];                        
            byte prefix_byte = 0;

            // Wir schreiben zumindest das prefix-byte
            num_bytes = 1;

            // Normalen unpgepackten decimal-Wert schreiben
            WriteDecimal(unpacked_bytes, offset, value);

            // Decimal-Wert packen und gleichzietig das prefix_byte konstruieren.
            for (int n_half = 0; n_half < 8; ++n_half)
            {
                if (unpacked_bytes[2*n_half] == 0 && unpacked_bytes[2*n_half + 1] == 0)
                    continue;

                // Dieser Teil ist nicht null.
                target[offset + num_bytes] = unpacked_bytes[2*n_half];
                target[offset + num_bytes + 1] = unpacked_bytes[2*n_half + 1];

                // Entsprechendes Prefix-Bit setzen ...
                prefix_byte |= (byte)(1 << n_half);
                num_bytes += 2;                
            }

            // Prefix-Byte schreiben
            target[offset] = prefix_byte;            
        }

        /// <summary>
        /// Decodes the length of a packed decimal
        /// </summary>
        /// <param name="header_byte"></param>
        /// <returns></returns>
        public static int DecodePackedDecimalLength(byte header_byte)
        {
            int n_bytes = 1;
            while (header_byte != 0)
            {
                if ((header_byte & 0x1) != 0)
                    n_bytes += 2;

                header_byte >>= 1;
            }

            return n_bytes;
        }

        /// <summary>
        /// Reads a packed decimal value
        /// </summary>
        /// <param name="num_bytes"></param>
        /// <param name="source"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static decimal ReadPackedDecimal(out int num_bytes, byte[] source, int offset)
        {
            byte[] unpacked_bytes = new byte[DecimalLength];
            byte prefix_byte = source[offset];

            // Wir lesen zumindest das Prefix-Byte
            num_bytes = 1;

            // Decimal-Wert entpacken
            for (int n_half = 0; n_half < 8; ++n_half)
            {
                if ((prefix_byte & (1 << n_half)) == 0)
                    continue;

                // Dieser Teil ist nicht null.
                unpacked_bytes[2 * n_half] = source[offset + num_bytes];
                unpacked_bytes[2 * n_half + 1] = source[offset + num_bytes + 1];

                // Entsprechendes Prefix-Bit setzen ...
                prefix_byte |= (byte)(1 << n_half);
                num_bytes += 2;
            }

            return ReadDecimal(unpacked_bytes, 0);
        }

        /// <summary>
        /// Computes the number of bytes needed for saving the given decimal value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int GetPackedDecimalLength(decimal value)
        {
            int num_bytes = 1;
            byte[] unpacked_bytes = new byte[DecimalLength];
            
            // Länge berechnen
            for (int n_half = 0; n_half < 8; ++n_half)
            {
                if (unpacked_bytes[2 * n_half] == 0 && unpacked_bytes[2 * n_half + 1] == 0)
                    continue;

                num_bytes += 2;
            }
            
            return num_bytes;
        }

        #endregion        

        #region UTF8-Strings
        /// <summary>
        /// Returns the byte length of the given UTF8 - String (including prefix)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int GetUtf8ByteLength(string value)
        {
            int utf8_bytes = Encoding.UTF8.GetByteCount(value);
            return utf8_bytes + GetPackedInt32Length(value.Length);
        }

        /// <summary>
        /// Writes the given UTF8-String to the byte stream
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int WriteUtf8String(byte[] data, int offset, string value)
        {
            int byte_length = Encoding.UTF8.GetByteCount(value);
            int packed_int_length = 0;

            WritePackedInt32(out packed_int_length, data, offset, byte_length);
            Encoding.UTF8.GetBytes(value, 0, value.Length, data, offset + Int32Length);

            return byte_length + packed_int_length;
        }

        /// <summary>
        /// Reads an UTF8 String from the given byte stream
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ReadUtf8String(byte[] data, int offset, out int read_bytes)
        {
            int byte_length = ReadPackedInt32(out read_bytes, data, offset);

            string result = Encoding.UTF8.GetString(data, offset + Int32Length, byte_length);
            read_bytes += byte_length;

            return result;
        }
        #endregion        

        #region Packed Int32       
        /// <summary>
        /// Packs a 32Bit value
        /// </summary>
        /// <param name="num_bytes"></param>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        public static void WritePackedInt32(out int num_bytes, byte[] data, int offset, int value)
        {
            // Nibble-Counter zurücksetzen, Marker-Byte auf 0 setzen      
            int used_nibbles = 0;
            data[offset] = (byte)0x00;

            for (int n_nibble = 0; n_nibble < 8; ++n_nibble)
            {
                uint nibble_mask = (uint)(0x0F << (n_nibble * 4));
                uint nibble_value = (uint)(value & nibble_mask) >> (n_nibble * 4);

                if (nibble_value != 0)
                {
                    // Dieses Nibble hat einen Wert der verschieden von Null ist.
                    data[offset] |= (byte)(1 << n_nibble);

                    if ((used_nibbles % 2) == 0)
                    {
                        // Erstes Nibble im Byte
                        data[offset + 1 + (used_nibbles / 2)] = (byte)nibble_value;
                    }
                    else
                    {
                        // Zweites Nibble im Byte
                        data[offset + 1 + (used_nibbles / 2)] |= (byte)(nibble_value << 4);
                    }

                    used_nibbles += 1;
                }
            }

            // Anzahl der benötigten Bytes merken
            num_bytes = 1 + (used_nibbles + 1) / 2;
        }

        /// <summary>
        /// Decodes a packet 32 bit value
        /// </summary>
        /// <param name="num_bytes"></param>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static int ReadPackedInt32(out int num_bytes, byte[] data, int offset)
        {
            uint result = 0;
            int used_nibbles = 0;

            for (int n_nibble = 0; n_nibble < 8; ++n_nibble)
            {
                uint nibble_use_mask = (uint)(1 << n_nibble);

                // Wird dieses Nibble verwendet?
                if (((uint)data[offset] & nibble_use_mask) != 0)
                {
                    uint base_byte = data[offset + 1 + (used_nibbles / 2)];
                    uint nibble_value;

                    if ((used_nibbles % 2) == 0)
                    {
                        // Erstes Nibble im Byte
                        nibble_value = (uint)(base_byte & 0x0F);
                    }
                    else
                    {
                        // Zweites Nibble im Byte
                        nibble_value = (uint)(base_byte & 0xF0) >> 4;
                    }

                    result |= (uint)(nibble_value << (n_nibble * 4));
                    used_nibbles += 1;
                }
            }

            num_bytes = 1 + (used_nibbles + 1) / 2;
            return (int)result;
        }

        /// <summary>
        /// Returns the length of a packed int32 value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int GetPackedInt32Length(int value)
        {
            int used_nibbles = 0;

            for (int n_nibble = 0; n_nibble < 8; ++n_nibble)
            {
                uint nibble_mask = (uint)(0x0F << (n_nibble * 4));
                if ((value & nibble_mask) != 0)
                    used_nibbles += 1;
            }

            return 1 + (used_nibbles + 1) / 2;
        }

        /// <summary>
        /// Decodes the int32 length
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int DecodePackedInt32Length(byte value)
        {
            int used_nibbles = 0;

            while (value != 0)
            {
                if ((value & 0x01) != 0)
                    ++used_nibbles;

                value >>= 1;
            }

            return 1 + (used_nibbles + 1) / 2;
        }
        #endregion

        #region Packed Int64
        /// <summary>
        /// Packs a 64Bit value
        /// </summary>
        /// <param name="num_bytes"></param>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        public static void WritePackedInt64(out int num_bytes, byte[] data, int offset, long value)
        {            
            num_bytes = 1;

            // Zurücksetzen
            data[offset] = 0x00;

            for (int n_byte = 0; n_byte < 8; ++n_byte)
            {
                long byte_mask = (long)0xFF << (n_byte * 8);

                if ((value & byte_mask) != 0)
                {
                    // Byte als verwendet markieren
                    data[offset] |= (byte)(1 << n_byte);
                    data[offset + num_bytes] = (byte)((value & byte_mask) >> (n_byte * 8));
                    num_bytes += 1;
                }
            }
        }

        /// <summary>
        /// Decodes a packet 64 bit value
        /// </summary>
        /// <param name="num_bytes"></param>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static long ReadPackedInt64(out int num_bytes, byte[] data, int offset)
        {
            long result = 0;
            num_bytes = 1;

            for (int n_byte = 0; n_byte < 8; ++n_byte)
            {                
                if ((data[offset] & (1 << n_byte)) != 0)
                {
                    result |= (long)data[offset + num_bytes] << (n_byte * 8);
                    num_bytes += 1;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the length of a packed int64 value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int GetPackedInt64Length(long value)
        {
            int used_bytes = 0;

            for (int n_byte = 0; n_byte < 8; ++n_byte)
            {
                long byte_mask = (long)0xFF << (n_byte * 8);
                if ((value & byte_mask) != 0)
                    used_bytes += 1;
            }

            return 1 + used_bytes;
        }

        /// <summary>
        /// Decodes the int32 length
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int DecodePackedInt64Length(byte value)
        {
            int used_bytes = 0;

            while (value != 0)
            {
                if ((value & 0x01) != 0)
                    ++used_bytes;

                value >>= 1;
            }

            return 1 + used_bytes;
        }
        #endregion

    }
}
