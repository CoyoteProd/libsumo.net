namespace LibSumo.Net.lib.command.common
{



	/// <summary>
	/// @author  Alexander Bischof
	/// </summary>
	public class Disconnect : CommonCommand
	{

		private readonly CommandKey commandKey = CommandKey.commandKey(0, 0, 0);

		 public Disconnect()
		{

			// use fabric method
		}

		public static Disconnect disconnect()
		{

			return new Disconnect();
		}


		new public byte[] getBytes(int counter)
		{

			return new byte[] {(byte) FrameType.ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK, ChannelType.JUMPINGSUMO_CONTROLLER_TO_DEVICE_ACK_ID.Id, (byte) counter, 15, 0, 0, 0, commandKey.ProjectId, commandKey.ClazzId, commandKey.CommandId, 0};
		}


		new public Acknowledge Acknowledge
		{
			get
			{
    
				return Acknowledge.AckAfter;
			}
		}


		public override string ToString()
		{

			return "Disconnect";
		}
	}

}