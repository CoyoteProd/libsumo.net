using System;
using System.Collections.Generic;
using System.Text;

namespace LibSumo.Net.Protocol
{
    internal class SumoConstantsCustom
    {
        // ARNETWORKAL_FRAME_TYPE is From : https://github.com/Parrot-Developers/libARNetworkAL/blob/master/Includes/libARNetworkAL/ARNETWORKAL_Frame.h
        public enum ARNETWORKAL_FRAME_TYPE
        {
            ARNETWORKAL_FRAME_TYPE_UNINITIALIZED = 0, //**< Unknown type. Don't use */
            ARNETWORKAL_FRAME_TYPE_ACK, //**< Acknowledgment type. Internal use only */
            ARNETWORKAL_FRAME_TYPE_DATA, //**< Data type. Main type for data that does not require an acknowledge */
            ARNETWORKAL_FRAME_TYPE_DATA_LOW_LATENCY, //**< Low latency data type. Should only be used when needed */
            ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK, //**< Data that should have an acknowledge type. This type can have a long latency */
            ARNETWORKAL_FRAME_TYPE_MAX, //**< Unused, iterator maximum value */
        }
        // NETWORK_DC_VIDEO_DATA_ID is from :  https://github.com/Parrot-Developers/libARCommands/blob/master/WiresharkPlugin/protocol.h
        public const int NETWORK_DC_VIDEO_DATA_ID = 0x7D;
        public const int NETWORK_DC_SOUND_DATA_ID = 0x7C;
    }
}
