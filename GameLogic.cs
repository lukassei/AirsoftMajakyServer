using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

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
        private static volatile Stopwatch _gameLenght;
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
        public int autoGameEndPoints { get; set; }
        /// <summary>
        /// Mód hry
        /// 1 = Domination, 2 = selective domination, 
        /// </summary>
        private int GameMode { get; set; }
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
        public void ChangeGameMode(int _mode)
        {
            GameMode = _mode;
            switch (GameMode)
            {
                case 1:
                    StartGameModeDomination();
                    break;
                case 2:
                    //StartGameModeSelectiveDomination
                    break;
            }
        }
        /// <summary>
        /// Spustí vše potřebné pro hraní módu Domintaion
        /// </summary>
        public void StartGameModeDomination()
        {
            UIWorker.PrepareGridForDomination();
            StartStationTimeKeeper();
        }
        public void StartGame()
        {
            if (Comunication.activeClients.Count == 0 || Comunication.activeClients == null)
                throw new Exception("Nemůžete spustit hru, když není připojen alespoň jeden maják!");
            if (isGameRunning != 0)
                throw new Exception("Pro začátek nové hry musíte nejdřív ukončit hru probíhající!");
            isGameRunning = 1;
            foreach(Majak m in Comunication.activeClients.ToList<Majak>())
            {
                m.StopwatchReset();
            }
            GameLenght.Restart();
        }
        public void PauseGame()
        {
            if (isGameRunning != 1)
                throw new Exception("Hra je již pozastavena, a nebo žádná hra právě neprobíhá!");
            isGameRunning = 2;
            foreach (Majak m in Comunication.activeClients.ToList<Majak>())
            {
                m.StopwatchStop();
            }
            GameLenght.Stop();
        }
        public void UnpauseGame()
        {
            if (isGameRunning != 2)
                throw new Exception("Hra již běží, a nebo žádná hra neprobíhá!");
            isGameRunning = 1;
            foreach (Majak m in Comunication.activeClients.ToList<Majak>())
            {
                m.StopwatchResume();
            }
            GameLenght.Start();
        }
        public void EndGame()
        {
            if (isGameRunning == 0)
                throw new Exception("Žádná hra právě neprobíhá!");
            isGameRunning = 0;
            foreach (Majak m in Comunication.activeClients.ToList<Majak>())
            {
                m.StopwatchStop();
            }
            BlueScore = 0;
            RedScore = 0;
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
                if(Comunication.activeClients != null)
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
                        if(autoGameEndPoints > 0)
                            CheckWetherGameEndPointsReached();
                    }
                    catch { }
                }
                else
                {

                }
                Thread.Sleep(100);
            }
        }
        /// <summary>
        /// Funkce zkontroluje zda je již splněna podmínka pro automatické ukončení hry.
        /// </summary>
        private void CheckWetherGameEndPointsReached()
        {
            if(BlueScore >= autoGameEndPoints && RedScore >= autoGameEndPoints)
            {
                MessageBox.Show("Hra by skončila remízou! Prodlužuju hru o 200 bodů.");
                autoGameEndPoints += 200;
            }
            if(BlueScore >= autoGameEndPoints)
            {
                EndGame();
                autoGameEndPoints = 0;
                MessageBox.Show("Modrý tým dosáhl výherního množství bodů. Modrý tým vyhrál. Hra automaticky ukončena.", "Modrý tým vyhrál.");
            }
            if(RedScore >= autoGameEndPoints)
            {
                EndGame();
                autoGameEndPoints = 0;
                MessageBox.Show("Červený tým dosáhl výherního množství bodů. Červený tým vyhrál. Hra automaticky ukončena.", "Červený tým vyhrál.");
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
