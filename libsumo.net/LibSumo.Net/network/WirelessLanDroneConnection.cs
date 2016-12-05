using System.Collections.Generic;
using System.Threading;

namespace LibSumo.Net.lib.network
{

	//using Command = LibSumo.Net.lib.command.Command;
	using CommandException = LibSumo.Net.lib.command.CommandException;
	//using CommonCommand = LibSumo.Net.lib.command.common.CommonCommand;
	using CurrentDate = LibSumo.Net.lib.command.common.CurrentDate;
	using CurrentTime = LibSumo.Net.lib.command.common.CurrentTime;
	using Pong = LibSumo.Net.lib.command.common.Pong;
	using EventListener = LibSumo.Net.lib.listener.iEventListener;
	using HandshakeRequest = LibSumo.Net.lib.network.handshake.HandshakeRequest;
	using HandshakeResponse = LibSumo.Net.lib.network.handshake.HandshakeResponse;
	using TcpHandshake = LibSumo.Net.lib.network.handshake.TcpHandshakeService;
    using System.Collections.Concurrent;
    using System;
    using LibSumo.Net.lib.command;
    using System.IO;
    using System.Net.Sockets;
    using System.Net;
    using LibSumo.Net.lib.network.handshake;
    using LibSumo.Net.Network;
    using LibSumo.Net.lib.command.movement;
    using LibSumo.Net.Command.Interfaces;






	/// <summary>
	/// Represents the queue wireless lan connection to the drone.
	/// 
	/// @author  Tobias Schneider
	/// </summary>
	public class WirelessLanDroneConnection : iDroneConnection
	{
	
		private const string CONTROLLER_TYPE = "_arsdk-0902._udp";
        
        private static readonly log4net.ILog LOGGER = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly BlockingCollection<iCommand> commonCommandQueue = new BlockingCollection<iCommand>(25);
        private readonly BlockingCollection<iCommand> commandQueue = new BlockingCollection<iCommand>(25);
		private readonly List<EventListener> eventListeners = new List<EventListener>();

		private readonly string deviceIp;
		private readonly int tcpPort;
		private readonly string wirelessLanName;
        
        private bool listenToResponse = true;
        private bool sendCommands = true;

		private int devicePort;
		//private byte noAckCounter = 0;
		//private byte ackCounter = 0;

        private IPEndPoint SumoRemote;
        private byte[] nextSequenceNumbers;

        private int maxWaitingTime = 100; // at least every x ms a command is sent to the drone



		public WirelessLanDroneConnection(string deviceIp, int tcpPort, string wirelessLanName)
		{

			LOGGER.Info(String.Format("Creating " + this.GetType().Name + " for {0}:{0}...", deviceIp, tcpPort));

			this.deviceIp = deviceIp;
			this.tcpPort = tcpPort;
			this.wirelessLanName = wirelessLanName;

            //this.clock = Clock.systemDefaultZone();

            nextSequenceNumbers = new byte[2];
			
		}

		public void connect()
		{

			LOGGER.Info("Connecting to drone...");
            listenToResponse = true;
			//HandshakeRequest handshakeRequest = new HandshakeRequest(wirelessLanName, CONTROLLER_TYPE);
            //TcpHandshake DeviceShake = new TcpHandshake(deviceIp, tcpPort);
            //HandshakeResponse handshakeResponse = DeviceShake.shake(handshakeRequest);
            
            HandshakeResponse handshakeResponse;

            try
            {
                handshakeResponse = new TcpHandshakeService(deviceIp, tcpPort).shake(new HandshakeRequest(wirelessLanName));
                devicePort = handshakeResponse.C2d_port;
                SumoRemote = new IPEndPoint(IPAddress.Parse(deviceIp), devicePort);

                LOGGER.Info(String.Format("Connected to drone - Handshake completed with {0}", handshakeResponse));

            }
            catch (IOException e)
            {                               
                //throw new ConnectionException("Error while trying to connect to the drone - check your connection", e);
            }

			

			sendCommand(CurrentDate.currentDate());
			sendCommand(CurrentTime.currentTime());

			runResponseHandler();
            runConsumer(commandQueue, true); // only run heartbeat here
            runConsumer(commonCommandQueue, false); // don't use long running split commands here.
		}

        public void disconnect()
        {
            LOGGER.Debug("Disconnecting from drone...");
            listenToResponse = false;
            sendCommands = false;
            LOGGER.Debug("Disconnected from drone");
        }

