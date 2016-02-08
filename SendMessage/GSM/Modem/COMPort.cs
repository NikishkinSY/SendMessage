using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SendMessage
{
    public class COMPort
    {
        SerialPort Port { get; set; }
        public bool IsOpen { get { return (Port != null ? Port.IsOpen : false); } }
        internal event EventHandler<DataEventArgs> OnDataReceived;
        void RiseEvent(object sender, DataEventArgs e)
        { if (OnDataReceived != null) OnDataReceived(sender, e); }

        public string PortName
        {
            get { return Port.PortName; }
            set { Port.PortName = value; }
        }
        public int BaudRate
        {
            get { return Port.BaudRate; }
            set { Port.BaudRate = value; }
        }
        public Parity Parity
        {
            get { return Port.Parity; }
            set { Port.Parity = value; }
        }
        public int DataBits
        {
            get { return Port.DataBits; }
            set { Port.DataBits = value; }
        }
        public StopBits StopBits
        {
            get { return Port.StopBits; }
            set { Port.StopBits = value; }
        }
        public Encoding Encoding
        {
            get { return Port.Encoding; }
            set { Port.Encoding = value; }
        }
        public TimeSpan MaxPauseInIncomingData { get; set; }

        #region locking Objects
        private object lockOpenCloseSend = new object();
        #endregion

        void DefaultValues()
        {
            this.Port = new SerialPort();
            //выключаем проверку четности
            this.Port.Parity = Parity.None;
            //размерность пакета
            this.Port.DataBits = 8;
            //кол-во стоповых битов в байте
            this.Port.StopBits = StopBits.One;
            //протокол обмена
            this.Port.Handshake = Handshake.None;
            this.Port.ReadTimeout = 5000;
            this.Port.WriteTimeout = 5000;
            this.Port.DataReceived += Port_DataReceived;
            this.Port.NewLine = '\r'.ToString();
            this.Port.Encoding = Encoding.UTF8;
            this.MaxPauseInIncomingData = new TimeSpan(0,0,0,0,300);
        }

        public COMPort()
        {
            DefaultValues();
        }

        public COMPort(string portName, int baudRate)
        {
            DefaultValues();
            this.Port.PortName = portName;
            this.Port.BaudRate = baudRate;
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] buffer = ReceiveData((SerialPort)sender);
            RiseEvent(this, new DataEventArgs(buffer));
        }

        public virtual byte[] ReceiveData(SerialPort Port)
        {
            List<byte> Bytes = new List<byte>();
            int CountTimes = (int)Math.Round(
                1000 / MaxPauseInIncomingData.TotalMilliseconds);
            int MaxDelays = CountTimes > 0 ? CountTimes : 1;
            while (IsOpen && Port.BytesToRead > 0 && MaxDelays-- > 0)
            {
                byte[] buffer = new byte[Port.BytesToRead];
                Port.Read(buffer, 0, buffer.Length);
                Bytes.AddRange(buffer);
                Thread.Sleep(MaxPauseInIncomingData); // 1.2мс для получения одного байта данных при скорости 9600кб/с
            }
            return Bytes.ToArray();
        }

        public void Write(string command, bool addCr)
        {
            lock (lockOpenCloseSend)
            {
                if (addCr)
                    Port.WriteLine(command);
                else
                    Port.Write(command);
            }
        }

        public void Open()
        {
            lock (lockOpenCloseSend)
            {
                Port.Open();
            }
        }
        
        public void Close()
        {
            lock (lockOpenCloseSend)
            {
                Port.Close();
            }
        }

        public static List<byte[]> BytesSplit(byte[] Source, params byte[] delimeter)
        {
            List<byte[]> Parts = new List<byte[]>();
            List<byte> Part = new List<byte>();

            if (Source != null)
            {
                foreach (byte Byte in Source)
                {
                    if (delimeter != null && delimeter.Count(s => s == Byte) > 0)
                    {
                        if (Part.Count > 0)
                        {
                            Parts.Add(Part.ToArray());
                            Part.Clear();
                        }
                    }
                    else
                        Part.Add(Byte);
                }

                if (Part.Count > 0)
                    Parts.Add(Part.ToArray());
            }

            return Parts;
        }

        public override string ToString()
        {
            return String.Format("{0}:{1}", PortName, BaudRate);
        }

    }

    public class DataEventArgs : EventArgs
    {
        byte[] Data;
        public DataEventArgs(byte[] data)
        {
            this.Data = data;
        }
        public byte[] GetData()
        {
            return Data;
        }
    }
}
