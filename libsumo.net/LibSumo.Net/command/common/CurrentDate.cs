using System;
using System.IO;
namespace LibSumo.Net.lib.command.common
{

	/// <summary>
	/// @author  Alexander Bischof
	/// @author  Tobias Schneider
	/// </summary>
	public  class CurrentDate : CommonCommand
	{

		private readonly CommandKey commandKey = CommandKey.commandKey(0, 4, 0);
		//private readonly Clock clock;

		protected internal CurrentDate()
		{

			//this.clock = clock;
		}

		public static CurrentDate currentDate()
		{

			return new CurrentDate();
		}


		new public byte[] getBytes(int counter)
		{

			byte[] header = new byte[] {(byte) FrameType.ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK, ChannelType.JUMPINGSUMO_CONTROLLER_TO_DEVICE_ACK_ID.Id, (byte) counter, 15, 0, 0, 0, commandKey.ProjectId, commandKey.ClazzId, commandKey.CommandId, 0};

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


		new public Acknowledge Acknowledge
		{
			get
			{
    
				return Acknowledge.AckAfter;
			}
		}


		public override string ToString()
		{

            return "CurrentDate{" + DateTime.Now.ToString("yyyy-MM-dd") + '}';
		}


		public new int waitingTime()
		{

			return 150;
		}
	}

}