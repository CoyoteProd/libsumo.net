namespace LibSumo.Net.lib.command.movement
{



	/// <summary>
	/// @author  Alexander Bischof
	/// </summary>
	public class JumpMotorProblemChanged : Command
	{

		private readonly CommandKey commandKey = CommandKey.commandKey(3, 3, 2);

		JumpMotorProblemChanged()
		{

			// use fabric method
		}

		public static JumpMotorProblemChanged jumpMotorProblemChanged()
		{

			return new JumpMotorProblemChanged();
		}


		new public byte[] getBytes(int counter)
		{

			return new byte[] {(byte) FrameType.ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK, ChannelType.JUMPINGSUMO_CONTROLLER_TO_DEVICE_ACK_ID.Id, (byte) counter, 15, 0, 0, 0, commandKey.ProjectId, commandKey.ClazzId, commandKey.CommandId, 0, 1, 0, 0, 0};
		}


		new public Acknowledge Acknowledge
		{
			get
			{
    
				return Acknowledge.None;
			}
		}
	}

}