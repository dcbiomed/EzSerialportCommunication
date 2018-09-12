using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DC_ezCommunication.Model
{
    class SerialPortModel
    {
        public delegate void SerialPortEventHandler(Object sender, SerialPortEventArgs e);
    }

    public class SerialPortEventArgs : EventArgs
    {
        public bool isOpend = false;
        public Byte[] receiveBytes = null;
    }
}
