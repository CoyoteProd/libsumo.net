using LibSumo.Net;
using LibSumo.Net.Events;
using LibSumo.Net.Logger;
using LibSumo.Net.Network;
using LibSumo.Net.Protocol;
using LibSumo.Net.Streams;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
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
        private SumoVideo video;
        private SumoAudioPlayer audioPlayer;
        private SumoAudioRecorder audioRecorder;
        private SumoSender sender;
        private SumoKeyboardPiloting piloting;

        private bool Ping_Should_run = true;
        private bool AlwaysPing;
        private static SumoInformations sumoInformations;
        #endregion

        #region properties
        /// <summary>
        /// If you want process video in OpenCV separate window Set EnableOpenCV to true
        /// In this case controller.ImageAvailable is not fired
        /// </summary>
        public bool EnableOpenCV { private get; set; }
        public bool IsConnected { get; set; }        
        private bool CurrentDroneRX { get; set; }
        private bool CurrentDroneTX { get; set; }

        #endregion

        #region Constructor
        /// <summary>
        /// Use this constructor if you want be ready to pilot your Sumo
        /// </summary>
        /// <param name="_piloting">Keybord class to controle Sumo</param>
        /// <param name="DevicesList">List of known devices to discovert</param>
        /// <param name="_AlwaysPing">Continuous ping after discover one drone</param>
        public SumoController(out SumoKeyboardPiloting _piloting, List<string> DevicesList = null, bool _AlwaysPing = false) : this (DevicesList, _AlwaysPing)
        {                        
            _piloting = new SumoKeyboardPiloting();
            piloting = _piloting;                                    
        }

        /// <summary>
        /// If you want to have your proper piloting system
        /// </summary>
        /// <param name="DeviceIP"></param>
        public SumoController(List<string> DevicesList = null, bool _AlwaysPing = false)
        {
            // Process List of devices
            AlwaysPing = _AlwaysPing;
            if (DevicesList != null)
            {   // Start Discover thread for each device
                foreach (string device in DevicesList)
                    StartPingThread(device);
            }
            else
            {
                // No device list so try to detect default device
                StartPingThread(deviceIp);
            }

            LOGGER.GetInstance.Info(String.Format("Starting Controller"));
            EnableOpenCV = false;
            sumoInformations = new SumoInformations();
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
            string strInitMsg = @"{ ""controller_type"":""computer"", ""controller_name"":""vilcoyote"", ""d2c_port"":""" + d2c_port + @"""}";
            //string strInitMsg = @"{ ""controller_type"":""a5xelte"", ""controller_name"":""SM-A510F"", ""d2c_port"":"+ d2c_port + @",""audio_codec"":3,""arstream2_client_stream_port"":55004,""arstream2_client_control_port"":55005}";
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
        private void StartPingThread(string _deviceIp)
        {
            Task.Run(() =>
            {
                
                while (Ping_Should_run)
                {
                    Ping pinger = new Ping();
                    try
                    {
                        PingReply reply = pinger.Send(_deviceIp, 100);
                        if (reply.Status == IPStatus.Success)
                        {
                            sumoInformations.DeviceName = _deviceIp;
                            SumoEventArgs evtArgs = new SumoEventArgs(SumoEnumCustom.TypeOfEvents.Discovered, null)
                            {
                                SumoInformations = sumoInformations
                            };
                            OnSumoEvents(evtArgs);
                            if(!AlwaysPing) return;
                        }
                    }
                    catch (PingException)
                    {
                        // Discard PingExceptions
                    }
                    Thread.Sleep(1000);
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
        public bool Connect(string name)
        {
            if (IPAddress.TryParse(name, out IPAddress ip))
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    this.deviceIp = name;
                    return Connect();
                }
            }
            else
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(name);
                string resolvedIP = hostEntry.AddressList[0].ToString();
                this.deviceIp = resolvedIP;
                return Connect();
            }
            return false;            
        }

        /// <summary>
        /// Establish a connection to the drone and start receiving and sending data
        /// </summary>
        /// <returns></returns>
        public bool Connect()
        {
            if (IsConnected) Disconnect();
            Ping_Should_run = false;
            Task<bool> disc = Task<bool>.Factory.StartNew(() => _discovery());
            disc.Wait();
            IsConnected = disc.Result;
            if (IsConnected)
            {
                this.sender = new SumoSender(this.deviceIp, this.c2d_port);
                this.receiver = new SumoReceiver(this.deviceIp, this.d2c_port, this.sender, ref sumoInformations);
                this.receiver.SumoEvents += Receiver_SumoEvents;

                // Set Video
                this.video = new SumoVideo(this.receiver);
                video.ImageInSeparateOpenCVWindow = EnableOpenCV;
                this.video.ImageAvailable += Display_ImageAvailable;
                
                // Run Threads
                this.receiver.RunThread();
                this.sender.RunThread();
                this.sender.Init();
                this.video.RunThread();
                if (this.piloting != null)
                {
                    this.piloting.InstallHook();
                    this.piloting.RunPilotingThread();
                    this.piloting.RunKeyboardThread();
                }

                EnableVideo();

                // Inform UI that is connected                
                OnSumoEvents(new SumoEventArgs(SumoEnumCustom.TypeOfEvents.Connected, sumoInformations));
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
                if (this.video != null)
                {
                    this.video.Disconnect();
                }
                if (this.sender != null)
                {
                    this.sender.Disconnect();
                }
                if (this.receiver != null)
                {
                    this.receiver.Disconnect();
                }
                if (this.audioPlayer != null)
                    this.audioPlayer.Disconnect();
            }
        }



        #endregion

        #region Audio
        /// <summary>
        /// Init Drone Audio
        /// </summary>
        public void InitAudio()
        {
            // Set Audio
            this.audioPlayer = new SumoAudioPlayer();
            this.receiver.EnableRXAudioProcessing(this.audioPlayer);
            this.audioRecorder = new SumoAudioRecorder(this.sender);
            // Set StreamDirection
            SetAudioDroneRX(true);
        }

        public void StreamAudioToDrone(string item)
        {
            this.audioRecorder.Start(item);
        }            

        /// <summary>
        /// Set Audio Streram Direction if drone is capable
        /// </summary>
        /// <param name="v"></param>
        /// <returns>true if operation is possible</returns>
        public bool SetAudioDroneTX(bool v)
        {
            CurrentDroneTX = v;
            return SetAudioStreamDirection(CurrentDroneRX, CurrentDroneTX);
        }

        /// <summary>
        /// Set Audio Streram Direction if drone is capable
        /// </summary>
        /// <param name="v"></param>
        /// <returns>true if operation is possible</returns>
        public bool SetAudioDroneRX(bool v)
        {
            CurrentDroneRX = v;
            return SetAudioStreamDirection(CurrentDroneRX, CurrentDroneTX);
        }
        
        /// <summary>
        /// Private Set Audio Streram Direction if drone is capable
        /// </summary>
        /// <param name="DroneTX">Drone can send recorded audio</param>
        /// <param name="DroneRX">Drone can Speak Audio</param>
        private bool SetAudioStreamDirection(bool DroneRX, bool DroneTX)
        {
            byte data = Encodebool(new bool[] { false, false, false, false, false, false, DroneRX, DroneTX });
            this.sender.Send(SumoCommandsGenerated.Audio_ControllerReadyForStreaming_cmd(data));
            return true;
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
        public void SendMove(sbyte speed, sbyte turn)
        {
            this.sender.Send(SumoCommandsCustom.Move_cmd(speed, turn));
        }

        /// <summary>
        /// a pre-programmed movement or acrobatics
        /// </summary>
        /// <param name="Animation"></param>
        public void StartAnimation(SumoEnumGenerated.SimpleAnimation_id Animation)
        {
            this.sender.Send(SumoCommandsGenerated.Animations_SimpleAnimation_cmd(Animation)); // Commands.Animation_cmd(Animation));
        }

        /// <summary>
        /// Set Sumo in Postures
        /// standing, jumper, kicker
        /// </summary>
        /// <param name="Posture">enum[standing, jumper, kicker]</param>
        public void ChangePostures(SumoEnumGenerated.Posture_type Posture)
        {
            this.sender.Send(SumoCommandsGenerated.Piloting_Posture_cmd(Posture)); // Commands.Postures_cmd(Posture));
        }

        /// <summary>
        /// Start a Jump Action
        /// </summary>
        /// <param name="JumpType">enum[long, high]</param>
        public void StartJump(SumoEnumGenerated.Jump_type JumpType)
        {
            this.sender.Send(SumoCommandsGenerated.Animations_Jump_cmd(JumpType)); // Commands.Jump_cmd(JumpType));
        }

        /// <summary>
        /// Quick Turn
        /// </summary>
        /// <param name="Angle">in radian</param>
        /// <returns></returns>
        public void QuickTurn(float Angle)
        {
            this.sender.Send(SumoCommandsGenerated.Piloting_addCapOffset_cmd(Angle)); //  Commands.Turn_cmd(Angle));
        }

        public void ChangeVolume(byte Volume)
        {
            this.sender.Send(SumoCommandsGenerated.AudioSettings_MasterVolume_cmd(Volume));// Commands.Volume_cmd(Volume));
        }

        public void SetAudioTheme(SumoEnumGenerated.Theme_theme AudioTheme)
        {
            this.sender.Send(SumoCommandsGenerated.AudioSettings_Theme_cmd(AudioTheme)); // Commands.AudioTheme_cmd(AudioTheme));
        }

        public void JumpLoad()
        {
            this.sender.Send(SumoCommandsGenerated.Animations_JumpLoad_cmd()); // Commands.JumpLoading_cmd());
        }

        public void CancelJump()
        {
            this.sender.Send(SumoCommandsGenerated.Animations_JumpCancel_cmd()); // Commands.CancelJump_cmd());
        }

        public void STOP()
        {
            this.sender.Send(SumoCommandsGenerated.Animations_JumpStop_cmd()); // Commands.STOP_cmd());
        }

        public void StartBoost()
        {            
            this.sender.Send(SumoCommandsGenerated.Animations_StartAnimation_cmd( SumoEnumGenerated.StartAnimation_anim.BOOST)); 
        }

        public void Headlight_on()
        {
            this.sender.Send(SumoCommandsGenerated.Headlights_intensity_cmd(255, 255)); // Commands.Set_Headlight(255,255));
        }
        public void Headlight_off()
        {
            this.sender.Send(SumoCommandsGenerated.Headlights_intensity_cmd(0, 0)); // Commands.Set_Headlight(0, 0));
        }

        public void Headlight_Value(double newValue)
        {
            byte v = (byte)Convert.ToUInt16(newValue);
            this.sender.Send(SumoCommandsGenerated.Headlights_intensity_cmd(v, v));
        }

        public void SetProductName(string name)
        {
            this.sender.Send(SumoCommandsGenerated.Settings_ProductName_cmd(name));
        }
                

        private byte Encodebool(bool[] arr)
        {
            byte val = 0;
            foreach (bool b in arr)
            {
                val <<= 1;
                if (b) val |= 1;
            }
            return val;
        }

        #endregion

        internal void EnableVideo()
        {
            this.sender.Send(SumoCommandsGenerated.MediaStreaming_VideoEnable_cmd(1));// Commands.Set_media_streaming_cmd(true));            
        }
        internal void DisableVideo()
        {
            this.sender.Send(SumoCommandsGenerated.MediaStreaming_VideoEnable_cmd(0)); // Commands.Set_media_streaming_cmd(false));
        }

        #region Accessory BOX
        public void OpenBox()
        {
            // TODO : if connected
            if (sumoInformations.IsCapapableOf(SumoInformations.Capability.Box))
            {
                using (Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                {
                    IPAddress serverAddr = IPAddress.Parse(deviceIp);
                    IPEndPoint endPoint = new IPEndPoint(serverAddr, 4567);
                    byte[] send_buffer = Encoding.ASCII.GetBytes("open\0");
                    sock.SendTo(send_buffer, endPoint);
                }
            }
        }

        public void CloseBox()
        {

            using (Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                IPAddress serverAddr = IPAddress.Parse(deviceIp);
                IPEndPoint endPoint = new IPEndPoint(serverAddr, 4567);
                byte[] send_buffer = Encoding.ASCII.GetBytes("close\0");
                sock.SendTo(send_buffer, endPoint);
            }
        }

        public void AddCapabilities(SumoInformations.Capability cap)
        {            
            sumoInformations.AddCapabilities(cap);
        }

        public void RemoveCapabilities(SumoInformations.Capability cap)
        {
            sumoInformations.RemoveCapabilities(cap);
        }

        #endregion


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

        public void setWifiBand(SumoEnumGenerated.WifiSelection_band band)
        {
            sender.Send(SumoCommandsGenerated.NetworkSettings_WifiSelection_cmd(SumoEnumGenerated.WifiSelection_type.auto, band, 0));
        }
    }
}