using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace SendMessage
{
    class CallElement : ConfigurationElement
    {
        [ConfigurationProperty("PhoneNumber", DefaultValue = "+70000000000", IsRequired = true, IsKey = true)]
        [RegexStringValidator(@"\+(\d{11})")]
        public string PhoneNumber
        {
            get { return (string)this["PhoneNumber"]; }
            set { this["PhoneNumber"] = value; }
        }
    }
}
