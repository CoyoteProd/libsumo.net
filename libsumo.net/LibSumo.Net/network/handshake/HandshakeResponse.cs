namespace LibSumo.Net.lib.network.handshake
{

	/// <summary>
	/// @author  Alexander Bischof
	/// </summary>
	public class HandshakeResponse
	{

		private string status;
		private int c2d_port;
		private int arstream_fragment_size;
		private int arstream_fragment_maximum_number;
		private int arstream_max_ack_interval;
		private int c2d_update_port;
		private int c2d_user_port;

		public virtual string Status
		{
			get
			{
    
				return status;
			}
			set
			{
    
				this.status = value;
			}
		}




		public virtual int C2d_port
		{
			get
			{
    
				return c2d_port;
			}
			set
			{
    
				this.c2d_port = value;
			}
		}




		public virtual int Arstream_fragment_size
		{
			get
			{
    
				return arstream_fragment_size;
			}
			set
			{
    
				this.arstream_fragment_size = value;
			}
		}




		public virtual int Arstream_max_ack_interval
		{
			get
			{
    
				return arstream_max_ack_interval;
			}
			set
			{
    
				this.arstream_max_ack_interval = value;
			}
		}




		public virtual int Arstream_fragment_maximum_number
		{
			get
			{
    
				return arstream_fragment_maximum_number;
			}
			set
			{
    
				this.arstream_fragment_maximum_number = value;
			}
		}




		public virtual int C2d_update_port
		{
			get
			{
    
				return c2d_update_port;
			}
			set
			{
    
				this.c2d_update_port = value;
			}
		}




		public virtual int C2d_user_port
		{
			get
			{
    
				return c2d_user_port;
			}
			set
			{
    
				this.c2d_user_port = value;
			}
		}




		public override string ToString()
		{

			return "DeviceAnswer{" + "status='" + status + '\'' + ", c2d_port=" + c2d_port + ", arstream_fragment_size=" + arstream_fragment_size + ", arstream_fragment_maximum_number=" + arstream_fragment_maximum_number + ", c2d_update_port=" + c2d_update_port + ", c2d_user_port=" + c2d_user_port + '}';
		}
	}

}