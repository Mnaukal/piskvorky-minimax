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

        private void button_ttc1_Click(object sender, RoutedEventArgs e)
        {
            // zobrazit nové okno (TicTacToe)
            Window_TicTacToe ttt1 = new Window_TicTacToe();
            ttt1.Show();
        }

        private void button_ttc2_Click(object sender, RoutedEventArgs e)
        {
            // zobrazit nové okno (TicTacToe) + hloubka
            Window_TicTacToe_hloubka ttt2 = new Window_TicTacToe_hloubka();
            ttt2.Show();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult vypnout = MessageBox.Show("Uzavření tohoto okna vypne všechny proníhající hry. Opravdu chcete skončit?", "Konec?", MessageBoxButton.YesNo, MessageBoxImage.Warning);      

            if (vypnout == MessageBoxResult.Yes)
                Application.Current.Shutdown(); 
            else
                e.Cancel = true;
        }
    }
}
