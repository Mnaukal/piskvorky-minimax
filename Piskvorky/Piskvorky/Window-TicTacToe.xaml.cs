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

namespace Piskvorky
{
    /// <summary>
    /// Interaction logic for Window_TicTacToe.xaml
    /// </summary>
    public partial class Window_TicTacToe : Window
    {
        public Window_TicTacToe()
        {
            InitializeComponent();
        }

        private void button_policko_Click(object sender, RoutedEventArgs e)
        {
            Button _btn = sender as Button;

            // řádek a sloupec, na který uživatel klikl
            int radek = (int)_btn.GetValue(Grid.RowProperty);
            int sloupec = (int)_btn.GetValue(Grid.ColumnProperty);
            MessageBox.Show(string.Format("Řádek: {0}, sloupec: {1}", radek, sloupec));
        }
    }
}
