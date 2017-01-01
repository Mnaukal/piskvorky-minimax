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
using System.ComponentModel;
using System.Net;

namespace Piskvorky
{
    /// <summary>
    /// Interaction logic for Window_Piskvorky.xaml
    /// </summary>
    public partial class Window_Piskvorky_AlfaBeta_optimalizace : Window
    {
        private int VELIKOST = 3;
        private int MAXHLOUBKA = 3;
        private int VYHRA = 5;
        private int[,] plocha;
        private int pocetVolnych;
        private NaTahu naTahu = NaTahu.hrac;
        private Tah vybranyTah;
        private bool konecHry = false;
        private bool Testovani = false;
        int levy, pravy, horni, dolni;
        string zahraneTahy;
        System.Diagnostics.Stopwatch mereniCasu;

        private int[,] Smery = new int[,] { { 0, 1 }, { -1, 1 }, { -1, 0 }, { -1, -1 } }; // různé směry použité v hodnotící funkci: { posun v radku, posun ve sloupci }

        // pro spuštění MiniMaxu na pozadí -> UI se nezasekne
        private readonly BackgroundWorker pozadi = new BackgroundWorker();

        public Window_Piskvorky_AlfaBeta_optimalizace(int velikost, int hloubka, int vyhra, bool testovani)
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

            if (!testovani)
            {
                label_cas.Visibility = Visibility.Hidden;
                checkBox_vypnoutPocitac.Visibility = Visibility.Hidden;
                checkBox_vypnoutPocitac.IsEnabled = false;
            }
            Testovani = testovani;

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

