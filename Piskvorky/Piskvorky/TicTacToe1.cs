using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace Piskvorky
{
    public class TicTacToe1
    {
        public Window_TicTacToe hraciOkno;
        const int VELIKOST = 3;

        private int[,] plocha = new int[VELIKOST, VELIKOST];
        private int pocetVolnych;
        private NaTahu naTahu = NaTahu.hrac;
        private Tah vybranyTah;
        private bool konecHry = false;

        public TicTacToe1(Window_TicTacToe okno)
        {
            hraciOkno = okno;
        }

        public void Start(NaTahu zacinajici)
        {
            // vyčištění hrací plochy
            plocha = new int[VELIKOST, VELIKOST];
            pocetVolnych = VELIKOST * VELIKOST;
            konecHry = false;

            hraciOkno.Dispatcher.BeginInvoke(new Action(() =>
            {
                hraciOkno.label_ohodnoceni.Content = "";

                foreach (Button b in hraciOkno.grid_hraciPlocha.Children)
                {
                    b.Content = "";
                }
            }));

            naTahu = zacinajici;

            if (zacinajici == NaTahu.pocitac)
            {
                // najdi tah
                MiniMax(1);

                //umisti tah
                UmistitTah(vybranyTah.Radek, vybranyTah.Sloupec);

                //změní, kdo je na tahu -> Hráč
                naTahu = NaTahu.hrac;
            }
        }

        /// <summary>
        /// Minimax
        /// </summary>
        /// <param name="minMax">-1 => min; 1 => max</param>
        private int MiniMax(int minMax)
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
                            tahy.Add(new Tah(i, j, MiniMax(-minMax))); // další MiniMax

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
                    return (int)hodnoceni;
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

        private void UmistitTah(int radek, int sloupec)
        {
            hraciOkno.Dispatcher.BeginInvoke(new Action(() =>
            {
                Button tlacitkoNaPozici = hraciOkno.grid_hraciPlocha.Children
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
            }));
            pocetVolnych--;

            if (Ohodnoceni() != 0) //konec hry
            {
                konecHry = true;

                int? hodnoceni = Ohodnoceni();

                hraciOkno.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (hodnoceni == null) // remíza
                    {
                        hraciOkno.label_ohodnoceni.Content = "Remíza!";
                    }
                    else if (naTahu == NaTahu.pocitac) // vyhrál počítač
                    {
                        hraciOkno.label_ohodnoceni.Content = "Prohrál jsi!";
                    }
                    else // vyhrál hráč - tohle se nestane :D
                    {
                        hraciOkno.label_ohodnoceni.Content = "Vyhrál jsi?!";
                    }
                }));
            }
        }

        public void KlikNaPolicko(int radek, int sloupec)
        {

        }
    }
}
