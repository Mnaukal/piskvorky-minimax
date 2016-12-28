using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piskvorky
{
    class StavHry
    {
        public bool Dohrano;
        public int Ohodnoceni;

        public StavHry(bool dohrano, int ohodnoceni)
        {
            Ohodnoceni = ohodnoceni;
            Dohrano = dohrano;
        }

        public StavHry(bool dohrano = false)
        {
            Dohrano = dohrano;
        }
    }

    public class Tah
    {
        public int Radek, Sloupec;
        public int Hodnota;

        public Tah()
        { }

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
