using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SendMessage
{
    public interface INotice
    {
        string Message { get; set; }
        Contact Contact { get; set; }
        NoticeType Type { get; }
    }
    public class Notice: INotice
    {
        public string Message { get; set; }
        public Contact Contact { get; set; }
        public virtual NoticeType Type { get; }

        public override string ToString()
        {
            return String.Format("Type:{0}, Contact:({1}), Message:{2}", Type, Contact, Message);
        }
    }

    public class SMSNotice: Notice
    {
        public override NoticeType Type { get { return NoticeType.SMS; } }

        internal SMSNotice(string message, Contact contact)
        {
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
