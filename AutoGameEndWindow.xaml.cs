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
        private MainWindow Mw { get; set; }
        public AutoGameEndWindow(MainWindow mw)
        {
            InitializeComponent();
            Mw = mw;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int time;
                int minutes = int.Parse(HoursTextBox.Text) * 60;
                time = minutes + int.Parse(MinutesTextBox.Text);
                Mw.TimeForAutoEndInMinutes = time;
                this.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Chyba - nepodařilo se nastavit automatické ukončení hry.");
            }
        }
    }
}
