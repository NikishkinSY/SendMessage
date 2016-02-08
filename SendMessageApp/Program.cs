using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SendMessage;
using System.Configuration;
using System.Collections.Concurrent;

namespace SendMessageApp
{
    class Program
    {
        static ConcurrentBag<Notice> notices = new ConcurrentBag<Notice>();
        static void Main(string[] args)
        {
            
            Core.SendMessageEvent += Core_SendMessageEvent;
            Core.Start();
            foreach (Notice notice in Core.SendMessage("OPC BOOM!"))
                notices.Add(notice);
            //Core.SendMessage("OPC BOOM!");
            //Core.SendMessage("SPD BOOM!");
            //Core.SendMessage("VS BOOM!");
            //Core.SendMessage("WEB BOOM!");
            //Core.SendMessage("Zulu BOOM!");
            //Core.SendMessage("DB BOOM!");
            //Core.SendMessage("SNMP BOOM!");
            //Core.SendMessage("ZigBee BOOM!");
            //Core.SendMessage("Navigator BOOM!");
            Console.ReadKey();
            Core.Stop();
        }

        private static void Core_SendMessageEvent(object sender, SendMessageEventArgs e)
        {
            if (e.MessageType == MessageType.SMS)
            {
                Notice notice = (Notice)e.Data;
                if (notices.TryTake(out notice))
                {
                    Console.WriteLine(String.Format("SMS has sent ({0})", notice));
                }
            }
            else
                Console.WriteLine(e);
        }
    }
}
