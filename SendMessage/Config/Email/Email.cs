using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace SendMessage
{
    class EmailElement : ConfigurationElement
    {
        [ConfigurationProperty("Mailboxes")]
        public MailboxElementCollection Mailboxes { get { return (MailboxElementCollection)base["Mailboxes"]; } }
        [ConfigurationProperty("Addresses")]
        public AddressElementCollection Emails { get { return (AddressElementCollection)base["Addresses"]; } }
    }

    class MailboxElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new MailboxElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((MailboxElement)element).From;
        }
    }

    class AddressElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new AddressElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((AddressElement)element).Address;
        }
    }
}
