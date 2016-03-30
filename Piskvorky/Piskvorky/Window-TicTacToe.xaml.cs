using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Shapes;

namespace Piskvorky
{
    /// <summary>
    /// Interaction logic for Window_TicTacToe.xaml
    /// </summary>
    public partial class Window_TicTacToe : Window
    {
        public TicTacToe1 ttc1;

        public Window_TicTacToe()
        {
            InitializeComponent();

            Thread vlakno = new Thread(ttc1.Start);
            vlakno.Start(NaTahu.hrac);

            Start(NaTahu.hrac);
        }

        private void button_policko_Click(object sender, RoutedEventArgs e)
        {
            if (naTahu == NaTahu.hrac && !konecHry)
            {
                Button _btn = sender as Button;

                // řádek a sloupec, na který uživatel klikl
                int radek = (int)_btn.GetValue(Grid.RowProperty);
                int sloupec = (int)_btn.GetValue(Grid.ColumnProperty);
                //MessageBox.Show(string.Format("Řádek: {0}, sloupec: {1}", radek, sloupec));

                if (plocha[radek, sloupec] == 0) //políčko je prázdné
                {
                    // umísti hráčův tah
                    UmistitTah(radek, sloupec);

                    //změní, kdo je na tahu
                    naTahu = NaTahu.pocitac;

                    //najdi tah pro počítač
                    MiniMax(1);

                    //umísti tah počítače
                    UmistitTah(vybranyTah.Radek, vybranyTah.Sloupec);

                    //změní, kdo je na tahu -> Hráč
                    naTahu = NaTahu.hrac;
                }
                else
                {
                    MessageBox.Show("Tady není volné políčko! Hrajte jinam.");
                }
            }
        }

        

        public void UmistitTah(int radek, int sloupec)
        {

        }

        private void button_start_hrac_Click(object sender, RoutedEventArgs e)
        {
            Start(NaTahu.hrac);
        }

        private void button_start_pocitac_Click(object sender, RoutedEventArgs e)
        {
            Start(NaTahu.pocitac);
        }
    }

    public enum NaTahu { hrac = 1, pocitac = -1, konec }

    public class Tah
    {
        public int Radek, Sloupec;
        public int Hodnota;

        public Tah()
        {}

        public Tah(int radek, int sloupec, int hodnota)
        {
            Radek = radek;
            Sloupec = sloupec;
            Hodnota = hodnota;
        }

        public override string ToString()
        {
            return Radek + ", " + Sloupec + ": " + Hodnota;
        }
    }
}
