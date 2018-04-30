using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SPcontrol
{
    public class IzoE
    {
        string MAP = "IzoEnergetic.txt";

        public Dictionary<double, double> mapDict = new Dictionary<double, double>();

        public IzoE()
        {
            readFillDictionary();
        }

        protected void readFillDictionary()
        {
            //
            try
            {
                StreamReader rdr = new StreamReader(MAP);
                string FileInhalt = rdr.ReadToEnd();
                string LINE_ENDING = Environment.NewLine;
                rdr.Close();
                char[] SplitOn = { ',' };
                string[] fSplitted = FileInhalt.Replace(LINE_ENDING, ",").Split(SplitOn);
                int length = fSplitted.Length;
                for (int i = 0; i < length - 2; i = i + 2)
                {
                    mapDict.Add(Convert.ToDouble(fSplitted[i], CultureInfo.InvariantCulture), Convert.ToDouble(fSplitted[i + 1], CultureInfo.InvariantCulture));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("DEBUG IzoE:"+ex.ToString());
            }
        }

        public double GetNiDividedByLight(double Ni, double eV, double time)
        {
            double ats = 0.0d;
            double ev_rounded = Math.Round(eV, 3, MidpointRounding.AwayFromZero);
            //eV reikalinga atrinkimui, kokį skaičių naudoti
            Console.WriteLine("eV : " + eV.ToString());
            Console.WriteLine("eV rounded: " + ev_rounded.ToString());
            if (mapDict.ContainsKey(ev_rounded))
            {
                double light = mapDict[ev_rounded];
                ats = Math.Round(((60.0d / time * Ni) / ((light))), 1, MidpointRounding.AwayFromZero);
                Console.WriteLine("SELECTED LIGHT VALUE " + light.ToString());
            }
            else
            {
                Console.WriteLine("THERE WASN'T LIGHT");
                ats = (60.0d / time * Ni); //Nieko nekeičiam, jei nėra light[eV];, tik grąžiname (Ni);
            }
            return ats;
        }


    }
}
