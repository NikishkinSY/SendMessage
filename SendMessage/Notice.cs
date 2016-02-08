using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SendMessage
{
    internal interface INotice
    {
        string Message { get; set; }
        IReceiver Receiver { get; set; }
        NoticeType Type { get; }
    }
    public class Notice: INotice
    {
        public string Message { get; set; }
        public IReceiver Receiver { get; set; }
        public virtual NoticeType Type { get; }

        public override string ToString()
        {
            return String.Format("Type:{0}, Receiver:({1}), Message:{2}", Type, Receiver, Message);
        }
    }

    public class SMSNotice: Notice
    {
        public override NoticeType Type { get { return NoticeType.SMS; } }

        internal SMSNotice(string message, SMS sms)
        {
            this.Message = message;
            this.Receiver = sms;
        }
    }
    //public class EmailNotice : Notice
    //{
    //    public override NoticeType Type { get { return NoticeType.Email; } }

    //    internal EmailNotice(string message, E address)
    //    {
    //        this.Message = message;
    //        this.Address = address;
    //    }
    //}

    public enum NoticeType
    {
        SMS,
        Email
    }
}
