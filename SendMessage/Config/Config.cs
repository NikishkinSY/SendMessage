using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace SendMessage
{
    class SendMessageConfig : ConfigurationSection
    {
        static SendMessageConfig settings;
        internal static SendMessageConfig Settings { get { return settings; } }

        internal static void Init()
        {
            SendMessageConfig.settings = ConfigurationManager.GetSection("SendMessage") as SendMessageConfig;
        }

        [ConfigurationProperty("Modems")]
        public ModemElementCollection Modems { get { return (ModemElementCollection)base["Modems"]; } }
        [ConfigurationProperty("Mailboxes")]
        public MailboxElementCollection Mailboxes { get { return (MailboxElementCollection)base["Mailboxes"]; } }
        [ConfigurationProperty("Contacts")]
        public ContactElementCollection Contacts { get { return (ContactElementCollection)base["Contacts"]; } }
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

    class ContactElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ContactElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ContactElement)element).Description;
        }
    }

}
