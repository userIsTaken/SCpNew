﻿using System;
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
            {3.9,0.432933},
            {3.925,0.4451},
            {3.95,0.457299},
            {3.975,0.4672},
            {4,0.477046},
            {4.025,0.4941},
            {4.05,0.511157},
            {4.075,0.5313},
            {4.1,0.551425},
            {4.125,0.5668},
            {4.15,0.58222},
            {4.175,0.5933},
            {4.2,0.604407},
            {4.225,0.6154},
            {4.25,0.626287},
            {4.275,0.6352},
            {4.3,0.644083},
            {4.325,0.6528},
            {4.35,0.661569},
            {4.375,0.6734},
            {4.4,0.685313},
            {4.425,0.6986},
            {4.45,0.711958},
            {4.475,0.7264},
            {4.5,0.740906},
            {4.525,0.7593},
            {4.55,0.777667},
            {4.575,0.7957},
            {4.6,0.813734},
            {4.625,0.829},
            {4.65,0.844312},
            {4.675,0.8596},
            {4.7,0.874955},
            {4.725,0.8874},
            {4.75,0.899801},
            {4.775,0.9102},
            {4.8,0.920673},
            {4.825,0.9309},
            {4.85,0.941189},
            {4.875,0.9512},
            {4.9,0.961218},
            {4.925,0.9681},
            {4.95,0.974934},
            {4.975,0.9813},
            {5,0.98768},
            {5.025,0.9909},
            {5.05,0.994183},
            {5.075,0.9948},
            {5.1,0.995384},
            {5.125,0.9914},
            {5.15,0.987366},
            {5.175,0.9784},
            {5.2,0.969452},
            {5.225,0.9548},
            {5.25,0.940206},
            {5.275,0.9183},
            {5.3,0.896422},
            {5.325,0.8697},
            {5.35,0.842982},
            {5.375,0.8126},
            {5.4,0.782288},
            {5.425,0.752},
            {5.45,0.72168},
            {5.475,0.6925},
            {5.5,0.663229},
            {5.525,0.6375},
            {5.55,0.611805},
            {5.575,0.5888},
            {5.6,0.565795},
            {5.625,0.5451},
            {5.65,0.524362},
            {5.675,0.5069},
            {5.7,0.489361},
            {5.725,0.4711},
            {5.75,0.452752},
            {5.775,0.4345},
            {5.8,0.416151},
            {5.825,0.3985},
            {5.85,0.380825},
            {5.875,0.3656},
            {5.9,0.350353},
            {5.925,0.3356},
            {5.95,0.320936},
            {5.975,0.3101},
            {6,0.299228},
            {6.025,0.292},
            {6.05,0.284845},
            {6.075,0.2799},
            {6.1,0.274961},
            {6.125,0.2709},
            {6.15,0.266922},
            {6.175,0.264},
            {6.2,0.26114},
            {6.225,0.2567},
            {6.25,0.252319},
            {6.275,0.246},
            {6.3,0.239637},
            {6.325,0.2367},
            {6.35,0.233739},
            {6.375,0.2306},
            {6.4,0.227442}
        }; //Užpildyta reikšmėmis apie šviesos intensyvumą.

        public static double GetNiDividedByLight(double Ni, double eV, double time)
        {
            double ats = 0.0d;
            double ev_rounded = Math.Round(eV, 3, MidpointRounding.AwayFromZero);
            //eV reikalinga atrinkimui, kokį skaičių naudoti
            Console.WriteLine("eV : " + eV.ToString());
            Console.WriteLine("eV rounded: " + ev_rounded.ToString());
            if (LightArray.ContainsKey(ev_rounded))
            {
                double light = LightArray[ev_rounded];
                ats = Math.Round(((60.0d/time*Ni) / ((light))), 1, MidpointRounding.AwayFromZero) ;
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
