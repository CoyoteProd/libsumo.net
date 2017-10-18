namespace LibSumo.Net.lib.network.handshake
{
	/// <summary>
	/// @author  Alexander Bischof
	/// </summary>
	public class HandshakeRequest
	{
		//private string controller_name;
		//private string controller_type;
		//private int d2c_port = 54321;

		public HandshakeRequest(string controller_name)
		{
			this.controller_name = controller_name;
			//this.controller_type = controller_type;
            this.controller_type = "._arsdk-0902._udp";
            this.d2c_port = 54321;
		}

		public virtual string controller_name{ get; set;}
		public virtual string controller_type{ get; set;}
		public int d2c_port { get; set;}

		public override string ToString()
		{
			return "DeviceInit{" + "controller_name='" + controller_name + '\'' + ", controller_type='" + controller_type + '\'' + ", d2c_port=" + d2c_port + '}';
		}
	}
}