            // spustit nejdřív jen do hloubky 1
            int predchoziHloubka = MAXHLOUBKA;
            MAXHLOUBKA = 1;
            Max(1, int.MinValue, int.MaxValue);
            MAXHLOUBKA = predchoziHloubka;
            if(vybranyTah.Hodnota >= 999990) // výhra
            {
                // je vybrán výherní tah
            }
            else
                Max(1, int.MinValue, int.MaxValue); // hledáme s původní MAX hloubkou
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
                            pozadi.RunWorkerAsync(new Tah(radek, sloupec, 0)); // poslední hráčův tah
                    }
                }
                else
                {
                    MessageBox.Show("Tady není volné políčko! Hrajte jinam.");
                }
            }
            else
            {
                if (naTahu == NaTahu.pocitac && !konecHry && checkBox_vypnoutPocitac.IsChecked == true)
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
            if (pozadi.IsBusy)
                pozadi.CancelAsync(); // přerušení předchozího výpočtu
            // vyčištění hrací plochy
            plocha = new int[VELIKOST, VELIKOST];
            pocetVolnych = VELIKOST * VELIKOST;
            dolni = pravy = 0;
            levy = horni = VELIKOST;
            zahraneTahy = "";
            konecHry = false;
            label_ohodnoceni.Content = "";

            foreach (ContentControl b in grid_hraciPlocha.Children.OfType<ContentControl>())
            {
                b.Content = "";
            }

            naTahu = zacinajici;

            if (zacinajici == NaTahu.pocitac)
            {
                if (checkBox_vypnoutPocitac.IsChecked == false) // není zapnut testovací mód
                {
                    UmistitTah(VELIKOST / 2, VELIKOST / 2); // první tah na střed

                    progressBar_tahPocitace.Visibility = Visibility.Hidden;
                    label_tahPocitace.Visibility = Visibility.Hidden;

                    naTahu = NaTahu.hrac;
                }
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

            if (radek - 2 < horni)
                horni = Math.Max(radek - 2, 0);
            if (radek + 2 > dolni)
                dolni = Math.Min(radek + 2, VELIKOST);
            if (sloupec - 2 < levy)
                levy = Math.Max(sloupec - 2, 0);
            if (sloupec + 2 > pravy)
                pravy = Math.Min(sloupec + 2, VELIKOST);

            zahraneTahy += radek + ":" + sloupec + "; ";

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
                    PosliDataNaServer();
                }

            }
        }

        void PosliDataNaServer()
        {
            WebRequest request = WebRequest.Create("http://thetopfer.com/piskvorky/piskvorky.php?velikost=" + VELIKOST.ToString() + "&hloubka=" + MAXHLOUBKA.ToString() + "&vyhra=" + VYHRA.ToString() + "&tahy=" + zahraneTahy);
            request.Method = "GET";
            WebResponse response = request.GetResponse();
            System.IO.Stream dataStream = response.GetResponseStream();
            System.IO.StreamReader reader = new System.IO.StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            Console.WriteLine(responseFromServer);
            reader.Close();
            dataStream.Close();
            response.Close();
        }

        /// <summary>
        /// Ohodnocení hracího pole podle posledního zahraného tahu
        /// </summary>
        /// <returns>aktuální ohodnocení hracího pole</returns>
        private StavHry Ohodnoceni(bool ukazovatHodnoceniPolicek = false)
        {
            int hodnoceni = 0;

            for (int radek = horni; radek < dolni; radek++)
            {
                for (int sloupec = levy; sloupec < pravy; sloupec++) // pro každé políčko hrací plochy spočítáme ohodnocení a sečteme
                {
                    if (plocha[radek, sloupec] == 0)
                        continue; // prázdné políčko

                    int hodnoceniPolicka = 0;

                    for (int i = 0; i < 4; i++) // vyzkoušíme 4 směry
                    {
                        int symbol = plocha[radek, sloupec];
                        float pocetVRadeDopredu = 0;
                        float pocetVRadeZpet = 0;
                        bool volnyKonecDopredu = false, volnyKonecDozadu = false;

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
                                    //pocetVRadeDopredu += 0.1f;
                                    volnyKonecDopredu = true;
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
                                    //pocetVRadeZpet += 0.1f;
                                    volnyKonecDozadu = true;
                                }
                                break;
                            }
                        }

                        float pocetVRade = pocetVRadeDopredu + pocetVRadeZpet - 1f;

                        if (pocetVRade >= VYHRA) // někdo má 5 v řadě
                        {
                            if (symbol == 1) // počítač prohrál
                                return new StavHry(true, -1000000);
                            else if (symbol == -1) // počítač vyhrál
                                return new StavHry(true, 1000000);
                        }

                        if (!volnyKonecDopredu && !volnyKonecDozadu) // zablokovane z obou stran -> 0 bodů
                        {
                            hodnoceniPolicka += 0;
                        }
                        else if(volnyKonecDopredu && volnyKonecDozadu) // oba konce volné
                        {
                            if(pocetVRade >= 2)
                                hodnoceniPolicka += (int)Math.Pow(7, pocetVRade) * symbol * -1;
                        }
                        else // jeden volný konec
                        {
                            if (pocetVRade >= 2)
                                hodnoceniPolicka += (int)Math.Pow(5, pocetVRade) * symbol * -1;
                        }
                    }

                    hodnoceni += hodnoceniPolicka;

                    if (ukazovatHodnoceniPolicek && Testovani)
                    {
                        Label labelNaPozici = grid_hraciPlocha.Children
                            .OfType<Label>()
                            .Cast<Label>()
                            .First(e => Grid.GetRow(e) == radek && Grid.GetColumn(e) == sloupec);
                        labelNaPozici.Content = hodnoceniPolicka.ToString();
                    }
                }
            }

            if (pocetVolnych == 0)
                return new StavHry(true, 0); //remíza

            return new StavHry(false, hodnoceni); // nedohráno
        }

        /// <summary>
        /// Maximalizační část minimaxu
        /// </summary>
        /// <param name="hloubka">hloubka prohledávání</param>
        /// <returns></returns>
        private int Max(int hloubka, int alfa, int beta)
        {
            if (pozadi.CancellationPending) // přerušení při zavření okna
                return 0;

            StavHry hodnoceni = Ohodnoceni();

            if (hodnoceni.Dohrano == false && hloubka <= MAXHLOUBKA) // nedohráno
            {
                int maximum = int.MinValue;

                // projdi všehcna pole
                /*for (int i = VELIKOST / 2; i < VELIKOST && i >= 0; i = (i - VELIKOST / 2f < 0) ? VELIKOST - i : VELIKOST - i - 1)
                {
                    for (int j = VELIKOST / 2; j < VELIKOST && j >= 0; j = (j - VELIKOST / 2f < 0) ? VELIKOST - j : VELIKOST - j - 1)
                    {*/

                for (int i = (horni + dolni) / 2; i < dolni && i >= horni; i = (i - (horni + dolni) / 2f < 0) ? (horni + dolni) - i : (horni + dolni) - i - 1) // řádky - postupně od středu
                {
                    for (int j = (levy + pravy) / 2; j < pravy && j >= levy; j = (j - (levy + pravy) / 2f < 0) ? (levy + pravy) - j : (levy + pravy) - j - 1) // sloupce - postupně od středu
                    {
                        if (plocha[i, j] == 0) // volné pole
                        {
                            // umístit tah do plochy 
                            plocha[i, j] = -1;
                            pocetVolnych--;

                            // nově nastavit okraje prohledávání (dočasně)
                            int predchoziLevy = levy;
                            int predchoziPravy = pravy;
                            int predchoziHorni = horni;
                            int predchoziDolni = dolni;
                            if (i - 2 < horni)
                                horni = Math.Max(i - 2, 0);
                            if (i + 2 > dolni)
                                dolni = Math.Min(i + 2, VELIKOST);
                            if (j - 2 < levy)
                                levy = Math.Max(j - 2, 0);
                            if (j + 2 > pravy)
                                pravy = Math.Min(j + 2, VELIKOST);

                            //najít další tah
                            int hodnotaTahu = Min(hloubka + 1, alfa, beta); // vnitřní Min
                            alfa = Math.Max(alfa, hodnotaTahu);

                            //if (hloubka == 1)
                            //    Console.WriteLine(new Tah(i, j, hodnotaTahu));

                            if (hodnotaTahu > maximum) // nový nejlepší tah
                            {
                                maximum = hodnotaTahu;
                                    if (hloubka == 1)
                                    {
                                        vybranyTah = new Tah(i, j, hodnotaTahu);
                                        //Console.WriteLine(vybranyTah);
                                    }
                            }

                            // smazat tah z plochy
                            plocha[i, j] = 0; // znovu uvolnit pole 
                            pocetVolnych++;
                            // vrátit okraje prohledávání
                            levy= predchoziLevy;
                            pravy = predchoziPravy;
                            horni = predchoziHorni;
                            dolni = predchoziDolni;

                            if (beta <= alfa)
                                goto konecCyklu;
                        }
                    }
                }
                konecCyklu:

                if (maximum == int.MinValue) //neexistuje žádný tah
                    return 0;

                //if (hloubka == 1)
                //    Console.WriteLine("---------------");
                return maximum + hloubka;
            }
            else // dohráno nebo překročena hloubka
            {
                return hodnoceni.Ohodnoceni + hloubka;
            }
        }

        /// <summary>
        /// Minimalizační část minimaxu
        /// </summary>
        /// <param name="hloubka">hloubka prohledávání</param>
        /// <returns></returns>
        private int Min(int hloubka, int alfa, int beta)
        {
            if (pozadi.CancellationPending) // přerušení při zavření okna
                return 0;

            StavHry hodnoceni = Ohodnoceni();

            if (hodnoceni.Dohrano == false && hloubka <= MAXHLOUBKA) // nedohráno
            {
                int minimum = int.MaxValue;

                // projdi všehcna pole
                for (int i = (horni + dolni) / 2; i < dolni && i >= horni; i = (i - (horni + dolni) / 2f < 0) ? (horni + dolni) - i : (horni + dolni) - i - 1) // řádky
                {
                    for (int j = (levy + pravy) / 2; j < pravy && j >= levy; j = (j - (levy + pravy) / 2f < 0) ? (levy + pravy) - j : (levy + pravy) - j - 1) // sloupce
                    {
                        if (plocha[i, j] == 0) // volné pole
                        {
                            // umístit tah do plochy 
                            plocha[i, j] = 1;
                            pocetVolnych--;

                            // nově nastavit okraje prohledávání (dočasně)
                            int predchoziLevy = levy;
                            int predchoziPravy = pravy;
                            int predchoziHorni = horni;
                            int predchoziDolni = dolni;
                            if (i - 2 < horni)
                                horni = Math.Max(i - 2, 0);
                            if (i + 2 > dolni)
                                dolni = Math.Min(i + 2, VELIKOST);
                            if (j - 2 < levy)
                                levy = Math.Max(j - 2, 0);
                            if (j + 2 > pravy)
                                pravy = Math.Min(j + 2, VELIKOST);

                            //najít další tah
                            int hodnotaTahu = Max(hloubka + 1, alfa, beta); // vnitřní Max
                            beta = Math.Min(beta, hodnotaTahu);

                            if (hodnotaTahu < minimum) // nový nejlepší tah
                            {
                                minimum = hodnotaTahu;
                            }

                            // smazat tah z plochy
                            plocha[i, j] = 0; // znovu uvolnit pole 
                            pocetVolnych++;
                            // vrátit okraje prohledávání
                            levy = predchoziLevy;
                            pravy = predchoziPravy;
                            horni = predchoziHorni;
                            dolni = predchoziDolni;

                            if (beta <= alfa)
                                goto konecCyklu;
                        }
                    }
                }
                konecCyklu:

                if (minimum == int.MaxValue) //neexistuje žádný tah
                    return 0;

                return minimum - hloubka;
            }
            else // dohráno nebo překročena hloubka
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