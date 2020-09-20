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
    /// Interaction logic for AutoGameEndWindow.xaml
    /// </summary>
    public partial class AutoGameEndWindow : Window
    {
        private GameLogic Game { get; set; }
        public AutoGameEndWindow(GameLogic _game)
        {
            InitializeComponent();
            Game = _game;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Game.autoGameEndPoints = int.Parse(PointsToEndTheGameTextBox.Text);
                Close();
            }
            catch
            {
                MessageBox.Show("Body musí obsahovat pouze celá čísla.", "Chyba!");
            }
        }
    }
}
