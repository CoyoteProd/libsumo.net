namespace LibSumo.Net.lib.command
{

	/// <summary>
	/// @author  Alexander Bischof
	/// </summary>
	public enum FrameType
	{

		// < Unknown type. Don't use
		ARNETWORKAL_FRAME_TYPE_UNINITIALIZED,

		// < Acknowledgment type. Internal use only
		ARNETWORKAL_FRAME_TYPE_ACK,

		// < Data type. Main type for data that does not require an acknowledge
		ARNETWORKAL_FRAME_TYPE_DATA,

		// < Low latency data type. Should only be used when needed
		ARNETWORKAL_FRAME_TYPE_DATA_LOW_LATENCY,

		// < Data that should have an acknowledge type. This type can have a long latency
		ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK,

		// < Unused, iterator maximum value
		ARNETWORKAL_FRAME_TYPE_MAX,
	}

}