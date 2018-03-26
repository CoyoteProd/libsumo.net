using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARSdk3To.Net_Helpers
{
    class Program
    {
        static void Main(string[] args)
        {

            // Generate Enum and Constant found in XML to .Net Class
            // All XML can be found here : https://github.com/Parrot-Developers/arsdk-xml        
            // Output is ConstOutput.txt and EnumOutput.txt
            ParseSdkXML parser = new ParseSdkXML(new string[] { "common.xml", "jpsumo.xml" } );
            parser.Download();
            parser.Parse();
        }
    }
}
