namespace LibSumo.Net.lib.command.animation
{



	/// <summary>
	/// @author  Alexander Bischof
	/// @author  Tobias Schneider
	/// </summary>
	public class Metronome : Command
	{

		private readonly CommandKey commandKey = CommandKey.commandKey(3, 2, 4);

		protected internal Metronome()
		{

			// use fabric method
		}

		public static Metronome metronome()
		{

			return new Metronome();
		}


		public new byte[] getBytes(int counter)
		{

			return new byte[] {(byte) FrameType.ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK, ChannelType.JUMPINGSUMO_CONTROLLER_TO_DEVICE_ACK_ID.Id, (byte) counter, 15, 0, 0, 0, commandKey.ProjectId, commandKey.ClazzId, commandKey.CommandId, 0, 4, 0, 0, 0};
		}


        public new Acknowledge Acknowledge
		{
			get
			{
    
				return Acknowledge.AckBefore;
			}
		}


		public override string ToString()
		{

			return "Metronome";
		}


        public new int waitingTime()
		{

			return 3000;
		}
	}

}