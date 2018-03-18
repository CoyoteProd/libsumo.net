using LibSumo.Net.Hook;
using LibSumo.Net.Protocol;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace LibSumo.Net.Events
{ 
    public class ImageEventArgs : System.EventArgs
    {
        public OpenCvSharp.Mat RawImage { get; set; }
        public ImageEventArgs(OpenCvSharp.Mat RawImage)
        {
            this.RawImage = RawImage;

        }
    }

    public class MoveEventArgs : System.EventArgs
    {
        public sbyte Speed { get; set; }
        public sbyte Turn { get; set; }
        public MoveEventArgs(sbyte _speed, sbyte _turn)
        {
            this.Speed = _speed;
            this.Turn = _turn;

        }
    }

    public class KeyboardEventArgs : System.EventArgs
    {
        public HookUtils.VirtualKeyStates CurrentKey { get; set; }
        public bool IsPressed { get; set; }
        public KeyboardEventArgs(HookUtils.VirtualKeyStates _currentKey, bool _isPressed)
        {
            this.CurrentKey = _currentKey;
            this.IsPressed = _isPressed;

        }
    }
    

    /// <summary>
    /// General purpose Arguments for Sumo Changes
    /// </summary>
    public class SumoEventArgs : System.EventArgs
    {
        public SumoEnum.Posture Posture { get; set; }
        public int BatteryLevel { get; set; }
        public int Rssi { get; set; }
        public int LinkQuality { get; set; }
        public SumoEnum.BatteryAlert BatteryAlert { get; set; }
        public sbyte Speed { get; set; }
        public sbyte Turn { get; set; }

        public SumoEnum.TypeOfEvents TypeOfEvent { get; set; }
        public byte Volume { get; internal set; }

        public SumoEventArgs(SumoEnum.TypeOfEvents _typeOfEvents)
        {
            TypeOfEvent = _typeOfEvents;
        }

    }
}
