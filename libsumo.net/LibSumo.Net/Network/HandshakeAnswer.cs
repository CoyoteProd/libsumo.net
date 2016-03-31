using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSumoUni
{
    class HandshakeAnswer
    {
        private String status;
        private int c2d_port;
        private int arstream_fragment_size;
        private int arstream_fragment_maximum_number;
        private int arstream_max_ack_interval;
        private int c2d_update_port;
        private int c2d_user_port;

        public String getStatus() {
            return status;
        }

        public void setStatus(String status) {
            this.status = status;
        }

        public int getC2d_port() {
            return c2d_port;
        }

        public void setC2d_port(int c2d_port) {
            this.c2d_port = c2d_port;
        }

        public int getArstream_fragment_size() {
            return arstream_fragment_size;
        }

        public void setArstream_fragment_size(int arstream_fragment_size) {
            this.arstream_fragment_size = arstream_fragment_size;
        }

        public int getArstream_max_ack_interval() {
            return arstream_max_ack_interval;
        }

        public void setArstream_max_ack_interval(int arstream_max_ack_interval) {
            this.arstream_max_ack_interval = arstream_max_ack_interval;
        }

        public int getArstream_fragment_maximum_number() {
            return arstream_fragment_maximum_number;
        }

        public void setArstream_fragment_maximum_number(int arstream_fragment_maximum_number) {
            this.arstream_fragment_maximum_number = arstream_fragment_maximum_number;
        }

        public int getC2d_update_port() {
            return c2d_update_port;
        }

        public void setC2d_update_port(int c2d_update_port) {
            this.c2d_update_port = c2d_update_port;
        }

        public int getC2d_user_port() {
            return c2d_user_port;
        }

        public void setC2d_user_port(int c2d_user_port) {
            this.c2d_user_port = c2d_user_port;
        }

        
        public String toString() {
            return "DeviceAnswer{" +
                   "status='" + status + '\'' +
                   ", c2d_port=" + c2d_port +
                   ", arstream_fragment_size=" + arstream_fragment_size +
                   ", arstream_fragment_maximum_number=" + arstream_fragment_maximum_number +
                   ", c2d_update_port=" + c2d_update_port +
                   ", c2d_user_port=" + c2d_user_port +
                   '}';
        }
    }
}
