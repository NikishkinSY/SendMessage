using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace SendMessage
{
    class GSMElement : ConfigurationElement
    {
        [ConfigurationProperty("Modems")]
        public ModemElementCollection Modems { get { return (ModemElementCollection)base["Modems"]; } }
        [ConfigurationProperty("SMSes")]
        public SMSElementCollection SMSes { get { return (SMSElementCollection)base["SMSes"]; } }
        [ConfigurationProperty("Calls")]
        public SMSElementCollection Calls { get { return (SMSElementCollection)base["Calls"]; } }
    }

    class ModemElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ModemElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ModemElement)element).PortName;
        }
    }

    class SMSElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new SMSElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((SMSElement)element).PhoneNumber;
        }
    }

    class CallElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new CallElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((CallElement)element).PhoneNumber;
        }
    }
}
