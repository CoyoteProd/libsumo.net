using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSumoUni.Network
{
    class CommandReader
    {
        private byte[] data;

        public CommandReader(byte[] data)
        {
            this.data = data;
        }

        public static CommandReader commandReader(byte[] data)
        {
            return new CommandReader(data);
        }

        public bool isPing()
        {
            return data[0] == 2 && data[1] == 0;
        }

        public bool isLinkQualityChanged()
        {
            return isProjectClazzCommand(3, 11, 4);
        }

        public bool isWifiSignalChanged()
        {
            return isProjectClazzCommand(0, 5, 7);
        }

        private bool isProjectClazzCommand(int project, int clazz, int command)
        {
            return data[7] == project && data[8] == clazz && data[9] == command;
        }

    }
}
