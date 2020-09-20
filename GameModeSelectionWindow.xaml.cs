using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Airsoft_Majaky
{
    /// <summary>
    /// Interaction logic for GameModeSelectionWindow.xaml
    /// </summary>
    public partial class GameModeSelectionWindow : Window
    {
        private MainWindow MW { get; set; }
        private GameLogic Game { get; set; }
        public GameModeSelectionWindow(MainWindow _mw, GameLogic _game)
        {
            InitializeComponent();
            MW = _mw;
            Game = _game;
        }

        private void GameModeSelectionConfirm_Click(object sender, RoutedEventArgs e)
        {
            int selected = SelectedGameModeBox.SelectedIndex;
            switch(selected)
            {
                case 0: //domination
                    Game.ChangeGameMode(1);
                    MW.StartServer();
                    MW.Visibility = Visibility.Visible;
                    Close();
                    break;
                case 1: //selective domination
                    MessageBox.Show("Vybraný herní mód není v této verzi aplikace dostupný. Na jeho implementaci v budoucích verzích se usilovně pracuje. :) ", "Vybraný herní mód není dostuný.");
                    break;
            }
            
        }
    }
}
