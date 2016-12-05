using LibSumo.Net.Command.Interfaces;
using LibSumo.Net.Network;
namespace LibSumo.Net.lib.command.common
{


	/// <summary>
    /// 

    //0  4  7   8  9
    //---------
    //   x  0   3  1 - unknown
    //   x  0   5  0 - unknown
    //  12  5,  1  0 - Battery -> data[11] ist immer 0 - wird alle 10 sequencen gesendet | [4, 126, 73, 12, 0, 0, 0, 0, 5, 1, 0, 50]
    //  15  3   1  1 - unknown -> data[11] ist immer 1
    //   0  3   3  0 - unknown -> data[11] ist immer 1
    //  15  3   3  2 - unknown -> data[11] ist immer 0
    //  15  3  19  0 - unknown -> data[11] ist 1 oder 0
    // Receiving Packet: [4, 126, 8, 21, 0, 0, 0, 0, 5, 2, 0, 0, 105, 110, 116, 101, 114, 110, 97, 108, 0]

	/// </summary>
    public class Pong : iCommonCommand
	{

        private readonly int sequenceNumberToAck;
        private readonly PacketType packetType = PacketType.ACK;

        public Pong(int sequenceNumberToAck)
		{

            this.sequenceNumberToAck = sequenceNumberToAck;
		}

		public static Pong pong(int counter)
		{

			return new Pong(counter);
		}


		public byte[] getBytes(int counter)
		{
            return new byte[] { (byte) packetType, unchecked((byte)0xfe), (byte)this.sequenceNumberToAck, 8, 0, 0, 0, (byte)this.sequenceNumberToAck };
		}

	

		public override string ToString()
		{

			return "Pong";
		}

        


        public Network.PacketType getPacketType()
        {
            return packetType;
        }
    }

}