using LibSumo.Net.Events;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LibSumo.Net.Streams
{
    class SumoAudioPlayer
    {
        private bool IsConnected { get; set; }
        private bool mPlaying = false;       
        private WaveOut mAudioTrack;
        private BufferedWaveProvider buffer;


        public SumoAudioPlayer()
        {            
            var waveFormat = new WaveFormat(8000, 16, 1);
            buffer = new BufferedWaveProvider(waveFormat)
            {
                BufferDuration = TimeSpan.FromSeconds(10),
                DiscardOnBufferOverflow = true
            };
        }

        internal void Start()
        {
            if (!mPlaying)
            {
                mPlaying = true;
                if (mAudioTrack != null)
                {
                    mAudioTrack.Play();
                }
            }
        }

        internal void Stop()
        {
            if (mPlaying)
            {
                mPlaying = false;
                if (mAudioTrack != null)
                {
                    mAudioTrack.Pause();                    
                }
            }
        }

        internal void ConfigureCodec()
        {                   
            mAudioTrack = new WaveOut(WaveCallbackInfo.FunctionCallback());
            mAudioTrack.Init(buffer);
            if (mPlaying)
            {
                mAudioTrack.Play();
            }            
        }
                      

        internal void Disconnect()
        {
            mAudioTrack.Stop();
            mPlaying = false;                        
            IsConnected = false;
        }

        internal void OnDataReceived(byte[] currentFrame)
        {
            if (mPlaying && mAudioTrack != null)
            {               
                buffer.AddSamples(currentFrame, 0, currentFrame.Length);
            }
        }
        
    }
}
