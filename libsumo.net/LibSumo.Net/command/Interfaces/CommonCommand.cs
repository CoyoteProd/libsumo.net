using System;
namespace LibSumo.Net.lib.command.common
{


	/// <summary>
	/// Interface for basic/common commands to keep the connection to the drone or something else.
	/// 
	/// @author  Tobias Schneider - schneider@synyx.de
	/// </summary>
	public class CommonCommand : Command
	{

        /// <summary>
        /// Returns the byte package of the specific command.
        /// 
        /// <para>TODO: describe the Package format and counter</para>
        /// </summary>
        /// <param name="counter">
        /// </param>
        /// <returns>  byte package of command </returns>
        public new byte[] getBytes(int counter)
        {
            throw (new Exception("be not used"));
        }


        /// <summary>
        /// TODO Describe why this is needed.
        /// 
        /// @return
        /// </summary>
        public new Acknowledge Acknowledge { get; set; }

		/// <summary>
		/// Time to wait to send the next command after this on.
		/// 
		/// <para>Define the time to wait after a command was send to the drone to wait until the next command should be fired.
		/// This will be send at 1/10 Hz</para>
		/// </summary>
		/// <returns>  time to wait until send next command to the drone </returns>

        public new int waitingTime()
		{
	
			return 100;
		}
       
    }

}