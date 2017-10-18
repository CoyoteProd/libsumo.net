using LibSumo.Net.lib.listener;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSumo.Net.listner
{
    public class CommonEventListener : iEventListener
    {
        public bool filterProject(byte[] data, int project, int clazz, int cmd)
        {
            return data[7] == project && data[8] == clazz && data[9] == cmd;
        }

        public void consume(byte[] data)
        { }

        public bool test(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
