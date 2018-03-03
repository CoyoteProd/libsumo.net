using System;
using System.Collections.Generic;
using System.Text;

namespace LibSumo.Net.Protocol
{
    public class SumoEnum
    {
        /// <summary>
        /// Jumping
        /// </summary>
        public enum Jump
        {
            /// <summary>
            ///  perform a Long jump;
            /// </summary>
            LongJump,
            /// <summary>
            ///  perform a high jump;
            /// </summary>
            HighJump
        }

        /// <summary>
        /// 
        /// </summary>
        public enum Posture
        {
            /// <summary>
            /// Auto-Balance Mode
            /// </summary>
            standing,
            /// <summary>
            /// Normal rolling mode
            /// </summary>
            jumper,
            /// <summary>
            /// Ready to Push Object
            /// </summary>
            kicker,
            Stuck,
            Unknown
        };

        /// <summary>
        /// Pre-programmed acrobatics
        /// </summary>
        public enum Animation
        {
            stop,
            spin,
            tap,
            slowshake,
            metronome,
            oudulation,
            spinjump,
            spintoposture,
            spiral,
            slalom
        }

        public enum BatteryAlert
        {
            NoAlert,
            CriticalBatteryAlert,
            LowBatteryAlert
        }
        public enum MediaStreamingState
        {
            enabled,
            disabled,
            error
        }

        /// <summary>
        /// State of jump load
        /// </summary>
        public enum JumpLoad
        {
            /// <summary>
            /// Unknown state(obsolete).
            /// </summary>
            unknown,

            /// <summary>
            /// Unloaded state.
            /// </summary>
            unloaded,

            /// <summary>
            /// Loaded state.
            /// </summary>
            loaded,

            /// <summary>
            /// Unknown state(obsolete).
            /// </summary>
            busy,

            /// <summary>
            /// Unloaded state and low battery.
            /// </summary>
            low_battery_unloaded,

            /// <summary>
            /// Loaded state and low battery.
            /// </summary>
            low_battery_loaded
        }
        /// <summary>
        /// State about the jump motor problem
        /// Enum describing the problem of the motor
        /// </summary>
        public enum MotorProblem
        {            			                  
			none,
            //Motor blocked					
            blocked,
            //	Motor over heated
            over_heated
				
        }

        public enum TypeOfEvents
        {
            PostureEvent,
            BatteryLevelEvent, 
            BatteryAlertEvent,
            PilotingEvent,
            Connected,
            Disconnected,
            Discovered,
            RSSI
        }
    }
}
