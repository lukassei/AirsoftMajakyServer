using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Airsoft_Majaky
{
    /// <summary>Třída se využívá k aktualizaci zobrazovaných údajů.</summary>
    ///
    static class UIWorker
    {
        static volatile MainWindow _mw;
        public static MainWindow MW
        {
            get
            {
                return _mw;
            }
            set
            {
                _mw = (MainWindow)value;
            }
        }
        /// <summary>Funkce vytvoří nový řádek pro nový maják v záložkách Přehled časů a Kontrola spojení</summary>
        public static void CreateNewStationLine(Majak m)
        {
            App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
            {
                RowDefinition gridRow1 = new RowDefinition();
                MW.grid1.RowDefinitions.Add(gridRow1);



                TextBlock MajakID = new TextBlock();
                MajakID.Text = string.Format("Maják {0}", m.ID);
                MajakID.FontSize = 23;
                MajakID.VerticalAlignment = VerticalAlignment.Center;
                MajakID.HorizontalAlignment = HorizontalAlignment.Center;

                Border BorderMajakID = new Border();
                BorderMajakID.BorderThickness = new Thickness(2, 1, 1, 1);
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
                BorderStopwatchRed.BorderThickness = new Thickness(1, 1, 2, 1);
                BorderStopwatchRed.BorderBrush = new SolidColorBrush(Colors.Black);
                BorderStopwatchRed.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFBDDDC"));
                BorderStopwatchRed.Child = StopwatchRed;
                Grid.SetColumn(BorderStopwatchRed, 2);
                Grid.SetRow(BorderStopwatchRed, m.ID + 1);

                MW.grid1.RegisterName(StopwatchBlue.Name, StopwatchBlue);
                MW.grid1.RegisterName(StopwatchRed.Name, StopwatchRed);
                MW.grid1.Children.Add(BorderMajakID);
                MW.grid1.Children.Add(BorderStopwatchBlue);
                MW.grid1.Children.Add(BorderStopwatchRed);

                RowDefinition gridRow2 = new RowDefinition();
                gridRow2.Height = new GridLength(40);
                MW.grid2.RowDefinitions.Add(gridRow2);

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
                MW.grid2.RegisterName(MajakSpojeni.Name, MajakSpojeni);
                MW.grid2.Children.Add(MajakSpojeniTxt);
                MW.grid2.Children.Add(MajakSpojeni);
            });
        }
        /// <summary>Funkce aktualizuje časy majáků v záložce Přehled Časů každých 0.1S</summary>
        public static void UpdateTimesOfStation(Majak m)
        {
            App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
            {
                TextBlock t_m = (TextBlock)MW.grid1.FindName(string.Format("majak{0}_modry", m.ID));
                TextBlock t_r = (TextBlock)MW.grid1.FindName(string.Format("majak{0}_cerveny", m.ID));

                if (t_m != null && t_r != null)
                {
                    t_m.Text = m.ReturnBlueTime().ToString("hh\\:mm\\:ss\\.f");
                    t_r.Text = m.ReturnRedTime().ToString("hh\\:mm\\:ss\\.f");
                }

                MW.CompleteBlueTime_txt.Text = string.Format("{0} (s)", GameLogic.BlueScore);
                MW.CompleteRedTime_txt.Text = string.Format("{0} (s)", GameLogic.RedScore);
                MW.delkahry.Text = GameLogic.GameLenght.Elapsed.ToString("hh\\:mm\\:ss\\.f");
            });
        }
        /// <summary>
        /// Funkce aktualizuje stav připojení majáku v záložce Kontrola Spojení
        /// </summary>
        /// <param name="m">Maják, který má být aktualizován</param>
        public static void UpdateStationConnectionStatus(Majak m)
        {
            string s;
            if (!m.isConnected)
                s = "Maják není připojen.";
            else
            {
                string b = "";
                switch (m.Color)
                {
                    case "B":
                        b = "Modrá";
                        break;
                    case "R":
                        b = "Červená";
                        break;
                    case "N":
                        b = "Neutrální";
                        break;
                    case "A":
                        b = "Obě barvy";
                        break;
                }
                s = string.Format("Maják je připojen - Barva majáku: {0}", b);
            }
            App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
            {
                TextBlock t_s = (TextBlock)_mw.grid2.FindName(String.Format("majak{0}_spojeni", m.ID));
                if (t_s != null)
                {
                    t_s.Text = s;
                    if (m.isConnected)
                        t_s.Foreground = System.Windows.Media.Brushes.DarkGreen;
                    else
                        t_s.Foreground = System.Windows.Media.Brushes.Red;
                }
            });
        }
    }
}
