using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SendMessage
{
    public abstract class Receiver: IReceiver
    {
        public string Address { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return String.Format("Address:{0}, Description:{1}", Address, Description);
        }
    }
}
