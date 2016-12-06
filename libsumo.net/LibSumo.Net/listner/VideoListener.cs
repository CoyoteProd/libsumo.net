using LibSumo.Net.Math;
using LibSumo.Net.Network;
using System;
using System.IO;

namespace LibSumo.Net.lib.listener
{



	/// <summary>
	/// @author  Tobias Schneider
	/// </summary>
	public class VideoListener : iEventListener
	{

		private static string FRAME_JPG = "frame.jpg";
        private static byte[] lastJpeg = null;
        private bool writeToDisk=true;
        //private float frameRate = 0;
        long lastFrame = MovingAverage.CurrentTimeMillis() - 20;
        MovingAverage average = new MovingAverage(20);

		private VideoListener()
		{

			//fileOutputStream = new System.IO.FileStream("video.mp4", System.IO.FileMode.Create, System.IO.FileAccess.Write);
		}

		public static VideoListener videoListener()
		{

			return new VideoListener();
		}

        public void consume(byte[] data) 
        {
            //MathContext mc = new MathContext(2, RoundingMode.HALF_UP);
            average.add( (MovingAverage.CurrentTimeMillis() - lastFrame)/1000);

            //LOGGER.debug("consuming video packet at a framerate of {}", new BigDecimal(1).divide(average.getAverage(),mc));
            byte[] jpeg = getJpeg(data);
            if (writeToDisk) 
            {                
                using (FileStream fos = new FileStream(FRAME_JPG, FileMode.Create)) 
                {
                    //LOGGER.debug("writing video jpg to " + file.getAbsolutePath());    
                    fos.Write(jpeg, 0, jpeg.Length);
                }                
            }
            lastFrame = MovingAverage.CurrentTimeMillis();
        }
              

        private byte[] getJpeg(byte[] data)
		{

			byte[] jpegData = new byte[data.Length];
			Array.Copy(data, 12, jpegData, 0, data.Length - 12);

			return jpegData;
		}

        public byte[] getLastJpeg()
        {
            return lastJpeg;
        }

   
        public void setWriteToDisk(bool enable)
        {
            this.writeToDisk = enable;
        }

        public bool test(byte[] data)
        {

            bool jpgStart = ((int)data[12] == -1) && ((int)data[13] == -40);

            return data[0] == (byte)PacketType.DATA_LOW_LATENCY && data[1] == 125 && jpgStart;
        }

	}

}