using System;
using System.Text;

namespace LibSumo.Net.lib.command.common
{


	/// <summary>
	/// @author  Alexander Bischof
	/// </summary>
	public class NullTerminatedString
	{

		private readonly string @string;

		public NullTerminatedString(string @string)
		{

			this.@string = @string;
		}

		public byte[] getNullTerminatedString()
		{

			try
			{
				byte[] stringBytes = @string.GetBytes(Encoding.UTF8);
				byte[] ntBytes = new byte[stringBytes.Length + 1];
				Array.Copy(stringBytes, 0, ntBytes, 0, stringBytes.Length);

				return ntBytes;
			}
			catch (Exception e)
			{
				throw(e);
			}
		}
	}

}