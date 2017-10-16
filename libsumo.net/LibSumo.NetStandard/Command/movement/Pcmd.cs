using LibSumo.Net.Network;
using System;
namespace LibSumo.Net.lib.command.movement
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static LibSumo.Net.lib.command.ChannelType.JUMPINGSUMO_CONTROLLER_TO_DEVICE_NONACK_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static LibSumo.Net.lib.command.FrameType.ARNETWORKAL_FRAME_TYPE_DATA;

	/// <summary>
	/// Parrot command.
	/// 
	/// <para>Responsible for the movements of the drone.</para>
	/// 
	/// @author  Alexander Bischof
	/// @author  Tobias Schneider
	/// </summary>
	public class Pcmd : iCommand
	{
		private readonly CommandKey commandKey = CommandKey.commandKey(3, 0, 0);
        private sbyte speed { get; set; }
        private sbyte turn { get; set; }
        //private sbyte degrees { get; set; }
     
		private readonly int? waitingTime_Renamed;
        private PacketType packetType = PacketType.DATA; //FrameType.ARNETWORKAL_FRAME_TYPE_DATA;

		protected internal Pcmd(int speed, int degrees, int? waitingTime)
		{
			if (waitingTime != null && waitingTime < 0)
			{
                throw new ArgumentException(String.Format("Waiting time must be greater or equal zero but is {0}", waitingTime));
			}

            if (speed < -128 || speed > 127)
			{
				throw new ArgumentException(String.Format("Movement: Speed must be between -128 and 127 but is %s", speed));
			}

			this.speed = (sbyte) speed;
			this.turn = (sbyte) degreeToPercent(degrees);
			this.waitingTime_Renamed = waitingTime;
		}

		/// <summary>
		/// This is one of the main methods to move a parrot drone.
		/// </summary>
		/// <param name="speed">  how fast the electronic engine of the drone should spin </param>
		/// <param name="degrees">  how much the drone will turn around his own axe in degree (°)
		/// </param>
		/// <returns>  an immutable command with the given inputs </returns>
		public static Pcmd pcmd(int speed, int degrees)
		{
			return new Pcmd(speed, degrees, null);
		}

		/// <summary>
		/// This is one of the main methods to move a parrot drone.
		/// </summary>
		/// <param name="speed">  how fast the electronic engine of the drone should spin </param>
		/// <param name="degrees">  how much the drone will turn around his own axe in degree (°) </param>
		/// <param name="waitingTime">  set the waiting time to send the next command
		/// </param>
		/// <returns>  an immutable command with the given inputs </returns>
		public static Pcmd pcmd(int speed, int degrees, int waitingTime)
		{
			return new Pcmd(speed, degrees, waitingTime);
		}

		/// <summary>
		/// Converts the degrees to percent on a circle.
		/// 
		/// <para>R/360 = P/100 -> P = R*100/360</para>
		/// </summary>
		/// <param name="degrees">  to convert in percents of a circle
		/// </param>
		/// <returns>  percents from given degrees </returns>
		private int degreeToPercent(int degrees)
		{
			return degrees * 100 / 360;
		}

		public byte[] getBytes(int counter)
		{
			byte touchscreen = 1;

            return new byte[] { (byte)packetType, ChannelType.JUMPINGSUMO_CONTROLLER_TO_DEVICE_NONACK_ID.Id, (byte)counter, 
                                14, 0, 0, 0, commandKey.ProjectId, commandKey.ClazzId, commandKey.CommandId, 0, 
                                touchscreen, (byte)speed, (byte)turn };
		}
        
        public PacketType getPacketType()
        {
            return packetType;
        }
        	
		public override string ToString()
		{
			return "Pcmd{" + "speed=" + speed + ", turn=" + turn + '}';
		}

		public int waitingTime()
		{
			if (waitingTime_Renamed == null)
			{
				return this.waitingTime();
			}

			return this.waitingTime_Renamed.Value;
		}

        public iCommand clone(int waitingTime)
        {
            return new Pcmd(speed, turn, waitingTime);
        }
	}
}