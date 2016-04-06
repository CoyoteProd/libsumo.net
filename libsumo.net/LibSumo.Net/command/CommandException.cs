using System;

namespace LibSumo.Net.lib.command
{

	/// <summary>
	/// Exception that will be thrown in something goes wrong in <seealso cref="Command"/>s.
	/// 
	/// @author  Tobias Schneider
	/// </summary>
	public class CommandException : Exception
	{

		public CommandException(string message, Exception cause) : base(message, cause)
		{

		}
	}

}