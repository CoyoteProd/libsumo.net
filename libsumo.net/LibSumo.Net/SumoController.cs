using LibSumo.Net;
using LibSumo.Net.Events;
using LibSumo.Net.Logger;
using LibSumo.Net.Network;
using LibSumo.Net.Protocol;
using LibSumo.Net.Video;
using Newtonsoft.Json.Linq;
using System;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LibSumo.Net
{
    /// <summary>
    /// Main Controller of Jumping Sumo
    /// </summary>
    public class SumoController :IDisposable
    {
        #region Private Fields
        private string deviceIp = "192.168.2.1";
        private int discoveryPort = 44444;
        private int d2c_port = 43210;

        private int c2d_port;
        private int fragment_size;
        private int fragments_per_frame;

        private SumoReceiver receiver;
        private SumoDisplay display;
        private SumoSender sender;
        private SumoKeyboardPiloting piloting;
        
        #endregion

        #region properties
        /// <summary>
        /// If you want process video in OpenCV separate window Set EnableOpenCV to true
        /// In this case controller.ImageAvailable is not fired
        /// </summary>
        public bool EnableOpenCV { private get; set; }
        public bool IsConnected { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Use this constructor if you want be ready to pilot your Sumo
        /// </summary>
        /// <param name="_piloting">Keybord class to controle Sumo</param>
        /// <param name="DeviceIP"></param>
        public SumoController(out SumoKeyboardPiloting _piloting, string DeviceIP = "")
        {
            // For future use
            if (!String.IsNullOrEmpty(DeviceIP)) deviceIp = DeviceIP;

            LOGGER.GetInstance.Info(String.Format("Starting Controller..."));
            _piloting = new SumoKeyboardPiloting();
            piloting = _piloting;
            EnableOpenCV = false;

            // Start Discover
            StartDiscoverThread();
        }

        /// <summary>
        /// If you want to have your proper piloting system
        /// </summary>
        /// <param name="DeviceIP"></param>
        public SumoController(string DeviceIP = "")
        {
            // For future use
            if (!String.IsNullOrEmpty(DeviceIP)) deviceIp = DeviceIP;
            LOGGER.GetInstance.Info(String.Format("Starting Controller..."));
            EnableOpenCV = false;

            // Start Discover
            StartDiscoverThread();
        }
        #endregion
        
        #region Private Methods
        /// <summary>
        /// Initiates discovery with the jumping sumo (via TCP)
        /// </summary>
        /// <returns></returns>
        private bool _discovery()
        {

            LOGGER.GetInstance.Info("Connecting to Jumping sumo...");
            //string strInitMsg = @"{ ""controller_type"":""computer"", ""controller_name"":""a5xelte"", ""d2c_port"":""" + d2c_port + @"""}";
            string strInitMsg = @"{ ""controller_type"":""a5xelte"", ""controller_name"":""SM-A510F"", ""d2c_port"":"+ d2c_port + @",""audio_codec"":3,""arstream2_client_stream_port"":55004,""arstream2_client_control_port"":55005}";
            try
            {
                TcpClient tcpSocket = new TcpClient();
                tcpSocket.Connect(deviceIp, discoveryPort);
                NetworkStream nwStream = tcpSocket.GetStream();

                //Send to device
                byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(strInitMsg);
                nwStream.Write(bytesToSend, 0, bytesToSend.Length);

                //Reads json response
                byte[] bytesToRead = new byte[tcpSocket.ReceiveBufferSize];
                int bytesRead = nwStream.Read(bytesToRead, 0, tcpSocket.ReceiveBufferSize);
                tcpSocket.Close();

                String responseLine = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead).Replace("\0", string.Empty);
                LOGGER.GetInstance.Info(String.Format("Connected to drone - Handshake completed with {0}", responseLine));

                // Read the JSON            
                JObject config_data = JObject.Parse(responseLine); // it will parse deserialize Json object

                // Store config data but not used
                this.c2d_port = int.Parse(config_data["c2d_port"].ToString());
                this.fragment_size = int.Parse(config_data["arstream_fragment_size"].ToString());
                this.fragments_per_frame = int.Parse(config_data["arstream_fragment_maximum_number"].ToString());
                return true;
            }
            catch (Exception e)
            { // Failed to Connect
                LOGGER.GetInstance.Info("Failed to connect to drone... ");
                LOGGER.GetInstance.Info(e.Message);
            }
            return false;

        }

        /// <summary>
        /// Simple network discovery
        /// TODO : To Improve
        /// </summary>
        private void StartDiscoverThread()
        {
            Task.Run(() =>
            {
                bool Ping_Should_run = true;
                while (Ping_Should_run)
                {
                    Ping pinger = new Ping();
                    try
                    {
                        PingReply reply = pinger.Send(deviceIp, 100);
                        if (reply.Status == IPStatus.Success)
                        {                            
                            OnSumoEvents(new SumoEventArgs( SumoEnum.TypeOfEvents.Discovered));
                            Ping_Should_run = false;
                        }
                    }
                    catch (PingException)
                    {
                        // Discard PingExceptions
                    }
                }
            });
        }

        /// <summary>
        /// A new frame is available
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Display_ImageAvailable(object sender, ImageEventArgs e)
        {
            OnImage(e);
        }

        #endregion

        #region Connect/Disconnect methods
        /// <summary>
        /// Establish a connection to the drone and start receiving and sending data
        /// </summary>
        /// <returns></returns>
        public bool Connect()
        {
            Task<bool> disc = Task<bool>.Factory.StartNew(() => _discovery());
            disc.Wait();
            IsConnected = disc.Result; // await Task.Run(() => _discovery());            
            if (IsConnected)
            {
                this.sender = new SumoSender(this.deviceIp, this.c2d_port);
                this.receiver = new SumoReceiver(this.deviceIp, this.d2c_port, this.sender);
                //this.receiver.BatteryLevelAvailable += Receiver_BatteryLevelAvailable;
                this.receiver.SumoEvents += Receiver_SumoEvents;
                // Set Video
                this.display = new SumoDisplay(this.receiver);
                display.ImageInSeparateOpenCVWindow = EnableOpenCV;
                this.display.ImageAvailable += Display_ImageAvailable;

                // Run Threads
                this.receiver.Run();
                this.sender.Run();
                this.sender.Init();
                this.display.Run();
                if (this.piloting != null)
                {
                    this.piloting.InstallHook();
                    this.piloting.RunPilotingThread();
                    this.piloting.RunKeyboardThread();
                }                

                EnableVideo();  

                // Inform UI that is connected                
                OnSumoEvents(new SumoEventArgs(SumoEnum.TypeOfEvents.Connected));
            }
            return IsConnected;
        }

        private void Receiver_SumoEvents(object sender, SumoEventArgs e)
        {
            OnSumoEvents(e);
        }
        

        /// <summary>
        /// Stops sending, receiving and display threads and closes associated resources
        /// </summary>
        public void Disconnect()
        {
            if (IsConnected)
            {
                if (this.display != null)
                {
                    this.display.Disconnect();
                }
                if (this.sender != null)
                {
                    this.sender.Disconnect();
                }
                if (this.receiver != null)
                {
                    this.receiver.Disconnect();
                }
            }
        }

       

        #endregion

        #region SumoControl methods
        /// <summary>
        ///  Apply the given speed and turn to the Jumping Sumo
        ///         :param speed: [-100, 100]
        ///         :param turn:  [-100, 100]
        ///         :return:
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="turn"></param>
        public void Move(sbyte speed, sbyte turn)
        {
            this.sender.Send(Commands.Move_cmd(speed, turn));
        }

        /// <summary>
        /// a pre-programmed movement or acrobatics
        /// </summary>
        /// <param name="Animation"></param>
        public void Animation(SumoEnum.Animation Animation)
        {
            this.sender.Send(Commands.Animation_cmd(Animation));
        }

        /// <summary>
        /// Set Sumo in Postures
        /// standing, jumper, kicker
        /// </summary>
        /// <param name="Posture">enum[standing, jumper, kicker]</param>
        public void Postures(SumoEnum.Posture Posture)
        {
            this.sender.Send(Commands.Postures_cmd(Posture));
        }

        /// <summary>
        /// Start a Jump Action
        /// </summary>
        /// <param name="JumpType">enum[long, high]</param>
        public void Jump(SumoEnum.Jump JumpType)
        {
            this.sender.Send(Commands.Jump_cmd(JumpType));
        }

        /// <summary>
        /// Quick Turn
        /// </summary>
        /// <param name="Angle">in radian</param>
        /// <returns></returns>
        public void QuickTurn(float Angle)
        {            
            this.sender.Send(Commands.Turn_cmd(Angle));
        }

        public void Volume(byte Volume)
        {         
            this.sender.Send(Commands.Volume_cmd(Volume));
        }
        public void SetAuDioThemeVolume(SumoEnum.AudioTheme AudioTheme)
        {            
            this.sender.Send(Commands.AudioTheme_cmd(AudioTheme));
        }

        public void JumpLoad()
        {            
            this.sender.Send(Commands.JumpLoading_cmd());
        }

        public void CancelJump()
        {            
            this.sender.Send(Commands.CancelJump_cmd());
        }

        public void STOP()
        {
            this.sender.Send(Commands.STOP_cmd());
        }

        public void Headlight_on()
        {
            this.sender.Send(Commands.Set_Headlight(255,255));
        }
        public void Headlight_off()
        {
            this.sender.Send(Commands.Set_Headlight(0, 0));
        }
       
        #endregion
       
        internal void EnableVideo()
        {
            this.sender.Send(Commands.Set_media_streaming_cmd(true));            
        }
        internal void DisableVideo()
        {
            this.sender.Send(Commands.Set_media_streaming_cmd(false));
        }


        #region Event Handler


        public delegate void ImageEventHandler(object sender, ImageEventArgs e);
        public event ImageEventHandler ImageAvailable;
        protected virtual void OnImage(ImageEventArgs e)
        {
            ImageAvailable?.Invoke(this, e);
        }       

        public delegate void SumoEventHandler(object sender, SumoEventArgs e);
        public event SumoEventHandler SumoEvents;
        protected virtual void OnSumoEvents(SumoEventArgs e)
        {
            SumoEvents?.Invoke(this, e);
        }


        #endregion

        public void Dispose()
        {
            if (piloting != null)
                piloting.UnInstallHook();
        }

    }
}