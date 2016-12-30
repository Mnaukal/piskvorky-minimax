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
using System.ComponentModel;

namespace Piskvorky
{
    /// <summary>
    /// Interaction logic for Window_TicTacToe_hloubka.xaml
    /// </summary>
    public partial class Window_TicTacToe_MINMAX_hloubka : Window
    {
        const int VELIKOST = 3;
        private int[,] plocha = new int[VELIKOST, VELIKOST];
        private int pocetVolnych;
        private NaTahu naTahu = NaTahu.hrac;
        private Tah vybranyTah;
        private bool konecHry = false;
        System.Diagnostics.Stopwatch mereniCasu;

        // pro spuštění MiniMaxu na pozadí -> UI se nezasekne
        private readonly BackgroundWorker pozadi = new BackgroundWorker();

        public Window_TicTacToe_MINMAX_hloubka()
        {
            InitializeComponent();

            pozadi.DoWork += Pozadi_DoWork;
            pozadi.RunWorkerCompleted += Pozadi_RunWorkerCompleted;

            Start(NaTahu.hrac);
        }

        // spustit minimax na pozadí
        private void Pozadi_DoWork(object sender, DoWorkEventArgs e)
        {
            // zobrazit informaci o probíhajícím tahu počítače
            this.Dispatcher.Invoke(new Action(() =>
            {
                progressBar_tahPocitace.Visibility = Visibility.Visible;
                label_tahPocitace.Visibility = Visibility.Visible;
            }));

            mereniCasu = System.Diagnostics.Stopwatch.StartNew(); // měření délky běhu minimaxu

            Max(1);
        }

        // spustí se po dokončení úkolu na pozadí -> umístí tah počítače a změní, kdo je na tahu
        private void Pozadi_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            UmistitTah(vybranyTah.Radek, vybranyTah.Sloupec);

            progressBar_tahPocitace.Visibility = Visibility.Hidden;
            label_tahPocitace.Visibility = Visibility.Hidden;

            mereniCasu.Stop();
            label_cas.Content = mereniCasu.ElapsedMilliseconds;

            naTahu = NaTahu.hrac;
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

