using System;
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

        
    }
}