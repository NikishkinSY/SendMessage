using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SendMessage
{
    public class SendMessageEventArgs : EventArgs
    {
        public DateTime DateTime { get; set; }
        public EventType EventType { get; set; }
        public MessageType MessageType { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }

        public SendMessageEventArgs(EventType eventType, MessageType messageType, string message, object data)
        {
            this.DateTime = DateTime.Now;
            this.EventType = eventType;
            this.MessageType = messageType;
            this.Message = message;
            this.Data = data;
        }
        public SendMessageEventArgs(EventType eventType, MessageType messageType, string message)
        {
            this.DateTime = DateTime.Now;
            this.EventType = eventType;
            this.MessageType = messageType;
            this.Message = message;
        }

        public override string ToString()
        {
            var message = String.Format("{0} {1} {2} \"{3}\"", DateTime, EventType, MessageType, Message);
            message += Data != null ? String.Format(" ({0})", Data) : string.Empty;
            return message;

        }
    }

    public enum MessageType
    {
        Debug,
        Note,
        Warning,
        Error,
        AT,
        SMS,
        Email,
        Call,
        Data
    }

    public enum EventType
    {
        Tx,
        Rx,
        Sys
    }
}
