using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace SendMessage
{
    class AddressElement : ConfigurationElement
    {
        [ConfigurationProperty("Address", DefaultValue = "1@yandex.ru", IsRequired = true, IsKey = true)]
        [RegexStringValidator(@"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z")]
        public string Address
        {
            get { return (string)this["Address"]; }
            set { this["Address"] = value; }
        }
    }
}
