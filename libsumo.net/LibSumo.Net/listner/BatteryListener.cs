using System;
namespace LibSumo.Net.lib.listener
{


	/// <summary>
	/// @author  Tobias Schneider
	/// </summary>
	public class BatteryListener : EventListener
	{

		private readonly Action<byte> consumer;

		public BatteryListener(Action<byte> consumer)
		{

			this.consumer = consumer;
		}

		public static BatteryListener batteryListener(Action<byte> consumer)
		{

			return new BatteryListener(consumer);
		}


		new public void eventFired(byte[] data)
		{
			if (filterProject(data, 0, 5, 1))
			{
				consumer.Invoke(data[11]);
			}
		}
	}

}