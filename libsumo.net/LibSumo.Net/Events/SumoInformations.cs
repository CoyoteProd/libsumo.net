using System;
using System.Collections.Generic;
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
        // If battery is leff than LowBatteryLevelAlert -> User can Take Action
        public const int LowBatteryLevelAlert = 10;
        public bool IsBatteryUnderLevelAlert
        {
            get
            {
                if (BatteryLevel == 0) return false;
                return BatteryLevel <= LowBatteryLevelAlert;
                
            }
        }

        public SumoEnumGenerated.WifiSelectionChanged_type WifiType { get; internal set; }
        public SumoEnumGenerated.WifiSelectionChanged_band WifiBand { get; internal set; }
        public byte WifiChannel { get; internal set; }
        public int AudioTheme { get; set; }

        public SumoInformations()
        {
            Capabilities = new List<Capability>();
            AddCapabilities(Capability.Jump); // Default all drone can Jump
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
                    AddCapabilities(Capability.Box); // TODO : Check if box is here...
                    
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

        public void AddCapabilities(Capability cap)
        {
            LOGGER.GetInstance.Info(String.Format("Add Capabilities: {0}", cap));
            Capabilities.Add(cap);

        }
        public void RemoveCapabilities(Capability cap)
        {
            LOGGER.GetInstance.Info(String.Format("Remove Capabilities: {0}", cap));
            Capabilities.Remove(cap);

        }


    }
}