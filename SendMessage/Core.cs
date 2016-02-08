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
        static ConcurrentBag<Modem> Modems { get; set; }
        static ConcurrentBag<SMS> SMSes { get; set; }
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
            SMSes = new ConcurrentBag<SMS>();
            SMSNotices = new ConcurrentQueue<SMSNotice>();

            AutoMapper.Mapper.CreateMap<Modem, ModemElement>();
            AutoMapper.Mapper.CreateMap<COMPort, COMPortElement>();
            AutoMapper.Mapper.CreateMap<ModemElement, Modem>();
            AutoMapper.Mapper.CreateMap<COMPortElement, COMPort>();
            AutoMapper.Mapper.CreateMap<SMS, SMSElement>();
            AutoMapper.Mapper.CreateMap<SMSElement, SMS>();
        }

        static void Init()
        {
            //init config
            SendMessageConfig.Init();
            var modems = AutoMapper.Mapper.Map<IEnumerable<ModemElement>, List<Modem>>(SendMessageConfig.Settings.GSM.Modems.Cast<ModemElement>());
            var smses = AutoMapper.Mapper.Map<IEnumerable<SMSElement>, List<SMS>>(SendMessageConfig.Settings.GSM.SMSes.Cast<SMSElement>());
            foreach (Modem modem in modems)
            {
                //add event handler
                modem.OnEvent += OnEvent;
                Modems.Add(modem);                
            }
            foreach (SMS sms in smses)
                SMSes.Add(sms);
            //add mailboxes
            //add emails
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
        }
        /// <summary>
        /// Send message
        /// </summary>
        /// <param name="message">message to sent</param>
        /// <returns>collection notices which must be perfomed</returns>
        public static IEnumerable<Notice> SendMessage(string message)
        {
            List<Notice> notices = new List<Notice>();
            foreach (SMS sms in SMSes)
            {
                SMSNotice smsNotice = new SMSNotice(message, sms);
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
