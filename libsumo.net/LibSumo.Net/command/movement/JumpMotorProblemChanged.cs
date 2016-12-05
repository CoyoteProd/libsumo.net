using LibSumo.Net.Network;
namespace LibSumo.Net.lib.command.movement
{



	/// <summary>
	/// @author  Alexander Bischof
	/// </summary>
	public class JumpMotorProblemChanged : iCommand
	{

		private readonly CommandKey commandKey = CommandKey.commandKey(3, 3, 2);
        private readonly PacketType packetType = PacketType.DATA_WITH_ACK;

		JumpMotorProblemChanged()
		{

			// use fabric method
		}

		public static JumpMotorProblemChanged jumpMotorProblemChanged()
		{

			return new JumpMotorProblemChanged();
		}


		public byte[] getBytes(int counter)
		{

			return new byte[] {(byte) FrameType.ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK, ChannelType.JUMPINGSUMO_CONTROLLER_TO_DEVICE_ACK_ID.Id, (byte) counter, 15, 0, 0, 0, commandKey.ProjectId, commandKey.ClazzId, commandKey.CommandId, 0, 1, 0, 0, 0};
		}
	                         
        public PacketType getPacketType() 
        {
            return packetType;
        }
    }

}