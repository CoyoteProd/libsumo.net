using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSumoUni.Command
{
    public class Pcmd : ICommand {
        private CommandKey commandKey = CommandKey.commandKey(3, 0, 0);
        private byte speed;
        private byte turn;

        public Pcmd(int speed, int turn) {
            this.speed = (byte) speed;
            this.turn = (byte) turn;
        }

        public static Pcmd pcmd(int speed, int turn) {
            return new Pcmd(speed, turn);
        }

        
        public byte[] getBytes(int counter) {
            byte touchscreen = 1;
            return new byte[]{(byte) FrameType.ARNETWORKAL_FRAME_TYPE_DATA,
                    ChannelType.JUMPINGSUMO_CONTROLLER_TO_DEVICE_NONACK_ID,
                    (byte) counter,
                    14, 0, 0, 0,
                    commandKey.getProjectId(), commandKey.getClazzId(), commandKey.getCommandId(), 0,
                    touchscreen, speed, turn};
        }
    }
}
