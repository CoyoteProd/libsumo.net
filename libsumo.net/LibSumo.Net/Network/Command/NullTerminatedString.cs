using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSumoUni.Command
{
    public class NullTerminatedString {
  
    public static byte[] getNullTerminatedString(String str) {
        try {
            byte[] stringBytes = Encoding.UTF8.GetBytes(str);
            byte[] ntBytes = new byte[stringBytes.Length + 1];
            
            Array.Copy(stringBytes, 0, ntBytes, 0, stringBytes.Length);

            return ntBytes;
        } catch (Exception e) {
            throw(e);
        }
    }
}
}
