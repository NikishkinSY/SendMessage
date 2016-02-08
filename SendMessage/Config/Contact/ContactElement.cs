using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace SendMessage
{
    class ContactElement : ConfigurationElement
    {
        [ConfigurationProperty("Description", DefaultValue = "User1", IsRequired = true, IsKey = true)]
        public string Description
        {
            get { return (string)this["Description"]; }
            set { this["Description"] = value; }
        }

        [ConfigurationProperty("PhoneNumber")]
        [RegexStringValidator(@"\+(\d{11})")]
        public string PhoneNumber
        {
            get { return (string)this["PhoneNumber"]; }
            set { this["PhoneNumber"] = value; }
        }

        [ConfigurationProperty("Email")]
        [RegexStringValidator(@"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z")]
        public string Email
        {
            get { return (string)this["Email"]; }
            set { this["Email"] = value; }
        }
    }
}
