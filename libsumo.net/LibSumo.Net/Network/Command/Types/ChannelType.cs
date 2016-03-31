using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSumoUni.Command
{
       public class ChannelType 
       {
           public static byte JUMPINGSUMO_CONTROLLER_TO_DEVICE_NONACK_ID = (10);
           public static byte JUMPINGSUMO_CONTROLLER_TO_DEVICE_ACK_ID = (11);
           public static byte JUMPINGSUMO_CONTROLLER_TO_DEVICE_VIDEO_ACK_ID = (13);
           public static byte JUMPINGSUMO_DEVICE_TO_CONTROLLER_NAVDATA_ID = ((256 / 2) - 1);
           public static byte JUMPINGSUMO_DEVICE_TO_CONTROLLER_EVENT_ID = ((256 / 2) - 2);
           public static byte JUMPINGSUMO_DEVICE_TO_CONTROLLER_VIDEO_DATA_ID = ((256 / 2) - 3);

            //private byte id;

            //ChannelType(int id) {
            //    this.id = (byte) id;
            //}

            //public byte getId() {
            //    return id;
            //}
    }
}
