namespace LibSumo.Net.lib.command.multimedia
{



	/// <summary>
	/// Audio theme command.
	/// 
	/// <para>Responsible for the selection of the audio theme</para>
	/// 
	/// @author  Alexander Bischof
	/// @author  Tobias Schneider
	/// </summary>
	public  class AudioTheme : Command
	{

		public enum Theme
		{

			Default,
			Robot,
			Insect,
			Monster
		}

		private readonly CommandKey commandKey = CommandKey.commandKey(3, 12, 1);
		private readonly Theme theme;

		protected internal AudioTheme(AudioTheme.Theme theme)
		{

			this.theme = theme;
		}

		public static AudioTheme audioTheme(AudioTheme.Theme theme)
		{

			return new AudioTheme(theme);
		}


		new public  byte[] getBytes(int counter)
		{

			return new byte[] {(byte) FrameType.ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK, ChannelType.JUMPINGSUMO_CONTROLLER_TO_DEVICE_ACK_ID.Id, (byte) counter, 15, 0, 0, 0, commandKey.ProjectId, commandKey.ClazzId, commandKey.CommandId, 0, (byte) theme, 0, 0, 0};
		}


		new public Acknowledge Acknowledge
		{
			get
			{
    
				return Acknowledge.AckBefore;
			}
		}


		public override string ToString()
		{

			return "AudioTheme{" + "theme=" + theme + '}';
		}
	}

}