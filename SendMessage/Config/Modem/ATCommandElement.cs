using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace SendMessage
{
    class InitATCommandElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ATCommandElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ATCommandElement)element).ATRequest;
        }
    }

    class ATCommandElement : ConfigurationElement
    {
        [ConfigurationProperty("ATRequest", DefaultValue = "AT", IsRequired = true, IsKey = true)]
        public string ATRequest
        {
            get { return (string)this["ATRequest"]; }
            set { this["ATRequest"] = value; }
        }

        [ConfigurationProperty("ExpectedATResponse", DefaultValue = ATResponse.OK, IsRequired = true)]
        public ATResponse ExpectedATResponse
        {
            get { return (ATResponse)this["ExpectedATResponse"]; }
            set { this["ExpectedATResponse"] = value; }
        }

        [ConfigurationProperty("TimesToRepeat", DefaultValue = 1)]
        [IntegerValidator(MinValue = 1, MaxValue = 100, ExcludeRange = false)]
        public int TimesToRepeat
        {
            get { return (int)this["TimesToRepeat"]; }
            set { this["TimesToRepeat"] = value; }
        }

        [ConfigurationProperty("WaitForResponse", DefaultValue = "00:00:10")]
        [TimeSpanValidator(MinValueString = "00:00:00", MaxValueString = "00:00:10", ExcludeRange = false)]
        public TimeSpan WaitForResponse
        {
            get { return (TimeSpan)this["WaitForResponse"]; }
            set { this["WaitForResponse"] = value; }
        }
    }
}
