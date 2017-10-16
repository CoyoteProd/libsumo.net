using LibSumo.Net.listner;
using System;

namespace LibSumo.Net.lib.listener
{
	/// <summary>
	/// @author  Tobias Schneider
	/// </summary>
    public class PCMDListener : CommonEventListener
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
		 
        public new void consume(byte[] data)
        {
            consumer.Invoke(System.Text.Encoding.UTF8.GetString(new byte[] {data[11]} ));
        }

        public new  bool test(byte[] data)
        {
            //LOGGER.debug("check for PCMD  packet");
            return filterProject(data, 3, 1, 0);
        }
	}
}