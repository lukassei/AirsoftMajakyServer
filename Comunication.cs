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

namespace Airsoft_Majaky
{
    public class Comunication
    {
        TcpListener server = null;
        public static volatile List<Majak> activeClients;
        public List<Request> RequestsToEvaluation;
        public List<Request> RequestsToRespond;
        public MainWindow mw { get; set; }
        public bool isConOk { get; set; }

        public Comunication(string ip, int port, MainWindow _mw)
        {
            mw = _mw;
            activeClients = new List<Majak>();
            RequestsToEvaluation = new List<Request>();
            RequestsToRespond = new List<Request>();
            IPAddress localAddr = IPAddress.Parse(ip);
            server = new TcpListener(localAddr, port);
            server.Start();
            StartConnectionCheck();
            StartGameStatusChecker();
            StartRequestCleaner();
            StartListener();
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
                        activeClients.Add(m);
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
                                RequestsToEvaluation.Add(r);
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
                            req.Sender.Color = m.Color;
                            req.Sender.ID = m.ID;
                            req.Sender.Blue_StopWatch = m.Blue_StopWatch;
                            req.Sender.Red_StopWatch = m.Red_StopWatch;
                            m.isConnected = false;
                            activeClients.Remove(m);
                            found = true;
                        }
                    }
                    if (!found)
                    {
                        req.Sender.ID = activeClients.Count;
                        mw.CreateNewMajakLine(req.Sender);
                    }
                    req.Sender.MACAddress = datasplt[1];
                    r = new Request(1, req.Sender);
                    RequestsToRespond.Add(r);
                    break;
                case "PONG":
                    isConOk = true;
                    break;
                case "PING":
                    r = new Request(3, req.Sender);
                    RequestsToRespond.Add(r);
                    break;
                case "CHGTM":
                    //zavolat změnu týmu
                    if (StopwatchSystem.ChangeTeam(int.Parse(datasplt[1]), datasplt[2]))
                    {
                        r = new Request(2, req.Sender); //pokud je hra zapnutá dojde ke změnění majáku a odešle se potvrzení arduinu 
                        RequestsToRespond.Add(r);
                    }
                    else
                    {
                        if (MainWindow.isGameRunning == 2)
                        {
                            r = new Request(4, req.Sender); //Pokud je hra pozastavená nedojde ke změně majáku a odešle se zpráva arduinu, že hra neběží
                            RequestsToRespond.Add(r);
                        }
                        else if (MainWindow.isGameRunning == 0)
                        {
                            r = new Request(7, req.Sender); //Pokud je hra vypnutá nedojde ke změně majáku a odešle se zpráva arduinu, že hra neběží
                            RequestsToRespond.Add(r);
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
                    RequestsToRespond.Add(r);
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
                            m.Client.GetStream().Write(response_byte, 0, response_byte.Length);
                            m.Client.GetStream().Flush();
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
                            m.Client.GetStream().Write(response_byte, 0, response_byte.Length);
                            m.Client.GetStream().Flush();
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
                            m.Client.GetStream().Write(response_byte, 0, response_byte.Length);
                            m.Client.GetStream().Flush();
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
                            m.Client.GetStream().Write(response_byte, 0, response_byte.Length);
                            m.Client.GetStream().Flush();
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
                                majak.Client.GetStream().Write(response_byte, 0, response_byte.Length);
                                majak.Client.GetStream().Flush();
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
                                majak.Client.GetStream().Write(response_byte, 0, response_byte.Length);
                                majak.Client.GetStream().Flush();
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
                                majak.Client.GetStream().Write(response_byte, 0, response_byte.Length);
                                majak.Client.GetStream().Flush();
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
                            m.Client.GetStream().Write(response_byte, 0, response_byte.Length);
                            m.Client.GetStream().Flush();
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
                            m.Client.GetStream().Write(response_byte, 0, response_byte.Length);
                            m.Client.GetStream().Flush();
                        }
                        catch { }
                        break;
                }
            }      
        }
        public void StartConnectionCheck()
        {
            Thread t = new Thread(new ParameterizedThreadStart(CheckConnectionStatus));
            t.IsBackground = true;
            t.Start(mw);
        }
        public void CheckConnectionStatus(Object __mw) //Upravit aby na konci aktualizoval stav na záložce Kontroly spojení
        {
            Stopwatch s = new Stopwatch();
            MainWindow _mw = (MainWindow)__mw;
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
                            RequestsToRespond.Add(r);
                            s.Restart();
                            while (s.Elapsed < TimeSpan.FromSeconds(5) && !isConOk)
                            {

                            }
                            if (!isConOk)
                            {
                                m.isConnected = false;
                                m.Client.Close();
                                if(m.ID == 0)
                                {
                                    activeClients.Remove(m);
                                }
                                App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
                                {
                                    TextBlock t_s = (TextBlock)_mw.grid2.FindName(String.Format("majak{0}_spojeni", m.ID));
                                    if(t_s != null)
                                    {
                                        t_s.Text = string.Format("Maják není připojen.");
                                        t_s.Foreground = System.Windows.Media.Brushes.Red;
                                    }
                                });
                            }
                            else //Pokud je připojení v pořádku a client odpověděl, tak aktualizuj stav na záložce kontrola spojení v MainWindow
                            {
                                string barva = null;
                                if (m.Color == "R")
                                    barva = "Červená";
                                else if (m.Color == "B")
                                    barva = "Modrá";
                                else
                                    barva = "Neutrální";
                                App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
                                {
                                    TextBlock t_s = (TextBlock)_mw.grid2.FindName(String.Format("majak{0}_spojeni", m.ID));
                                    if(t_s != null)
                                    {
                                        t_s.Text = string.Format("Maják připojen. - Barva majáku: {0}", barva);
                                        t_s.Foreground = System.Windows.Media.Brushes.Green;
                                    }
                                });                               
                            }
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

        public void StartGameStatusChecker()
        {
            Thread t = new Thread(new ParameterizedThreadStart(GameStatusChecker));
            t.IsBackground = true;
            t.Start(mw);
        }
        public void GameStatusChecker(Object __mw)
        {
            MainWindow _mw = (MainWindow)__mw;
            Request r = null;
            int previousState = 0;

            while (true)
            {
                if (MainWindow.isGameRunning != previousState)
                {
                    if (MainWindow.isGameRunning == 1 && previousState == 2)
                    {
                        r = new Request(6);
                        RequestsToRespond.Add(r);
                    }
                    else if(MainWindow.isGameRunning == 2 && previousState == 1)
                    {
                        r = new Request(5);
                        RequestsToRespond.Add(r);
                    }
                    else if (MainWindow.isGameRunning == 0 && previousState == 1 || MainWindow.isGameRunning == 0 && previousState == 2)
                    {
                        r = new Request(7);
                        RequestsToRespond.Add(r);
                    }

                    previousState = MainWindow.isGameRunning;
                }
                Thread.Sleep(200);
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
                    foreach(Request r in RequestsToEvaluation.ToList<Request>())
                    {
                        DataEvaluation(r);
                        RequestsToEvaluation.Remove(r);
                    }
                    foreach(Request r in RequestsToRespond.ToList<Request>())
                    {
                        SendResponse(r);
                        RequestsToRespond.Remove(r);
                    }
                }
                Thread.Sleep(50);
            }
        }
    }
}
