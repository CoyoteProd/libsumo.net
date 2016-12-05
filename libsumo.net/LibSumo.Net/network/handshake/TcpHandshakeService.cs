namespace LibSumo.Net.lib.network.handshake
{

    using Newtonsoft.Json;
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    //using ObjectMapper = com.fasterxml.jackson.databind.ObjectMapper;
    //using SerializationFeature = com.fasterxml.jackson.databind.SerializationFeature;



	/// <summary>
	/// @author  Alexander Bischof
	/// </summary>
	public class TcpHandshakeService : IDisposable
	{

        //private readonly TcpClient tcpSocket;
        private Socket tcpSocket;
        private IPEndPoint remoteEP;
		//private readonly PrintWriter tcpOut;
		//private readonly System.IO.StreamReader tcpIn;
        String deviceIp;
        int tcpPort;

		public TcpHandshakeService(string deviceIp, int tcpPort)
		{
            //tcpSocket = new TcpClient();
            tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.deviceIp = deviceIp;
            this.tcpPort = tcpPort;
            IPAddress ip = IPAddress.Parse(deviceIp);
            remoteEP = new IPEndPoint( ip, tcpPort);

			//tcpSocket = new Socket(deviceIp, tcpPort);
			//tcpOut = new PrintWriter(tcpSocket.OutputStream, true);
			//tcpIn = new System.IO.StreamReader(tcpSocket.InputStream);
		}

		public virtual HandshakeResponse shake(HandshakeRequest handshakeRequest)
		{

            String str = JsonConvert.SerializeObject(handshakeRequest);
            return tcpHandshakeResult(str);
		}


		private HandshakeResponse tcpHandshakeResult(string shakeData)
		{

            tcpSocket.Connect(remoteEP);
            //tcpSocket.Connect(deviceIp, tcpPort);
            //NetworkStream nwStream = tcpSocket.GetStream();

            //Send to device            
            byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(shakeData);
            tcpSocket.Send(bytesToSend);
            Thread.Sleep(500);
            //nwStream.Write(bytesToSend, 0, bytesToSend.Length);

            //Reads json response

            byte[] bytesToRead = new byte[tcpSocket.ReceiveBufferSize];
            int bytesRead = tcpSocket.Receive(bytesToRead);
            
            //int bytesRead = nwStream.Read(bytesToRead, 0, tcpSocket.ReceiveBufferSize);
            String responseLine = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);

            HandshakeResponse deviceAnswer = JsonConvert.DeserializeObject<HandshakeResponse>(responseLine);
            tcpSocket.Close();
            return deviceAnswer;
		}

		public void close()
		{			
			tcpSocket.Close();			
		}

        public void Dispose()
        {
            close();
        }
    }

}