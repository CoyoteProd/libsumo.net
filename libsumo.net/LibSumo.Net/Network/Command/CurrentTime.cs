using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSumoUni.Command
{
    public class CurrentTime : ICommand {

       

        private CommandKey commandKey = CommandKey.commandKey(0, 4, 1);

        public static CurrentTime currentTime() {
            return new CurrentTime();
        }
       
        public byte[] getBytes(int counter) {

            byte[] header = {(byte) FrameType.ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK,
                    ChannelType.JUMPINGSUMO_CONTROLLER_TO_DEVICE_ACK_ID, (byte) counter, 15, 0, 0, 0,
                    commandKey.getProjectId(), commandKey.getClazzId(), commandKey.getCommandId(), 0};

            try
            {
                using (var outputStream = new MemoryStream())
                {
                    // Stream encodes as UTF-8 by default; specify other encodings in this constructor
                    using (var ps = new StreamWriter(outputStream))
                    {
                        ps.Write(header);
                        ps.Write(NullTerminatedString.getNullTerminatedString(String.Format("{0:'T'HHmmssZZZ", DateTime.Now)));
                    }

                    // Extract bytes from MemoryStream
                    return outputStream.ToArray();
                }
            }
            catch (IOException e)
            {
                throw (e);
            }
        }
    }
}
