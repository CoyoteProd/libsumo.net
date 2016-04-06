namespace LibSumo.Net.lib.command.multimedia
{



	/// <summary>
	/// @author  Alexander Bischof
	/// @author  Tobias Schneider
	/// </summary>
	public  class Volume : Command
	{

		private readonly CommandKey commandKey = CommandKey.commandKey(3, 12, 0);
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private readonly byte volume_Renamed;

		protected internal Volume(int volume)
		{

			if (volume < 0 || volume > 100)
			{
				throw new System.ArgumentException("Audio: Volume must be between 0 and 100.");
			}

			this.volume_Renamed = (byte) volume;
		}

		public static Volume volume(int volume)
		{

			return new Volume(volume);
		}


		new public byte[] getBytes(int counter)
		{

			return new byte[] {(byte) FrameType.ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK, ChannelType.JUMPINGSUMO_CONTROLLER_TO_DEVICE_ACK_ID.Id, (byte) counter, 12, 0, 0, 0, commandKey.ProjectId, commandKey.ClazzId, commandKey.CommandId, 0, volume_Renamed, 0};
		}


		new public Acknowledge Acknowledge
		{
			get
			{
    
				return Acknowledge.AckBefore;
			}
		}


		public override string ToString()
		{

			return "Volume{" + "volume=" + volume_Renamed + '}';
		}
	}

}