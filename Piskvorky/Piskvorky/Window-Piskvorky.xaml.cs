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
    /// Interaction logic for Window_Piskvorky.xaml
    /// </summary>
    public partial class Window_Piskvorky : Window
    {
        private int VELIKOST = 3;
        private int MAXHLOUBKA = 3;
        private int VYHRA = 5;
        private int[,] plocha;
        private int pocetVolnych;
        private NaTahu naTahu = NaTahu.hrac;
        private Tah vybranyTah;
        private bool konecHry = false;
        System.Diagnostics.Stopwatch mereniCasu;

        private int[,] Smery = new int[,] { { 0, 1 }, { -1, 1 }, { -1, 0 }, { -1, -1 } }; // různé směry použité v hodnotící funkci: { posun v radku, posun ve sloupci }

        // pro spuštění MiniMaxu na pozadí -> UI se nezasekne
        private readonly BackgroundWorker pozadi = new BackgroundWorker();

        public Window_Piskvorky(int velikost, int hloubka, int vyhra)
        {
            InitializeComponent();
            
            //nastavit velikost, hloubku a výhru
            VELIKOST = velikost;
            MAXHLOUBKA = hloubka;
            VYHRA = vyhra;

            //vytvořit UI
            for (int i = 0; i < velikost; i++)
            {
                grid_hraciPlocha.RowDefinitions.Add(new RowDefinition());
                grid_hraciPlocha.ColumnDefinitions.Add(new ColumnDefinition());
            }
            grid_hraciPlocha.Width = velikost * 30;
            grid_hraciPlocha.Height = velikost * 30;

            //vytvořit políčka hrací plochy (tlačítka)
            for (int i = 0; i < velikost; i++)
            {
                for (int j = 0; j < velikost; j++)
                {
                    Button b = new Button();
                    b.Name = "button_policko_" + i + "_" + j;
                    b.Content = "";
                    Grid.SetColumn(b, i); // sloupec
                    Grid.SetRow(b, j); // řádek
                    b.Click += button_policko_Click;
                    b.Background = Brushes.White;
                    b.BorderBrush = new SolidColorBrush(Color.FromRgb(68, 68, 68));
                    b.BorderThickness = new Thickness(1, 1, 1, 1);
                    b.FontSize = 20;

                    grid_hraciPlocha.Children.Add(b);

                    Label l = new Label();
                    l.Name = "label_policko_" + i + "_" + j;
                    l.Content = "";
                    l.HorizontalContentAlignment = HorizontalAlignment.Center;
                    l.VerticalContentAlignment = VerticalAlignment.Center;
                    l.Padding = new Thickness(0, 0, 0, 0);
                    Grid.SetColumn(l, i); // sloupec
                    Grid.SetRow(l, j); // řádek
                    l.Background = Brushes.Transparent;
                    l.BorderBrush = Brushes.Transparent;
                    l.FontSize = 12;
                    l.IsHitTestVisible = false;
                    l.Foreground = Brushes.Green;

                    grid_hraciPlocha.Children.Add(l);
                }
            }

            pozadi.DoWork += Pozadi_DoWork;
            pozadi.RunWorkerCompleted += Pozadi_RunWorkerCompleted;
            pozadi.WorkerSupportsCancellation = true;

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

            MiniMax(1, 1);
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

                    //najdi tah pro počítač
                    // MiniMax(1, 1);

                    //umísti tah počítače
                    // UmistitTah(vybranyTah.Radek, vybranyTah.Sloupec);

                    //změní, kdo je na tahu -> Hráč
                    // naTahu = NaTahu.hrac;

                    //spustit MiniMax(1, 1) na pozadí a potom UmistitTah() a změnit, kdo je na tahu
                    if(!konecHry)
                        pozadi.RunWorkerAsync(new Tah(radek, sloupec, 0)); // poslední hráčův tah
                }
                else
                {
                    MessageBox.Show("Tady není volné políčko! Hrajte jinam.");
                }
            }
            else
            {
                MessageBox.Show("Teď nejsi na tahu.");
            }
        }

        private void Start(NaTahu zacinajici)
        {
            if (pozadi.IsBusy)
                pozadi.CancelAsync(); // přerušení předchozího výpočtu
            // vyčištění hrací plochy
            plocha = new int[VELIKOST, VELIKOST];
            pocetVolnych = VELIKOST * VELIKOST;
            konecHry = false;
            label_ohodnoceni.Content = "";

            foreach (ContentControl b in grid_hraciPlocha.Children.OfType<ContentControl>())
            {
                b.Content = "";
            }

            naTahu = zacinajici;

            if (zacinajici == NaTahu.pocitac)
            {
                //spustit MiniMax(1, 1) na pozadí a potom UmistitTah() a změnit, kdo je na tahu
                pozadi.RunWorkerAsync();
            }
        }

        private void UmistitTah(int radek, int sloupec)
        {
            Button tlacitkoNaPozici = grid_hraciPlocha.Children
                .OfType<Button>()
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

            if (Ohodnoceni(true).Dohrano == true) //konec hry
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
        /// Ohodnocení hracího pole podle posledního zahraného tahu
        /// </summary>
        /// <returns>aktuální ohodnocení hracího pole</returns>
        private StavHry Ohodnoceni(bool ukazovatHodnoceniPolicek = false)
        {
            int hodnoceni = 0;

            for (int radek = 0; radek < VELIKOST; radek++)
            {
                for (int sloupec = 0; sloupec < VELIKOST; sloupec++) // pro každé políčko hrací plochy spočítáme ohodnocení a sečteme
                {
                    if (plocha[radek, sloupec] == 0)
                        continue; // prázdné políčko

                    int hodnoceniPolicka = 0;

                    for (int i = 0; i < 4; i++) // vyzkoušíme 4 směry
                    {
                        int symbol = plocha[radek, sloupec];
                        float pocetVRadeDopredu = 0;
                        float pocetVRadeZpet = 0;

                        for (int j = 0; j <= VYHRA; j++) // až počet výherních symbolů v řadě (5) polí "dopředu" 
                        {
                            if ((radek + j * Smery[i, 0] < 0) || (radek + j * Smery[i, 0] >= VELIKOST) || (sloupec + j * Smery[i, 1] < 0) || (sloupec + j * Smery[i, 1] >= VELIKOST)) // index je mimo rozsah
                                break;

                            if (plocha[radek + j * Smery[i, 0], sloupec + j * Smery[i, 1]] == symbol) // symbol (tah hráče) na políčku o j políček daleko v daném směru je stejný jako předchozí symbol
                            {
                                // symbol je stejný -> zvýšit počet v řade
                                pocetVRadeDopredu++;
                            }
                            else
                            {
                                if (plocha[radek + j * Smery[i, 0], sloupec + j * Smery[i, 1]] == 0) // tah není na konci zablokován
                                {
                                    pocetVRadeDopredu += 0.1f;
                                }
                                break;
                            }
                        }

                        for (int j = 0; j >= -VYHRA; j--) // až počet výherních symbolů v řadě (5) polí "zpět"
                        {
                            if ((radek + j * Smery[i, 0] < 0) || (radek + j * Smery[i, 0] >= VELIKOST) || (sloupec + j * Smery[i, 1] < 0) || (sloupec + j * Smery[i, 1] >= VELIKOST)) // index je mimo rozsah
                                break;

                            if (plocha[radek + j * Smery[i, 0], sloupec + j * Smery[i, 1]] == symbol) // symbol (tah hráče) na políčku o j políček daleko v daném směru je stejný jako předchozí symbol
                            {
                                // symbol je stejný -> zvýšit počet v řade
                                pocetVRadeZpet++;
                            }
                            else
                            {
                                if (plocha[radek + j * Smery[i, 0], sloupec + j * Smery[i, 1]] == 0) // tah není na konci zablokován
                                {
                                    pocetVRadeZpet += 0.1f;
                                }
                                break;
                            }
                        }

                        float pocetVRade = pocetVRadeDopredu + pocetVRadeZpet - 1f;
                        hodnoceniPolicka += (int)Math.Pow(10, pocetVRade) * symbol * -1;

                        if (pocetVRade >= VYHRA) // někdo má 5 v řadě
                        {
                            if (symbol == -(int)naTahu) // hrac na tahu prohral
                                return new StavHry(true, -100000);
                            else if (symbol == (int)naTahu) // hrac na tahu vyhral
                                return new StavHry(true, 100000);
                        }
                    }

                    hodnoceni += hodnoceniPolicka;

                    if (ukazovatHodnoceniPolicek)
                    {
                        Label labelNaPozici = grid_hraciPlocha.Children
                            .OfType<Label>()
                            .Cast<Label>()
                            .First(e => Grid.GetRow(e) == radek && Grid.GetColumn(e) == sloupec);
                        labelNaPozici.Content = hodnoceniPolicka.ToString();
                    }
                }
            }

            if (ukazovatHodnoceniPolicek)
                Console.WriteLine(hodnoceni);

            if (pocetVolnych == 0)
                return new StavHry(true, 0); //remíza

            return new StavHry(false, hodnoceni); // nedohráno
        }

        /// <summary>
        /// Minimax
        /// </summary>
        /// <param name="minMax">-1 => min; 1 => max</param>
        /// <param name="hloubka">hloubka rekurze</param>
        /// <param name="radek">řádek zkoušeného/posledního tahu</param>
        /// <param name="sloupec">sloupec zkoušeného/posledního tahu</param>
        private int MiniMax(int minMax, int hloubka)
        {
            if (pozadi.CancellationPending) // přerušení při zavření okna
                return 0;

            StavHry hodnoceni = Ohodnoceni();

            if (hodnoceni.Dohrano == false && hloubka <= MAXHLOUBKA) // nedohráno a nepřekročena hloubka
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
                    if (hloubka == 1)
                        Console.WriteLine(tahy[i].Radek + ", " + tahy[i].Sloupec + ": " + tahy[i].Hodnota);

                    if (tahy[i].Hodnota * minMax > maximum) // * minMax (+1/-1) mění hledání minima a maxima -> po vynásobení je hodnocení vždy kladné
                    {
                        maximum = tahy[i].Hodnota * minMax; // nové maximum
                        vybranyTah = tahy[i];
                    }
                }
                if (hloubka == 1)
                    Console.WriteLine("---------------");
                return (maximum - hloubka) * minMax;
            }
            else // dohráno nebo překročena hloubka
            {
                return hodnoceni.Ohodnoceni + hloubka * minMax;
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ScrollViewer_hraciPlocha.ScrollToVerticalOffset(ScrollViewer_hraciPlocha.ScrollableHeight / 2);
            ScrollViewer_hraciPlocha.ScrollToHorizontalOffset(ScrollViewer_hraciPlocha.ScrollableWidth / 2);
        }

        private void ScrollViewer_hraciPlocha_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer_hraciPlocha_scale.ScaleX += e.Delta > 0 ? 0.1 : -0.1;
            ScrollViewer_hraciPlocha_scale.ScaleY += e.Delta > 0 ? 0.1 : -0.1;

            if (ScrollViewer_hraciPlocha_scale.ScaleX < 0.1)
                ScrollViewer_hraciPlocha_scale.ScaleX = 0.1;
            if (ScrollViewer_hraciPlocha_scale.ScaleY < 0.1)
                ScrollViewer_hraciPlocha_scale.ScaleY = 0.1;

            e.Handled = true;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (pozadi.IsBusy)
                pozadi.CancelAsync();
        }
    }
}