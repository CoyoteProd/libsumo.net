using System;

namespace LibSumo.Net.lib.command.movement
{



	/// <summary>
	/// @author  Alexander Bischof
	/// @author  Tobias Schneider
	/// </summary>
	public  class Jump : Command
	{

		public enum Type
		{
			Long,
			High
		}

		private readonly CommandKey commandKey = CommandKey.commandKey(3, 2, 3);
		private readonly Type type;

		protected internal Jump(Type type)
		{

			this.type = type;
		}

		public static Jump jump(Type type)
		{

			return new Jump(type);
		}


		new public byte[] getBytes(int counter)
		{

			return new byte[] {(byte) FrameType.ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK, ChannelType.JUMPINGSUMO_CONTROLLER_TO_DEVICE_ACK_ID.Id, (byte) counter, 15, 0, 0, 0, commandKey.ProjectId, commandKey.ClazzId, commandKey.CommandId, 0, (byte) type, 0, 0, 0};
		}


		new public Acknowledge Acknowledge
		{
			get
			{
    
				return Acknowledge.AckBefore;
			}
		}


		public override string ToString()
		{

			return "Jump";
		}


		public new int waitingTime()
		{

			return 5000;
		}
	}

}