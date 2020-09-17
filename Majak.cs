using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Airsoft_Majaky
{
    public class Majak
    {
        public TcpClient Client { get; set; }
        public int ID { get; set; }
        public bool isConnected { get; set; }
        private string color;
        public string Color//N = neutral (nothing is lit up) B = Blue R = Read, A = all (both colors are light up)
        {
            get
            {
                if(MainWindow.isGameRunning == 0)
                {
                    color = "N";
                    return "N";
                }
                else if(MainWindow.isGameRunning == 2)
                {
                    return "A";
                }
                else
                {
                    return color;
                }
            }
            set
            {
                if (value == "B" || value.Contains("B"))
                    color = "B";
                else if (value == "R" || value.Contains("R"))
                    color = "R";
                else if (value == "N" || value.Contains("N"))
                    color = "N";
                else if (value == "A" || value.Contains("A"))
                    color = "A";
                if(color == "R")
                {
                    Red_StopWatch.Start();
                    Blue_StopWatch.Stop();
                }
                else if(color == "B")
                {
                    Red_StopWatch.Stop();
                    Blue_StopWatch.Start();
                }
                else if(color == "N")
                {
                    Red_StopWatch.Stop();
                    Red_StopWatch.Stop();
                }
            }
        }
        public string MACAddress { get; set; }
        public Stopwatch Red_StopWatch { get; set; }
        public Stopwatch Blue_StopWatch { get; set; }
        public double BlueTimeInSeconds { get; set; }
        public double RedTimeInSeconds { get; set; }
        public Majak(TcpClient _client)
        {
            Client = _client;
            isConnected = true;
            Red_StopWatch = new Stopwatch();
            Blue_StopWatch = new Stopwatch();
            Color = "N";
            BlueTimeInSeconds = 0;
            RedTimeInSeconds = 0;
        }
        public void StopwatchStop()
        {
            Red_StopWatch.Stop();
            Blue_StopWatch.Stop();
        }
        public void StopwatchResume()
        {
            if(Color == "R")
            {
                Red_StopWatch.Start();
                Blue_StopWatch.Stop();
            }
            else if(Color == "B")
            {
                Blue_StopWatch.Start();
                Red_StopWatch.Stop();
            }
            else if (Color == "N")
            {
                Blue_StopWatch.Stop();
                Red_StopWatch.Stop();
            }
        }
        public void StopwatchReset()
        {
            Red_StopWatch.Reset();
            Blue_StopWatch.Reset();
            BlueTimeInSeconds = 0;
            RedTimeInSeconds = 0;
        }
    }
}
