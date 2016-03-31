using LibSumoUni.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSumoUni.Network.Command
{
    class Disconnect : ICommand {

        private CommandKey commandKey = CommandKey.commandKey(0, 0, 0);

        public static Disconnect disconnect() {
            return new Disconnect();
        }

   
        public byte[] getBytes(int counter) {

            return new byte[]{(byte) FrameType.ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK,
                    ChannelType.JUMPINGSUMO_CONTROLLER_TO_DEVICE_ACK_ID, (byte) counter, 15, 0, 0, 0,
                    commandKey.getProjectId(), commandKey.getClazzId(), commandKey.getCommandId(), 0};

        }
    }
}
