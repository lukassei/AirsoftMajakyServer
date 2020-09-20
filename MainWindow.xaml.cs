using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Airsoft_Majaky
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static volatile int isGameRunningVar;
        public static int isGameRunning
        {
            get
            {
                return isGameRunningVar;
            }
            set
            {
                isGameRunningVar = value;
            }
        }
        public int TimeForAutoEndInMinutes { get; set; }
        private GameLogic Game { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            StartServer();
            isGameRunning = 0;
            UIWorker.MW = this;
            Game = new GameLogic();
            Game.StartGameModeDomination();

        }
        private void StartServer()
        {
            Thread t = new Thread(delegate ()
            {
                string ipaddres = "";
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ipaddres = ip.ToString();
                    }
                }
                // replace the IP with your system IP Address...
                Comunication CC = new Comunication(ipaddres, 11800, this);
            });
            t.IsBackground = true;
            t.Start();
        }

        private void ZacitHru_Click(object sender, RoutedEventArgs e)
        {
            Game.StartGame();
        }

        private void PozastavitHru_Click(object sender, RoutedEventArgs e)
        {
            Game.PauseGame();

        }
        private void ObnovitHru_Click(object sender, RoutedEventArgs e)
        {
            Game.UnpauseGame();
        }

        private void UkoncitHru_Click(object sender, RoutedEventArgs e)
        {
            Game.EndGame();
        }
        private void UkoncitHru_Automaticky()
        {
            
        }

        private void AutoGameEnd()
        {
            
        }



        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult d = MessageBox.Show("Opravdu chcete ukončit aplikaci?", "Ukončit aplikaci", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (d == MessageBoxResult.Yes)
            {
                Environment.Exit(0);
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void AutomatickeUkonceniHry_Click(object sender, RoutedEventArgs e)
        {
            if (isGameRunning != 0)
                MessageBox.Show("Automatické ukončení nelze nastavit na již běžící hře. \nPro nastavení automatického ukončení musí být hra ukončena. ", "Nelze nastavit automatické ukončení!");
            else
            {
                AutoGameEndWindow a = new AutoGameEndWindow(this);
                a.Show();
            }
        }
    }
}
