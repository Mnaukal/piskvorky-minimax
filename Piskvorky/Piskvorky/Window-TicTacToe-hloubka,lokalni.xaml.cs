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

namespace Piskvorky
{
    /// <summary>
    /// Interaction logic for Window_TicTacToe_hloubka_lokalni.xaml
    /// </summary>
    public partial class Window_TicTacToe_hloubka_lokalni : Window
    {
        const int VELIKOST = 3;
        private int[,] plocha = new int[VELIKOST, VELIKOST];
        private int pocetVolnych;
        private NaTahu naTahu = NaTahu.hrac;
        private Tah vybranyTah;
        private bool konecHry = false;

        private int[,] Smery = new int[,] { { 0, 1 }, { -1, 1 }, { -1, 0 }, { -1, -1 } }; // různé směry použité v hodnotící funkci: { posun v radku, posun ve sloupci }

        // pro spuštění MiniMaxu na pozadí -> UI se nezasekne
        private readonly BackgroundWorker pozadi = new BackgroundWorker();

        public Window_TicTacToe_hloubka_lokalni()
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

            MiniMax(1, 1, ((Tah)e.Argument).Radek, ((Tah)e.Argument).Sloupec);
        }

        // spustí se po dokončení úkolu na pozadí -> umístí tah počítače a změní, kdo je na tahu
        private void Pozadi_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            UmistitTah(vybranyTah.Radek, vybranyTah.Sloupec);

            progressBar_tahPocitace.Visibility = Visibility.Hidden;
            label_tahPocitace.Visibility = Visibility.Hidden;

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
                //najdi tah pro počítač
                // MiniMax(1, 1);

                //umísti tah počítače
                // UmistitTah(vybranyTah.Radek, vybranyTah.Sloupec);

                //změní, kdo je na tahu -> Hráč
                // naTahu = NaTahu.hrac;

                //spustit MiniMax(1, 1) na pozadí a potom UmistitTah() a změnit, kdo je na tahu
                pozadi.RunWorkerAsync(new Tah(0, 0, 0)); // první tah do levého horního rohu (na základě předchozích pozorování)
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

            if (Ohodnoceni(radek, sloupec) != 0) //konec hry
            {
                konecHry = true;

                int? hodnoceni = Ohodnoceni(radek, sloupec);

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

        /// <summary>
        /// Ohodnocení hracího pole podle posledního zahraného tahu
        /// </summary>
        /// <param name="radek">řádek posledného umístěného tahu</param>
        /// <param name="sloupec">sloupec posledného umístěného tahu</param>
        /// <returns>aktuální ohodnocení hracího pole</returns>
        private int? Ohodnoceni(int radek, int sloupec)
        {
            for (int i = 0; i < 4; i++) // vyzkoušíme 4 směry
            {
                int symbol = 0;
                int pocetVRade = 0;
                for (int j = -2; j <= 2; j++) // dvě políčka před a dvě políčka za aktuální tah (tvorba trojic)
                {
                    if ((radek + j * Smery[i, 0] < 0) || (radek + j * Smery[i, 0] >= VELIKOST) || (sloupec + j * Smery[i, 1] < 0) || (sloupec + j * Smery[i, 1] >= VELIKOST)) // index je mimo rozsah
                        continue;

                    if (plocha[radek + j * Smery[i, 0], sloupec + j * Smery[i, 1]] == symbol) // symbol (tah hráče) na políčku o j políček daleko v daném směru je stejný jako předchozí symbol
                    {
                        // symbol je stejný -> zvýšit počet v řade
                        pocetVRade++;
                    }
                    else
                    {
                        // symbol je jiný -> počet v řadě je 1
                        symbol = plocha[radek + j * Smery[i, 0], sloupec + j * Smery[i, 1]];
                        pocetVRade = 1;
                    }

                }

                if (pocetVRade >= 3) // trojice
                {
                    if (symbol == -(int)naTahu) // hrac na tahu prohral
                        return -10;
                    else if (symbol == (int)naTahu) // hrac na tahu vyhral
                        return 10;
                    
                }
            }
            if (pocetVolnych == 0)
                return null; //remíza

            return 0; // nedohráno
        }

        /// <summary>
        /// Minimax
        /// </summary>
        /// <param name="minMax">-1 => min; 1 => max</param>
        /// <param name="hloubka">hloubka rekurze</param>
        /// <param name="radek">řádek zkoušeného/posledního tahu</param>
        /// <param name="sloupec">sloupec zkoušeného/posledního tahu</param>
        private int MiniMax(int minMax, int hloubka, int radek, int sloupec)
        {
            int? hodnoceni = Ohodnoceni(radek, sloupec);

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
                            tahy.Add(new Tah(i, j, MiniMax(-minMax, hloubka + 1, i, j))); // další MiniMax

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
