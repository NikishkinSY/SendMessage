using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SendMessage
{
    public abstract class Sender: ISender
    {
        public abstract string Name { get; }
        public abstract bool SendMessage(Notice notice);
        public abstract void Start();
        public abstract bool Stop();

        public override string ToString()
        {
            return String.Format("{0}", Name);
        }
    }
}
