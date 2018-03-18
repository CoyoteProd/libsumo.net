using LibSumo.Net.Events;
using LibSumo.Net.Helpers;
using LibSumo.Net.Logger;
using LibSumo.Net.Protocol;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private SumoSender sender;
        private UInt16 current_frame_no;
        private List<byte[]> parts;
        private List<byte[]> frames;
        #endregion

        #region Constructor
        public SumoReceiver(string host, int port, SumoSender _sender)
        {

            this.Host = host;
            this.Port = port;
            
            this.sender = _sender;
            SumoRemote = new IPEndPoint(IPAddress.Any, port);
            this.BatteryLevel = 0;

            // Video frames                    
            this.frames = new List<byte[]>();
            this.mutex_frames_Lock = new object(); // Lock();
        }

        #endregion

        public void Run()
        {
            this.IsConnected = true;
            Task.Run( () => SumoReceive());
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
            using (var udpClient = new UdpClient(SumoRemote))
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
                        LOGGER.GetInstance.Warn("[SumoReceiver] socket.recv() timed out with message : " + e.Message);
                        break;
                    }
                }
                LOGGER.GetInstance.Info("[SumoReceiver] Thread Stopped");
            }
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
            if (data_type == Constants.ARNETWORKAL_FRAME_TYPE_ACK)
            {
                LOGGER.GetInstance.Debug("ACK packet received");
            }
            else if (data_type == Constants.ARNETWORKAL_FRAME_TYPE_DATA || data_type == Constants.ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK)
            {
                // Frame requires an ACK, send one
                if (data_type == Constants.ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK)
                {
                    byte[] ack = _create_ack_packet(data_type, buffer_id, seq_no);
                    this.sender.SendAck(ack);
                    //LOGGER.GetInstance.Debug(String.Format("Sending ACK for {0} {1} {2} {3}", data_type, buffer_id, seq_no, frame_size));
                }

                // We received a data packet, log it (do more in future?)
                var Result1 = StructConverter.Unpack("<BBH", payload.SubArray(":4"));
                byte cmd_project = (byte)Result1[0];
                byte cmd_class = (byte)Result1[1];
                UInt16 cmd_id = (UInt16)Result1[2];
                if (cmd_project == Constants.PROJECT_COMMOM)
                {
                    if (Tuple.Create(cmd_class, cmd_id).Equals(Tuple.Create<byte, UInt16>(5, 4)))
                    {
                        var date = payload.SubArray("4:");
                        LOGGER.GetInstance.Info(String.Format("Date updated to: {0}", Encoding.ASCII.GetString(date)));
                    }
                    else if (Tuple.Create(cmd_class, cmd_id).Equals(Tuple.Create<byte, UInt16>(5, 5)))
                    {
                        var time = payload.SubArray("4:");
                        LOGGER.GetInstance.Info(String.Format("Time updated to: {0}", Encoding.ASCII.GetString(time)));
                    }
                    else if (Tuple.Create(cmd_class, cmd_id).Equals(Tuple.Create<byte, UInt16>(5, 7)))
                    {                        
                        Int16 rssi = (Int16)StructConverter.Unpack("<h", payload.SubArray("4:"))[0];                        
                        var evt = new SumoEventArgs(SumoEnum.TypeOfEvents.RSSI)
                        {
                            Rssi = rssi
                        };
                        OnSumoEvents(evt);
                    }
                    else if (Tuple.Create(cmd_class, cmd_id).Equals(Tuple.Create<byte, UInt16>(5, 1)))
                    {
                        var battery = payload.SubArray("4:5")[0];                        
                        BatteryLevel = battery;
                        var evt = new SumoEventArgs(SumoEnum.TypeOfEvents.BatteryLevelEvent)
                        {
                            BatteryLevel = BatteryLevel
                        };
                        OnSumoEvents(evt);
                    }
                    else if (Tuple.Create(cmd_class, cmd_id).Equals(Tuple.Create<byte, UInt16>(5, 0)))
                    {  // All states have been sent
                        var data = payload.SubArray("4:");
                        LOGGER.GetInstance.Debug(String.Format("All states have been sent: {0}", BitConverter.ToString(data)));
                    }
                    else if (Tuple.Create(cmd_class, cmd_id).Equals(Tuple.Create<byte, UInt16>(5, 9)))
                    {  // Device model
                        byte Model = payload.SubArray("4:")[0];
                        LOGGER.GetInstance.Debug(String.Format("Device Model: {0}", Model));
                    }
                    else if (Tuple.Create(cmd_class, cmd_id).Equals(Tuple.Create<byte, UInt16>(23, 0)))
                    {  // All states have been sent
                        var data = StructConverter.Unpack("<BB", payload.SubArray("4:"));                        
                        LOGGER.GetInstance.Debug(String.Format("Headlight change: {0} {1}", data[0], data[1] ));
                    }
                    else
                    {
                        var data = payload.SubArray("4:");
                        LOGGER.GetInstance.Debug(String.Format("DataFrame | Project: {0}, Class: {1}, Id: {2} data: {3}", cmd_project, cmd_class, cmd_id, BitConverter.ToString(data)));
                    }
                }
                else if (cmd_project == Constants.PROJECT_SUMO)
                {
                    if (Tuple.Create(cmd_class, cmd_id).Equals(Tuple.Create<byte, UInt16>(1, 2)))
                    {
                        var Result2 = StructConverter.Unpack("<bh", payload.SubArray("4:"));
                        byte speed = (byte)Result2[0];
                        Int16 real_speed = (Int16)Result2[1];
                        LOGGER.GetInstance.Info(String.Format("Speed updated to {0} ({1} cm/s)", speed, real_speed));
                    }else if (Tuple.Create(cmd_class, cmd_id).Equals(Tuple.Create<byte, UInt16>(1, 0)))
                    { 
                        var state = (Int32)StructConverter.Unpack("<i", payload.SubArray("4:"))[0];
                        LOGGER.GetInstance.Debug(String.Format("State of posture changed: {1} ({0})", state, (SumoEnum.Posture)state).ToString());                        
                        var evt = new SumoEventArgs(SumoEnum.TypeOfEvents.PostureEvent)
                        {
                            Posture = (SumoEnum.Posture)state
                        };
                        OnSumoEvents(evt);
                    }
                    else if (Tuple.Create(cmd_class, cmd_id).Equals(Tuple.Create<byte, UInt16>(3, 0)))
                    { 
                        var state = (Int32)StructConverter.Unpack("<i", payload.SubArray("4:"))[0];
                        LOGGER.GetInstance.Warn(String.Format("State of jump load changed: {1} ({0})", state, (SumoEnum.JumpLoad)state).ToString());
                    }
                    else if (Tuple.Create(cmd_class, cmd_id).Equals(Tuple.Create<byte, UInt16>(3, 1)))
                    {
                        var state = (Int32)StructConverter.Unpack("<i", payload.SubArray("4:"))[0];
                        LOGGER.GetInstance.Warn(String.Format("State of jump type changed: {1} ({0})", state, (SumoEnum.Jump)state).ToString());
                    }
                    else if (Tuple.Create(cmd_class, cmd_id).Equals(Tuple.Create<byte, UInt16>(3, 2)))
                    { 
                        var state = (Int32)StructConverter.Unpack("<i", payload.SubArray("4:"))[0];
                        LOGGER.GetInstance.Error(String.Format("State about the jump motor problem: {1} ({0})", state, (SumoEnum.MotorProblem)state).ToString());
                    }
                    else if (Tuple.Create(cmd_class, cmd_id).Equals(Tuple.Create<byte, UInt16>(11,4)))
                    {
                        var linkQuality = (byte)StructConverter.Unpack("<B", payload.SubArray("4:"))[0];                        
                        var evt = new SumoEventArgs(SumoEnum.TypeOfEvents.LinkQuality)
                        {
                            LinkQuality = linkQuality
                        };
                        OnSumoEvents(evt);
                    }
                    else if (Tuple.Create(cmd_class, cmd_id).Equals(Tuple.Create<byte, UInt16>(19, 0)))
                    {
                        var state = (Int32)StructConverter.Unpack("<i", payload.SubArray("4:"))[0];
                        LOGGER.GetInstance.Info(String.Format("Media streaming state is: {1}({0})", state, (SumoEnum.MediaStreamingState)state).ToString());
                    }
                    else if (Tuple.Create(cmd_class, cmd_id).Equals(Tuple.Create<byte, UInt16>(13, 0)))
                    {
                        var VolumeState = (byte)payload.SubArray("4:")[0];
                        //LOGGER.GetInstance.Info(String.Format("Volume state is: {0}", VolumeState));
                        var evt = new SumoEventArgs(SumoEnum.TypeOfEvents.VolumeChange)
                        {
                            Volume = VolumeState
                        };
                        OnSumoEvents(evt);
                    }
                    else if (Tuple.Create(cmd_class, cmd_id).Equals(Tuple.Create<byte, UInt16>(1, 1)))
                    {
                        var state = (Int32)StructConverter.Unpack("<i", payload.SubArray("4:"))[0];                        
                        var evt = new SumoEventArgs(SumoEnum.TypeOfEvents.BatteryAlertEvent)
                        {
                            BatteryAlert = (SumoEnum.BatteryAlert)state
                        };
                        OnSumoEvents(evt);

                    }                    
                    else
                    {
                        var data = payload.SubArray("4:");                        
                        LOGGER.GetInstance.Debug(String.Format("DataFrame | Project: {0}, Class: {1}, Id: {2} data: {3}", cmd_project, cmd_class, cmd_id, BitConverter.ToString(data)));
                    }
                }
                else
                {
                    var data = payload.SubArray("4:");
                    LOGGER.GetInstance.Debug(String.Format("DataFrame | Project: {0}, Class: {1}, Id: {2} data: {3}", cmd_project, cmd_class, cmd_id, BitConverter.ToString(data)));
                }

                
            }
            else if (data_type == Constants.ARNETWORKAL_FRAME_TYPE_DATA_LOW_LATENCY)
            {
                // We received an ARStream packet, process it
                this._process_video_frame(buffer_id, frame_size, payload);
            }
            else
            {
                LOGGER.GetInstance.Warn(String.Format("Unknown header type: {0} {1} {2} {3}", data_type, buffer_id, seq_no, frame_size));
            }
        }

        private void _process_video_frame(byte buffer_id, UInt32 frame_size, byte[] payload)
        {
            if (buffer_id != Constants.VIDEO_DATA_BUFFER)
            {
                // Stream data from another low-latency buffer (maybe audio?)
                LOGGER.GetInstance.Debug(String.Format("ARStream | buffer: {0}, size: {1}", buffer_id, frame_size - 7));
                return;
            }
            var Result = StructConverter.Unpack("<HBBB", payload.SubArray(":5"));
            UInt16 frame_no = (UInt16)Result[0];
            byte frame_flags = (byte)Result[1];
            byte frag_no = (byte)Result[2];
            byte frags_per_frame = (byte)Result[3];
            byte[] fragment = payload.SubArray("5:");
            // We got a fragment for a different frame
            if (frame_no != this.current_frame_no)
            {
                // Reset frame number and fragment buffer
                this.current_frame_no = frame_no;
                this.parts = new List<byte[]>(frags_per_frame); //{ null } * frags_per_frame;
            }
            if (parts != null)
            {
                if (this.parts.Count >= frag_no+1)
                {
                    if (this.parts.ElementAt(frag_no) != null)
                    {
                        LOGGER.GetInstance.Debug(String.Format("Duplicate fragment | Frame: {0}, Fragment: {1}", frame_no, frag_no));
                        return;
                    }
                }
                                
                this.parts.Add(fragment);                

                // We've received the entire frame
                if (!this.parts.Contains(null))
                {
                    lock (mutex_frames_Lock)
                    {                        
                        this.frames.Add(this.parts.SelectMany(a => a).ToArray());
                    }
                }
                
                                
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
            Debug.Assert(data_type == Constants.ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK);
            // The payload of an ACK frame is the sequence no. of the data frame
            byte payload = seq_no;
            // The buffer id is 128 + base_id
            byte[] buffer = StructConverter.Pack("<BBBI", new object[] { Constants.ARNETWORKAL_FRAME_TYPE_ACK, (byte)buffer_id + 128, (byte)0, (byte)8 }); //"<BBBI"
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
                if (this.frames.Count == 0)
                {
                    return null;
                }
                var frame = this.frames.Last();
                this.frames = new List<byte[]>();
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