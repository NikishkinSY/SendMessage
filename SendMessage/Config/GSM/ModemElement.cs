using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace SendMessage
{
    class ModemElement : ConfigurationElement
    {
        [ConfigurationProperty("COMPort")]
        public COMPortElement COMPort { get { return (COMPortElement)base["COMPort"]; } }
        
        //id
        public string PortName { get { return COMPort.PortName; } }

        [ConfigurationProperty("PauseBetweenOpen", DefaultValue = "00:00:10")]
        [TimeSpanValidator(MinValueString = "00:00:01", MaxValueString = "00:30:00", ExcludeRange = false)]
        public TimeSpan PauseBetweenOpen
        {
            get { return (TimeSpan)this["PauseBetweenOpen"]; }
            set { this["PauseBetweenOpen"] = value; }
        }

        [ConfigurationProperty("TimesToRepeatATCommand", DefaultValue = 1)]
        [IntegerValidator(MinValue = 1, MaxValue = 100, ExcludeRange = false)]
        public int TimesToRepeatATCommand
        {
            get { return (int)this["TimesToRepeatATCommand"]; }
            set { this["TimesToRepeatATCommand"] = value; }
        }

        [ConfigurationProperty("WaitForATCommand", DefaultValue = "00:00:10")]
        [TimeSpanValidator(MinValueString = "00:00:00", MaxValueString = "00:00:10", ExcludeRange = false)]
        public TimeSpan WaitForATCommand
        {
            get { return (TimeSpan)this["WaitForATCommand"]; }
            set { this["WaitForATCommand"] = value; }
        }

        [ConfigurationProperty("InitATCommands", DefaultValue = "AT+CMGF=1;")]
        public string InitATCommands
        {
            get { return (string)this["InitATCommands"]; }
            set { this["InitATCommands"] = value; }
        }
    }
}