		public void sendCommand(iCommand command)
		{
			try
			{
                int waitingTime = command.waitingTime();
				if (command is Pcmd)
				{
                    LOGGER.Debug(String.Format("putting Pcmd's into queue {0} for time {1}", command, waitingTime));
                    while (waitingTime > 0)
                    { // generate enough Pcmds for the expected waiting time                      
                        iCommand newCommand = ((Pcmd)command).clone((waitingTime > maxWaitingTime) ? maxWaitingTime : waitingTime);
                        commonCommandQueue.Add(newCommand);
                        waitingTime = waitingTime - maxWaitingTime;
                    }
				}
				else
				{
					
                    LOGGER.Debug(String.Format("putting command into queue {0}", command));
                    if (command is iCommonCommand) 
                    {
                        //commonCommandQueue.offer(command);
                        commonCommandQueue.Add(command);
                    } else 
                    {
                        commandQueue.Add(command);
                        //commandQueue.offer(command);
                    }

				}
			}
			catch (Exception e)
			{
				LOGGER.Info("Could not add " + command + " to a queue. " + e.Message);
			}
		}


		public virtual void addEventListener(EventListener eventListener)
		{

			this.eventListeners.Add(eventListener);
		}


		/// <summary>
		/// Drone response handler.
		/// 
		/// <para>Will listen to the udp packages send from the drone to the receiver and looks up what command it is and react
		/// to it</para>
		/// </summary>
		private void runResponseHandler()
		{

			new Thread(() =>
			{
						// Answer with a Pong
				try
				{
					using (var udpClient = new UdpClient(devicePort))
					{
						LOGGER.Info(String.Format("Listing for response on port %s", devicePort));
						int pingCounter = 0;
						while (listenToResponse) 
					    {							                                                       
							byte[] data = udpClient.Receive(ref SumoRemote);;

							if (data[1] == 126)
							{
								LOGGER.Info("Ping");
								sendCommand(Pong.pong(data[3]));
								if (pingCounter > 10 && pingCounter % 10 == 1)
								{
									sendCommand(CurrentDate.currentDate());
									sendCommand(CurrentTime.currentTime());
								}
								pingCounter++;
								continue;
							}
							eventListeners.ForEach(eventListener => eventListener.eventFired(data));
						}
					}
				}
				catch (IOException)
				{
					LOGGER.Warn("Error occurred while receiving packets from the drone.");
				}
			}).Start();
		}


        private void runConsumer(BlockingCollection<iCommand> queue, bool heartbeat)
		{

			LOGGER.Info("Creating a specific command queue consumer...");

			new Thread(() =>
			{
				try
				{
                    using (var sumoSocket = new UdpClient())
					{
                        while (sendCommands)
						{
							try
							{
								iCommand command = queue.Take();
                                if (command != null)
                                {
                                    int cnt = getNextSequenceNumber(command);
                                    byte[] packet = command.getBytes(cnt);
                                    sumoSocket.Send(packet, packet.Length, SumoRemote);
                                    LOGGER.Info(String.Format("Sending command: {0}", command));
                                    Thread.Sleep(command.waitingTime());
                                }
                                else  // send "null" PCmd (0,0) =  ("go by 0 speed") when queue is empty
                                {
                                    if (heartbeat)
                                    {
                                        Pcmd nullCommand = Pcmd.pcmd(0, 0, maxWaitingTime);
                                        byte[] nullMovePacket = nullCommand.getBytes(getNextSequenceNumber(nullCommand));
                                        //LOGGER.Debug("empty queue,  sending null command '{}' with packet {}", nullCommand, convertAndCutPacket(nullMovePacket, false));
                                        sumoSocket.Send(nullMovePacket, nullMovePacket.Length, SumoRemote);
                                    }
                                    Thread.Sleep(maxWaitingTime);
                                    //MILLISECONDS.sleep(maxWaitingTime);
                                }
							}
							catch (Exception e)
							{
								throw new CommandException("Got interrupted while taking a command", e);
							}
						}
					}
				}
				catch (IOException)
				{
					LOGGER.Warn("Error occurred while sending packets to the drone.");
				}
			}).Start();
		}

        private int getNextSequenceNumber(iCommand command) 
        {

            byte packetTypeId = (byte)command.getPacketType();    
            byte nextSequenceNumber = nextSequenceNumbers[packetTypeId];
            nextSequenceNumbers[packetTypeId] = (byte) (nextSequenceNumber + 1);
    
            return nextSequenceNumber;
        }

     

        	
	}

}