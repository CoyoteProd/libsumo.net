namespace LibSumo.Net.lib.command.common
{


	/// <summary>
	/// @author  Alexander Bischof
	/// </summary>
	public class Pong : CommonCommand
	{

		private readonly int counter;

		public Pong(int counter)
		{

			this.counter = counter;
		}

		public static Pong pong(int counter)
		{

			return new Pong(counter);
		}


		new public byte[] getBytes(int counter)
		{
			return new byte[] {1, unchecked((byte) 0xfe), (byte) this.counter, 8, 0, 0, 0, (byte) this.counter};
		}


		new public Acknowledge Acknowledge
		{
			get
			{
    
				return Acknowledge.None;
			}
		}


		public override string ToString()
		{

			return "Pong";
		}


		public new int waitingTime()
		{

			return 50;
		}
	}

}