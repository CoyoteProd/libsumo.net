using LibSumo.Net.Network;
using System;
namespace LibSumo.Net.lib.command
{

	/// <summary>
	/// This interface represents a command sent to the jumping sumo.
	/// 
	/// @author  Alexander Bischof
	/// @author  Tobias Schneider
	/// </summary>
	public interface iCommand
	{
        /// <summary>
        /// Returns the byte package of the specific command.
        /// 
        /// <para>TODO: describe the Package format and counter</para>
        /// </summary>
        /// <param name="counter">
        /// </param>
        /// <returns>  byte package of command </returns>
        byte[] getBytes(int sequenceNumber);
        
        PacketType getPacketType();

        
		/// <summary>
		/// Define the time to wait after a command was send to the drone to wait until the next command should be fired.
		/// </summary>
		/// <returns>  time to wait until send next command to the drone </returns>
        /// 
        //int waitingTime();
		//{	
//			return 500;
		//}
	}


    public static class CommandExtensions
    {
        public static int waitingTime(this iCommand cmd)
        {
            if (cmd.waitingTime() == 0) return 100;
            else return cmd.waitingTime();
        }
    }

}