using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SendMessage
{
    public abstract class Receiver: IReceiver
    {
        public string Description { get; set; }

        public override string ToString()
        {
            return String.Format("Description:{0}", Description);
        }
    }
}
