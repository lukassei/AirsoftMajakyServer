using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Diagnostics;
using System.Windows.Controls;
using System.Collections.Concurrent;
using System.IO.Ports;

namespace Airsoft_Majaky
{
    public class Comunication
    {
        TcpListener server = null;
        SerialPort COM = null;
        public static volatile List<Majak> activeClients;
        private int IDForNextNewConnection = 1;
        private static volatile ConcurrentQueue<Request> _requestsToEvaluation;
        private static ConcurrentQueue<Request> RequestsToEvaluation
        {
            get
            {
                return _requestsToEvaluation;
            }
            set
            {
                _requestsToEvaluation = value;
            }
        }
        private static volatile ConcurrentQueue<Request> _requestsToRespond;
        private static ConcurrentQueue<Request> RequestsToRespond
        {
            get
            {
                return _requestsToRespond;
            }
            set
            {
                _requestsToRespond = value;
            }
        }
        public ConcurrentBag<Request> RequestToEval;
        public bool isConOk { get; set; }

        public Comunication(string ip, int port)
        {
            activeClients = new List<Majak>();
            RequestsToEvaluation = new ConcurrentQueue<Request>();
            RequestsToRespond = new ConcurrentQueue<Request>();
            
            StartListeningOnCOMPort();
            //IPAddress localAddr = IPAddress.Parse(ip);   -- Will be used later for Android clients (not needed now for test purposes)
            //server = new TcpListener(localAddr, port);
            //server.Start();
            //StartConnectionCheck();
            StartRequestCleaner();
            //StartListener();
        }
        public void StartListeningOnCOMPort()
        {
            bool isFine = false;
            while(!isFine)
            {
                try
                {
                    COM = new SerialPort();
                    COM.DtrEnable = true;  // <<< For Leonardo
                    COM.RtsEnable = true;  // <<< For Leonardo
                    COM.DataBits = 8;
                    COM.StopBits = StopBits.One;
                    COM.BaudRate = 9600;
                    COM.PortName = "COM8";
                    COM.Open();
                    Thread t = new Thread(HandleCOMPortInc);
                    t.Start();
                    isFine = true;
                }
                catch { }
            }
        }
        private void HandleCOMPortInc()
        {
            int i;
            string data = "";
            Byte[] bytes = new Byte[256];
            Majak majak = null;
            while (true)
            {
                try
                {
                    data = COM.ReadLine();
                }
                catch
                {
                    COM.Close();
                    StartListeningOnCOMPort();
                    break;
                }
                string[] data_array = data.Split('!');
                foreach (string s in data_array)
                {
                    if (s != null && s.Contains('?'))
                    {
                        string[] d = s.Split('?');
                        if (d[0] == "CNN")
                        {
                            majak = new Majak();
                        }
                        else
                        {
                            foreach (Majak m in activeClients.ToList<Majak>())
                            {
                                if (m.ID == int.Parse(d[1]))
                                {
                                    majak = m;
                                }
                            }
                        }
                        Request r = new Request(s, majak);
                        RequestsToEvaluation.Enqueue(r);
                    }
                }
            }
        }
        public void StartListener()
        {
            try
            {
                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();
                    if(client != null)
                    {
                        Majak m = new Majak(client);

                        Thread t = new Thread(new ParameterizedThreadStart(HandleDevice));
                        t.Start(m);
                        //activeClients.Add(m);
                    }
                }
            }
            catch(SocketException e)
            {
                MessageBox.Show("Nelze spustit server", e.ToString());
                server.Stop();
            }
        }
        public void HandleDevice(Object obj)
        {
            Majak majak = (Majak)obj;
            if (majak != null && majak.isConnected)
            {
                var stream = majak.Client.GetStream();

                string data = null;
                Byte[] bytes = new Byte[256];
                int i;

                try
                {
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        //string hex = BitConverter.ToString(bytes);
                        data = Encoding.ASCII.GetString(bytes, 0, i); //text který dorazil od arduina - potřeba ho dále vyhodnotit

                        //vytvoří požadavek na vyhodnocení zprávy a přídá ho na pořadník
                        string[] data_array = data.Split('!');
                        foreach(string s in data_array)
                        {
                            if(s != null)
                            {
                                Request r = new Request(s, majak);
                                RequestsToEvaluation.Enqueue(r);
                            }
                        }
                    }
                }
                catch //Pokud nastane chyba uzavře spojení a vypne toto vlákno
                {
                    majak.isConnected = false;
                    majak.Client.Close();
                    return;
                }
            }
        }

        public void DataEvaluation(Request req) //vyhodnotí a násladně vytvoří požadavek na odpověď, který přídá na konec odpovídací fronty
        {
            //split data by ":"
            Request r = null;
            string[] datasplt = req.Data.Split('?');
            switch (datasplt[0])
            {
                case "CNN": //upravit aby pracoval s MAC adresou
                    //string format  CNN:{MACAddress}
                    bool found = false;
                    foreach (Majak m in activeClients.ToList<Majak>())
                    {
                        if (m.MACAddress == datasplt[1])
                        {
                            req.Sender.Color = m.color;
                            req.Sender.ID = m.ID;
                            req.Sender.Blue_StopWatch = m.Blue_StopWatch;
                            req.Sender.Red_StopWatch = m.Red_StopWatch;
                            m.isConnected = false;
                            activeClients.Remove(m);
                            activeClients.Add(req.Sender);
                            found = true;
                        }

                    }
                    if (!found)
                    {
                        req.Sender.ID = IDForNextNewConnection;
                        IDForNextNewConnection++;
                        activeClients.Add(req.Sender);
                        UIWorker.CreateNewStationLine(req.Sender);
                    }
                    req.Sender.MACAddress = datasplt[1];
                    r = new Request(1, req.Sender);
                    RequestsToRespond.Enqueue(r);
                    break;
                case "PONG":
                    isConOk = true;
                    break;
                case "PING":
                    r = new Request(3, req.Sender);
                    RequestsToRespond.Enqueue(r);
                    break;
                case "CHGTM":
                    //zavolat změnu týmu
                    if (GameLogic.ChangeStationsTeam(req.Sender, datasplt[2]))
                    {
                        r = new Request(2, req.Sender); //pokud je hra zapnutá dojde ke změnění majáku a odešle se potvrzení arduinu 
                        RequestsToRespond.Enqueue(r);
                    }
                    else
                    {
                        if (GameLogic.isGameRunning == 2)
                        {
                            r = new Request(4, req.Sender); //Pokud je hra pozastavená nedojde ke změně majáku a odešle se zpráva arduinu, že hra neběží
                            RequestsToRespond.Enqueue(r);
                        }
                        else if (GameLogic.isGameRunning == 0)
                        {
                            r = new Request(7, req.Sender); //Pokud je hra vypnutá nedojde ke změně majáku a odešle se zpráva arduinu, že hra neběží
                            RequestsToRespond.Enqueue(r);
                        }
                    }
                    break;
                case "TMCR": //TMCR?ID?Barva pro přidání času navíc?čas v milisecundách!
                    //korekce času kvůli chybě ve spojení
                        if (req.Sender.ID == int.Parse(datasplt[1]))
                        {
                            if (datasplt[2] == "R")
                            {
                                req.Sender.Red_TimeSpan = req.Sender.Red_TimeSpan.Add(TimeSpan.FromMilliseconds(Convert.ToDouble(datasplt[3])));
                                req.Sender.Blue_TimeSpan = req.Sender.Blue_TimeSpan.Subtract(TimeSpan.FromMilliseconds(Convert.ToDouble(datasplt[3])));
                            }
                            else if (datasplt[2] == "B")
                            {
                                req.Sender.Red_TimeSpan = req.Sender.Red_TimeSpan.Subtract(TimeSpan.FromMilliseconds(Convert.ToDouble(datasplt[3])));
                                req.Sender.Blue_TimeSpan = req.Sender.Blue_TimeSpan.Add(TimeSpan.FromMilliseconds(Convert.ToDouble(datasplt[3])));
                            }
                            else if (datasplt[2] == "N")
                            {
                                req.Sender.Red_TimeSpan = req.Sender.Red_TimeSpan.Subtract(TimeSpan.FromMilliseconds(Convert.ToDouble(datasplt[3])));
                                req.Sender.Blue_TimeSpan = req.Sender.Blue_TimeSpan.Subtract(TimeSpan.FromMilliseconds(Convert.ToDouble(datasplt[3])));
                            }
                        }
                    r = new Request(9, req.Sender);
                    RequestsToRespond.Enqueue(r);
                    break;
            }
        }
        public void SendResponse(Request r)
        {
            int i = r.TypeOfResponse;
            Majak m = r.Sender;

            string response = null;
            Byte[] response_byte = null;

            if (m != null && m.isConnected || i == 5 || i == 6 || i == 7)
            {
                switch (i)
                {
                    case 1: //potvrdí arduinu/clientu připojení
                            //connection validation
                        response = String.Empty;
                        response = string.Format("CNN:{0}:{1}:CFM:!", m.ID, m.Color);
                        response_byte = null;
                        response_byte = System.Text.Encoding.ASCII.GetBytes(response);
                        try
                        {
                            //m.Client.GetStream().Write(response_byte, 0, response_byte.Length);
                            //m.Client.GetStream().Flush();
                            COM.WriteLine(response);
                        }
                        catch { }
                        break;
                    case 2: //potvrdí arduinu/clientu změnu týmu
                            //confirms change of team
                        response = String.Empty;
                        response = string.Format("CHGTM:{0}:{1}:CFM:!", m.ID, m.Color);
                        response_byte = null;
                        response_byte = System.Text.Encoding.ASCII.GetBytes(response);
                        try
                        {
                            //m.Client.GetStream().Write(response_byte, 0, response_byte.Length);
                            //m.Client.GetStream().Flush();
                            COM.WriteLine(response);
                        }
                        catch { }
                        break;
                    case 3: //Pokud arduino/client pošle PING request odpoví zprávou "PONG" (pro ověření že spojení je aktivní)
                        response = String.Empty;
                        response = "PONG:!";
                        response_byte = null;
                        response_byte = System.Text.Encoding.ASCII.GetBytes(response);
                        try
                        {
                            //m.Client.GetStream().Write(response_byte, 0, response_byte.Length);
                            //m.Client.GetStream().Flush();
                            COM.WriteLine(response);
                        }
                        catch { }
                        break;
                    case 4:
                        response = String.Empty;
                        response = string.Format("CHGTM:{0}:GMPAUSED:!", m.ID);
                        response_byte = null;
                        response_byte = System.Text.Encoding.ASCII.GetBytes(response);
                        try
                        {
                            //m.Client.GetStream().Write(response_byte, 0, response_byte.Length);
                            //m.Client.GetStream().Flush();
                            COM.WriteLine(response);
                        }
                        catch { }
                        break;
                    case 5: //send game pause
                        response = String.Empty;
                        response_byte = null;
                        response = "GAME_PAUSE:!";
                        response_byte = System.Text.Encoding.ASCII.GetBytes(response);
                        foreach (Majak majak in activeClients)
                        {
                            try
                            {
                                //majak.Client.GetStream().Write(response_byte, 0, response_byte.Length);
                                //majak.Client.GetStream().Flush();
                                COM.WriteLine(response);
                            }
                            catch { }
                        }
                        break;
                    case 6:
                        response = String.Empty;
                        response_byte = null;
                        foreach (Majak majak in activeClients)
                        {
                            response = String.Empty;
                            response_byte = null;
                            response = string.Format("GAME_UNPAUSE:{0}:{1}:!", majak.ID, majak.Color);
                            response_byte = System.Text.Encoding.ASCII.GetBytes(response);
                            try
                            {
                                //majak.Client.GetStream().Write(response_byte, 0, response_byte.Length);
                                //majak.Client.GetStream().Flush();
                                COM.WriteLine(response);
                            }
                            catch { }
                        }
                        break;
                    case 7:
                        response = String.Empty;
                        response_byte = null;
                        response = "GAME_END:!";
                        response_byte = System.Text.Encoding.ASCII.GetBytes(response);
                        foreach (Majak majak in activeClients)
                        {
                            try
                            {
                                //majak.Client.GetStream().Write(response_byte, 0, response_byte.Length);
                                //majak.Client.GetStream().Flush();
                                COM.WriteLine(response);
                            }
                            catch { }
                        }
                        break;
                    case 8:
                        response = String.Empty;
                        response = "PING:!";
                        response_byte = null;
                        response_byte = System.Text.Encoding.ASCII.GetBytes(response);
                        try
                        {
                            //m.Client.GetStream().Write(response_byte, 0, response_byte.Length);
                            //m.Client.GetStream().Flush();
                            COM.WriteLine(response);
                        }
                        catch { }
                        break;
                    case 9:
                        response = String.Empty;
                        response = "TMCR:CFM:!";
                        response_byte = null;
                        response_byte = System.Text.Encoding.ASCII.GetBytes(response);
                        try
                        {
                            //m.Client.GetStream().Write(response_byte, 0, response_byte.Length);
                            //m.Client.GetStream().Flush();
                            COM.WriteLine(response);
                        }
                        catch { }
                        break;
                }
            }      
        }
        public void StartConnectionCheck()
        {
            Thread t = new Thread(CheckConnectionStatus);
            t.IsBackground = true;
            t.Start();
        }
        public void CheckConnectionStatus() //Upravit aby na konci aktualizoval stav na záložce Kontroly spojení
        {
            Stopwatch s = new Stopwatch();
            while (true)
            {
                foreach (Majak m in activeClients.ToList<Majak>())
                {
                    if (m.isConnected)
                    {
                        isConOk = false;
                        var stream = m.Client.GetStream();
                        Byte[] bytes = new Byte[256];

                        try
                        {
                            Request r = new Request(8, m);
                            RequestsToRespond.Enqueue(r);
                            s.Restart();
                            while (s.Elapsed < TimeSpan.FromSeconds(5) && !isConOk)
                            {

                            }
                            if (!isConOk)
                            {
                                m.isConnected = false;
                                m.Client.Close();
                            }
                            UIWorker.UpdateStationConnectionStatus(m);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.ToString());
                            m.isConnected = false;
                            m.Client.Close();
                        }
                    }                  
                }
                
                Thread.Sleep(3000);
            }
        }
        /// <summary>
        /// Když se hra spustí/pozastaví/ukončí/obnoví vytvoří nový požadavek na odeslání informací k majákům
        /// </summary>
        /// <param name="transition">0 = Ukončení hry, 1 = Obnovení pozastavené hry, 2 = pozastavení hry</param>
        public static void NotifyGameStatusChanged(int transition)
        {
            Request r = null;
            switch (transition)
            {
                case 0:
                    r = new Request(7);
                    RequestsToRespond.Enqueue(r);
                    break;
                case 1:
                    r = new Request(6);
                    RequestsToRespond.Enqueue(r);
                    break;
                case 2:
                    r = new Request(5);
                    RequestsToRespond.Enqueue(r);
                    break;
            }
        }

        public void StartRequestCleaner()
        {
            Thread t = new Thread(RequestCleaner);
            t.IsBackground = true;
            t.Start();
        }
        public void RequestCleaner()
        {
            while (true)
            {
                if(RequestsToEvaluation.Count > 0 || RequestsToRespond.Count > 0)
                {
                    try
                    {
                        Request result;
                        while(RequestsToEvaluation.Count > 0)
                        {
                            if (RequestsToEvaluation.TryDequeue(out result))
                            {
                                if (result != null)
                                    DataEvaluation(result);
                            }
                        }
                    }
                    catch { }
                    try
                    {
                        Request result;
                        while(RequestsToRespond.Count > 0)
                        {
                            if (RequestsToRespond.TryDequeue(out result))
                            {
                                if (result != null)
                                    SendResponse(result);
                            }
                        }
                    }
                    catch { }
                }
                Thread.Sleep(50);
            }
        }
    }
}
