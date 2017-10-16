using LibSumo.Net.Network;

namespace LibSumo.Net.lib.command.animation
{
	/// <summary>
	/// @author  Alexander Bischof
	/// @author  Tobias Schneider
	/// </summary>
	public class Ondulation : iCommand
	{
		private readonly CommandKey commandKey = CommandKey.commandKey(3, 2, 4);
        private readonly PacketType packetType = PacketType.DATA_WITH_ACK;

		protected internal Ondulation()
		{
			// use fabric method
		}

		public static Ondulation ondulation()
		{
			return new Ondulation();
		}

		public byte[] getBytes(int counter)
		{
            return new byte[] { (byte)packetType, ChannelType.JUMPINGSUMO_CONTROLLER_TO_DEVICE_ACK_ID.Id, (byte)counter, 15, 0, 0, 0, commandKey.ProjectId, commandKey.ClazzId, commandKey.CommandId, 0, 5, 0, 0, 0 };
		}

        public PacketType getPacketType()
        {
            return packetType;
        }

		public override string ToString()
		{
			return "Ondulation";
		}

        public int waitingTime()
		{
			return 1500;
		}
	}
}