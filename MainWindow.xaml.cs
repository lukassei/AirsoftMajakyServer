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
        public StopwatchSystem SS { get; set; }
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
        /// <summary>
        /// 0 = game end, 1 = game running, 2 = game paused
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            StartServer();
            SS = new StopwatchSystem();
            SS.mw = this;
            SS.Prep();
            isGameRunning = 0;
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
            if (isGameRunning == 0) //zkontrolovat, že všechny majáky jsou připojeny
            {
                if (SS.GameBegin())
                    isGameRunning = 1;
            }
            else if (isGameRunning == 1)
                MessageBox.Show("Hra již běží!", "Chyba - Nepodařilo se spustit hru!");
            else
                MessageBox.Show("Hra je pozastavena. \nNejdřív musíte hru ukončit! \n\nPokud si přejete hru obnovit, využijte tlačítko Obnovení hry.", "Chyba - Nepodařilo se spustit hru!");
            try
            {
                if(TimeForAutoEndInMinutes > 0)
                {
                    Thread t = new Thread(AutoGameEnd);
                    t.IsBackground = true;
                    t.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Chyba - Nepodařilo se spustit automatické ukončení hry.");
            }
        }

        private void PozastavitHru_Click(object sender, RoutedEventArgs e)
        {
            if (isGameRunning == 1)
            {
                if (SS.GamePause())
                    isGameRunning = 2;
            }
            else if (isGameRunning == 0)
                MessageBox.Show("Hra není spuštěna. \nNelze pozastavit hru, která není zaplá.", "Chyba - Nepodařilo se pozastavit hru!");
            else
                MessageBox.Show("Hra je již pozastavena. \nPokud si přejete hru obnovit, využijte tlačítko Obnovení hry. \nPokud si přejete hru ukončit, využijte tlačítko ukončení hry.", "Chyba - Nepodařilo se pozastavit hru!");

        }
        private void ObnovitHru_Click(object sender, RoutedEventArgs e)
        {
            if (isGameRunning == 2)
            {
                isGameRunning = 1;
                if (!SS.GameUnpause())
                    isGameRunning = 2;
            }
            else if (isGameRunning == 1)
                MessageBox.Show("Hra momentálně běží. \nNelze obnovit hru, která není pozastavena. \n\nPokud si přejete hru restartovat, využijte tlačítko ukončit hru a násladně hru zapněte tlačítkem Zapnout hru.", "Chyba - Nepodařilo se obnovit hru!");
            else
                MessageBox.Show("Žádná hra momentálně není pozastavena. \nPokud si přejete spustit novou hru, využijte tlačítko Začít hru.", "Chyba - Nepodařilo se obnovit hru!");
        }

        private void UkoncitHru_Click(object sender, RoutedEventArgs e)
        {
            //zastavit veškeré stopky, restartovat zapnout na vlajkách obě barvy
            if (isGameRunning == 1 || isGameRunning == 2)
            {
                if (SS.GameEnd())
                {
                    isGameRunning = 0;
                    MessageBox.Show("Hra byla úspěšně ukončena. Finální časy naleznete na záložce Přehled časů.", "Hra ukončena");
                }
            }
            else
                MessageBox.Show("Žádná hra momentálně neběží. \nNelze ukončit neexistující hru! \n\nPokud hledáte časy z již ukončené hry, naleznete je v záložce Přehled časů.", "Chyba - Nepodařilo se ukončit hru!");
        }
        private void UkoncitHru_Automaticky()
        {
            //zastavit veškeré stopky, restartovat zapnout na vlajkách obě barvy
            if (isGameRunning == 1 || isGameRunning == 2)
            {
                if (SS.GameEnd())
                {
                    isGameRunning = 0;
                    MessageBox.Show("Čas vypršel! \nHra byla úspěšně automaticky ukončena. \n\nFinální časy naleznete na záložce Přehled časů.", "Hra ukončena");
                }
            }
            else
                MessageBox.Show("Žádná hra momentálně neběží. \nNelze ukončit neexistující hru! \n\nPokud hledáte časy z již ukončené hry, naleznete je v záložce Přehled časů.", "Chyba - Nepodařilo se ukončit hru!");
        }

        private void AutoGameEnd()
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            while (s.Elapsed < TimeSpan.FromMinutes(TimeForAutoEndInMinutes))
            {
                if (isGameRunning == 0 && s.IsRunning || isGameRunning == 2 && s.IsRunning)
                    s.Stop();
                if (isGameRunning == 1 && !s.IsRunning)
                    s.Start();
            }
            UkoncitHru_Automaticky();
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
        public void CreateNewMajakLine(Majak m)
        {
            App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
            {
                RowDefinition gridRow1 = new RowDefinition();
                grid1.RowDefinitions.Add(gridRow1);



                TextBlock MajakID = new TextBlock();
                MajakID.Text = string.Format("Maják {0}", m.ID);
                MajakID.FontSize = 23;
                MajakID.VerticalAlignment = VerticalAlignment.Center;
                MajakID.HorizontalAlignment = HorizontalAlignment.Center;

                Border BorderMajakID = new Border();
                BorderMajakID.BorderThickness = new Thickness(2,1,1,1);
                BorderMajakID.BorderBrush = new SolidColorBrush(Colors.Black);
                BorderMajakID.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFBFBB1"));
                BorderMajakID.Child = MajakID;
                Grid.SetColumn(BorderMajakID, 0);
                Grid.SetRow(BorderMajakID, m.ID + 1);

                TextBlock StopwatchBlue = new TextBlock();
                StopwatchBlue.Text = "xxx";
                StopwatchBlue.FontSize = 18;
                StopwatchBlue.VerticalAlignment = VerticalAlignment.Center;
                StopwatchBlue.HorizontalAlignment = HorizontalAlignment.Center;
                StopwatchBlue.Name = string.Format("majak{0}_modry", m.ID);

                Border BorderStopwatchBlue = new Border();
                BorderStopwatchBlue.BorderThickness = new Thickness(1);
                BorderStopwatchBlue.BorderBrush = new SolidColorBrush(Colors.Black);
                BorderStopwatchBlue.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE5F7FB"));
                BorderStopwatchBlue.Child = StopwatchBlue;
                Grid.SetColumn(BorderStopwatchBlue, 1);
                Grid.SetRow(BorderStopwatchBlue, m.ID + 1);

                TextBlock StopwatchRed = new TextBlock();
                StopwatchRed.Text = "xxx";
                StopwatchRed.FontSize = 18;
                StopwatchRed.VerticalAlignment = VerticalAlignment.Center;
                StopwatchRed.HorizontalAlignment = HorizontalAlignment.Center;
                StopwatchRed.Name = string.Format("majak{0}_cerveny", m.ID);
                

                Border BorderStopwatchRed = new Border();
                BorderStopwatchRed.BorderThickness = new Thickness(1,1,2,1);
                BorderStopwatchRed.BorderBrush = new SolidColorBrush(Colors.Black);
                BorderStopwatchRed.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFBDDDC"));
                BorderStopwatchRed.Child = StopwatchRed;
                Grid.SetColumn(BorderStopwatchRed, 2);
                Grid.SetRow(BorderStopwatchRed, m.ID + 1);

                grid1.RegisterName(StopwatchBlue.Name, StopwatchBlue);
                grid1.RegisterName(StopwatchRed.Name, StopwatchRed);
                grid1.Children.Add(BorderMajakID);
                grid1.Children.Add(BorderStopwatchBlue);
                grid1.Children.Add(BorderStopwatchRed);

                RowDefinition gridRow2 = new RowDefinition();
                gridRow2.Height = new GridLength(40);
                grid2.RowDefinitions.Add(gridRow2);

                TextBlock MajakSpojeniTxt = new TextBlock();
                MajakSpojeniTxt.Text = string.Format("Spojení - maják {0}", m.ID);
                MajakSpojeniTxt.FontSize = 15;
                MajakSpojeniTxt.VerticalAlignment = VerticalAlignment.Center;
                MajakSpojeniTxt.HorizontalAlignment = HorizontalAlignment.Center;
                Grid.SetColumn(MajakSpojeniTxt, 0);
                Grid.SetRow(MajakSpojeniTxt, m.ID);

                TextBlock MajakSpojeni = new TextBlock();
                MajakSpojeni.Text = "---";
                MajakSpojeni.FontSize = 15;
                MajakSpojeni.VerticalAlignment = VerticalAlignment.Center;
                MajakSpojeni.HorizontalAlignment = HorizontalAlignment.Left;
                MajakSpojeni.Name = string.Format("majak{0}_spojeni", m.ID);
                Grid.SetColumn(MajakSpojeni, 1);
                Grid.SetRow(MajakSpojeni, m.ID);
                grid2.RegisterName(MajakSpojeni.Name, MajakSpojeni);
                grid2.Children.Add(MajakSpojeniTxt);
                grid2.Children.Add(MajakSpojeni);
            });

        }
    }
}
