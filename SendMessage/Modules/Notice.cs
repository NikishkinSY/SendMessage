using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SendMessage
{
    public interface INotice
    {
        DateTime DateTime { get; set; }
        string Message { get; set; }
        Contact Contact { get; set; }
        NoticeType Type { get; }
    }
    public class Notice: INotice
    {
        public DateTime DateTime { get; set; }
        public string Message { get; set; }
        public Contact Contact { get; set; }
        public virtual NoticeType Type { get; }

        public override string ToString()
        {
            return String.Format("{0} {1} ({2}) \"{3}\"", DateTime.ToShortTimeString(), Type, Contact, Message);
        }
    }

    public class SMSNotice: Notice
    {
        public override NoticeType Type { get { return NoticeType.SMS; } }

        internal SMSNotice(string message, Contact contact)
        {
            this.DateTime = DateTime.Now;
            this.Message = message;
            this.Contact = contact;
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
        Email,
        Call
    }
}
