using System;
using System.IO;

namespace LibSumo.Net.lib.listener
{



	/// <summary>
	/// @author  Tobias Schneider
	/// </summary>
	public class VideoListener : iEventListener
	{

		private readonly System.IO.FileStream fileOutputStream;
		private int pictureCounter = 0;
		private byte[] temp = new byte[] {};

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public VideoListener() throws java.io.FileNotFoundException
		public VideoListener()
		{

			fileOutputStream = new System.IO.FileStream("video.mp4", System.IO.FileMode.Create, System.IO.FileAccess.Write);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static VideoListener videoListener() throws java.io.FileNotFoundException
		public static VideoListener videoListener()
		{

			return new VideoListener();
		}


		new public virtual void eventFired(byte[] data)
		{

			if (data[1] == 125)
			{
				concatenateByteArrays(temp, getJpegDate(data));

				if (pictureCounter == 15)
				{
					try
					{
						fileOutputStream.Write(temp, 0, temp.Length);
					}
					catch (IOException e)
					{
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
					}
				}

				pictureCounter++;
			}
		}


		private byte[] getJpegDate(byte[] data)
		{

			byte[] jpegData = new byte[data.Length];
			Array.Copy(data, 12, jpegData, 0, data.Length - 12);

			return jpegData;
		}


		private byte[] concatenateByteArrays(byte[] a, byte[] b)
		{

			byte[] result = new byte[a.Length + b.Length];
			Array.Copy(a, 0, result, 0, a.Length);
			Array.Copy(b, 0, result, a.Length, b.Length);

			Console.WriteLine("a " + a.ToString());
			Console.WriteLine("b " + b.ToString());
			Console.WriteLine("result " + result.ToString());

			return result;
		}
	}

}