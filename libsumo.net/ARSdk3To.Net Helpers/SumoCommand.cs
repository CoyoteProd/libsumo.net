using System.Collections.Generic;

namespace ARSdk3To.Net_Helpers
{
    internal class SumoCommand
    {
        public byte prj { get; internal set; }
        public byte cls { get; internal set; }
        public byte cmd { get; internal set; }
        public List<SumoArg> args { get; internal set; }
        public string Name { get; internal set; }
        public string Desc { get; internal set; }
        public string Result { get; internal set; }
    }
}