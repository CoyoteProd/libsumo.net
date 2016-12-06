using LibSumo.Net.listner;
using System;
namespace LibSumo.Net.lib.listener
{


	/// <summary>
	/// @author  Tobias Schneider
	/// </summary>
    public class BatteryListener : CommonEventListener
	{

		private readonly Action<byte> consumer;

		private BatteryListener(Action<byte> consumer)
		{
			this.consumer = consumer;
		}

		public static BatteryListener batteryListener(Action<byte> consumer)
		{
			return new BatteryListener(consumer);
		}


		 
        public new void consume(byte[] data) 
        {
            //LOGGER.debug("consuming battery packet");
            consumer.Invoke(data[11]);
        }

            
        public new bool test(byte[] data) 
        {
            //LOGGER.debug("check for battery packet");
            return filterProject(data, 0, 5, 1);
        }
	}

}