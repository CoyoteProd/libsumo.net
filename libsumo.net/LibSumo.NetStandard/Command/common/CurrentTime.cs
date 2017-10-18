using LibSumo.Net.Command.Interfaces;
using LibSumo.Net.Network;
using System;
using System.IO;
namespace LibSumo.Net.lib.command.common
{
	/// <summary>
	/// @author  Alexander Bischof
	/// @author  Tobias Schneider
	/// </summary>
    public class CurrentTime : iCommonCommand
	{
		private static readonly String TIME_FORMATTER = "'T'HHmmssZZZ";
		private readonly CommandKey commandKey = CommandKey.commandKey(0, 4, 1);
        private readonly PacketType packetType = PacketType.DATA_WITH_ACK;
		//private readonly Clock clock;

		protected internal CurrentTime()
		{
			//this.clock = clock;
		}

		public static CurrentTime currentTime()
		{
			return new CurrentTime();
		}

		public byte[] getBytes(int counter)
		{
            byte[] header = new byte[] { (byte)packetType, ChannelType.JUMPINGSUMO_CONTROLLER_TO_DEVICE_ACK_ID.Id, (byte)counter, 15, 0, 0, 0, commandKey.ProjectId, commandKey.ClazzId, commandKey.CommandId, 0 };

			try
			{
                using (MemoryStream outputStream = new MemoryStream())
				{
                    // Stream encodes as UTF-8 by default; specify other encodings in this constructor
                    using (var ps = new StreamWriter(outputStream))
                    {
					    ps.Write(header);
					    ps.Write(new NullTerminatedString(DateTime.Now.ToString(TIME_FORMATTER)).getNullTerminatedString());
                    }
					return outputStream.ToArray();
				}
			}
			catch (Exception e)
			{
				throw new CommandException("Could not generate CurrentTime command.", e);
			}
		}

        public PacketType getPacketType()
        {
            return packetType;
        }

		public override string ToString()
		{
            return "CurrentTime{" + DateTime.Now.ToString(TIME_FORMATTER) + '}';
		}

		public int waitingTime()
		{
			return 150;
		}
	}
}