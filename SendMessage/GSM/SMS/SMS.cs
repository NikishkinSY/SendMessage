using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SendMessage
{
    public class SMS: Receiver
    {
        public string PhoneNumber
        {
            get { return Address; }
            set { Address = value; }
        }   
    }
}
