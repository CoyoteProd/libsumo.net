using LibSumo.Net.Events;
using LibSumo.Net.Helpers;
using LibSumo.Net.Logger;
using LibSumo.Net.Protocol;
using LibSumo.Net.Streams;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LibSumo.Net.Network
{

    /// <summary>
    /// Receives data from the Jumping Sumo
    /// </summary>
    internal class SumoReceiver
    {
        #region Properties
        public string Host { get; set; }
        public int Port { get; set; }
        private bool IsConnected { get; set; }
        public int BatteryLevel { get; set; }

        #endregion

        #region Private Fields
        private object mutex_frames_Lock;
        private IPEndPoint SumoRemote;
        private SumoSender _sender;

        private SumoInformations _sumoInformations;
        
        // Video Frames
        private UInt16 current_videoframe_no;
        private List<byte[]> VideoParts;
        private List<byte[]> VideoFrames;

        // Audio Frames
        private SumoAudioPlayer _audioPlayer;        
        private List<byte[]> AudioFrames;

        private UdpClient udpClient;
        private Task ReceiveThread;

        #endregion

        #region Constructor
        public SumoReceiver(string host, int port, SumoSender sender, ref SumoInformations sumoInformations)
        {

            this.Host = host;
            this.Port = port;
            
            this._sender = sender;
            SumoRemote = new IPEndPoint(IPAddress.Any, port);
            this.BatteryLevel = 0;

            // Video frames                    
            this.VideoFrames = new List<byte[]>();
            this.mutex_frames_Lock = new object(); // Lock();

            // Audio Frames
            this.AudioFrames = new List<byte[]>();
            
            _sumoInformations = sumoInformations;
        }

        #endregion

        public void RunThread()
        {
            if (ReceiveThread!=null && ReceiveThread.Status == TaskStatus.Running)
                IsConnected = false;

            if (udpClient == null)
            {
                try
                {
                    udpClient = new UdpClient(SumoRemote);
                }
                catch
                {
                }
            }

            this.IsConnected = true;
            ReceiveThread = Task.Run( () => SumoReceive());
        }
        /// <summary>
        /// Stops the main loop and closes the connection to the Jumping Sumo
        /// </summary>
        public void Disconnect()
        {
            this.IsConnected = false;
        }


        #region Private Methods
        private async void SumoReceive()
        {

           
            LOGGER.GetInstance.Info("[SumoReceiver] Thread Started");
            while (this.IsConnected)
            {
                byte[] packet;
                try
                {                        
                    var receivedResults = await udpClient.ReceiveAsync();
                    packet = receivedResults.Buffer;
                    while (packet.Length > 0)
                    {
                        // A packet can have one or more frames
                        var Result = _split_frames(packet);
                        byte[] frame = Result.Item1;
                        packet = Result.Item2;
                        if (frame == null)
                        {
                            break;
                        }
                        // Process the next frame
                        this._process_frame(frame);
                    }
                }
                catch (Exception e)
                {
                    LOGGER.GetInstance.Error("[SumoReceiver] socket.recv() timed out with message : " + e.Message);
                    break;
                }
            }
            LOGGER.GetInstance.Info("[SumoReceiver] Thread Stopped");
            
        }           
        
        private void _process_frame(byte[] frame)
        {
            var Result = _read_header(frame);
            byte data_type = Result.Item1;
            byte buffer_id = Result.Item2;
            byte seq_no = Result.Item3;
            UInt32 frame_size = Result.Item4;
            byte[] payload = frame.SubArray("7:");
            // We received an ACK for a packet we sent
            if (data_type == (byte)SumoConstantsCustom.ARNETWORKAL_FRAME_TYPE.ARNETWORKAL_FRAME_TYPE_ACK)
            {
                LOGGER.GetInstance.Debug("ACK packet received");
            }
            else if (data_type == (byte)SumoConstantsCustom.ARNETWORKAL_FRAME_TYPE.ARNETWORKAL_FRAME_TYPE_DATA || 
                     data_type == (byte)SumoConstantsCustom.ARNETWORKAL_FRAME_TYPE.ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK)
            {
                // Frame requires an ACK, send one
                if (data_type == (byte)SumoConstantsCustom.ARNETWORKAL_FRAME_TYPE.ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK)
                {
                    byte[] ack = _create_ack_packet(data_type, buffer_id, seq_no);
                    this._sender.SendAck(ack);
                    //LOGGER.GetInstance.Debug(String.Format("Sending ACK for {0} {1} {2} {3}", data_type, buffer_id, seq_no, frame_size));
                }

                // We received a data packet
                var Result1 = StructConverter.Unpack("<BBH", payload.SubArray(":4"));
                byte cmd_project = (byte)Result1[0];
                byte cmd_class = (byte)Result1[1];
                UInt16 cmd_id = (UInt16)Result1[2];


                if (Tuple.Create(cmd_project, cmd_class, cmd_id).Equals(SumoConstantsGenerated.common_SettingsState_ProductNameChanged))
                {
                    // Product name Changed
                    var name = Encoding.ASCII.GetString(payload.SubArray("4:")).Trim('\0');
                    LOGGER.GetInstance.Info(String.Format("Drone Name: {0}", name));
                }
                else if (Tuple.Create(cmd_project, cmd_class, cmd_id).Equals(SumoConstantsGenerated.common_CommonState_CurrentDateChanged))
                {
                    var date = payload.SubArray("4:");
                    LOGGER.GetInstance.Info(String.Format("Date updated to: {0}", Encoding.ASCII.GetString(date)));
                } else if (Tuple.Create(cmd_project, cmd_class, cmd_id).Equals(SumoConstantsGenerated.common_CommonState_CurrentTimeChanged))
                {
                    var time = payload.SubArray("4:");
                    LOGGER.GetInstance.Info(String.Format("Time updated to: {0}", Encoding.ASCII.GetString(time)));
                } else if (Tuple.Create(cmd_project, cmd_class, cmd_id).Equals(SumoConstantsGenerated.common_CommonState_WifiSignalChanged))
                {
                    Int16 rssi = (Int16)StructConverter.Unpack("<h", payload.SubArray("4:"))[0];
                    _sumoInformations.Rssi = rssi;
                    var evt = new SumoEventArgs(SumoEnumCustom.TypeOfEvents.RSSI, _sumoInformations);
                    OnSumoEvents(evt);
                } else if (Tuple.Create(cmd_project, cmd_class, cmd_id).Equals(SumoConstantsGenerated.common_CommonState_BatteryStateChanged))
                {
                    var battery = payload.SubArray("4:5")[0];
                    BatteryLevel = battery;
                    _sumoInformations.BatteryLevel = BatteryLevel;
                    var BatEvt = new SumoEventArgs(SumoEnumCustom.TypeOfEvents.BatteryLevelEvent, _sumoInformations);
                    OnSumoEvents(BatEvt);
                }
                else if (Tuple.Create(cmd_project, cmd_class, cmd_id).Equals(SumoConstantsGenerated.common_CommonState_AllStatesChanged))
                {
                    // All states have been sent
                    var State = payload.SubArray("4:");
                    LOGGER.GetInstance.Debug(String.Format("All states have been sent: {0}", BitConverter.ToString(State)));
                    _sumoInformations.BuildDefaultCapabilities();
                }
                else if (Tuple.Create(cmd_project, cmd_class, cmd_id).Equals(SumoConstantsGenerated.common_CommonState_ProductModel))
                {
                    // Device model
                    byte Model = payload.SubArray("4:")[0];
                    LOGGER.GetInstance.Info(String.Format("Device Model: {1} {0}", Model, (SumoEnumGenerated.ProductModel_model)Model).ToString());
                    // Add Capabilities 
                    _sumoInformations.BuildCapabilities((SumoEnumGenerated.ProductModel_model)Model);
                    var CapEvt = new SumoEventArgs(SumoEnumCustom.TypeOfEvents.CapabilitiesChange, _sumoInformations);
                    OnSumoEvents(CapEvt);
                }
                else if (Tuple.Create(cmd_project, cmd_class, cmd_id).Equals(SumoConstantsGenerated.common_HeadlightsState_intensityChanged))
                {
                    var HeadData = StructConverter.Unpack("<BB", payload.SubArray("4:"));
                    LOGGER.GetInstance.Debug(String.Format("Headlight change: {0} {1}", HeadData[0], HeadData[1]));
                }
                else if (Tuple.Create(cmd_project, cmd_class, cmd_id).Equals(SumoConstantsGenerated.common_AudioState_AudioStreamingRunning))
                {
                    var AudioStateData = payload.SubArray("4:")[0];
                    if ((AudioStateData & (1 << 0)) != 0)
                    {
                        LOGGER.GetInstance.Info(String.Format("Drone is ready to stream to controller (Drone TX ON)"));
                        if (_audioPlayer != null)
                        {
                            _audioPlayer.Start();
                        }
                    }
                    else if ((AudioStateData & (1 << 0)) == 0)
                    {
                        LOGGER.GetInstance.Info(String.Format("Drone has cancelled to stream to controller (Drone TX OFF)"));
                        if (_audioPlayer != null)
                        {
                            _audioPlayer.Stop();
                        }
                    }

                    if ((AudioStateData & (1 << 1)) != 0)
                    {
                        LOGGER.GetInstance.Info(String.Format("Drone is ready to receive stream from Controller (Drone RX ON)"));
                    }
                    else if ((AudioStateData & (1 << 1)) == 0)
                    {
                        LOGGER.GetInstance.Info(String.Format("Drone has cancelled to receive stream from Controller (Drone RX OFF)"));
                    }
                }
                else if (Tuple.Create(cmd_project, cmd_class, cmd_id).Equals(SumoConstantsGenerated.jpsumo_PilotingState_SpeedChanged))
                {
                    var Result2 = StructConverter.Unpack("<bh", payload.SubArray("4:"));
                    sbyte speed = (sbyte)Result2[0];
                    Int16 real_speed = (Int16)Result2[1];
                    LOGGER.GetInstance.Debug(String.Format("Speed updated to {0} ({1} cm/s)", speed, real_speed));
                    _sumoInformations.Speed = speed;
                    var evt = new SumoEventArgs(SumoEnumCustom.TypeOfEvents.SpeedChange, _sumoInformations);
                    OnSumoEvents(evt);
                }
                else if (Tuple.Create(cmd_project, cmd_class, cmd_id).Equals(SumoConstantsGenerated.jpsumo_PilotingState_PostureChanged))
                {
                    var state = (Int32)StructConverter.Unpack("<i", payload.SubArray("4:"))[0];
                    LOGGER.GetInstance.Debug(String.Format("State of posture changed: {1} ({0})", state, (SumoEnumGenerated.PostureChanged_state)state).ToString());
                    _sumoInformations.Posture = (SumoEnumGenerated.PostureChanged_state)state;
                    var evt = new SumoEventArgs(SumoEnumCustom.TypeOfEvents.PostureEvent, _sumoInformations);
                    OnSumoEvents(evt);
                }
                else if (Tuple.Create(cmd_project, cmd_class, cmd_id).Equals(SumoConstantsGenerated.jpsumo_AnimationsState_JumpLoadChanged))
                {
                    var state = (Int32)StructConverter.Unpack("<i", payload.SubArray("4:"))[0];
                    LOGGER.GetInstance.Warn(String.Format("State of jump load changed: {1} ({0})", state, (SumoEnumGenerated.JumpLoadChanged_state)state).ToString());
                }
                else if (Tuple.Create(cmd_project, cmd_class, cmd_id).Equals(SumoConstantsGenerated.jpsumo_AnimationsState_JumpTypeChanged))
                {
                    var state = (Int32)StructConverter.Unpack("<i", payload.SubArray("4:"))[0];
                    LOGGER.GetInstance.Warn(String.Format("State of jump type changed: {1} ({0})", state, (SumoEnumGenerated.JumpTypeChanged_state)state).ToString());
                }
                else if (Tuple.Create(cmd_project, cmd_class, cmd_id).Equals(SumoConstantsGenerated.jpsumo_AnimationsState_JumpMotorProblemChanged))
                {
                    var state = (Int32)StructConverter.Unpack("<i", payload.SubArray("4:"))[0];
                    LOGGER.GetInstance.Error(String.Format("State about the jump motor problem: {1} ({0})", state, (SumoEnumGenerated.JumpMotorProblemChanged_error)state).ToString());
                }
                else if (Tuple.Create(cmd_project, cmd_class, cmd_id).Equals(SumoConstantsGenerated.jpsumo_NetworkState_LinkQualityChanged))
                {
                    var linkQuality = (byte)StructConverter.Unpack("<B", payload.SubArray("4:"))[0];
                    _sumoInformations.LinkQuality = linkQuality;
                    var evt = new SumoEventArgs(SumoEnumCustom.TypeOfEvents.LinkQuality, _sumoInformations);
                    OnSumoEvents(evt);
                }
                else if (Tuple.Create(cmd_project, cmd_class, cmd_id).Equals(SumoConstantsGenerated.jpsumo_MediaStreamingState_VideoEnableChanged))
                {
                    var state = (Int32)StructConverter.Unpack("<i", payload.SubArray("4:"))[0];
                    LOGGER.GetInstance.Info(String.Format("Media streaming state is: {1}({0})", state, (SumoEnumGenerated.VideoStateChangedV2_state)state).ToString());
                }
                else if (Tuple.Create(cmd_project, cmd_class, cmd_id).Equals(SumoConstantsGenerated.jpsumo_AudioSettingsState_MasterVolumeChanged))
                {
                    var VolumeState = (byte)payload.SubArray("4:")[0];
                    LOGGER.GetInstance.Debug(String.Format("Volume state is: {0}", VolumeState));
                    _sumoInformations.Volume = VolumeState;
                    var evt = new SumoEventArgs(SumoEnumCustom.TypeOfEvents.VolumeChange, _sumoInformations);
                    OnSumoEvents(evt);
                }
                else if (Tuple.Create(cmd_project, cmd_class, cmd_id).Equals(SumoConstantsGenerated.jpsumo_PilotingState_AlertStateChanged))
                {
                    var state = (Int32)StructConverter.Unpack("<i", payload.SubArray("4:"))[0];
                    _sumoInformations.Alert = (SumoEnumGenerated.AlertStateChanged_state)state;
                    var evt = new SumoEventArgs(SumoEnumCustom.TypeOfEvents.AlertEvent, _sumoInformations);
                    OnSumoEvents(evt);
                    LOGGER.GetInstance.Debug(String.Format("AlertStateChanged: {0}", ((SumoEnumGenerated.AlertStateChanged_state)state).ToString()));
                }
                else if (Tuple.Create(cmd_project, cmd_class, cmd_id).Equals(SumoConstantsGenerated.jpsumo_NetworkSettingsState_WifiSelectionChanged))
                {                    
                    var Wifi = StructConverter.Unpack("<iiB", payload.SubArray("4:"));
                    int WifiType = (int)Wifi[0];
                    int WifiBand = (int)Wifi[1];
                    byte WifiChannel = (byte)Wifi[2];
                    LOGGER.GetInstance.Info(String.Format("Wifi Type:{0} Band:{1} Channel:{2}", ((SumoEnumGenerated.WifiSelectionChanged_type)WifiType).ToString(), ((SumoEnumGenerated.WifiSelectionChanged_band)WifiBand).ToString(), WifiChannel));
                    _sumoInformations.WifiType = (SumoEnumGenerated.WifiSelectionChanged_type)WifiType;
                    _sumoInformations.WifiBand = (SumoEnumGenerated.WifiSelectionChanged_band)WifiBand;
                    _sumoInformations.WifiChannel = WifiChannel;
                    var evt = new SumoEventArgs(SumoEnumCustom.TypeOfEvents.WifiChanged, _sumoInformations);
                    OnSumoEvents(evt);
                }
                else if (Tuple.Create(cmd_project, cmd_class, cmd_id).Equals(SumoConstantsGenerated.jpsumo_AudioSettingsState_ThemeChanged))
                {
                    var AudioTheme = (byte)StructConverter.Unpack("<B", payload.SubArray("4:"))[0];
                    LOGGER.GetInstance.Info(String.Format("Audio Theme Changed: {0} ", ((SumoEnumGenerated.ThemeChanged_theme)AudioTheme).ToString()));
                }
                else
                {
                    var data = payload.SubArray("4:");
                    LOGGER.GetInstance.Debug(String.Format("DataFrame | Project: {0}, Class: {1}, Id: {2} data: {3}", cmd_project, cmd_class, cmd_id, BitConverter.ToString(data)));
                }
            }
            else if (data_type == (byte)SumoConstantsCustom.ARNETWORKAL_FRAME_TYPE.ARNETWORKAL_FRAME_TYPE_DATA_LOW_LATENCY)
            {
                // We received an ARStream packet, process it
                this._process_stream_frame(buffer_id, frame_size, payload);
            }
            else
            {
                LOGGER.GetInstance.Warn(String.Format("Unknown header type: {0} {1} {2} {3}", data_type, buffer_id, seq_no, frame_size));
            }
        }

        internal void EnableRXAudioProcessing(SumoAudioPlayer audioPlayer)
        {
            this._audioPlayer = audioPlayer;
            this._audioPlayer.ConfigureCodec();            
        }

        private void _process_stream_frame(byte buffer_id, UInt32 frame_size, byte[] payload)
        {
            if (buffer_id == SumoConstantsCustom.NETWORK_DC_VIDEO_DATA_ID)
            {
                var Result = StructConverter.Unpack("<HBBB", payload.SubArray(":5"));
                UInt16 frame_no = (UInt16)Result[0];
                byte frame_flags = (byte)Result[1];
                byte frag_no = (byte)Result[2];
                byte frags_per_frame = (byte)Result[3];
                byte[] fragment = payload.SubArray("5:");
                // We got a fragment for a different frame
                if (frame_no != this.current_videoframe_no)
                {
                    // Reset frame number and fragment buffer
                    this.current_videoframe_no = frame_no;
                    this.VideoParts = new List<byte[]>(frags_per_frame); //{ null } * frags_per_frame;
                }
                if (VideoParts != null)
                {
                    if (this.VideoParts.Count >= frag_no + 1)
                    {
                        if (this.VideoParts.ElementAt(frag_no) != null)
                        {
                            LOGGER.GetInstance.Debug(String.Format("[Video] Duplicate fragment | Frame: {0}, Fragment: {1}", frame_no, frag_no));
                            return;
                        }
                    }

                    this.VideoParts.Add(fragment);

                    // We've received the entire frame
                    if (!this.VideoParts.Contains(null))
                    {
                        lock (mutex_frames_Lock)
                        {
                            this.VideoFrames.Add(this.VideoParts.SelectMany(a => a).ToArray());
                        }
                    }
                }
            }else if (buffer_id == SumoConstantsCustom.NETWORK_DC_SOUND_DATA_ID && _audioPlayer!=null)
            {   // Process Audio stream on Night/Race Drone
                //LOGGER.GetInstance.Debug(String.Format("Received Audio Stream ! "));

                var Result = StructConverter.Unpack("<HBBB", payload.SubArray(":5"));
                UInt16 frame_no = (UInt16)Result[0];
                byte frame_flags = (byte)Result[1];
                byte frag_no = (byte)Result[2];
                byte frags_per_frame = (byte)Result[3];

                // Originaly 5: , I think first 4 byte is for Audio information... or frame Header ? I dont know...
                // maybe because Audio Frame = HEADER_SIZE = 16 + DATA_SIZE = 256
                // From : https://github.com/Parrot-Developers/libARController/blob/master/JNI/java/com/parrot/arsdk/arcontroller/ARAudioFrame.java
                // https://github.com/Parrot-Developers/libARController/blob/master/Sources/ARCONTROLLER_AudioHeader.h
                // So Skip 16 first byte (5 frame header + 11 Audio Header)
                byte[] fragment = payload.SubArray("7:");
                

                // Write it in the shared Stream               
                _audioPlayer.OnDataReceived(fragment);

                // Debug                
                /*
                byte[] AudioHeader = payload; //.SubArray(":16");
                var r = StructConverter.Unpack("<QHHI", AudioHeader);

                using (var stream = new FileStream("AudioHeader.dat", FileMode.Append))
                {
                    stream.Write(fragment, 0, fragment.Length);
                    stream.Write(new byte[] { 0x0d, 0x0a }, 0, 2);
                } 
                */
                
            }
            else
            {
                // Stream data from another low-latency buffer (maybe audio?)
                LOGGER.GetInstance.Debug(String.Format("Unknow ARStream | buffer: {0}, size: {1}", buffer_id, frame_size - 7));
                return;
            }        
        }



        /// <summary>
        /// Returns the header portion of the given data
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static Tuple<byte, byte, byte, UInt32> _read_header(byte[] data)
        {
            var Result = StructConverter.Unpack("<BBBI", data.SubArray(":7"));
            byte data_type = (byte)Result[0];
            byte buffer_id = (byte)Result[1];
            byte seq_no = (byte)Result[2];
            UInt32 frame_size = (UInt32)Result[3];
            return Tuple.Create(data_type, buffer_id, seq_no, frame_size);
        }



        /// <summary>
        /// Returns (head, tail) where head is the first frame in the given data
        ///  and tail is the rest of the data    
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private Tuple<byte[], byte[]> _split_frames(byte[] data)
        {
            if (data.Length < 7)
            {
                // Must contain at least header, nothing to process
                return Tuple.Create<byte[], byte[]>(null, data.SubArray("-1:-1"));
            }
            var Result = _read_header(data);            
            //var data_type = Result.Item1;
            //var buffer_id = Result.Item2;
            //var seq_no = Result.Item3;            
            UInt32 frame_size = Result.Item4;
            return Tuple.Create(data.SubArray(":" + frame_size), data.SubArray(frame_size + ":"));
        }

        /// <summary>
        /// Create an ACK packet based on the given header information
        /// </summary>
        /// <param name="data_type"></param>
        /// <param name="buffer_id"></param>
        /// <param name="seq_no"></param>
        /// <returns></returns>
        private byte[] _create_ack_packet(byte data_type, byte buffer_id, byte seq_no)
        {
            Debug.Assert(data_type == (byte)SumoConstantsCustom.ARNETWORKAL_FRAME_TYPE.ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK);
            // The payload of an ACK frame is the sequence no. of the data frame
            byte payload = seq_no;
            // The buffer id is 128 + base_id
            byte[] buffer = StructConverter.Pack("<BBBI", new object[] { (byte)SumoConstantsCustom.ARNETWORKAL_FRAME_TYPE.ARNETWORKAL_FRAME_TYPE_ACK, (byte)buffer_id + 128, (byte)0, (byte)8 }); //"<BBBI"
            return buffer.Concat(new byte[] { payload }).ToArray();
        }
        #endregion

        /// <summary>
        /// Returns the last received frame and clears the buffer. Returns None if there is none
        /// </summary>
        /// <returns></returns>
        public byte[] Get_video_frame()
        {
            lock (mutex_frames_Lock)
            {
                if (this.VideoFrames.Count == 0)
                {
                    return null;
                }
                var frame = this.VideoFrames.Last();
                this.VideoFrames = new List<byte[]>();
                return frame;
            }
        }

        #region Event Handler       
        public delegate void SumoEventHandler(object sender, SumoEventArgs e);
        public event SumoEventHandler SumoEvents;
        protected virtual void OnSumoEvents(SumoEventArgs e)
        {
            SumoEvents?.Invoke(this, e);
        }
        #endregion

    }


}