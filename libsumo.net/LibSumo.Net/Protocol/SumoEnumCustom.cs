using System;
using System.Collections.Generic;
using System.Text;

namespace LibSumo.Net.Protocol
{
    public class SumoEnumCustom
    {
        public enum TypeOfEvents
        {
            PostureEvent,
            BatteryLevelEvent,
            AlertEvent,
            PilotingEvent,
            Connected,
            Disconnected,
            Discovered,
            RSSI,
            LinkQuality,
            VolumeChange,
            SpeedChange,
            CapabilitiesChange,
            WifiChanged,
            AudioThemeChanged,
            CSTMSupervisorStarted,
            BoxOpened,
            BoxError,
            BoxClosed
        }
    }
}
