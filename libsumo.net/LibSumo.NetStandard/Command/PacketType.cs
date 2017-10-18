using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSumo.Net.Network
{     
    public enum PacketType
    {
        UNINITIALIZED,
        ACK,
        DATA,
        DATA_LOW_LATENCY,
        DATA_WITH_ACK,
        MAX
    }        
}
