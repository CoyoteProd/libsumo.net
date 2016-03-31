using LibSumoUni.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSumoUni.Network.Command
{
    public class VideoStreaming : ICommand {
        private CommandKey commandKey = CommandKey.commandKey(3, 18, 0);

        private byte enable;

        public VideoStreaming(byte enable) {
            this.enable = enable;
        }

        public static VideoStreaming videoStreamingEnable() {
            return new VideoStreaming((byte) 1);
        }

        public static VideoStreaming videoStreamingDisable() {
            return new VideoStreaming((byte) 0);
        }

        public byte[] getBytes(int counter) {
            return new byte[]{
                    (byte) FrameType.ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK,
                    ChannelType.JUMPINGSUMO_CONTROLLER_TO_DEVICE_ACK_ID,
                    (byte) counter,
                    12, 0, 0, 0,
                    commandKey.getProjectId(), commandKey.getClazzId(), commandKey.getCommandId(), 0,
                    enable, 0};
        }
    }
}
