using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SendMessage
{
    public interface ISender
    {
        bool SendMessage(Notice notice);
        void Start();
        bool Stop();
    }
}
