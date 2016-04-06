namespace LibSumo.Net.lib.network.handshake
{

	/// <summary>
	/// @author  Alexander Bischof
	/// </summary>
	public class HandshakeRequest
	{

		private string controller_name;
		private string controller_type;
		private int d2c_port = 54321;

		public HandshakeRequest(string controller_name, string controller_type)
		{

			this.controller_name = controller_name;
			this.controller_type = controller_type;
		}

		public virtual string Controller_name
		{
			get
			{
    
				return controller_name;
			}
			set
			{
    
				this.controller_name = value;
			}
		}




		public virtual string Controller_type
		{
			get
			{
    
				return controller_type;
			}
			set
			{
    
				this.controller_type = value;
			}
		}




		public virtual int D2c_port
		{
			get
			{
    
				return d2c_port;
			}
			set
			{
    
				this.d2c_port = value;
			}
		}




		public override string ToString()
		{

			return "DeviceInit{" + "controller_name='" + controller_name + '\'' + ", controller_type='" + controller_type + '\'' + ", d2c_port=" + d2c_port + '}';
		}
	}

}