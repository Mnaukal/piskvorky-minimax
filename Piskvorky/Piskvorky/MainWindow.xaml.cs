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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Piskvorky
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Application.Current.Windows.Cast<Window>().Where(x => x.ToString().Contains("Piskvorky")).Count() > 1)
            {
                MessageBoxResult vypnout = MessageBox.Show("Uzavření tohoto okna vypne všechny proníhající hry. Opravdu chcete skončit?", "Konec?", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (vypnout == MessageBoxResult.Yes)
                    Application.Current.Shutdown();
                else
                    e.Cancel = true;
            }
        }

        private void button_ttc_Click(object sender, RoutedEventArgs e)
        {
            // zobrazit nové okno (TicTacToe) + hloubka
            Window_TicTacToe_hloubka ttt = new Window_TicTacToe_hloubka(false);
            ttt.Show();
        }       
        private void button_pis_Click(object sender, RoutedEventArgs e)
        {
            int velikost, hloubka, vyhra;
            if (int.TryParse(textBox_velikost.Text, out velikost) &&
                int.TryParse(textBox_hloubka.Text, out hloubka) &&
                int.TryParse(textBox_vyhra.Text, out vyhra))
            {
                Window_Piskvorky_AlfaBeta_optimalizace pis = new Window_Piskvorky_AlfaBeta_optimalizace(velikost, hloubka, vyhra, false);
                pis.Show();
            }
            else
            {
                MessageBox.Show("Velikost plochy, hloubka a počet na výhru musí být čísla");
            }
        }

        private void textBox_GotFocus(object sender, RoutedEventArgs e)
        {
            (sender as TextBox).SelectAll();
        }

        private void button_testovani_Click(object sender, RoutedEventArgs e)
        {
            MainWindow_Testovani test = new MainWindow_Testovani();
            test.Show();
        }
    }
}
