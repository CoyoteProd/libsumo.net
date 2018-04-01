using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ARSdk3To.Net_Helpers
{
    class ParseSdkXML
    {
        private List<String> xmlUrl;
        private string BaseUrl = "https://raw.githubusercontent.com/Parrot-Developers/arsdk-xml/master/xml/";
        public ParseSdkXML(String[] _xmlUrl)
        {
            xmlUrl = _xmlUrl.ToList();
        }

        public void Download()
        {
            // Download xml from base Url
            using (WebClient web = new WebClient())
            {
                foreach (string file in xmlUrl)
                {
                    web.DownloadFile(BaseUrl + file, Path.Combine(@"c:\temp\", file));
                }
            }
        }
        public void Parse()
        {
            List<SumoEnum> sList = new List<SumoEnum>();
            List<SumoProject> pList = new List<SumoProject>();
            List<SumoCommand> cList = new List<SumoCommand>();
            foreach (string file in xmlUrl)
            {
                using (StreamReader xmlReader = new StreamReader(Path.Combine(@"c:\temp\", file)))
                {
                    XmlRootAttribute xRoot = new XmlRootAttribute
                    {
                        ElementName = "project",
                        IsNullable = true
                    };
                    var serializer = new XmlSerializer(typeof(sumoProject), xRoot);
                    sumoProject Project = (sumoProject)serializer.Deserialize(xmlReader);
                    foreach (sumoProjectClass cls in Project.@class)
                    {
                        foreach (sumoProjectClassCmd cmd in cls.cmd)
                        {
                            // Skip deprecated cmd
                            if (cmd.deprecated) continue;
                            if (cmd.Text != null && cmd.Text[0].Contains("@deprecated")) continue;

                            // Store Command
                            if (!cls.name.Contains("State") && !cls.name.Contains("Event"))
                            {
                                SumoCommand c = new SumoCommand()
                                {
                                    prj = Project.id,
                                    cls = cls.id,
                                    cmd = cmd.id,
                                    Name = cls.name+"_"+cmd.name,
                                    Desc = cmd.comment!=null?cmd.comment.desc:"",
                                    Result = cmd.comment != null ? cmd.comment.result:""
                                };
                                if (String.IsNullOrWhiteSpace(c.Desc))
                                    c.Desc = cmd.Text!=null?cmd.Text[0]:"";
                                // Process Argument
                                if (cmd.arg != null)
                                {
                                    c.args = new List<SumoArg>();
                                    foreach (sumoProjectClassCmdArg arg in cmd.arg)
                                    {                                        
                                        SumoArg a = new SumoArg()
                                        {
                                            Type = arg.type,
                                            Name = arg.name,
                                        };

                                        if(a.Type.Equals("enum"))
                                        {
                                            a.EnumType = cmd.name + "_" + arg.name;
                                        }
                                        c.args.Add(a);
                                    }
                                }
                                cList.Add(c);
                            }

                            // Store Command Tuple
                            if (cls.name.Contains("State") || cls.name.Contains("Event"))
                            {
                                SumoProject p = new SumoProject()
                                {
                                    ProjectName = Project.name,
                                    ProjectId = Project.id,
                                    _ClassName = cls.name,
                                    _ClassId = cls.id,
                                    CmdName = cmd.name,
                                    CmdId = cmd.id,
                                    CmdComment = cmd.comment != null ? cmd.comment.title : "",
                                    CmdDesc = cmd.comment != null ? cmd.comment.desc.Replace("\\n", "\r\n///") : "",
                                    CmdText = cmd.Text != null ? cmd.Text[0].Trim('\t').Trim('\n').Trim('\t') : "",
                                    CmdResult = cmd.comment != null ? cmd.comment.result : "",
                                    CmdTriggered = cmd.comment != null ? cmd.comment.triggered : ""
                                };
                                
                                pList.Add(p);
                            }

                            //Store Enum
                            if (cmd.arg != null)
                            {                               
                                foreach (sumoProjectClassCmdArg arg in cmd.arg)
                                {
                                    if (arg.@enum != null)
                                    {
                                        for (int t = 0; t < arg.@enum.Length; t++)
                                        {

                                            if (arg.@type == "enum")
                                            {
                                                sumoProjectClassCmdArgEnum sEnum = arg.@enum[t];

                                                // Validation
                                                if (Regex.IsMatch(sEnum.name, @"^\d+") || sEnum.name.Equals("long") || sEnum.name.Equals("default"))
                                                    sEnum.name = "_" + sEnum.name;

                                                SumoEnum s = new SumoEnum
                                                {
                                                    EnumName = cmd.name + "_" + arg.name,
                                                    EnumFrom = cls.name,
                                                    EnumFromDescription = cls.Text != null ? cls.Text[0].Trim('\t').Trim('\n').Trim('\t') : "",
                                                    EnumDescription = arg.Text != null ? arg.Text[0].Trim('\t').Trim('\n').Trim('\t') : "",
                                                    EnumValueName = sEnum.name,
                                                    EnumValueId = t,
                                                    EnumValueDescription = sEnum.Value.Trim('\t').Trim('\n').Trim('\t')
                                                };
                                                sList.Add(s);
                                            }
                                        }
                                    }
                                }                                
                            }
                        }
                    }
                }
            }

            // Construct Enum            
            string lastEnumName = "";
            List<string> enumName = new List<string>();
            List<string> enumValue = new List<string>();
            StringBuilder sb = new StringBuilder();
            foreach (SumoEnum str in sList)
            {
                if (!lastEnumName.Equals(str.EnumName))
                {
                    // Skip if enum is already processed
                    if (enumName.Contains(str.EnumName)) continue;

                    // Close last enum
                    if (!String.IsNullOrEmpty(lastEnumName))
                    {
                        // remove last comma
                        sb.Remove(sb.LastIndexOf(','), 1);
                        // Close enum
                        sb.Append(String.Format("}};\r\n\r\n"));
                    }

                    // New Enum              
                    sb.Append("/// <summary>\r\n");
                    sb.Append(String.Format("/// {0} \r\n", str.EnumDescription.Replace("\n", " ")));
                    sb.Append(String.Format("/// From class : {0} \r\n", str.EnumFrom.Replace("\n", " ")));
                    sb.Append(String.Format("///            : {0} \r\n", str.EnumFromDescription.Replace("\n", " ")));
                    sb.Append("/// </summary>\r\n");
                    sb.Append(String.Format("public enum {0} \r\n{{", str.EnumName));
                    lastEnumName = str.EnumName;
                    enumName.Add(lastEnumName);
                    enumValue.Clear();
                }

                // Skip double enumValue
                if (enumValue.Contains(str.EnumValueName)) continue;
                enumValue.Add(str.EnumValueName);

                sb.Append("\t/// <summary>\r\n");
                sb.Append(String.Format("\t/// {0} \r\n", str.EnumValueDescription.Replace("\n", " ").Replace("\t", "")));
                sb.Append("\t/// </summary>\r\n");
                sb.Append(String.Format("\t{0},\r\n", str.EnumValueName));


            }
            // Close last enum
            sb.Append(String.Format("}};\r\n\r\n"));
            File.WriteAllText("SumoEnumGenerated.snippet", sb.ToString());


            // Construct Constant
            
            sb.Clear();
            StringBuilder sbTuple = new StringBuilder();
            string LastProject = "";
            List<string> projectName = new List<string>();
            List<string> className = new List<string>();
            List<string> cmdName = new List<string>();
            foreach (SumoProject prj in pList)
            {
                if(!LastProject.Equals(prj.ProjectName))
                {
                    //New Project
                    className.Clear();
                    cmdName.Clear();
                    LastProject = prj.ProjectName;
                }
                if (!projectName.Contains(prj.ProjectName))                
                    sb.Append(String.Format("public const int {0}_project = {1};\r\n", prj.ProjectName, prj.ProjectId));
                if (!className.Contains(prj._ClassName))
                    sb.Append(String.Format("public const int {0}_project_{1}_class = {2};\r\n", prj.ProjectName, prj._ClassName, prj._ClassId));
                if (!cmdName.Contains(prj.CmdName))
                {
                    sb.Append(String.Format("public const int {0}_project_{1}_class_{2}_cmd = {3};\r\n", prj.ProjectName, prj._ClassName, prj.CmdName, prj.CmdId));

                    // Create Tuple
                    sbTuple.Append("/// <summary>\r\n");
                    string comment;
                    if (!String.IsNullOrEmpty(prj.CmdComment))
                    {
                        sbTuple.Append(String.Format("/// {0} \r\n", prj.CmdComment));
                        comment = prj.CmdDesc.Replace(prj.CmdComment, "").Trim('.').Replace("    ", "");
                    }
                    else
                        comment = prj.CmdDesc.Trim('.').Replace("    ", "");

                    if (!String.IsNullOrWhiteSpace(comment))
                        sbTuple.Append(String.Format("/// {0} \r\n", prj.CmdDesc.Replace(prj.CmdComment, "").Trim('.').Replace("    ","")));
                    else if(!String.IsNullOrWhiteSpace(prj.CmdText))
                        sbTuple.Append(String.Format("/// {0} \r\n", prj.CmdText));

                    string triggered = prj.CmdTriggered;
                    if (!String.IsNullOrWhiteSpace(triggered))
                        sbTuple.Append(String.Format("/// Triggered: {0} \r\n", triggered));
                    
                    sbTuple.Append("/// </summary>\r\n");
                    sbTuple.Append(String.Format("public static Tuple<byte, byte, ushort> {0}_{1}_{2} = Tuple.Create<byte, byte, ushort>({0}_project, {0}_project_{1}_class, {0}_project_{1}_class_{2}_cmd);\r\n\r\n", prj.ProjectName, prj._ClassName, prj.CmdName));
                }
                projectName.Add(prj.ProjectName);
                className.Add(prj._ClassName);
                cmdName.Add(prj.CmdName);
            }
            File.WriteAllText("SumoConstantsGenerated.snippet", sb.Append(sbTuple.ToString()).ToString());


            // Create List of Command
            // return StructConverter.Pack("<BBHI", 3, 2, 4, (UInt32)Animation ); 
            StringBuilder csb = new StringBuilder();
            foreach(SumoCommand c in cList)
            {
                csb.Append("/// <summary>\r\n");
                csb.Append(String.Format("/// {0} \r\n", c.Name));
                csb.Append(String.Format("/// {0} \r\n", c.Desc).Replace("\\n\\n", "\\n").Replace("\\n", "\r\n///").Replace("    ", "").Replace("\t\t\t","/// "));
                csb.Append(String.Format("/// {0} \r\n", c.Result).Replace("\\n\\n", "\\n").Replace("\\n", "\r\n///").Replace("    ", "").Replace("\t\t\t", "/// "));
                string argStr = String.Format("{0}, {1}, {2} ",c.prj, c.cls, c.cmd);                
                string argDefStr = "";
                string strf = "BBH";
                if (c.args != null)
                {
                    csb.Append("/// args : ");
                    foreach (SumoArg a in c.args)
                    {
                        
                        csb.Append(String.Format("{0} ", a.Type));
                        switch (a.Type)
                        {
                            case ("u8"): strf += "B";
                                argDefStr += "byte";
                                break;
                            case ("enum"):
                                strf += "I";
                                argDefStr += "SumoEnumGenerated." + a.EnumType;
                                break;
                            case ("i8"): strf += "b";
                                argDefStr += "sbyte";
                                break;
                            case ("float"): strf += "d";
                                argDefStr += "float";
                                break;
                            case ("double"): strf += "d";
                                argDefStr += "double";
                                break;
                            case ("string"):
                                argDefStr += "String";
                                strf += "s";
                                break;
                        }

                        if(a.Type=="enum")                            
                            argStr += ", (UInt16)" + a.Name;
                        else
                            argStr += ", "+a.Name;

                        argDefStr += " " + a.Name +", ";
                    }
                    argDefStr = argDefStr.Remove(argDefStr.LastIndexOf(','), 1);
                    csb.Append("\r\n");
                }
                csb.Append("/// </summary>\r\n");

                csb.Append(String.Format("public static byte[] {0}_cmd({1}) \r\n{{\r\n", c.Name, argDefStr));
                csb.Append(String.Format("\treturn StructConverter.Pack(\"<{0}\", {1});\r\n",strf, argStr));
                csb.Append(String.Format("}}\r\n\r\n"));
            }
            File.WriteAllText("SumoCommandsGenerated.snippet", csb.ToString());
        }
    }
}
