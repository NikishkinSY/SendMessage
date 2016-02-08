using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace SendMessage
{
    class SMSElement : ConfigurationElement
    {
        [ConfigurationProperty("PhoneNumber", DefaultValue = "+70000000000", IsRequired = true, IsKey = true)]
        [RegexStringValidator(@"\+(\d{11})")]
        public string PhoneNumber
        {
            get { return (string)this["PhoneNumber"]; }
            set { this["PhoneNumber"] = value; }
        }

        [ConfigurationProperty("Description")]
        public string Description
        {
            get { return (string)this["Description"]; }
            set { this["Description"] = value; }
        }
    }
}
