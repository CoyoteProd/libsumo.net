using LibSumo.Net.Network;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibSumo.Net.Streams
{
    class SumoAudioRecorder
    {
        // private object mRecorder;
        // private bool mIsRecording = false;
        // private bool mReleased = false;
        private WaveIn waveSource = null;
        WaveFileWriter wfw;
        private SumoSender sumoSender;
        public SumoAudioRecorder(SumoSender _sender)
        {
            sumoSender = _sender;
            waveSource = new WaveIn(WaveCallbackInfo.FunctionCallback())
            {
                WaveFormat = new WaveFormat(8000, 16, 1),
                NumberOfBuffers = 1,
                BufferMilliseconds = 50
            };
            waveSource.DataAvailable += WaveSource_DataAvailable;
        }

        private void WaveSource_DataAvailable(object sender, WaveInEventArgs e)
        {
            // TODOD : Chek if data is larger than
            // Audio Frame = HEADER_SIZE = 16 + DATA_SIZE = 256 = 0x110 (272 byte)
            // Max 256 byte            
            sumoSender.SendAudioFrame(e.Buffer);           
        }
        

        /// <summary>
        /// Start Streaming to the Drone
        /// </summary>
        /// <param name="AudioPath">Path to the wav file to play</param>
        internal void Start(string AudioPath)
        {
            //mIsRecording = true;
            wfw = new WaveFileWriter(AudioPath, waveSource.WaveFormat);
            waveSource.StartRecording();
        }

        internal void Stop()
        {
            //mIsRecording = false;
            waveSource.StopRecording();
        }

        internal void Release()
        {
            Stop();                        
            waveSource.Dispose();
            //mReleased = true;            
        }


    }
}
