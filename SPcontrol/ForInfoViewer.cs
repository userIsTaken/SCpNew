using System;

namespace SPcontrol
{
    public class ForInfoViewer
    {
        public static string newLine = Environment.NewLine;
        public static string FirstLine()
        {
            string answ1 = String.Empty;
            string answ = String.Empty;
            answ1 = ExternalFunctions.retStringEqualSize("Eil.Nr.", 10) + ExternalFunctions.retStringEqualSize("hv, [eV]", 10) + ExternalFunctions.retStringEqualSize("Ni vertė", 10) + ExternalFunctions.retStringEqualSize("Ni/min.", 10) + ExternalFunctions.retStringEqualSize("Sqrt(Ni)", 10) + ExternalFunctions.retStringEqualSize("Įtampa, kV", 10);
            answ = answ1 + ExternalFunctions.retStringEqualSize("Dujos",8) + ExternalFunctions.retStringEqualSize("Laikas",10)+newLine;
            return answ;
        }
        public static string AddString( params object[] obj)
        {
            string answ = String.Empty;
            foreach(var i in obj)
            {
                string a = i.ToString();
                if (a.Length > 8)
                {
                    a = a.Substring(0, 4);
                }
                answ = answ + ExternalFunctions.retStringEqualSize(a, 10);
            }
            answ = answ + newLine;
            return answ;
        }
        public ForInfoViewer()
        {
            //def constr.
        }

        public static string GetLongDate()
        {
            string data = string.Empty;
            string dataYear = DateTime.Now.ToShortDateString();
            string dataTime = DateTime.Now.ToShortTimeString();
            string dY = dataYear.Replace("/", String.Empty).Replace("-", String.Empty).Replace(".",String.Empty);
            string dT = dataTime.Replace(":", "h").Replace(" ", String.Empty);
            data = dY + "_" + dT;
            return data;
        }
    }
}
