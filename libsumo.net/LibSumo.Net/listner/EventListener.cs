using System;
namespace LibSumo.Net.lib.listener
{

	/// <summary>
	/// @author  Alexander Bischof
	/// @author  Tobias Schneider
	/// </summary>
	public class EventListener
	{

        public void eventFired(byte[] data)
        {
            throw new Exception("not to be used");
        }
        
		public bool filterChannel(byte[] data, int frameType, int channel)
		{
	
			return data[0] == frameType && data[1] == channel;
		}



		public bool filterProject(byte[] data, int project, int clazz, int cmd)
		{
	
			return data[7] == project && data[8] == clazz && data[9] == cmd;
		}
	}

}