using LibSumo.Net.lib.command;
using LibSumo.Net.lib.command.animation;
using LibSumo.Net.lib.command.common;
using LibSumo.Net.lib.command.movement;
using LibSumo.Net.lib.command.multimedia;
using LibSumo.Net.lib.listener;
using LibSumo.Net.lib.network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSumo.Net
{
    public class DroneController
    {
        private static readonly log4net.ILog LOGGER = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private iDroneConnection droneConnection;

        public DroneController(iDroneConnection droneConnection)
        {
            LOGGER.Info("Creating DroneController");
            this.droneConnection = droneConnection;
            droneConnection.connect();
        }

        public void close()
        {
            droneConnection.disconnect();
        }

        /*
        public DroneController send(Command command)
        {
            this.droneConnection.sendCommand(command);
            return this;
        }
         */

        public DroneController pcmd(int speed, int degree)
        {
            this.droneConnection.sendCommand(Pcmd.pcmd(speed, degree));
            return this;
        }

        public DroneController forward()
        {
            pcmd(40, 0);
            return this;
        }

        public DroneController backward()
        {
            pcmd(-40, 0);
            return this;
        }

        public DroneController left()
        {
            left(90);
            return this;
        }

        public DroneController left(int degrees)
        {
            pcmd(0, -degrees);
            return this;
        }

        public DroneController right(int degrees)
        {
            pcmd(0, degrees);
            return this;
        }

        public DroneController right()
        {
            right(90);
            return this;
        }

        public DroneController forwardLeft()
        {
            pcmd(40, -90);
            return this;
        }

        public DroneController forwardRight()
        {
            pcmd(40, 90);
            return this;
        }

        public DroneController backwardLeft()
        {
            pcmd(-40, -90);
            return this;
        }

        public DroneController backwardRight()
        {
            pcmd(-40, 90);
            return this;
        }

        public DroneController jump(Jump.Type jumpType)
        {
            this.droneConnection.sendCommand(Jump.jump(jumpType));
            return this;
        }

        public DroneController jumpHigh()
        {
            this.droneConnection.sendCommand(Jump.jump(Jump.Type.High));
            return this;
        }

        public DroneController jumpLong()
        {
            this.droneConnection.sendCommand(Jump.jump(Jump.Type.Long));
            return this;
        }

        public DroneController stopAnimation()
        {
            this.droneConnection.sendCommand(StopAnimation.stopAnimation());
            return this;
        }

        public DroneController spin()
        {
            this.droneConnection.sendCommand(Spin.spin());
            return this;
        }

        public DroneController tap()
        {
            this.droneConnection.sendCommand(Tap.tap());
            return this;
        }

        public DroneController slowShake()
        {
            this.droneConnection.sendCommand(SlowShake.slowShake());
            return this;
        }

        public DroneController metronome()
        {
            this.droneConnection.sendCommand(Metronome.metronome());
            return this;
        }

        public DroneController ondulation()
        {
            this.droneConnection.sendCommand(Ondulation.ondulation());
            return this;
        }

        public DroneController spinJump()
        {
            this.droneConnection.sendCommand(SpinJump.spinJump());
            return this;
        }

        public DroneController spinToPosture()
        {
            this.droneConnection.sendCommand(SpinToPosture.spinToPosture());
            return this;
        }

        public DroneController spiral()
        {
            this.droneConnection.sendCommand(Spiral.spiral());
            return this;
        }

        public DroneController slalom()
        {
            this.droneConnection.sendCommand(Slalom.slalom());
            return this;
        }

        public DroneController addBatteryListener(Action<byte> consumer)
        {
            this.droneConnection.addEventListener(BatteryListener.batteryListener(consumer));
            return this;
        }

        public DroneController addPCMDListener(Action<String> consumer)
        {
            droneConnection.addEventListener(PCMDListener.pcmdlistener(consumer));
            return this;
        }

        public AudioController audio()
        {
            return new AudioController(this);
        }

        public VideoController video()
        {
            return new VideoController(this);
        }

        public class AudioController
        {
            private DroneController droneController;
            private iDroneConnection droneConnection;

            public AudioController(DroneController droneController)
            {
                this.droneController = droneController;
                this.droneConnection = droneController.droneConnection;
            }

            public AudioController theme(AudioTheme.Theme theme)
            {
                droneConnection.sendCommand(AudioTheme.audioTheme(theme));
                return this;
            }

            public AudioController volume(int volume)
            {
                droneConnection.sendCommand(Volume.volume(volume));
                return this;
            }

            public AudioController mute()
            {
                volume(0);
                return this;
            }

            public AudioController unmute()
            {
                volume(100);
                return this;
            }

            public DroneController drone()
            {
                return droneController;
            }
        }

        public class VideoController
        {
            private DroneController droneController;
            private iDroneConnection droneConnection;

            public VideoController(DroneController droneController)
            {
                this.droneController = droneController;
                this.droneConnection = droneController.droneConnection;
            }

            public VideoController enableVideo()
            {
                droneConnection.addEventListener(VideoListener.videoListener());
                droneConnection.sendCommand(VideoStreaming.enableVideoStreaming());
                return this;
            }

            public VideoController disableVideo()
            {
                droneConnection.sendCommand(VideoStreaming.disableVideoStreaming());
                return this;
            }

            public DroneController drone()
            {
                return droneController;
            }
        }
    }
}
