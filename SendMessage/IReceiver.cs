using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SendMessage
{
    public interface IReceiver
    {
        string Address { get; set; }
        string Description { get; set; }
    }
}
