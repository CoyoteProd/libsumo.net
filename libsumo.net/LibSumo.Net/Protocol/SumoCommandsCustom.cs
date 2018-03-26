using LibSumo.Net.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibSumo.Net.Protocol
{
    
    internal class SumoCommandsCustom
    {
                
        /// <summary>
        /// Creates a complete frame by prepending a header to the given payload
        //     Data type: normal data(2)
        //     Target buffer ID:
        //     Sequence number
        //     Frame size
        //     Payload
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        public static byte[] _pack_frame(byte[] payload)
        {
            var data_type = 2;
            var buffer_id = 10;
            var seq_no = 0;
            var frame_size = 7 + payload.Length;
            var header = StructConverter.Pack("<BBBI", data_type, buffer_id, seq_no, frame_size);
            return header.Concat(payload).ToArray();
        }

        /// <summary>
        /// Returns true if the given command is a pcmd command and false otherwise
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public static bool _is_pcmd(byte[] cmd)
        {
            // BBHBbb: Header (7) + payload (7)
            if (cmd.Length != 14)
            {
                return false;
            }
            var Result = StructConverter.Unpack("<BBH", cmd.SubArray("7:11"));
            return Tuple.Create<byte, byte, UInt16>((byte)Result[0], (byte)Result[1], (UInt16)Result[2]).Equals(Tuple.Create<byte, byte, UInt16>(3, 0, 0));
        }

        public static byte[] Move_cmd(sbyte speed, sbyte turn)
        {                        
            bool flag = ((speed != 0) || (turn != 0));
            return SumoCommandsGenerated.Piloting_PCMD_cmd(Convert.ToByte(flag), speed, turn);
        }        
    }
}
