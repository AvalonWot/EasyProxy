using System;
using System.Collections.Generic;
using System.Text;

namespace EasyProxy
{
    public class ProxyException : Exception
    {
        public ProxyException(string msg, Exception innter = null) : base(msg, innter)
        {

        }
    }
}
