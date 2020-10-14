using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Airsoft_Majaky
{
    public class RemoteControl
    {
        public string ID { get; set; }
        public TcpClient Client { get; set; }
        public RemoteControl(string _id, TcpClient _client)
        {
            ID = _id;
            Client = _client;
        }
    }
}
