using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSumoUni.Command
{
    public enum FrameType
    {
        ARNETWORKAL_FRAME_TYPE_UNINITIALIZED, /**< Unknown type. Don't use */
        ARNETWORKAL_FRAME_TYPE_ACK, /**< Acknowledgment type. Internal use only */
        ARNETWORKAL_FRAME_TYPE_DATA, /**< Data type. Main type for data that does not require an acknowledge */
        ARNETWORKAL_FRAME_TYPE_DATA_LOW_LATENCY, /**< Low latency data type. Should only be used when needed */
        ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK, /**< Data that should have an acknowledge type. This type can have a long latency */
        ARNETWORKAL_FRAME_TYPE_MAX, /**< Unused, iterator maximum value */
    }
}
