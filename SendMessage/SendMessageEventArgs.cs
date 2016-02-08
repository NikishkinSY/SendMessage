using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SendMessage
{
    public class SendMessageEventArgs : EventArgs
    {
        public EventType EventType { get; set; }
        public MessageType MessageType { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }

        public SendMessageEventArgs(EventType eventType, MessageType messageType, string message, object data)
        {
            this.EventType = eventType;
            this.MessageType = messageType;
            this.Message = message;
            this.Data = data;
        }
        public SendMessageEventArgs(EventType eventType, MessageType messageType, string message)
        {
            this.EventType = eventType;
            this.MessageType = messageType;
            this.Message = message;
        }

        public override string ToString()
        {
            var message = String.Format("EventType: {0}, MessageType:{1}, Message:\"{2}\"", EventType, MessageType, Message);
            message += Data != null ? String.Format(", Data:({0})", Data) : string.Empty;
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
