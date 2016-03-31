using LibSumoUni.Command;
using LibSumoUni.Network;
using LibSumoUni.Network.Command;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


namespace LibSumoUni
{
    public class DroneController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private UdpClient controller2DeviceSocket;
        private String deviceIp;
        private int tcpPort;
        private int controller2DevicePort;

        private byte nonackCounter = 0;
        private byte ackCounter = 0;
        private List<EventListener> eventListeners = new List<EventListener>();

        //private Set<String> loggingSet = new HashSet<>();

        // event
        public event EventHandler evtImageReady;

        public class ImageReadyEventArgs : EventArgs
        {
            public Image img { get; set; }            
        }

        protected virtual void OnImageReady(ImageReadyEventArgs e)
        {
            EventHandler handler = evtImageReady;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public DroneController(String deviceIp, HandshakeRequest handshakeRequest)
	    {
            this.tcpPort = 44444;
            this.deviceIp = deviceIp;
            log.Info(String.Format("Creating DroneController for {0}:{1}...", deviceIp, tcpPort));
            
            HandshakeAnswer handshakeAnswer = new TcpHandshake(deviceIp, tcpPort).shake(handshakeRequest);
            log.Info(String.Format("Handshake completed with {0}", handshakeAnswer));
            controller2DevicePort = handshakeAnswer.getC2d_port();

            // Establish remote UDP socket            
            controller2DeviceSocket = new UdpClient(deviceIp, controller2DevicePort);
                        

            // Start listner Thread                             
            StartUDPListener(controller2DevicePort);
            log.Info(String.Format("Listing for answers on port {0}", controller2DevicePort));

            sendCommand(CurrentDate.currentDate().getBytes(ackCounter++));
            sendCommand(CurrentTime.currentTime().getBytes(ackCounter++));
            sendCommand(VideoStreaming.videoStreamingEnable().getBytes(ackCounter++));

            //addAnswerSocket();
            //ThreadPool.QueueUserWorkItem(new DroneController().addAnswerSocket);

	    }

        private void StartUDPListener(int port)
        {
            Task.Run(async () =>
            {
                using (var udpClient = new UdpClient(port))
                {                    
                    while (true)
                    {
                        //IPEndPoint object will allow us to read datagrams sent from any source.
                        var receivedResults = await udpClient.ReceiveAsync();
                        byte[] data = receivedResults.Buffer;
                        
                        if (data[1] == 126)
                        {
                            sendCommand(Pong.pong().getBytes(data[3]));
                            return;
                        }

                        if (data[1] == 125)
                        {
                            // videoStream.write();
                            try
                            {   // TODO : byte to jpeg to UI
                                using (Image image = Image.FromStream(new MemoryStream(data)))
                                {
                                    ImageReadyEventArgs arg = new ImageReadyEventArgs();
                                    arg.img = image;
                                    OnImageReady(arg);
                                }

                                //FileOutputStream fos = new FileOutputStream("video.jpeg");
                                //fos.write(getJpegDate(data));
                            }
                            catch
                            { }
                            return;
                        }

                        //FIXME
                        CommandReader commandReader = CommandReader.commandReader(data);
                        if (commandReader.isPing() || commandReader.isLinkQualityChanged() ||
                                commandReader.isWifiSignalChanged())
                        {
                            return;
                        }

                        //System.out.println("---" + Arrays.toString(data));
                        //logIncoming(data);

                        //eventListeners.FindAll(eventListener->eventListener.eventFired(data));
                    }
                }
            });
        }
              

        //private void logIncoming(byte[] data) {

        //    bool add = loggingSet.add(Arrays.toString(data));
        //    if (add) {
        //        for (int i = 0; i < 10; i++) {
        //            System.out.print(data[i] + " ");
        //        }
        //        System.out.println();
        //    }
        //}

    
        public void close()  {
            sendCommand(Disconnect.disconnect().getBytes(ackCounter++));
            //controller2DeviceSocket.close();
        }

        public void sendCommand(byte[] packetAsBytes)
        {
            
            //DataWriter writer = new DataWriter(controller2DeviceSocket.OutputStream); 
            //DatagramPacket packet = new DatagramPacket(packetAsBytes, packetAsBytes.length, getByName(deviceIp), controller2DevicePort);

            log.Info(String.Format("Sending command: {0}", packetAsBytes.ToString()));
            controller2DeviceSocket.Send(packetAsBytes, packetAsBytes.Length);
            //writer.WriteBytes(packetAsBytes);
            //await writer.StoreAsync();
            //controller2DeviceSocket.send(packet);
            
        }


        public DroneController forward()  {
            this.sendCommand(Pcmd.pcmd(40, 0).getBytes(++nonackCounter));
            return this;
        }

        public DroneController backward()  {
            this.sendCommand(Pcmd.pcmd(-40, 0).getBytes(++nonackCounter));
            return this;
        }

        public DroneController left()  {
            this.sendCommand(Pcmd.pcmd(0, -25).getBytes(++nonackCounter));
            return this;
        }

        public DroneController left(int degrees)  {
            degrees = degrees % 180;
            this.sendCommand(Pcmd.pcmd(0, -25 * degrees / 90).getBytes(++nonackCounter));
            return this;
        }

        public DroneController right(int degrees)  {
            degrees = degrees % 180;
            this.sendCommand(Pcmd.pcmd(0, 25 * degrees / 90).getBytes(++nonackCounter));
            return this;
        }

        public DroneController right()  {
            this.sendCommand(Pcmd.pcmd(0, 25).getBytes(++nonackCounter));
            return this;
        }

        public DroneController pcmd(int speed, int turn)  {
            this.sendCommand(Pcmd.pcmd(speed, turn).getBytes(++nonackCounter));
            return this;
        }

        public DroneController jump(bool high)  {
            this.sendCommand(Jump.jump(high ? Jump.Type.High : Jump.Type.Long).getBytes(++ackCounter));
            return this;
        }

        //public DroneController stopAnnimation()  {
        //    this.sendCommand(StopAnimation.stopAnimation().getBytes(++ackCounter));
        //    return this;
        //}

        //public DroneController spin()  {
        //    this.sendCommand(Spin.spin().getBytes(++ackCounter));
        //    return this;
        //}

        //public DroneController tap()  {
        //    this.sendCommand(Tap.tap().getBytes(++ackCounter));
        //    return this;
        //}

        //public DroneController slowshake()  {
        //    this.sendCommand(SlowShake.slowShake().getBytes(++ackCounter));
        //    return this;
        //}

        //public DroneController metronome()  {
        //    this.sendCommand(Metronome.metronome().getBytes(++ackCounter));
        //    return this;
        //}

        //public DroneController ondulation()  {
        //    this.sendCommand(Ondulation.ondulation().getBytes(++ackCounter));
        //    return this;
        //}

        //public DroneController spinjump()  {
        //    this.sendCommand(SpinJump.spinJump().getBytes(++ackCounter));
        //    return this;
        //}

        //public DroneController spintoposture()  {
        //    this.sendCommand(SpinToPosture.spinToPosture().getBytes(++ackCounter));
        //    return this;
        //}

        //public DroneController spiral()  {
        //    this.sendCommand(Spiral.spiral().getBytes(++ackCounter));
        //    return this;
        //}

        //public DroneController slalom()  {
        //    this.sendCommand(Slalom.slalom().getBytes(++ackCounter));
        //    return this;
        //}

        //public AudioController audio() {
        //    return new AudioController();
        //}


        //public DroneController addCriticalBatteryListener(Consumer<BatteryState> consumer) {
        //    this.eventListeners.Add(data -> {
        //        if (filterProject(data, 3, 1, 1)) {
        //            consumer.accept(BatteryState.values()[data[11]]);
        //        }
        //    });
        //    return this;
        //}

        //public DroneController addBatteryListener(Consumer<Byte> consumer) {
        //    this.eventListeners.Add(data -> {
        //        if (filterProject(data, 0, 5, 1)) {
        //            consumer.accept(data[11]);
        //        }
        //    });
        //    return this;
        //}

        //public DroneController addPCMDListener(Consumer<String> consumer) {
        //    this.eventListeners.Add(data -> {
        //        if (filterProject(data, 3, 1, 0)) {
        //            consumer.accept("" + data[11]);
        //        }
        //    });
        //    return this;
        //}

        //public DroneController addOutdoorSpeedListener(Consumer<String> consumer) {
        //    this.eventListeners.Add(data -> {
        //        if (filterProject(data, 3, 17, 0)) {
        //            consumer.accept("" + data[11]);
        //        }
        //    });
        //    return this;
        //}

        private bool filterChannel(byte[] data, int frametype, int channel) {
            return data[0] == frametype && data[1] == channel;
        }

        private bool filterProject(byte[] data, int project, int clazz, int cmd) {
            return data[7] == project && data[8] == clazz && data[9] == cmd;
        }

        private byte[] getJpegDate(byte[] data) {
            byte[] jpegData = new byte[data.Length];
            Array.Copy(data, 12, jpegData, 0, data.Length - 12);
            return jpegData;
        }

        //public class AudioController {
        //    public AudioController robotTheme()  {
        //        sendCommand(AudioTheme.audioTheme(1).getBytes(++ackCounter));
        //        return this;
        //    }

        //    public AudioController insectTheme()  {
        //        sendCommand(AudioTheme.audioTheme(2).getBytes(++ackCounter));
        //        return this;
        //    }

        //    public AudioController monsterTheme()  {
        //        sendCommand(AudioTheme.audioTheme(3).getBytes(++ackCounter));
        //        return this;
        //    }

        //    public AudioController mute()  {
        //        sendCommand(Volume.volume(0).getBytes(++ackCounter));
        //        return this;
        //    }

        //    public AudioController unmute()  {
        //        sendCommand(Volume.volume(100).getBytes(++ackCounter));
        //        return this;
        //    }
        //}
    }
}
