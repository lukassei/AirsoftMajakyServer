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
                TextBlock t_CB = (TextBlock)MW.grid1.FindName(string.Format("CompleteBlueTime_txt"));
                TextBlock t_CR = (TextBlock)MW.grid1.FindName(string.Format("CompleteRedTime_txt"));
                TextBlock t_GL = (TextBlock)MW.grid1.FindName(string.Format("delkahry"));

                if (t_m != null && t_r != null)
                {
                    t_m.Text = m.ReturnBlueTime().ToString("hh\\:mm\\:ss\\.f");
                    t_r.Text = m.ReturnRedTime().ToString("hh\\:mm\\:ss\\.f");
                }
                
                t_CB.Text = string.Format("{0} (s)", GameLogic.BlueScore);
                t_CR.Text = string.Format("{0} (s)", GameLogic.RedScore);
                t_GL.Text = GameLogic.GameLenght.Elapsed.ToString("hh\\:mm\\:ss\\.f");
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
        public static void PrepareGridForDomination()
        {
            RowDefinition gridRow0 = new RowDefinition();
            gridRow0.Height = new GridLength(60);
            RowDefinition gridRow1 = new RowDefinition();
            gridRow1.Height = new GridLength(40);

            ColumnDefinition gridCol0 = new ColumnDefinition();
            gridCol0.Width = new GridLength(160);
            ColumnDefinition gridCol1 = new ColumnDefinition();
            gridCol1.Width = new GridLength(323);
            ColumnDefinition gridCol2 = new ColumnDefinition();
            gridCol2.Width = new GridLength(323);

            MW.grid1.RowDefinitions.Add(gridRow0);
            MW.grid1.RowDefinitions.Add(gridRow1);
            MW.grid1.ColumnDefinitions.Add(gridCol0);
            MW.grid1.ColumnDefinitions.Add(gridCol1);
            MW.grid1.ColumnDefinitions.Add(gridCol2);

            Border GameLenghtBorder = new Border();
            GameLenghtBorder.BorderBrush = new SolidColorBrush(Colors.Black);
            GameLenghtBorder.BorderThickness = new Thickness(2, 2, 1, 1);

            Grid GameLenghtGrid = new Grid();

            TextBlock GameLenghtText = new TextBlock();
            GameLenghtText.Text = "Délka hry:";
            GameLenghtText.FontSize = 15;
            GameLenghtText.HorizontalAlignment = HorizontalAlignment.Center;
            GameLenghtText.VerticalAlignment = VerticalAlignment.Top;

            TextBlock GameLenghtTime = new TextBlock();
            GameLenghtTime.Name = "delkahry";
            GameLenghtTime.FontSize = 16;
            GameLenghtTime.Text = "00:00";
            GameLenghtTime.HorizontalAlignment = HorizontalAlignment.Center;
            GameLenghtTime.VerticalAlignment = VerticalAlignment.Bottom;

            GameLenghtGrid.Children.Add(GameLenghtText);
            GameLenghtGrid.Children.Add(GameLenghtTime);
            GameLenghtBorder.Child = GameLenghtGrid;
            Grid.SetColumn(GameLenghtBorder, 0);
            Grid.SetRow(GameLenghtBorder, 0);
            MW.grid1.Children.Add(GameLenghtBorder);
            MW.grid1.RegisterName(GameLenghtTime.Name, GameLenghtTime);

            Border BlueTeamBorder = new Border();
            BlueTeamBorder.BorderBrush = new SolidColorBrush(Colors.Black);
            BlueTeamBorder.BorderThickness = new Thickness(1, 2, 1, 1);
            BlueTeamBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFBCEBF5"));
            Grid.SetColumn(BlueTeamBorder, 1);
            Grid.SetRow(BlueTeamBorder, 0);

            TextBlock BlueTeamHeader = new TextBlock();
            BlueTeamHeader.Text = "Modrý tým";
            BlueTeamHeader.FontSize = 23;
            BlueTeamHeader.HorizontalAlignment = HorizontalAlignment.Center;
            BlueTeamHeader.VerticalAlignment = VerticalAlignment.Center;
            BlueTeamBorder.Child = BlueTeamHeader;

            MW.grid1.Children.Add(BlueTeamBorder);

            Border RedTeamBorder = new Border();
            RedTeamBorder.BorderBrush = new SolidColorBrush(Colors.Black);
            RedTeamBorder.BorderThickness = new Thickness(1, 2, 2, 1);
            RedTeamBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF08883"));
            Grid.SetColumn(RedTeamBorder, 2);
            Grid.SetRow(RedTeamBorder, 0);

            TextBlock RedTeamHeader = new TextBlock();
            RedTeamHeader.Text = "Červený tým";
            RedTeamHeader.FontSize = 23;
            RedTeamHeader.HorizontalAlignment = HorizontalAlignment.Center;
            RedTeamHeader.VerticalAlignment = VerticalAlignment.Center;
            RedTeamBorder.Child = RedTeamHeader;
            MW.grid1.Children.Add(RedTeamBorder);

            Border TotalBorder = new Border();
            TotalBorder.BorderBrush = new SolidColorBrush(Colors.Black);
            TotalBorder.BorderThickness = new Thickness(2, 1, 1, 1);
            TotalBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB8FCA2"));
            Grid.SetColumn(TotalBorder, 0);
            Grid.SetRow(TotalBorder, 1);

            TextBlock TotalText = new TextBlock();
            TotalText.Text = "Celkem";
            TotalText.FontSize = 18;
            TotalText.HorizontalAlignment = HorizontalAlignment.Center;
            TotalText.VerticalAlignment = VerticalAlignment.Center;
            TotalBorder.Child = TotalText;
            MW.grid1.Children.Add(TotalBorder);

            Border TotalBlueBorder = new Border();
            TotalBlueBorder.BorderBrush = new SolidColorBrush(Colors.Black);
            TotalBlueBorder.BorderThickness = new Thickness(1);
            TotalBlueBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB8FCA2"));
            Grid.SetColumn(TotalBlueBorder, 1);
            Grid.SetRow(TotalBlueBorder, 1);

            TextBlock TotalBlueText = new TextBlock();
            TotalBlueText.Text = "XXX";
            TotalBlueText.FontSize = 18;
            TotalBlueText.HorizontalAlignment = HorizontalAlignment.Center;
            TotalBlueText.VerticalAlignment = VerticalAlignment.Center;
            TotalBlueText.Name = "CompleteBlueTime_txt";
            TotalBlueBorder.Child = TotalBlueText;
            MW.grid1.Children.Add(TotalBlueBorder);
            MW.grid1.RegisterName(TotalBlueText.Name, TotalBlueText);

            Border TotalRedBorder = new Border();
            TotalRedBorder.BorderBrush = new SolidColorBrush(Colors.Black);
            TotalRedBorder.BorderThickness = new Thickness(1,1,2,1);
            TotalRedBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB8FCA2"));
            Grid.SetColumn(TotalRedBorder, 2);
            Grid.SetRow(TotalRedBorder, 1);

            TextBlock TotalRedText = new TextBlock();
            TotalRedText.Text = "XXX";
            TotalRedText.FontSize = 18;
            TotalRedText.HorizontalAlignment = HorizontalAlignment.Center;
            TotalRedText.VerticalAlignment = VerticalAlignment.Center;
            TotalRedText.Name = "CompleteRedTime_txt";
            TotalRedBorder.Child = TotalRedText;
            MW.grid1.Children.Add(TotalRedBorder);
            MW.grid1.RegisterName(TotalRedText.Name, TotalRedText);
        }
    }
}
