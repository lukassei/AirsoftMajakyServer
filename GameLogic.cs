using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Airsoft_Majaky
{
    public class GameLogic
    {
        private static volatile int isGameRunningVar;
        /// <summary>
        /// 0 = Hra ukončena, 1 = Hra běží, 2 = Hra Pozastavena
        /// </summary>
        public static int isGameRunning
        {
            get
            {
                return isGameRunningVar;
            }
            set
            {
                switch (value)
                {
                    case 0:
                        if (isGameRunning != 0)
                            Comunication.NotifyGameStatusChanged(0);
                        break;
                    case 1:
                        if (isGameRunning == 2)
                            Comunication.NotifyGameStatusChanged(1);
                        break;
                    case 2:
                        if (isGameRunning == 1)
                            Comunication.NotifyGameStatusChanged(2);
                        break;
                }
                isGameRunningVar = value;
            }
        }
        static volatile Stopwatch _gameLenght;
        public static Stopwatch GameLenght
        {
            get
            {
                return _gameLenght;
            }
            set
            {
                _gameLenght = value;
            }
        }
        private static volatile int _blueScore;
        public static int BlueScore
        {
            get
            {
                return _blueScore;
            }
            set
            {
                _blueScore = value;
            }
        }
        private static volatile int _redScore;
        public static int RedScore
        {
            get
            {
                return _redScore;
            }
            set
            {
                _redScore = value;
            }
        }
        public GameLogic()
        {
            GameLenght = new Stopwatch();
        }
        /// <summary>
        /// Spustí vše potřebné pro hraní módu Domintaion
        /// </summary>
        public void StartGameModeDomination()
        {
            StartStationTimeKeeper();
        }
        public void StartGame()
        {
            isGameRunning = 1;
            foreach(Majak m in Comunication.activeClients.ToList<Majak>())
            {
                m.StopwatchReset();
            }
            GameLenght.Restart();
        }
        public void PauseGame()
        {
            isGameRunning = 2;
            foreach (Majak m in Comunication.activeClients.ToList<Majak>())
            {
                m.StopwatchStop();
            }
            GameLenght.Stop();
        }
        public void UnpauseGame()
        {
            isGameRunning = 1;
            foreach (Majak m in Comunication.activeClients.ToList<Majak>())
            {
                m.StopwatchResume();
            }
            GameLenght.Start();
        }
        public void EndGame()
        {
            isGameRunning = 0;
            foreach (Majak m in Comunication.activeClients.ToList<Majak>())
            {
                m.StopwatchStop();
            }
            GameLenght.Stop();
        }
        /// <summary>
        /// Domination - Spustí pravidelnou kontrolu časů majáků.
        /// </summary>
        private void StartStationTimeKeeper()
        {
            Thread t = new Thread(StationTimeKeeper);
            t.IsBackground = true;
            t.Start();
        }
        /// <summary>
        /// Domination - Funkce pravidelně kontroluje časy majáků a následně volá jejich aktualizaci. Funkce běží ve vlastním vlákně nepřetržitě.
        /// </summary>
        private void StationTimeKeeper()
        {
            while (true)
            {
                TimeSpan CompleteBlueTime = new TimeSpan();
                TimeSpan CompleteRedTime = new TimeSpan();
                try
                {
                    foreach (Majak m in Comunication.activeClients.ToList<Majak>())
                    {
                        CompleteBlueTime = CompleteBlueTime.Add(m.ReturnBlueTime());
                        CompleteRedTime = CompleteRedTime.Add(m.ReturnRedTime());
                        UIWorker.UpdateTimesOfStation(m);
                    }
                    BlueScore = int.Parse(Math.Floor(CompleteBlueTime.TotalSeconds).ToString());
                    RedScore = int.Parse(Math.Floor(CompleteRedTime.TotalSeconds).ToString());
                }
                catch { }
                Thread.Sleep(100);
            }
        }
        /// <summary>
        /// Funkce změní tým u zadaného majáku
        /// </summary>
        /// <param name="m">Maják, kterému se změní tým</param>
        /// <param name="newTeam">Nový tým pro daný maják</param>
        public static bool ChangeStationsTeam(Majak m, string newTeam)
        {
            m.Color = newTeam;
            if (m.Color == newTeam)
                return true;
            else
                return false;
        }

    }
}
