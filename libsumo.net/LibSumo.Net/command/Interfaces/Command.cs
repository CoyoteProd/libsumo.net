using System;
namespace LibSumo.Net.lib.command
{

	/// <summary>
	/// This interface represents a command sent to the jumping sumo.
	/// 
	/// @author  Alexander Bischof
	/// @author  Tobias Schneider
	/// </summary>
	public class Command
	{
        /// <summary>
        /// Returns the byte package of the specific command.
        /// 
        /// <para>TODO: describe the Package format and counter</para>
        /// </summary>
        /// <param name="counter">
        /// </param>
        /// <returns>  byte package of command </returns>
        public byte[] getBytes(int counter)
        {
            throw (new Exception("be not used"));
        }


        /// <summary>
        /// TODO Describe why this is needed.
        /// 
        /// @return
        /// </summary>
        public Acknowledge Acknowledge { get; set; }
		/// <summary>
		/// Define the time to wait after a command was send to the drone to wait until the next command should be fired.
		/// </summary>
		/// <returns>  time to wait until send next command to the drone </returns>
        /// 
		public int waitingTime()
		{	
			return 500;
		}
	}

}