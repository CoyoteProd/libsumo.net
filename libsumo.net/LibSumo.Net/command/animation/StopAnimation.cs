using LibSumo.Net.Network;
namespace LibSumo.Net.lib.command.animation
{



	/// <summary>
	/// @author  Alexander Bischof
	/// </summary>
	public class StopAnimation : iCommand
	{

		private readonly CommandKey commandKey = CommandKey.commandKey(3, 2, 4);
        private readonly PacketType packetType = PacketType.DATA_WITH_ACK;

		private StopAnimation()
		{
			// use fabric method
		}

		public static StopAnimation stopAnimation()
		{

			return new StopAnimation();
		}


		public byte[] getBytes(int counter)
		{

            return new byte[] {(byte)packetType, ChannelType.JUMPINGSUMO_CONTROLLER_TO_DEVICE_ACK_ID.Id, 
                                (byte) counter, 15, 0, 0, 0, commandKey.ProjectId, commandKey.ClazzId, 
                                commandKey.CommandId, 0, 0, 0, 0, 0};
            
		}
                			
        public PacketType getPacketType()
        {
            return packetType;
        }
    }

}