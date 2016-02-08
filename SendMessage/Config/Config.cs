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

        [ConfigurationProperty("GSM")]
        public GSMElement GSM { get { return (GSMElement)base["GSM"]; } }
        [ConfigurationProperty("Email")]
        public EmailElement Email { get { return (EmailElement)base["Email"]; } }
    }


    
}
