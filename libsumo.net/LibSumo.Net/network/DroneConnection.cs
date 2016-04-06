namespace LibSumo.Net.lib.network
{

	using Command = LibSumo.Net.lib.command.Command;
	using EventListener = LibSumo.Net.lib.listener.EventListener;


	/// <summary>
	/// @author  Tobias Schneider
	/// </summary>
	public interface DroneConnection
	{

		/// <summary>
		/// Connect with the drone with the constructor injected credentials.
		/// </summary>
		/// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void connect() throws java.io.IOException;
		void connect();


		/// <summary>
		/// Sends the given <seealso cref="Command"/> to the drone.
		/// </summary>
		/// <param name="command">  to send to drone
		/// </param>
		/// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void sendCommand(LibSumo.Net.lib.command.Command command) throws java.io.IOException;
		void sendCommand(Command command);


		/// <summary>
		/// Register the given <seealso cref="EventListener"/> to the <seealso cref="DroneConnection"/>.
		/// </summary>
		/// <param name="eventListener">  with the capsuled functionality </param>
		void addEventListener(EventListener eventListener);
	}

}