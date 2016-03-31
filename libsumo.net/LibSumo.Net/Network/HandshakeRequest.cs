using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LibSumoUni
{
    public class HandshakeRequest
    {
        
        public String controller_name { get; set; }        
        public String controller_type { get; set; }        
        public int d2c_port = 54321;

        public HandshakeRequest(String controller_name, String controller_type) {
            this.controller_name = controller_name;
            this.controller_type = controller_type;
        }

        public String toString() {
            return "DeviceInit{" +
                   "controller_name='" + controller_name + '\'' +
                   ", controller_type='" + controller_type + '\'' +
                   ", d2c_port=" + d2c_port +
                   '}';
        }
    }
}
