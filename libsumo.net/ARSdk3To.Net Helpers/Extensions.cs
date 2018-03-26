
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARSdk3To.Net_Helpers
{
    static class Extensions
    {
        public static int LastIndexOf(this StringBuilder sb, char find, bool ignoreCase = false, int startIndex = -1, CultureInfo culture = null)
        {
            if (sb == null) throw new ArgumentNullException(nameof(sb));
            if (startIndex == -1) startIndex = sb.Length - 1;
            if (startIndex < 0 || startIndex >= sb.Length) throw new ArgumentException("startIndex must be between 0 and sb.Lengh-1", nameof(sb));
            if (culture == null) culture = CultureInfo.InvariantCulture;

            int lastIndex = -1;
            if (ignoreCase) find = Char.ToUpper(find, culture);
            for (int i = startIndex; i >= 0; i--)
            {
                char c = ignoreCase ? Char.ToUpper(sb[i], culture) : (sb[i]);
                if (find == c)
                {
                    lastIndex = i;
                    break;
                }
            }
            return lastIndex;
        }
    }
}
