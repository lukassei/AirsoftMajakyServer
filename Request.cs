using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Airsoft_Majaky
{
    public class Request
    {
        public int TypeOfResponse { get; set; }
        public string Data { get; set; }
        public Majak Sender { get; set; }

        public Request(int r, Majak m)
        {
            TypeOfResponse = r;
            Sender = m;
        }
        public Request(int r)
        {
            TypeOfResponse = r;
            Sender = null;
        }
        public Request(string d, Majak m)
        {
            Data = d;
            Sender = m;
        }
    }
}
