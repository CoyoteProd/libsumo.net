using System;

namespace LibSumo.Net.lib.listener
{


	/// <summary>
	/// @author  Tobias Schneider
	/// </summary>
	public class PCMDListener : EventListener
	{

		private readonly Action<string> consumer;

		protected internal PCMDListener(Action<string> consumer)
		{

			this.consumer = consumer;
		}

		public static PCMDListener pcmdlistener(Action<string> consumer)
		{

			return new PCMDListener(consumer);
		}


		new public void eventFired(byte[] data)
		{

			if (filterProject(data, 3, 1, 0))
			{
				consumer.Invoke(Convert.ToString(data[11]));
			}
		}
	}

}