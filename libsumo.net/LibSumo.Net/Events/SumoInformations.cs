using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using LibSumo.Net.Logger;
using LibSumo.Net.Protocol;

namespace LibSumo.Net.Events
{
    /// <summary>
    /// Just a Holder to store Current Sumo Status
    /// </summary>
    public class SumoInformations
    {

        public SumoEnumGenerated.PostureChanged_state Posture { get; set; }
        public int BatteryLevel { get; set; }
        public int Rssi { get; set; }
        public int LinkQuality { get; set; }
        public SumoEnumGenerated.AlertStateChanged_state Alert { get; set; }
        public sbyte Speed { get; set; }
        public sbyte Turn { get; set; }
        public byte Volume { get; internal set; }
        public string DeviceName { get; internal set; }
        private List<Capability> Capabilities { get; set; }
        // If battery is less than LowBatteryLevelAlert -> User can Take Action
        public const int LowBatteryLevelAlert = 10;
        public string deviceIp { get; set; }
        public bool IsBatteryUnderLevelAlert
        {
            get
            {
                //if (BatteryLevel == 0) return false;
                return BatteryLevel <= LowBatteryLevelAlert;
                
            }
        }

        public SumoEnumGenerated.WifiSelectionChanged_type WifiType { get; internal set; }
        public SumoEnumGenerated.WifiSelectionChanged_band WifiBand { get; internal set; }
        public byte WifiChannel { get; internal set; }
        public int AudioTheme { get; set; }
        public string LastErrorStr { get; set; }

        public SumoInformations()
        {
            Capabilities = new List<Capability>();
            Capabilities.Add(Capability.Jump); // Default all drone can Jump
        }

        /// <summary>
        /// Jumping Capabilities
        /// </summary>
        public enum Capability
        {
            /// <summary>
            /// On Drone Race only, Speed can be boosted to 100cm/s
            /// </summary>
            Boost,
            /// <summary>
            /// Vilcoyote automatic Box, See https://github.com/CoyoteProd/Jumping-Box
            /// </summary>
            Box,
            /// <summary>
            /// On Race/Night, Drone is able to stream sound to/from drone
            /// </summary>
            Audio,
            /// <summary>
            /// All device can jump, but sometimes you want to Disable Jump (when jump with your accessory can be a problem...)
            /// </summary>
            Jump,
            /// <summary>
            /// Night drone have led on the front
            /// </summary>
            Light
        }

        public bool IsCapapableOf(Capability cap)
        {
            return Capabilities.Contains(cap);
        }

        public List<Capability> BuildDefaultCapabilities()
        {            
            AddCapabilities(SumoInformations.Capability.Jump);
            PingBox();
            return Capabilities;
        }
        /// <summary>
        /// Hard Coded capabilities.
        /// Can be modified to your needs
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public List<Capability> BuildCapabilities(SumoEnumGenerated.ProductModel_model model)
        {
            
            switch(model)
            {
                case (SumoEnumGenerated.ProductModel_model.JS_DIESEL):
                case (SumoEnumGenerated.ProductModel_model.JS_BUZZ):
                case (SumoEnumGenerated.ProductModel_model.JS_MARSHALL):
                    AddCapabilities(Capability.Audio);
                    AddCapabilities(Capability.Light);
                    
                    break;
                case (SumoEnumGenerated.ProductModel_model.SW_WHITE):
                case (SumoEnumGenerated.ProductModel_model.SW_BLACK):
                    AddCapabilities(Capability.Box); 
                    
                    break;
                case (SumoEnumGenerated.ProductModel_model.JS_MAX):
                case (SumoEnumGenerated.ProductModel_model.JS_JETT):
                case (SumoEnumGenerated.ProductModel_model.JS_TUKTUK):
                    AddCapabilities(Capability.Boost);
                    AddCapabilities(Capability.Audio);
                    
                    break;
            }
            return Capabilities;
        }

        private void AddCapabilities(Capability cap)
        {
            
            if (!Capabilities.Contains(cap))
            {
                LOGGER.GetInstance.Info(String.Format("Add Capabilities: {0}", cap));
                Capabilities.Add(cap);
            }else
                LOGGER.GetInstance.Info(String.Format("Add Capabilities: {0} already have", cap));

        }
        public void RemoveCapabilities(Capability cap)
        {
            
            if (Capabilities.Contains(cap))
            {
                LOGGER.GetInstance.Info(String.Format("Remove Capabilities: {0}", cap));
                Capabilities.Remove(cap);
            }else
                LOGGER.GetInstance.Info(String.Format("Remove Capabilities: {0} already removed", cap));

        }
        private void PingBox()
        {
            using (Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                byte[] receive_buffer = new byte[100];
                IPAddress serverAddr = IPAddress.Parse(deviceIp);
                IPEndPoint endPoint = new IPEndPoint(serverAddr, 4567);
                byte[] send_buffer = Encoding.ASCII.GetBytes("ping\0");
                sock.SendTo(send_buffer, endPoint);
                try
                {
                    int l = sock.Receive(receive_buffer);
                }
                catch (SocketException e)
                {
                    int x = e.ErrorCode;
                }

                var str = System.Text.Encoding.Default.GetString(receive_buffer).Trim('\0');
                if (str.Equals("pong"))
                {
                    AddCapabilities(Capability.Box);
                }

            }

        }

    }
}