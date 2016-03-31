using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSumoUni.Command
{
    public class Pong : ICommand {

        public static Pong pong() {
            return new Pong();
        }

      
        public byte[] getBytes(int counter) {
            return new byte[]{1, (byte) 0xfe, (byte) counter,8,0,0,0, (byte) counter};
        }
    }
}
