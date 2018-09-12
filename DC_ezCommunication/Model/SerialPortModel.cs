using System;
using System.IO.Ports;
using System.Threading;

namespace DC_ezCommunication.Model
{
    public delegate void SerialPortEventHandler(Object sender, SerialPortEventArgs e);
    public class SerialPortEventArgs : EventArgs
    {
        public bool isOpend = false;
        public Byte[] receivedBytes = null;
    }
    public class SerialPortModel
    {
        private SerialPort sp = new SerialPort();

        public event SerialPortEventHandler spReceiveDataEvent = null;
        public event SerialPortEventHandler spOpenEvent = null;
        public event SerialPortEventHandler spCloseEvent = null;

        private Object thisLock = new Object();

        /// <summary>
        /// When serial received data, will call this method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (sp.BytesToRead <= 0)
            {
                return;
            }
            lock (thisLock)
            {
                int len = sp.BytesToRead;
                Byte[] data = new Byte[len];
                try
                {
                    sp.Read(data, 0, len);
                }
                catch (System.Exception)
                {
                    //catch read exception
                }
                SerialPortEventArgs args = new SerialPortEventArgs();
                args.receivedBytes = data;
                if (spReceiveDataEvent != null)
                {
                    spReceiveDataEvent.Invoke(this, args);
                }
            }
        }
        /// <summary>
        /// Send bytes to device
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public bool Send(Byte[] bytes)
        {
            if (!sp.IsOpen)
            {
                return false;
            }

            try
            {
                sp.Write(bytes, 0, bytes.Length);
            }
            catch (System.Exception)
            {
                return false;   //write failed
            }
            return true;        //write successfully
        }

        /// <summary>
        /// Open Serial port
        /// </summary>
        /// <param name="portName"></param>
        /// <param name="baudRate"></param>
        /// <param name="dataBits"></param>
        /// <param name="stopBits"></param>
        /// <param name="parity"></param>
        /// <param name="handshake"></param>
        public void Open(string portName, String baudRate,
            string dataBits, string stopBits, string parity,
            string handshake)
        {
            if (sp.IsOpen)
            {
                Close();
            }
            sp.PortName = portName;
            sp.BaudRate = Convert.ToInt32(baudRate);
            sp.DataBits = Convert.ToInt16(dataBits);

            if (handshake == "None")
            {
                //Never delete this property
                sp.RtsEnable = true;
                sp.DtrEnable = true;
            }
            SerialPortEventArgs args = new SerialPortEventArgs();
            try
            {
                sp.StopBits = (StopBits)Enum.Parse(typeof(StopBits), stopBits);
                sp.Parity = (Parity)Enum.Parse(typeof(Parity), parity);
                sp.Handshake = (Handshake)Enum.Parse(typeof(Handshake), handshake);
                sp.WriteTimeout = 1000; /*Write time out*/
                sp.Open();
                sp.DataReceived += new SerialDataReceivedEventHandler(DataReceived);
                args.isOpend = true;
            }
            catch (System.Exception)
            {
                args.isOpend = false;
            }
            if (spOpenEvent != null)
            {
                spOpenEvent.Invoke(this, args);
            }
        }
        /// <summary>
        /// Close serial port
        /// </summary>
        public void Close()
        {
            Thread closeThread = new Thread(new ThreadStart(CloseSpThread));
            closeThread.Start();
        }
        /// <summary>
        /// Close serial port thread
        /// </summary>
        private void CloseSpThread()
        {
            SerialPortEventArgs args = new SerialPortEventArgs();
            args.isOpend = false;
            try
            {
                sp.Close(); //close the serial port
                sp.DataReceived -= new SerialDataReceivedEventHandler(DataReceived);
            }
            catch (Exception)
            {
                args.isOpend = true;
            }
            if (spCloseEvent != null)
            {
                spCloseEvent.Invoke(this, args);
            }
        }
    }
}