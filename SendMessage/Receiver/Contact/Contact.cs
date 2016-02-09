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
            return String.Format("{0} {1} {2} {3}", Description, PhoneNumber, Email, IsEnabledCall);
        }
    }
}
