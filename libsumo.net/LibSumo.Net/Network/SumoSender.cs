using LibSumo.Net;
using LibSumo.Net.Helpers;
using LibSumo.Net.Logger;
using LibSumo.Net.Protocol;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LibSumo.Net.Network
{
    /*            
        For full reference, see: http://developer.parrot.com/docs/bebop/ARSDK_Protocols.pdf
        The xml files for command definitions can be found here: https://github.com/Parrot-Developers/arsdk-xml/tree/master/xml

        A commands is identified by its first 4 bytes:
            - Project/Feature (1 byte)
            - Class ID in project/feature (1 byte)
            - Command ID in class (2 bytes) 
        All data is sent in Little Endian byte order       
    */

    /// <summary>
    /// Sends commands to the Jumping Sumo. PCMD commands are sent at a fixed frequency (every 25ms)
    /// </summary>
    internal class SumoSender
    {

        #region Properties
        public string Host { get; set; }
        public int Port { get; set; }
        #endregion

        #region Private Fields        
        private DefaultDict<int, int> seq_ids;
        private IPEndPoint SumoRemote;
        private UdpClient sumoSocket;
        private byte[] cmd;        
        private bool isConnected;
        #endregion

        #region Constructor
        public SumoSender(string host, int port)
        {
            this.Host = host;
            this.Port = port;            
            this.seq_ids = new DefaultDict<int, int>();
            SumoRemote = new IPEndPoint(IPAddress.Parse(host), port);
            sumoSocket = new UdpClient();

            // Initial command (no motion)                 
            this.cmd = SumoCommandsCustom._pack_frame(SumoCommandsCustom.Move_cmd(0, 0));            
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Updates the sequence number for the given framed command
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private byte[] _update_seq(byte[] cmd)
        {
            Debug.Assert(cmd.Length > 3);            
            var buffer_id = cmd[1];
            this.seq_ids[buffer_id] = (this.seq_ids[buffer_id] + 1) % 256;
            return cmd.SubArray(":2").Concat(new byte[] { (byte)this.seq_ids[buffer_id] }).Concat(cmd.SubArray("3:")).ToArray();
        }

        private void SumoSend()
        {
            try
            {
                LOGGER.GetInstance.Info("[SumoSender] Thread Started");                
                while (isConnected)
                {                                       
                    sumoSocket.Send(this.cmd, this.cmd.Length, SumoRemote);
                    this.cmd = SumoCommandsCustom._pack_frame(SumoCommandsCustom.Move_cmd(0, 0));                    
                    Thread.Sleep(40);
                }                
            }
            catch(Exception e)
            {
                LOGGER.GetInstance.Error(e.Message);
            }
            LOGGER.GetInstance.Info("[SumoSender] Thread Stopped");
        }

        public  void Init()
        {
            // Initial configuration            
            this.Send(SumoCommandsGenerated.Common_CurrentDate_cmd(DateTime.Now.ToString("yyyy-MM-dd\0"))); //this.Send(Commands.Sync_date_cmd());
            Thread.Sleep(25);
            this.Send(SumoCommandsGenerated.Common_CurrentTime_cmd(DateTime.Now.ToString("'T'HHmmsszzz\0").Replace(":", ""))); //this.Send(Commands.Sync_time_cmd());
            Thread.Sleep(25);
            this.Send(SumoCommandsGenerated.Common_AllStates_cmd()); //this.Send(Commands.RequestAllStates_cmd());
            Thread.Sleep(25);
            this.Send(SumoCommandsGenerated.Settings_AllSettings_cmd()); //this.Send(Commands.RequestAllConfig_cmd());            
        }

        #endregion

        #region Public Method

        /// <summary>
        /// Sends the given command to the Jumping Sumo. Non-PCMD commands are sent immediately
        /// while PCMD commands are sent at the next cycle(see run). cmd is the payload and the
        /// method creates a frame by prepending a header
        /// </summary>
        /// <param name="cmd"></param>
        public void Send(byte[] cmd)
        {
            if (isConnected)
            {
                if (cmd != null)
                {                                       
                    byte[] frame = this._update_seq(SumoCommandsCustom._pack_frame(cmd));
                    if (SumoCommandsCustom._is_pcmd(frame)) this.cmd = frame;                        
                    else sumoSocket.Send(frame, frame.Length, SumoRemote);                                                                        
                }
            }
        }

        /// <summary>
        /// Immediately send ACK
        /// Ack dosen't have header!
        /// </summary>
        /// <param name="cmd"></param>
        public void SendAck(byte[] cmd)
        {
            if (isConnected)            
                if (cmd != null)
                    sumoSocket.Send(cmd, cmd.Length, SumoRemote);                                               
        }
        public void RunThread()
        {
            isConnected = true;
            Task.Run(() => SumoSend());
        }

        /// <summary>
        /// Stops the main loop and closes the connection to the Jumping Sumo
        /// </summary>
        public void Disconnect()
        {            
            isConnected = false;
        }
        #endregion
    }



}