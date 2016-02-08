using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SendMessage
{
    public class ReceiveData
    {
        public virtual byte[] Bytes { get; set; }

        public virtual string String
        {
            get
            {
                if (this.Bytes != null)
                    return Encoding.UTF8.GetString(this.Bytes);
                else
                    return string.Empty;
            }
        }

        public virtual string HexBytes
        {
            get
            {
                if (this.Bytes != null)
                    return HexEncoding.BytesToHexString(this.Bytes);
                else
                    return string.Empty;
            }
        }

        public ReceiveData()
        {
        }
        public ReceiveData(byte[] Bytes)
        {
            this.Bytes = Bytes;
        }

        public override string ToString()
        {
            return String.Format("Data:{0}", HexBytes);
        }
    }

    public class ReceiveATResponse : ReceiveData
    {
        public virtual ATResponse ATResponse { get; set; }

        public ReceiveATResponse(byte[] Bytes, ATResponse atResponse)
        {
            this.ATResponse = atResponse;
            this.Bytes = Bytes;
        }

        public override string ToString()
        {
            return String.Format("Data:{0}, AT:{1}", HexBytes, ATResponse);
        }
    }
}
