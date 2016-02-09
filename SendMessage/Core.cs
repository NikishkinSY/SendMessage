using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SendMessage
{
    public static class Core
    {
        internal static ConcurrentBag<Modem> Modems { get; set; }
        //static ConcurrentBag<MailBox> MailBoxs { get; set; }
        internal static ConcurrentBag<Contact> Contacts { get; set; }
        //add calls
        /// <summary>
        /// All event from send messgae service
        /// </summary>
        public static event EventHandler<SendMessageEventArgs> SendMessageEvent;
        static void RiseEvent(object sender, SendMessageEventArgs e)
        { if (SendMessageEvent != null) SendMessageEvent(sender, e); }

        static ConcurrentQueue<SMSNotice> SMSNotices { get; set; }
        static bool cancellationTokenProccessSMSNotices { get; set; }
        static Task taskProccesSMSNotices { get; set; }

        static Core()
        {
            Modems = new ConcurrentBag<Modem>();
            Contacts = new ConcurrentBag<Contact>();
            SMSNotices = new ConcurrentQueue<SMSNotice>();

            AutoMapper.Mapper.CreateMap<Modem, ModemElement>();
            AutoMapper.Mapper.CreateMap<COMPort, COMPortElement>();
            AutoMapper.Mapper.CreateMap<ModemElement, Modem>();
            AutoMapper.Mapper.CreateMap<COMPortElement, COMPort>();
            AutoMapper.Mapper.CreateMap<Contact, ContactElement>();
            AutoMapper.Mapper.CreateMap<ContactElement, Contact>();
            AutoMapper.Mapper.CreateMap<ATCommand, ATCommandElement>();
            AutoMapper.Mapper.CreateMap<ATCommandElement, ATCommand>();
        }

        static void Init()
        {
            //init config
            SendMessageConfig.Init();
            var modems = AutoMapper.Mapper.Map<IEnumerable<ModemElement>, List<Modem>>(SendMessageConfig.Settings.Modems.Cast<ModemElement>());
            var contacts = AutoMapper.Mapper.Map<IEnumerable<ContactElement>, List<Contact>>(SendMessageConfig.Settings.Contacts.Cast<ContactElement>());
            foreach (Modem modem in modems)
            {
                //add event handler
                modem.OnEvent += OnEvent;
                Modems.Add(modem);                
            }
            //add mailboxes
            foreach (Contact contact in contacts)
                Contacts.Add(contact);
            
        }

        private static void OnEvent(object sender, SendMessageEventArgs e)
        {
            RiseEvent(sender, e);
        }
        /// <summary>
        /// Start service send message
        /// </summary>
        public static void Start()
        {
            //init core
            Core.Init();
            Scheduler.StartScheluder();
            //start gsmModems
            foreach (Modem gsmModem in Modems)
                gsmModem.Start();
            //start task proccess queue 
            ProccessMessagesAsync();
        }
        /// <summary>
        /// Stop service send message
        /// </summary>
        public static void Stop()
        {
            cancellationTokenProccessSMSNotices = true;
            foreach (Modem gsmModem in Modems)
                gsmModem.Stop();
            Scheduler.ShutdownScheluder(false);
        }
        /// <summary>
        /// Send message
        /// </summary>
        /// <param name="message">message to sent</param>
        /// <returns>collection notices which must be perfomed</returns>
        public static IEnumerable<Notice> SendMessage(string message)
        {
            List<Notice> notices = new List<Notice>();
            foreach (Contact contact in Contacts)
            {
                SMSNotice smsNotice = new SMSNotice(message, contact);
                notices.Add(smsNotice);
                SMSNotices.Enqueue(smsNotice);
            }

            //add EmailNotice
            return notices;
        }

        static void ProccessMessagesAsync()
        {
            if (taskProccesSMSNotices == null || taskProccesSMSNotices.Status == TaskStatus.Running)
            {
                taskProccesSMSNotices = new Task(() =>
                {
                    cancellationTokenProccessSMSNotices = false;
                    while (true)
                    {
                        if (!SMSNotices.IsEmpty)
                        {
                            foreach (Modem gsmModem in Modems.Where(s => s.IsFree).ToList())
                            {
                                SMSNotice smsNotice;
                                if (SMSNotices.TryDequeue(out smsNotice))
                                    gsmModem.SendMessage(smsNotice);
                            }
                            Thread.Sleep(1000);
                        }
                        if (cancellationTokenProccessSMSNotices)
                            break;
                    }
                });
                taskProccesSMSNotices.Start();
            }
        }
    }
}
