namespace LibSumo.Net.lib.command.multimedia
{



	/// <summary>
	/// @author  Alexander Bischof
	/// @author  Tobias Schneider
	/// </summary>
	public  class VideoStreaming : Command
	{

		private readonly CommandKey commandKey = CommandKey.commandKey(3, 18, 0);
		private readonly byte enable;

		protected internal VideoStreaming(byte enable)
		{

			this.enable = enable;
		}

		public static VideoStreaming enableVideoStreaming()
		{

			return new VideoStreaming((byte) 1);
		}


		public static VideoStreaming disableVideoStreaming()
		{

			return new VideoStreaming((byte) 0);
		}


		new public byte[] getBytes(int counter)
		{

			return new byte[] {(byte) FrameType.ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK, ChannelType.JUMPINGSUMO_CONTROLLER_TO_DEVICE_ACK_ID.Id, (byte) counter, 12, 0, 0, 0, commandKey.ProjectId, commandKey.ClazzId, commandKey.CommandId, 0, enable, 0};
		}


		new public Acknowledge Acknowledge
		{
			get
			{
    
				return Acknowledge.AckAfter;
			}
		}
	}

}