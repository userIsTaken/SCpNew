using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPcontrol
{
    
    public class ReadingParameters
    {
        public byte[] Answer = new byte[26]; //Čia bus saugomas atsakymas baitais;
        public SerialPort Port;
        public double Ts; //kas žinotume, kiek truks matavimas, sekundėmis!;
        public int ErrStatus; //klaidos kodas skaičiumi tikrinimams; 0 - OK, 1 - not OK;
        public string Exception; //bus saugoma visa klaida
        public string ExceptionMessage; //Trumpesnis klaidos variantas
        public byte[] Cmd;
        public int WaitTime;//?

        public ReadingParameters()
        {
            //==================================================
            //default constructor;
        }
    }
}
