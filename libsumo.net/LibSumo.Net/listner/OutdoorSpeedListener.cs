using System;

namespace LibSumo.Net.lib.listener
{


	/// <summary>
	/// @author  Tobias Schneider
	/// </summary>
	public class OutdoorSpeedListener : EventListener
	{

		private readonly Action<string> consumer;

		protected internal OutdoorSpeedListener(Action<string> consumer)
		{

			this.consumer = consumer;
		}

		public static OutdoorSpeedListener outdoorSpeedListener(Action<string> consumer)
		{

			return new OutdoorSpeedListener(consumer);
		}


		new public void eventFired(byte[] data)
		{

			if (filterProject(data, 3, 17, 0))
			{
				consumer.Invoke(Convert.ToString(data[11]));
			}
		}
	}

}