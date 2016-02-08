using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SendMessage
{
    public class Contact: Receiver
    {
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public bool IsEnabledCall { get; set; }

        public override string ToString()
        {
            return String.Format("Description:{0}, Address:{1}, Email:{2}, IsEnabledCall:{3}", Description, PhoneNumber, Email, IsEnabledCall);
        }
    }
}
