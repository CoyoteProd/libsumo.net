using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARSdk3To.Net_Helpers
{
    class SumoEnum
    {
        public String EnumName { get; set; }
        public String EnumValueName { get; set; }
        public int EnumValueId { get; set; }
        public String EnumValueDescription { get; set; }
        public String EnumDescription { get; internal set; }
        public String EnumFrom { get; internal set; }
        public String EnumFromDescription { get; internal set; }

        public SumoEnum()
        {
        }
    }

    class SumoProject
    {                            
        public string ProjectName { get; internal set; }
        public int ProjectId { get; internal set; }
        public string _ClassName { get; set; }
        public int _ClassId { get; internal set; }
        public string CmdName { get; internal set; }
        public int CmdId { get; internal set; }
        public string CmdDesc { get; internal set; }
        public string CmdComment { get; internal set; }
        public string CmdResult { get; internal set; }
        public string CmdTriggered { get; internal set; }
        public string CmdText { get; internal set; }
    }
}
