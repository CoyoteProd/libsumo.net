using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSumo.Net.Protocol
{
    class Constants
    {
        public static byte ARNETWORKAL_FRAME_TYPE_ACK = 1;
        public static byte ARNETWORKAL_FRAME_TYPE_DATA = 2;
        public static byte ARNETWORKAL_FRAME_TYPE_DATA_LOW_LATENCY = 3;
        public static byte ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK = 4;
        public static byte ARNETWORK_MANAGER_INTERNAL_BUFFER_ID_PING = 0;
        public static byte ARNETWORK_MANAGER_INTERNAL_BUFFER_ID_PONG = 1;
        public static double POSITION_TIME_DELTA = 0.2; //estimated to 5Hz

        public static byte PROJECT_COMMOM = 0;

        public static byte PROJECT_SUMO = 3;
            public static byte CLASS_PILOTING = 0;
            public static byte CLASS_PILOTING_STATE = 1;
                public static byte CMD_ALERTSTATECHANGED = 1;
        
            public static byte CLASS_ANIMATIONS = 2;
            public static byte CLASS_ANIMATIONS_STATE = 3;
            public static byte CLASS_SETTINGSSTATE = 5;
        

        public static byte VIDEO_DATA_BUFFER = 0x7D;
    }
}
