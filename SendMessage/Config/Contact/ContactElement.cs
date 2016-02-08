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

        [ConfigurationProperty("PhoneNumber", DefaultValue = "+79000000000")]
        [RegexStringValidator(@"\+(\d{11})")]
        public string PhoneNumber
        {
            get { return (string)this["PhoneNumber"]; }
            set { this["PhoneNumber"] = value; }
        }

        [ConfigurationProperty("Email", DefaultValue = "test@test.ru")]
        [RegexStringValidator(@"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z")]
        public string Email
        {
            get { return (string)this["Email"]; }
            set { this["Email"] = value; }
        }

        [ConfigurationProperty("IsEnabledCall", DefaultValue = false)]
        public bool IsEnabledCall
        {
            get { return (bool)this["IsEnabledCall"]; }
            set { this["IsEnabledCall"] = value; }
        }
    }
}
