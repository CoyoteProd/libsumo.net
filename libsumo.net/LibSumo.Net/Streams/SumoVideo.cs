using System.Threading;
using System.Threading.Tasks;
using LibSumo.Net.Events;
using LibSumo.Net.Logger;
using LibSumo.Net.Network;
using OpenCvSharp;

namespace LibSumo.Net.Streams
{
    /// <summary>
    /// Displays frames received from the Jumping Sumo
    /// </summary>
    internal class SumoVideo
    {
        #region Private Fields
        private SumoReceiver receiver;
        private bool IsConnected { get; set; }
        private string Window_name { get; set; }
        private VideoWriter writer; 
        #endregion

        public bool ImageInSeparateOpenCVWindow { get; set; }
        private bool _videoCaptureEnabled;
        private Timer VideoTimeout;
        public bool VideoCaptureEnabled
        {
            get
            {
                return _videoCaptureEnabled;
            }
            set
            {
                _videoCaptureEnabled = value;
                if (value == true)
                {
                    Size dsize = new Size(640, 480);
                    writer = new VideoWriter("video.avi", -1, 10, dsize);
                    VideoTimeout = new Timer(new TimerCallback(VideoTimeoutCallback),null,10000,Timeout.Infinite);                   
                }else
                {
                    VideoTimeout.Change(Timeout.Infinite, Timeout.Infinite);
                    VideoTimeout.Dispose();
                    writer.Dispose();
                }
                
            }
        }

        #region Constructor
        public SumoVideo(SumoReceiver _receiver)
        {            
            this.receiver = _receiver;            
            this.Window_name = "Sumo Display";            
        }
        #endregion

        public void RunThread()
        {
            this.IsConnected = true; ;
            Task.Run(() => VideoThread());
        }
        /// <summary>
        /// Stops the main loop and closes the display window
        /// </summary>
        public void Disconnect()
        {
            this.IsConnected = false;
            if (ImageInSeparateOpenCVWindow)
            {
                // TODO : try to resolve hang here
                //Cv2.DestroyWindow(this.window_name);                
            }
        }

        /// <summary>
        /// Video Thread
        /// </summary>
        private void VideoThread()
        {             
            LOGGER.GetInstance.Info("[SumoDisplay] Thread Started");
            
            while (this.IsConnected)
            {
                var frame = this.receiver.Get_video_frame();
                if (frame != null)
                {                    
                    Mat img = Mat.ImDecode(frame, ImreadModes.AnyColor);
                    if(ImageInSeparateOpenCVWindow)
                        Cv2.ImShow(this.Window_name, img);
                    else
                        OnImage(new ImageEventArgs(img));

                    if (VideoCaptureEnabled) writer.Write(img);
                    
                }
                if (ImageInSeparateOpenCVWindow)  Cv2.WaitKey(25);
                else Thread.Sleep(25);
            }
            LOGGER.GetInstance.Info("[SumoDisplay] Thread Stopped");         
        }
        
        private void VideoTimeoutCallback(object state)
        {
            VideoCaptureEnabled = false;
        }

    #region Events Handler
    public delegate void ImageEventHandler(object sender, ImageEventArgs e);
        public event ImageEventHandler ImageAvailable;
        protected virtual void OnImage(ImageEventArgs e)
        {
            ImageAvailable?.Invoke(this, e);
        }
        #endregion

    }

}