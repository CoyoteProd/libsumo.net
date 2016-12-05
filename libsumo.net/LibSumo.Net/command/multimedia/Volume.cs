using LibSumo.Net.Network;
namespace LibSumo.Net.lib.command.multimedia
{



	/// <summary>
	/// @author  Alexander Bischof
	/// @author  Tobias Schneider
	/// </summary>
	public  class Volume : iCommand
	{

		private readonly CommandKey commandKey = CommandKey.commandKey(3, 12, 0);
        private readonly PacketType packetType = PacketType.DATA_WITH_ACK;

		private readonly byte internal_volume;

		private Volume(int volume)
		{

			if (volume < 0 || volume > 100)
			{
				throw new System.ArgumentException("Audio: Volume must be between 0 and 100.");
			}

			this.internal_volume = (byte) volume;
		}

		public static Volume volume(int volume)
		{

			return new Volume(volume);
		}


		public byte[] getBytes(int counter)
		{

            return new byte[] { (byte)packetType, ChannelType.JUMPINGSUMO_CONTROLLER_TO_DEVICE_ACK_ID.Id, (byte)counter, 12, 0, 0, 0, commandKey.ProjectId, commandKey.ClazzId, commandKey.CommandId, 0, internal_volume, 0 };
		}


        public PacketType getPacketType()
        {
            return packetType;
        }


		public override string ToString()
		{

			return "Volume{" + "volume=" + internal_volume + '}';
		}
	}

}