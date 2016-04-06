namespace LibSumo.Net.lib.network.handshake
{

    using Newtonsoft.Json;
    using System;
    using System.Net.Sockets;
    using System.Text;
    //using ObjectMapper = com.fasterxml.jackson.databind.ObjectMapper;
    //using SerializationFeature = com.fasterxml.jackson.databind.SerializationFeature;



	/// <summary>
	/// @author  Alexander Bischof
	/// </summary>
	public class TcpHandshake : IDisposable
	{

        private readonly TcpClient tcpSocket;
		//private readonly PrintWriter tcpOut;
		//private readonly System.IO.StreamReader tcpIn;
        String deviceIp;
        int tcpPort;

		public TcpHandshake(string deviceIp, int tcpPort)
		{
            tcpSocket = new TcpClient();
            this.deviceIp = deviceIp;
            this.tcpPort = tcpPort;
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

            tcpSocket.Connect(deviceIp, tcpPort);
            NetworkStream nwStream = tcpSocket.GetStream();

            //Send to device
            byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(shakeData);
            nwStream.Write(bytesToSend, 0, bytesToSend.Length);

            //Reads json response

            byte[] bytesToRead = new byte[tcpSocket.ReceiveBufferSize];
            int bytesRead = nwStream.Read(bytesToRead, 0, tcpSocket.ReceiveBufferSize);
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