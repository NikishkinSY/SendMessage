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

        [ConfigurationProperty("InitATCommands")]
        public InitATCommandElementCollection InitATCommands { get { return (InitATCommandElementCollection)base["InitATCommands"]; } }

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

        [ConfigurationProperty("PeriodReboot", DefaultValue = "00:01:00")]
        [TimeSpanValidator(MinValueString = "00:01:00", MaxValueString = "356.00:00:00", ExcludeRange = false)]
        public TimeSpan PeriodReboot
        {
            get { return (TimeSpan)this["PeriodReboot"]; }
            set { this["PeriodReboot"] = value; }
        }

        [ConfigurationProperty("PeriodPing", DefaultValue = "00:10:00")]
        [TimeSpanValidator(MinValueString = "00:00:05", MaxValueString = "356.00:00:00", ExcludeRange = false)]
        public TimeSpan PeriodPing
        {
            get { return (TimeSpan)this["PeriodPing"]; }
            set { this["PeriodPing"] = value; }
        }

    }
}
