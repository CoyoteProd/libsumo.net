using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace LibSumo.Net.Helpers
{

    internal class StructConverter
    {
        // We use this function to provide an easier way to type-agnostically call the GetBytes method of the BitConverter class.
        // This means we can have much cleaner code below.
        private static byte[] TypeAgnosticGetBytes(object o)
        {
            if (o is int) return BitConverter.GetBytes((int)o);
            if (o is uint) return BitConverter.GetBytes((uint)o);
            if (o is long) return BitConverter.GetBytes((long)o);
            if (o is ulong) return BitConverter.GetBytes((ulong)o);
            if (o is short) return BitConverter.GetBytes((short)o);
            if (o is ushort) return BitConverter.GetBytes((ushort)o);
            if (o is double) return BitConverter.GetBytes((double)o);
            if (o is float) return BitConverter.GetBytes((float)o);
            if (o is sbyte) return new byte[] { BitConverter.GetBytes((sbyte)o)[0] };
            if (o is byte) return new byte[] { (byte)o };
            if (o is string) return Encoding.UTF8.GetBytes((string)o);
            throw new ArgumentException("Unsupported object type found");
        }

        private static string GetFormatSpecifierFor(object o)
        {
            if (o is int) return "i";
            if (o is uint) return "I";
            if (o is long) return "q";
            if (o is ulong) return "Q";
            if (o is short) return "h";
            if (o is ushort) return "H";            
            if (o is double) return "d";
            if (o is float) return "d";
            if (o is byte) return "B";
            if (o is sbyte) return "b";
            if (o is string) return "s";
            throw new ArgumentException("Unsupported object type found");
        }

        /// <summary>
        /// Convert a byte array into an array of objects based on Python's "struct.unpack" protocol.
        /// </summary>
        /// <param name="fmt">A "struct.pack"-compatible format string</param>
        /// <param name="bytes">An array of bytes to convert to objects</param>
        /// <returns>Array of objects.</returns>
        /// <remarks>You are responsible for casting the objects in the array back to their proper types.</remarks>
        public static object[] Unpack(string fmt, byte[] bytes)
        {
            //Debug.WriteLine("Format string is length {0}, {1} bytes provided.", fmt.Length, bytes.Length);

            // First we parse the format string to make sure it's proper.
            if (fmt.Length < 1) throw new ArgumentException("Format string cannot be empty.");
                        
            if (fmt.Substring(0, 1) == "<")
            {                
                fmt = fmt.Substring(1);
            }
            else if (fmt.Substring(0, 1) == ">")
            {
                throw new NotImplementedException();
            }

            // Now, we find out how long the byte array needs to be
            int totalByteLength = 0;
            foreach (char c in fmt.ToCharArray())
            {
                //Debug.WriteLine("  Format character found: {0}", c);
                switch (c)
                {
                    case 'q':
                    case 'Q':
                    case 'd':
                        totalByteLength += 8;
                        break;
                    case 'i':
                    case 'I':
                        totalByteLength += 4;
                        break;
                    case 'h':
                    case 'H':
                        totalByteLength += 2;
                        break;
                    case 'b':
                    case 'B':
                    case 'x':
                        totalByteLength += 1;
                        break;
                    default:
                        throw new ArgumentException("Invalid character found in format string.");
                }
            }

            // Ok, we can go ahead and start parsing bytes!
            int byteArrayPosition = 0;
            List<object> outputList = new List<object>();
            byte[] buf;

            //Debug.WriteLine("Processing byte array...");
            foreach (char c in fmt.ToCharArray())
            {
                switch (c)
                {
                    case 'd':
                        outputList.Add((object)(long)BitConverter.ToDouble(bytes, byteArrayPosition));
                        byteArrayPosition += 8;
                        //Debug.WriteLine("  Added double.");
                        break;
                    case 'q':
                        outputList.Add((object)(long)BitConverter.ToInt64(bytes, byteArrayPosition));
                        byteArrayPosition += 8;
                        //Debug.WriteLine("  Added signed 64-bit integer.");
                        break;
                    case 'Q':
                        outputList.Add((object)(ulong)BitConverter.ToUInt64(bytes, byteArrayPosition));
                        byteArrayPosition += 8;
                        //Debug.WriteLine("  Added unsigned 64-bit integer.");
                        break;
                    case 'l':
                    case 'i':
                        outputList.Add((object)(int)BitConverter.ToInt32(bytes, byteArrayPosition));
                        byteArrayPosition += 4;
                        //Debug.WriteLine("  Added signed 32-bit integer.");
                        break;                    
                    case 'I':
                    case 'L':
                        outputList.Add((object)(uint)BitConverter.ToUInt32(bytes, byteArrayPosition));
                        byteArrayPosition += 4;
                        //Debug.WriteLine("  Added unsignedsigned 32-bit integer.");
                        break;
                    case 'h':
                        outputList.Add((object)(short)BitConverter.ToInt16(bytes, byteArrayPosition));
                        byteArrayPosition += 2;
                        //Debug.WriteLine("  Added signed 16-bit integer.");
                        break;
                    case 'H':
                        outputList.Add((object)(ushort)BitConverter.ToUInt16(bytes, byteArrayPosition));
                        byteArrayPosition += 2;
                        //Debug.WriteLine("  Added unsigned 16-bit integer.");
                        break;
                    case 'b':
                        buf = new byte[1];
                        Array.Copy(bytes, byteArrayPosition, buf, 0, 1);
                        outputList.Add((object)(sbyte)buf[0]);
                        byteArrayPosition++;
                        //Debug.WriteLine("  Added signed byte");
                        break;
                    case 'B':
                        buf = new byte[1];
                        Array.Copy(bytes, byteArrayPosition, buf, 0, 1);
                        outputList.Add((object)(byte)buf[0]);
                        byteArrayPosition++;
                        //Debug.WriteLine("  Added unsigned byte");
                        break;
                    case 'x':
                        byteArrayPosition++;
                        //Debug.WriteLine("  Ignoring a byte");
                        break;
                    default:
                        throw new ArgumentException("You should not be here.");
                }
            }
            return outputList.ToArray();
        }
        
        /// <summary>
        /// Convert an array of objects to a byte array, along with a string that can be used with Unpack.
        /// </summary>
        /// <param name="items">An object array of items to convert</param>
        /// <param name="LittleEndian">Set to False if you want to use big endian output.</param>
        /// <param name="NeededFormatStringToRecover">Variable to place an 'Unpack'-compatible format string into.</param>
        /// <returns>A Byte array containing the objects provided in binary format.</returns>
        private static byte[] Pack(List<object> items, bool LittleEndian, out string NeededFormatStringToRecover)
        {

            // make a byte list to hold the bytes of output
            List<byte> outputBytes = new List<byte>();

            // should we be flipping bits for proper endinanness?
            bool endianFlip = (LittleEndian != BitConverter.IsLittleEndian);

            // start working on the output string
            string outString = ""; // (LittleEndian == false ? ">" : "<");

            // convert each item in the objects to the representative bytes
            foreach (object o in items)
            {
                byte[] theseBytes = TypeAgnosticGetBytes(o);
                if (endianFlip == true) theseBytes = (byte[])theseBytes.Reverse();
                outString += GetFormatSpecifierFor(o);
                outputBytes.AddRange(theseBytes);
            }

            NeededFormatStringToRecover = outString;

            return outputBytes.ToArray();

        }

        public static byte[] Pack(string fmt, params object[] objects)
        {
            //Debug.WriteLine("Format string is length {0}, {1} bytes provided.", fmt.Length, bytes.Length);

            // First we parse the format string to make sure it's proper.
            if (fmt.Length < 1) throw new ArgumentException("Format string cannot be empty.");
            if (fmt.Length-1 != objects.Length) throw new ArgumentException("Format string not match Array of Object");

            //bool endianFlip = false;
            if (fmt.Substring(0, 1) == "<")
            {                
                fmt = fmt.Substring(1);
            }
            else if (fmt.Substring(0, 1) == ">")
            {
                throw new NotImplementedException();
            }            
                        
            // Ok, we can go ahead and start parsing bytes!
            int objectArrayPosition = 0;
            List<object> outputList = new List<object>();            

            //Debug.WriteLine("Processing byte array...");
            foreach (char c in fmt.ToCharArray())
            {
                switch (c)
                {
                    
                    case 'd':
                        outputList.Add((float)Convert.ToDouble(objects[objectArrayPosition]));                        
                        //Debug.WriteLine("  Added double.");
                        break;                    
                    case 'q':
                        outputList.Add(Convert.ToInt64(objects[objectArrayPosition]));                        
                        //Debug.WriteLine("  Added signed 64-bit integer.");
                        break;
                    case 'Q':
                        outputList.Add(Convert.ToUInt64(objects[objectArrayPosition]));                        
                        //Debug.WriteLine("  Added unsigned 64-bit integer.");
                        break;
                    case 'l':
                    case 'i':
                        outputList.Add(Convert.ToInt32(objects[objectArrayPosition]));                        
                        //Debug.WriteLine("  Added signed 32-bit integer.");
                        break;
                    case 'I':
                    case 'L':
                        outputList.Add(Convert.ToUInt32(objects[objectArrayPosition]));                        
                        //Debug.WriteLine("  Added unsignedsigned 32-bit integer.");
                        break;
                    case 'h':
                        outputList.Add(Convert.ToInt16(objects[objectArrayPosition]));                        
                        //Debug.WriteLine("  Added signed 16-bit integer.");
                        break;
                    case 'H':
                        outputList.Add(Convert.ToUInt16(objects[objectArrayPosition]));                        
                        //Debug.WriteLine("  Added unsigned 16-bit integer.");
                        break;
                    case 'b':
                        outputList.Add(Convert.ToSByte(objects[objectArrayPosition]));
                        //Debug.WriteLine("  Added signed byte");
                        break;
                    case 'B':
                        outputList.Add(Convert.ToByte(objects[objectArrayPosition]));
                        //Debug.WriteLine("  Added unsigned byte");
                        break;
                    case 'x':                        
                        //Debug.WriteLine("  Ignoring a byte");
                        break;
                    case 's':
                        outputList.Add(objects[objectArrayPosition]);
                        break;
                    default:
                        throw new ArgumentException("You should not be here.");
                }
                objectArrayPosition++;
            }
                        
            byte[] OutByte = Pack(outputList, true, out string dummy);
            if(!dummy.Equals(fmt)) throw new NotSupportedException("Conversion Failed");
            return OutByte;
        }
        
    }
}
