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
    public class CurrentDate : iCommonCommand
	{

		private readonly CommandKey commandKey = CommandKey.commandKey(0, 4, 0);
        private readonly PacketType packetType = PacketType.DATA_WITH_ACK;

		protected internal CurrentDate()
		{

			//this.clock = clock;
		}

		public static CurrentDate currentDate()
		{

			return new CurrentDate();
		}


		public byte[] getBytes(int sequenceNumber)
		{

            byte[] header = new byte[] { (byte)packetType, ChannelType.JUMPINGSUMO_CONTROLLER_TO_DEVICE_ACK_ID.Id, (byte)sequenceNumber, 15, 0, 0, 0, commandKey.ProjectId, commandKey.ClazzId, commandKey.CommandId, 0 };

			try
			{
                using (var outputStream = new MemoryStream())
                {
                    // Stream encodes as UTF-8 by default; specify other encodings in this constructor
                    using (var ps = new StreamWriter(outputStream))
                    {
                        ps.Write(header);
                        ps.Write((new NullTerminatedString(DateTime.Now.ToString("yyyy-MM-dd"))).getNullTerminatedString());
                    }

                    // Extract bytes from MemoryStream
                    return outputStream.ToArray();
                }					
			}
			catch (Exception e)
			{
				throw new CommandException("Could not generate CurrentDate command.", e);
			}
		}


        public PacketType getPacketType()
        {
            return packetType;
        }


		public override string ToString()
		{

            return "CurrentDate{" + DateTime.Now.ToString("yyyy-MM-dd") + '}';
		}
		
	}

}