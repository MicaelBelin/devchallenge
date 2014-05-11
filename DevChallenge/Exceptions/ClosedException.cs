using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevChallenge
{
    public class ClosedException : Exception
    {
        private static string StdErrMsg = "Connection was closed";
        public ClosedException()
            : base(StdErrMsg)
        {
        }

        public ClosedException(Exception inner)
            : base(StdErrMsg, inner)
        {
        }
    }
}
