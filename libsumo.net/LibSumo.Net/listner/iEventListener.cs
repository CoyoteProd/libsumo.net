using System;
namespace LibSumo.Net.lib.listener
{

	/// <summary>
	/// @author  Alexander Bischof
	/// @author  Tobias Schneider
	/// </summary>
	public interface iEventListener
	{

        /**
        * Tries to consume the given data packet.
        *
        * @param  data  to consume
        */
        void consume(byte[] data);


        /**
         * @param  data
         *
         * @return  true if the packet could be consumed, false otherwise
         */
        bool test(byte[] data);
	}

}