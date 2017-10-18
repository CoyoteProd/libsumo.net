using LibSumo.Net.listner;
using System;
namespace LibSumo.Net.lib.listener
{
	/// <summary>
	/// @author  Tobias Schneider
	/// </summary>
	public class CriticalBatteryListener : CommonEventListener
	{
		private readonly Action<BatteryState> consumer;

        public CriticalBatteryListener(Action<BatteryState> consumer)
		{
			this.consumer = consumer;
		}

        public static CriticalBatteryListener criticalBatteryListener(Action<BatteryState> consumer)
		{
			return new CriticalBatteryListener(consumer);
		}

		new public void eventFired(byte[] data)
		{
			if (filterProject(data, 3, 1, 1))
			{
                BatteryState bat;
                Enum.TryParse(Convert.ToString(data[11]), out bat);
                consumer.Invoke(bat);
                //consumer.Invoke(Enum.GetValues(typeof(BatteryState))[data[11]]);
            }
        }
	}
}