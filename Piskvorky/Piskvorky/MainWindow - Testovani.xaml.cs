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
    public partial class MainWindow_Testovani : Window
    {
        public MainWindow_Testovani()
        {
            InitializeComponent();
        }

        private void button_ttc1_Click(object sender, RoutedEventArgs e)
        {
            // zobrazit nové okno (TicTacToe)
            Window_TicTacToe ttt1 = new Window_TicTacToe();
            ttt1.Show();
        }

        private void button_ttc2_Click(object sender, RoutedEventArgs e)
        {
            // zobrazit nové okno (TicTacToe) + hloubka
            Window_TicTacToe_hloubka ttt2 = new Window_TicTacToe_hloubka(true);
            ttt2.Show();
        }       

        private void button_ttc3_Click(object sender, RoutedEventArgs e)
        {
            // zobrazit nové okno (TicTacToe) + hloubka, lokální hodnocení
            Window_TicTacToe_hloubka_lokalni ttt3 = new Window_TicTacToe_hloubka_lokalni();
            ttt3.Show();
        }

        private void button_ttc4_Click(object sender, RoutedEventArgs e)
        {
            // zobrazit nové okno (TicTacToe) + hloubka, Min Max zvlášť
            Window_TicTacToe_MINMAX_hloubka ttt4 = new Window_TicTacToe_MINMAX_hloubka(true);
            ttt4.Show();
        }

        private void button_pis1_Click(object sender, RoutedEventArgs e)
        {
            int velikost, hloubka, vyhra;
            if(int.TryParse(textBox_velikost1.Text, out velikost) &&
                int.TryParse(textBox_hloubka1.Text, out hloubka) &&
                int.TryParse(textBox_vyhra1.Text, out vyhra))
            {
                // zobrazit nové okno Piskvorky 1
                Window_Piskvorky pis1 = new Window_Piskvorky(velikost, hloubka, vyhra);
                pis1.Show();
            }
            else
            {
                MessageBox.Show("Velikost plochy, hloubka a počet na výhru musí být čísla");
            }            
        }

        private void button_pis2_Click(object sender, RoutedEventArgs e)
        {
            int velikost, hloubka, vyhra;
            if (int.TryParse(textBox_velikost1.Text, out velikost) &&
                int.TryParse(textBox_hloubka1.Text, out hloubka) &&
                int.TryParse(textBox_vyhra1.Text, out vyhra))
            {
                // zobrazit nové okno (Piskvorky) + lokální
                Window_Piskvorky_lokalni pis2 = new Window_Piskvorky_lokalni(velikost, hloubka, vyhra);
                pis2.Show();
            }
            else
            {
                MessageBox.Show("Velikost plochy, hloubka a počet na výhru musí být čísla");
            }
        }

        private void button_pis3_Click(object sender, RoutedEventArgs e)
        {
            int velikost, hloubka, vyhra;
            if (int.TryParse(textBox_velikost1.Text, out velikost) &&
                int.TryParse(textBox_hloubka1.Text, out hloubka) &&
                int.TryParse(textBox_vyhra1.Text, out vyhra))
            {
                // zobrazit nové okno Piskvorky 3
                Window_Piskvorky_MINMAX pis3 = new Window_Piskvorky_MINMAX(velikost, hloubka, vyhra);
                pis3.Show();
            }
            else
            {
                MessageBox.Show("Velikost plochy, hloubka a počet na výhru musí být čísla");
            }
        }

        private void button_pis4_Click(object sender, RoutedEventArgs e)
        {
            int velikost, hloubka, vyhra;
            if (int.TryParse(textBox_velikost1.Text, out velikost) &&
                int.TryParse(textBox_hloubka1.Text, out hloubka) &&
                int.TryParse(textBox_vyhra1.Text, out vyhra))
            {
                // zobrazit nové okno Piskvorky 4
                Window_Piskvorky_AlfaBeta pis4 = new Window_Piskvorky_AlfaBeta(velikost, hloubka, vyhra);
                pis4.Show();
            }
            else
            {
                MessageBox.Show("Velikost plochy, hloubka a počet na výhru musí být čísla");
            }
        }

        private void button_pis5_Click(object sender, RoutedEventArgs e)
        {
            int velikost, hloubka, vyhra;
            if (int.TryParse(textBox_velikost1.Text, out velikost) &&
                int.TryParse(textBox_hloubka1.Text, out hloubka) &&
                int.TryParse(textBox_vyhra1.Text, out vyhra))
            {
                // zobrazit nové okno Piskvorky 4
                Window_Piskvorky_AlfaBeta_optimalizace pis5 = new Window_Piskvorky_AlfaBeta_optimalizace(velikost, hloubka, vyhra, true);
                pis5.Show();
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
    }
}
