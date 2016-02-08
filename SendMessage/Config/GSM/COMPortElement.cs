using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace SendMessage
{
    public class COMPortElement: ConfigurationElement
    {
        [ConfigurationProperty("PortName", DefaultValue = "COM1", IsRequired = true, IsKey = true)]
        [RegexStringValidator(@"COM\d+")]
        public string PortName
        {
            get { return (string)this["PortName"]; }
            set { this["PortName"] = value; }
        }

        [ConfigurationProperty("BaudRate", DefaultValue = 9600, IsRequired = true)]
        [IntegerValidator(MinValue = 1200, MaxValue = 115200, ExcludeRange = false)]
        public int BaudRate
        {
            get { return (int)this["BaudRate"]; }
            set { this["BaudRate"] = value; }
        }

        [ConfigurationProperty("Parity", DefaultValue = System.IO.Ports.Parity.None)]
        public System.IO.Ports.Parity Parity
        {
            get { return (System.IO.Ports.Parity)this["Parity"]; }
            set { this["Parity"] = value; }
        }

        [ConfigurationProperty("DataBits", DefaultValue = 8)]
        [IntegerValidator(MinValue = 5, MaxValue = 8, ExcludeRange = false)]
        public int DataBits
        {
            get { return (int)this["DataBits"]; }
            set { this["DataBits"] = value; }
        }

        [ConfigurationProperty("StopBits", DefaultValue = System.IO.Ports.StopBits.One)]
        public System.IO.Ports.StopBits StopBits
        {
            get { return (System.IO.Ports.StopBits)this["StopBits"]; }
            set { this["StopBits"] = value; }
        }

        [ConfigurationProperty("Handshake", DefaultValue = System.IO.Ports.Handshake.None)]
        public System.IO.Ports.Handshake Handshake
        {
            get { return (System.IO.Ports.Handshake)this["Handshake"]; }
            set { this["Handshake"] = value; }
        }

        //[ConfigurationProperty("Encoding", DefaultValue = Encoding.UTF8)]
        //public Encoding Encoding
        //{
        //    get { return (Encoding)this["Encoding"]; }
        //    set { this["Encoding"] = value; }
        //}

        [ConfigurationProperty("MaxPauseInIncomingData", DefaultValue = "00:00:00.300")]
        [TimeSpanValidator(MinValueString = "00:00:00", MaxValueString = "00:00:10", ExcludeRange = false)]
        public TimeSpan MaxPauseInIncomingData
        {
            get { return (TimeSpan)this["MaxPauseInIncomingData"]; }
            set { this["MaxPauseInIncomingData"] = value; }
        }


    }
}
