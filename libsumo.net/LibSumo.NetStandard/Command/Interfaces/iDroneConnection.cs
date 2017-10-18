namespace LibSumo.Net.lib.network
{
    using LibSumo.Net.lib.command;
    using LibSumo.Net.lib.listener;
    //using Command = LibSumo.Net.lib.command.Command;
    using EventListener = LibSumo.Net.lib.listener.iEventListener;

	/// <summary>
	/// @author  Tobias Schneider
	/// </summary>
	public interface iDroneConnection
	{
		/// <summary>
		/// Connect with the drone with the constructor injected credentials.
		/// </summary>
		/// <exception cref="IOException"> </exception>
		void connect();
        void disconnect();

		/// <summary>
		/// Sends the given <seealso cref="Command"/> to the drone.
		/// </summary>
		/// <param name="command">  to send to drone
		/// </param>
		/// <exception cref="IOException"> </exception>
		void sendCommand(iCommand command);

		/// <summary>
		/// Register the given <seealso cref="EventListener"/> to the <seealso cref="iDroneConnection"/>.
		/// </summary>
		/// <param name="eventListener">  with the capsuled functionality </param>
		void addEventListener(iEventListener eventListener);
	}
}