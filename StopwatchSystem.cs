using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;

namespace Airsoft_Majaky
{
    public class StopwatchSystem
    {
        public Stopwatch delkahry_stopky { get; private set; }


        public MainWindow mw { get; set; }


        //připraví všechny stopky
        public void Prep()
        {
            delkahry_stopky = new Stopwatch();

            NotifTime();

        }

        //po spuštění hry začne počítat její délku
        public bool GameBegin()
        {
            try
            {
                foreach(Majak m in Comunication.activeClients)
                {
                    m.StopwatchReset();
                }

                delkahry_stopky.Reset();
                delkahry_stopky.Start();
                return true;
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString(), "Chyba spouštění hry!");
                return false;
            }
        }

        //pozastaví všechny stopky
        public bool GamePause()
        {
            try
            {
                foreach(Majak m in Comunication.activeClients)
                {
                    m.StopwatchStop();
                }
                delkahry_stopky.Stop();

                return true;
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString(), "Chyba - Hru se nepodařilo pozastavit!");
                return false;
            }
        }

        //obnoví stopky
        public bool GameUnpause()
        {
            try
            {
                foreach(Majak m in Comunication.activeClients)
                {
                    m.StopwatchResume();
                }
                delkahry_stopky.Start();
                return true;
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString(), "Chyba - Hru se nepodařilo obnovit!");
                return false;
            }
        }
        public bool GameEnd()
        {
            try
            {
                foreach(Majak m in Comunication.activeClients)
                {
                    m.StopwatchStop();
                }
                delkahry_stopky.Stop();
                return true;
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString(), "Chyba - Nepodařilo se ukončit hru!");
                return true;
            }
        }
        public static bool ChangeTeam(int _cisloMajaku, string _novyTym)
        {
            if (MainWindow.isGameRunning == 1)
            {
                foreach(Majak m in Comunication.activeClients)
                {
                    if(m.ID == _cisloMajaku)
                    {
                        m.Color = _novyTym;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        private void NotifTime()
        {
            Thread t = new Thread(new ParameterizedThreadStart(OnTimerTick));
            t.IsBackground = true;
            t.Start(mw);
        }
        private void OnTimerTick(Object _mw)
        {
            MainWindow __mw = (MainWindow)_mw;
            while (true)
            {
                    App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
                    {
                        double CompleteBlueTimeInSeconds = 0;
                        double CompleteRedTimeInSeconds = 0;
                        TimeSpan CompleteBlueTime = new TimeSpan();
                        TimeSpan CompleteRedTime = new TimeSpan();
                        foreach (Majak m in Comunication.activeClients.ToList<Majak>())
                        {
                            TextBlock t_m = (TextBlock)__mw.grid1.FindName(string.Format("majak{0}_modry", m.ID));
                            TextBlock t_r = (TextBlock)__mw.grid1.FindName(string.Format("majak{0}_cerveny", m.ID));

                            if (t_m != null && t_r != null)
                            {
                                t_m.Text = m.ReturnBlueTime().ToString("hh\\:mm\\:ss\\.f");
                                t_r.Text = m.ReturnRedTime().ToString("hh\\:mm\\:ss\\.f");
                            }
                            CompleteBlueTimeInSeconds += m.ReturnBlueTime().TotalSeconds;
                            CompleteRedTimeInSeconds += m.ReturnRedTime().TotalSeconds;
                            CompleteBlueTime = CompleteBlueTime.Add(m.ReturnBlueTime());
                            CompleteRedTime = CompleteRedTime.Add(m.ReturnRedTime());



                        }
                        __mw.CompleteBlueTime_txt.Text = string.Format("{0} ({1})", CompleteBlueTime.ToString("hh\\:mm\\:ss\\.f"), Math.Floor(CompleteBlueTimeInSeconds));
                        __mw.CompleteRedTime_txt.Text = string.Format("{0} ({1})", CompleteRedTime.ToString("hh\\:mm\\:ss\\.f"), Math.Floor(CompleteRedTimeInSeconds));
                        __mw.delkahry.Text = delkahry_stopky.Elapsed.ToString("hh\\:mm\\:ss\\.f");

                    });
                

                Thread.Sleep(100);
            }
        }
    }
}