                    if (checkBox_vypnoutPocitac.IsChecked == false) // není zapnut testovací mód
                    {
                        //spustit MiniMax(1, 1) na pozadí a potom UmistitTah() a změnit, kdo je na tahu
                        if (!konecHry)
                            pozadi.RunWorkerAsync();
                    }
                }
                else
                {
                    MessageBox.Show("Tady není volné políčko! Hrajte jinam.");
                }
            }
            else
            {
                if(naTahu == NaTahu.pocitac && !konecHry && checkBox_vypnoutPocitac.IsChecked == true)
                {
                    Button _btn = sender as Button;
                    int radek = (int)_btn.GetValue(Grid.RowProperty);
                    int sloupec = (int)_btn.GetValue(Grid.ColumnProperty);
                    if (plocha[radek, sloupec] == 0) //políčko je prázdné
                    {
                        UmistitTah(radek, sloupec);
                        naTahu = NaTahu.hrac;
                    }
                    else
                    {
                        MessageBox.Show("Tady není volné políčko! Hrajte jinam.");
                    }
                }
                else
                    MessageBox.Show("Teď nejsi na tahu.");
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
                if (checkBox_vypnoutPocitac.IsChecked == false) // není zapnut testovací mód
                {
                    //spustit MiniMax(1, 1) na pozadí a potom UmistitTah() a změnit, kdo je na tahu
                    pozadi.RunWorkerAsync();
                }
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

                Image i = new Image();
                i.Source = new BitmapImage(new Uri("krizek.png", UriKind.Relative));
                tlacitkoNaPozici.Content = i;
            }
            else if (naTahu == NaTahu.pocitac)
            {
                plocha[radek, sloupec] = -1;

                Image i = new Image();
                i.Source = new BitmapImage(new Uri("kolecko.png", UriKind.Relative));
                tlacitkoNaPozici.Content = i;
            }
            pocetVolnych--;

            if (Ohodnoceni().Dohrano == true) //konec hry
            {
                konecHry = true;

                StavHry hodnoceni = Ohodnoceni();

                if (hodnoceni.Ohodnoceni == 0) // remíza
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

        /// <summary>
        /// Ohodnocení hracího pole
        /// </summary>
        /// <returns>aktuální ohodnocení hracího pole</returns>
        private StavHry Ohodnoceni()
        {
            if (plocha[0, 0] + plocha[0, 1] + plocha[0, 2] == -3 ) //hrac na tahu vyhral
                return new StavHry(true, 10);
            if (plocha[1, 0] + plocha[1, 1] + plocha[1, 2] == -3 )
                return new StavHry(true, 10);
            if (plocha[2, 0] + plocha[2, 1] + plocha[2, 2] == -3 )
                return new StavHry(true, 10);
            if (plocha[0, 0] + plocha[1, 0] + plocha[2, 0] == -3 )
                return new StavHry(true, 10);
            if (plocha[0, 1] + plocha[1, 1] + plocha[2, 1] == -3 )
                return new StavHry(true, 10);
            if (plocha[0, 2] + plocha[1, 2] + plocha[2, 2] == -3 )
                return new StavHry(true, 10);
            if (plocha[0, 0] + plocha[1, 1] + plocha[2, 2] == -3 )
                return new StavHry(true, 10);
            if (plocha[0, 2] + plocha[1, 1] + plocha[2, 0] == -3 )
                return new StavHry(true, 10);

            if (plocha[0, 0] + plocha[0, 1] + plocha[0, 2] == 3) //hrac na tahu prohral
                return new StavHry(true, -10);
            if (plocha[1, 0] + plocha[1, 1] + plocha[1, 2] == 3)
                return new StavHry(true, -10);
            if (plocha[2, 0] + plocha[2, 1] + plocha[2, 2] == 3)
                return new StavHry(true, -10);
            if (plocha[0, 0] + plocha[1, 0] + plocha[2, 0] == 3)
                return new StavHry(true, -10);
            if (plocha[0, 1] + plocha[1, 1] + plocha[2, 1] == 3)
                return new StavHry(true, -10);
            if (plocha[0, 2] + plocha[1, 2] + plocha[2, 2] == 3)
                return new StavHry(true, -10);
            if (plocha[0, 0] + plocha[1, 1] + plocha[2, 2] == 3)
                return new StavHry(true, -10);
            if (plocha[0, 2] + plocha[1, 1] + plocha[2, 0] == 3)
                return new StavHry(true, -10);

            if (pocetVolnych == 0)
                return new StavHry(true, 0); //remíza

            return new StavHry(false); // nedohráno
        }

        /// <summary>
        /// Maximalizační část minimaxu
        /// </summary>
        /// <param name="hloubka">hloubka prohledávání</param>
        /// <returns></returns>
        private int Max(int hloubka)
        {
            StavHry hodnoceni = Ohodnoceni();

            if (hodnoceni.Dohrano == false) // nedohráno
            {
                int maximum = int.MinValue;

                // projdi všehcna pole
                for (int i = 0; i < VELIKOST; i++)
                {
                    for (int j = 0; j < VELIKOST; j++)
                    {
                        if (plocha[i, j] == 0) // volné pole
                        {
                            // umístit tah do plochy 
                            plocha[i, j] = -1;
                            pocetVolnych--;

                            //najít další tah
                            int hodnotaTahu = Min(hloubka + 1); // vnitřní Min

                            if (hloubka == 1) // debug
                                Console.WriteLine(i + ", " + j + ": " + hodnotaTahu);

                            if (hodnotaTahu > maximum) // nový nejlepší tah
                            {
                                maximum = hodnotaTahu;
                                if(hloubka == 1)
                                    vybranyTah = new Tah(i, j, hodnotaTahu);
                            }

                            // smazat tah z plochy
                            plocha[i, j] = 0; // znovu uvolnit pole 
                            pocetVolnych++;
                        }
                    }
                }

                if (maximum == int.MinValue) //neexistuje žádný tah
                    return 0;

                if (hloubka == 1)
                    Console.WriteLine("---------------");
                return maximum + hloubka;
            }
            else
            {
                return hodnoceni.Ohodnoceni + hloubka;
            }
        }

        /// <summary>
        /// Minimalizační část minimaxu
        /// </summary>
        /// <param name="hloubka">hloubka prohledávání</param>
        /// <returns></returns>
        private int Min(int hloubka)
        {
            StavHry hodnoceni = Ohodnoceni();

            if (hodnoceni.Dohrano == false) // nedohráno
            {
                int minimum = int.MaxValue;

                // projdi všehcna pole
                for (int i = 0; i < VELIKOST; i++)
                {
                    for (int j = 0; j < VELIKOST; j++)
                    {
                        if (plocha[i, j] == 0) // volné pole
                        {
                            // umístit tah do plochy 
                            plocha[i, j] = 1;
                            pocetVolnych--;

                            //najít další tah
                            int hodnotaTahu = Max(hloubka + 1); // vnitřní Max

                            if (hodnotaTahu < minimum) // nový nejlepší tah
                            {
                                minimum = hodnotaTahu;
                            }

                            // smazat tah z plochy
                            plocha[i, j] = 0; // znovu uvolnit pole 
                            pocetVolnych++;
                        }
                    }
                }

                if (minimum == int.MaxValue) //neexistuje žádný tah
                    return 0;

                return minimum - hloubka;
            }
            else
            {
                return hodnoceni.Ohodnoceni - hloubka;
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