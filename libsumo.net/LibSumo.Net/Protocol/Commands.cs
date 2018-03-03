using LibSumo.Net.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibSumo.Net.Protocol
{
    internal class Commands
    {

        #region Frame Methods
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
        #endregion

        #region Movement Methods
        /// <summary>
        ///  Project: jpsumo(3), Class: Piloting (0), Command: PCMD (0)
        /// </summary>
        /// <param name="speed">Speed: [-100, 100]</param>
        /// <param name="turn">Turn: [-100, 100]</param>
        /// <returns></returns>
        public static byte[] Move_cmd(sbyte speed, sbyte turn)
        {
            // ARCOMMANDS_ID_PROJECT_JUMPINGSUMO = 3,
            // ARCOMMANDS_ID_JUMPINGSUMO_CLASS_PILOTING = 0,
            // ARCOMMANDS_ID_JUMPINGSUMO_PILOTING_CMD_PCMD = 0,
            // Touch - Boolean for "touch screen".            
            byte Project = 0x03; byte Class = 0x00; UInt16 Cmd = 0x00;
            bool Touch = ((speed != 0) || (turn != 0));
            return StructConverter.Pack("<BBHBbb", Project, Class, Cmd, Convert.ToByte(Touch), speed, turn );            
        }

        public static byte[] Animation_cmd(SumoEnum.Animation Animation)
        {            
            return StructConverter.Pack("<BBHI", 3, 2, 4, (UInt32)Animation ); 
        }

        public static byte[] Postures_cmd(SumoEnum.Posture Posture)
        {           
            return StructConverter.Pack("<BBHI",  3,  0,  1, (UInt16)Posture );
        }

        /// <summary>
        /// Request a jump
        /// </summary>
        /// <param name="JumpType"></param>
        /// <returns></returns>
        public static byte[] Jump_cmd(SumoEnum.Jump JumpType)
        {            
            return StructConverter.Pack("<BBHI", 3, 2, 3, (UInt32)JumpType);
        }
        /// <summary>
        /// Request jump loading
        /// </summary>
        /// <returns></returns>
        public static byte[] JumpLoading_cmd()
        {
            return StructConverter.Pack("<BBH", 3, 2, 2);
        }
        /// <summary>
        /// Cancel jump and come back to previous state (if possible).
        /// </summary>
        /// <returns></returns>
        public static byte[] CancelJump_cmd()
        {
            return StructConverter.Pack("<BBH", 3, 2, 1);
        }

        /// <summary>
        /// Stop jump, emergency jump stop, stop jump motor and stay there.
        /// </summary>
        /// <returns></returns>
        public static byte[] STOP_cmd()
        {
            return StructConverter.Pack("<BBH", 3, 2, 0);
        }
        /// <summary>
        /// Quick Turn
        /// </summary>
        /// <param name="Angle">in radian</param>
        /// <returns></returns>
        public static byte[] Turn_cmd(float Angle)
        {            
            return StructConverter.Pack("<BBHd", 3, 0, 2, Angle);
        }
        #endregion

        #region Control Methods
        /// <summary>
        /// Set Master Audio Volume
        /// </summary>
        /// <param name="Volume">Master audio volume [0:100]</param>
        /// <returns></returns>
        public static byte[] Volume_cmd(byte Volume)
        {            
            return StructConverter.Pack("<BBHB", 3, 12, 0, Volume);
        }

        public static byte[] RequestAllStates_cmd()
        {
            return StructConverter.Pack("<BBH", 0, 4, 0);
        }
        public static byte[] RequestAllConfig_cmd()
        {
            return StructConverter.Pack("<BBH", 0, 2, 0);
        }
      

        /// <summary>
        /// Enable/Disabe Video Streaming
        /// </summary>
        /// <param name="enable"></param>
        /// <returns></returns>
        public static byte[] Set_media_streaming_cmd(bool enable = true)
        {            
            return StructConverter.Pack("<BBHB", 3, 18, 0, enable);
        }

        /// <summary>
        /// Project: Commom(0), Class: Common(4), Command: CurrentDate(1)
        ///    Date (ISO-8601 format)
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static byte[] Sync_date_cmd(DateTime date)
        {
            // ARCOMMANDS_ID_PROJECT_COMMON = 0,
            // ARCOMMANDS_ID_COMMON_CLASS_COMMON = 4,
            // ARCOMMANDS_ID_COMMON_COMMON_CMD_CURRENTDATE = 1,
            // Date with ISO-8601 format
            //return struct.pack("BBH", 0, 4, 1) + datetime.datetime.now() .isoformat() + '\0'            
            return (new byte[] { 0x00, 0x04, 0x01, 0x00 }).Concat(Encoding.UTF8.GetBytes(DateTime.Now.ToString("yyyy-MM-dd\0"))).ToArray();
        }

        
        /// <summary>
        ///  Project: Commom(0), Class: Common(4), Command: CurrentDate(2)
        ///     Time (ISO-8601 format)
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static byte[] Sync_time_cmd(DateTime date)
        {
            // ARCOMMANDS_ID_PROJECT_COMMON = 0,
            // ARCOMMANDS_ID_COMMON_CLASS_COMMON = 4,
            // ARCOMMANDS_ID_COMMON_COMMON_CMD_CURRENTDATE = 1,
            // Date with ISO-8601 format
            //return struct.pack("BBH", 0, 4, 1) + datetime.datetime.now() .isoformat() + '\0'            
            return (new byte[] { 0x00, 0x04, 0x01, 0x00 }).Concat(Encoding.UTF8.GetBytes(DateTime.Now.ToString("HH:mm:ss\0"))).ToArray();
        }

        

        #endregion
    }
}
