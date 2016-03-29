﻿using System;
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
    /// Interaction logic for Window_TicTacToe_hloubka.xaml
    /// </summary>
    public partial class Window_TicTacToe_hloubka : Window
    {
        const int VELIKOST = 3;
        private int[,] plocha = new int[VELIKOST, VELIKOST];
        private int pocetVolnych;
        private NaTahu naTahu = NaTahu.hrac;
        private Tah vybranyTah;
        private bool konecHry = false;

        public Window_TicTacToe_hloubka()
        {
            InitializeComponent();
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
                    MiniMax(1, 1);

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

        private void Start(NaTahu zacinajici)
        {
            // vyčištění hrací plochy
            plocha = new int[VELIKOST, VELIKOST];
            pocetVolnych = VELIKOST * VELIKOST;
            konecHry = false;
            label_ohodnoceni.Content = "";

            foreach (Button b in grid_hraciPlocha.Children)
            {
                b.Content = "";
            }

            naTahu = zacinajici;

            if (zacinajici == NaTahu.pocitac)
            {
                // najdi tah
                MiniMax(1, 1);

                //umisti tah
                UmistitTah(vybranyTah.Radek, vybranyTah.Sloupec);

                //změní, kdo je na tahu -> Hráč
                naTahu = NaTahu.hrac;
            }
        }

        private void UmistitTah(int radek, int sloupec)
        {
            Button tlacitkoNaPozici = grid_hraciPlocha.Children
                .Cast<Button>()
                .First(e => Grid.GetRow(e) == radek && Grid.GetColumn(e) == sloupec);

            if (naTahu == NaTahu.hrac)
            {
                plocha[radek, sloupec] = 1;
                tlacitkoNaPozici.Content = "X";
                tlacitkoNaPozici.Foreground = Brushes.Red;
            }
            else if (naTahu == NaTahu.pocitac)
            {
                plocha[radek, sloupec] = -1;
                tlacitkoNaPozici.Content = "O";
                tlacitkoNaPozici.Foreground = Brushes.Blue;
            }
            pocetVolnych--;

            if (Ohodnoceni() != 0) //konec hry
            {
                konecHry = true;

                int? hodnoceni = Ohodnoceni();

                if (hodnoceni == null) // remíza
                {
                    label_ohodnoceni.Content = "Remíza!";
                }
                else if (naTahu == NaTahu.pocitac) // vyhrál počítač
                {
                    label_ohodnoceni.Content = "Prohrál jsi!";
                }
                else // vyhrál hráč - tohle se nestane :D
                {
                    label_ohodnoceni.Content = "Vyhrál jsi?!";
                }

            }
        }

        private int? Ohodnoceni()
        {
            if (plocha[0, 0] + plocha[0, 1] + plocha[0, 2] == 3 * (int)naTahu) //hrac na tahu vyhral
                return 10;
            if (plocha[1, 0] + plocha[1, 1] + plocha[1, 2] == 3 * (int)naTahu)
                return 10;
            if (plocha[2, 0] + plocha[2, 1] + plocha[2, 2] == 3 * (int)naTahu)
                return 10;
            if (plocha[0, 0] + plocha[1, 0] + plocha[2, 0] == 3 * (int)naTahu)
                return 10;
            if (plocha[0, 1] + plocha[1, 1] + plocha[2, 1] == 3 * (int)naTahu)
                return 10;
            if (plocha[0, 2] + plocha[1, 2] + plocha[2, 2] == 3 * (int)naTahu)
                return 10;
            if (plocha[0, 0] + plocha[1, 1] + plocha[2, 2] == 3 * (int)naTahu)
                return 10;
            if (plocha[0, 2] + plocha[1, 1] + plocha[2, 0] == 3 * (int)naTahu)
                return 10;

            if (plocha[0, 0] + plocha[0, 1] + plocha[0, 2] == -3 * (int)naTahu) //hrac na tahu prohral
                return -10;
            if (plocha[1, 0] + plocha[1, 1] + plocha[1, 2] == -3 * (int)naTahu)
                return -10;
            if (plocha[2, 0] + plocha[2, 1] + plocha[2, 2] == -3 * (int)naTahu)
                return -10;
            if (plocha[0, 0] + plocha[1, 0] + plocha[2, 0] == -3 * (int)naTahu)
                return -10;
            if (plocha[0, 1] + plocha[1, 1] + plocha[2, 1] == -3 * (int)naTahu)
                return -10;
            if (plocha[0, 2] + plocha[1, 2] + plocha[2, 2] == -3 * (int)naTahu)
                return -10;
            if (plocha[0, 0] + plocha[1, 1] + plocha[2, 2] == -3 * (int)naTahu)
                return -10;
            if (plocha[0, 2] + plocha[1, 1] + plocha[2, 0] == -3 * (int)naTahu)
                return -10;

            if (pocetVolnych == 0)
                return null; //remíza

            return 0; //nedohráno
        }

        /// <summary>
        /// Minimax
        /// </summary>
        /// <param name="minMax">-1 => min; 1 => max</param>
        private int MiniMax(int minMax, int hloubka)
        {
            int? hodnoceni = Ohodnoceni();

            if (hodnoceni == 0)
            {
                List<Tah> tahy = new List<Tah>();

                // projdi všehcna pole
                for (int i = 0; i < VELIKOST; i++)
                {
                    for (int j = 0; j < VELIKOST; j++)
                    {
                        if (plocha[i, j] == 0) // volné pole
                        {
                            // umístit tah do plochy 
                            plocha[i, j] = -minMax; //(int)naTahu; // umístit tah
                            pocetVolnych--;
                            //naTahu = (NaTahu)(-(int)naTahu); // změnit hráče na tahu

                            //najít další tah
                            tahy.Add(new Tah(i, j, MiniMax(-minMax, hloubka + 1))); // další MiniMax

                            // smazat tah z plochy
                            plocha[i, j] = 0; // znovu uvolnit pole 
                            pocetVolnych++;
                            //naTahu = (NaTahu)(-(int)naTahu); // vrátit hráče na tahu
                        }
                    }
                }

                if (tahy.Count == 0) //neexistuje žádný tah
                    return 0;

                //najdi maximum/minimum
                int maximum = int.MinValue;
                for (int i = 0; i < tahy.Count; i++)
                {
                    if (tahy[i].Hodnota * minMax > maximum) // * minMax (+1/-1) mění hledání minima a maxima -> po vynásobení je hodnocení vždy kladné
                    {
                        maximum = tahy[i].Hodnota * minMax; // nové maximum
                        vybranyTah = tahy[i];
                    }
                }
                return maximum * minMax;
            }
            else
            {
                if (hodnoceni == null) // remíza
                    return 0;
                else
                    return (int)hodnoceni - hloubka;
            }
        }

        public enum NaTahu { hrac = 1, pocitac = -1, konec }

        private void button_start_hrac_Click(object sender, RoutedEventArgs e)
        {
            Start(NaTahu.hrac);
        }

        private void button_start_pocitac_Click(object sender, RoutedEventArgs e)
        {
            Start(NaTahu.pocitac);
        }
    }
}
