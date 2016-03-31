using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


namespace LibSumoUni
{
    class TcpHandshake
    {
        private TcpClient tcpSocket;
        //DataWriter writer;
        String deviceIp;
        int tcpPort;

        //private PrintWriter tcpOut;
        //private BufferedReader tcpIn;

        public TcpHandshake(String deviceIp, int tcpPort) 
        {
            tcpSocket = new TcpClient();
            this.deviceIp = deviceIp;
            this.tcpPort = tcpPort;

            //tcpOut = new PrintWriter(tcpSocket.getOutputStream(), true);
            //tcpIn = new BufferedReader(new InputStreamReader(tcpSocket.getInputStream()));            
        }

        public HandshakeAnswer shake(HandshakeRequest handshakeRequest)  
        {
            //ObjectMapper objectMapper = new ObjectMapper();
            //objectMapper.configure(SerializationFeature.INDENT_OUTPUT, true);
            //StringWriter shakeData = new StringWriter();
            //objectMapper.writeValue(shakeData, handshakeRequest);
            //return tcpHandshakeResult(shakeData.toString());
            String str = JsonConvert.SerializeObject(handshakeRequest);
            return tcpHandshakeResult(str);
        }

        private HandshakeAnswer tcpHandshakeResult(String shakeData) //
        {

            tcpSocket.Connect(deviceIp, tcpPort);
            NetworkStream nwStream = tcpSocket.GetStream();

            //Send to device
            byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(shakeData);
            nwStream.Write(bytesToSend, 0, bytesToSend.Length);
            
            //tcpSocket.send(shakeData);
            //tcpOut.println(shakeData);

            //Reads json response

            byte[] bytesToRead = new byte[tcpSocket.ReceiveBufferSize];
            int bytesRead = nwStream.Read(bytesToRead, 0, tcpSocket.ReceiveBufferSize);
            String responseLine = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
            
            //HandshakeAnswer deviceAnswer = null;
            //ObjectMapper objectMapper = new ObjectMapper();
            //while ((responseLine = tcpIn.readLine()) != null) 
            //{
                //responseLine = responseLine.substring(0, responseLine.lastIndexOf("}") + 1);
            HandshakeAnswer deviceAnswer = JsonConvert.DeserializeObject<HandshakeAnswer>(responseLine);
                //deviceAnswer = objectMapper.readValue(responseLine, HandshakeAnswer.class);
            //}
            tcpSocket.Close();
            return deviceAnswer;
        }

      
        //public void close()  
        //{
        //    tcpOut.close();
        //    tcpSocket.close();
        //    tcpIn.close();
        //}
    }
}
