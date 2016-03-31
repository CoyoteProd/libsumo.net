using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSumoUni.Command
{
    public class Jump : ICommand {
        private CommandKey commandKey = CommandKey.commandKey(3, 2, 3);

        public enum Type {
            Long, High
        }

        private Type type;

        public Jump(Type type) {
            this.type = type;
        }

        public static Jump jump(Type type) {
            return new Jump(type);
        }

        public byte[] getBytes(int counter) {
            return new byte[]{
                    (byte) FrameType.ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK,
                    ChannelType.JUMPINGSUMO_CONTROLLER_TO_DEVICE_ACK_ID,
                    (byte) counter,
                    15, 0, 0, 0,
                    commandKey.getProjectId(), commandKey.getClazzId(), commandKey.getCommandId(), 0,
                    (byte) type, 0, 0, 0};
        }
    }

}
