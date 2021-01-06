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
        public int TimeForAutoEndInMinutes { get; set; }
        private GameLogic Game { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            this.Visibility = Visibility.Hidden;
            UIWorker.MW = this;
            Game = new GameLogic();
            OpenGameSelectionWindow();
            

        }
        private void OpenGameSelectionWindow()
        {
            GameModeSelectionWindow Gw = new GameModeSelectionWindow(this, Game);
            Gw.Show();
        }
        public void StartServer()
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
                Comunication CC = new Comunication("192.168.137.1", 11800);
            });
            t.IsBackground = true;
            t.Start();
        }

        private void ZacitHru_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Game.StartGame();
                MessageBox.Show("Hra byla úspěšně spuštěna.", "Spuštění hry.");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Chyba!");
            }
        }

        private void PozastavitHru_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Game.PauseGame();
                MessageBox.Show("Hra byla úspěšně pozastaveno.", "Pozastavení hry.");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Chyba!");
            }

        }
        private void ObnovitHru_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Game.UnpauseGame();
                MessageBox.Show("Hra byla úspěšně obnovena.", "Obnovení hry.");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Chyba!");
            }
        }

        private void UkoncitHru_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Game.EndGame();
                MessageBox.Show("Hra byla uspěšně ukončena. Finální výsledky najdete v záložce Přehled Časů.", "Ukončení hry.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Chyba!");
            }
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
            AutoGameEndWindow w = new AutoGameEndWindow(Game);
            w.Show();
        }
    }
}
