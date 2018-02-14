using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//=====
//naudojama šviesos signalo korekcijai:
//=============================
namespace SPcontrol
{

    public static class LightCorrections
    {
        /**/
        public static Dictionary<double, double> LightArray = new Dictionary<double, double>()
        {
            {3.5,0.248},
            {3.55,0.27},
            {3.6,0.300634},
            {3.65,0.315992},
            {3.7,0.338861},
            {3.75,0.369096},
            {3.8,0.392817},
            {3.85,0.416353},
            {3.9,0.443685},
            {3.95,0.464652},
            {4,0.48543},
            {4.05,0.504318},
            {4.1,0.527994},
            {4.15,0.550564},
            {4.2,0.573004},
            {4.25,0.602795},
            {4.3,0.626751},
            {4.35,0.65179},
            {4.4,0.680258},
            {4.45,0.704332},
            {4.5,0.73043},
            {4.55,0.754078},
            {4.6,0.774472},
            {4.65,0.796915},
            {4.7,0.818053},
            {4.75,0.833379},
            {4.8,0.854546},
            {4.85,0.865762},
            {4.9,0.885987},
            {4.95,0.894255},
            {5,0.915677},
            {5.05,0.924378},
            {5.1,0.94333},
            {5.15,0.944722},
            {5.2,0.958263},
            {5.25,0.953126},
            {5.3,0.962359},
            {5.35,0.947819},
            {5.4,0.941957},
            {5.45,0.918161},
            {5.5,0.909792},
            {5.55,0.886149},
            {5.6,0.87733},
            {5.65,0.847249},
            {5.7,0.835849},
            {5.75,0.805504},
            {5.8,0.794114},
            {5.85,0.762305},
            {5.9,0.733679},
            {5.95,0.696834},
            {6,0.686415},
            {6.05,0.663079},
            {6.15,0.63}
        }; //Užpildyta reikšmėmis apie šviesos intensyvumą.

        public static double GetNiDividedByLight(double Ni, double eV, double time)
        {
            double ats = 0.0d;
            //eV reikalinga atrinkimui, kokį skaičių naudoti
            if(LightArray.ContainsKey(eV))
            {
                double light = LightArray[eV];
                ats = Math.Sqrt(60.0d/time*Ni) / (Math.Sqrt(light));
            }
            else
            {
                ats = Math.Sqrt(60.0d / time * Ni); //Nieko nekeičiam, jei nėra light[eV];, tik grąžiname sqrt(Ni);
            }
            return ats;
        }
    }
}
