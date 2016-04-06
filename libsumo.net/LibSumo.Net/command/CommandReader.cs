using System;
namespace LibSumo.Net.lib.command
{

	/// <summary>
	/// @author  Alexander Bischof
	/// </summary>
	public class CommandReader
	{

		private byte[] data;

		protected internal CommandReader(byte[] _data)
		{
            data = (byte[])_data.Clone();			
		}

		public static CommandReader commandReader(byte[] _data)
		{

			return new CommandReader(_data);
		}


		public virtual bool Ping
		{
			get
			{
    
				return data[0] == 2 && data[1] == 0;
			}
		}


		public virtual bool LinkQualityChanged
		{
			get
			{
    
				return isProjectClazzCommand(3, 11, 4);
			}
		}


		public virtual bool WifiSignalChanged
		{
			get
			{
    
				return isProjectClazzCommand(0, 5, 7);
			}
		}


		private bool isProjectClazzCommand(int project, int clazz, int command)
		{

			return data[7] == project && data[8] == clazz && data[9] == command;
		}
	}

}