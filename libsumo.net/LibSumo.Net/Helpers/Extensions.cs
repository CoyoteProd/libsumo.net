using System;
using System.Linq;

namespace LibSumo.Net.Helpers
{
    internal static class Extensions
    {
        /// <summary>
        /// Mimic Python Array[a::b]
        /// </summary>
        /// <param name="b"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static byte[] SubArray(this byte[] b, string arg)
        {
            int to; int from;            
            string[] s = arg.Split(':');
            if(String.IsNullOrEmpty(s[0])) from = 0;
            else from = int.Parse(s[0]);
            if (String.IsNullOrEmpty(s[1])) to = 0;
            else to = int.Parse(s[1]);                                                      
            int c = to - from;
            if (c < 0) { c = b.Length; }
            return b.Skip(from).Take(c).ToArray();
        }      
    }
}
