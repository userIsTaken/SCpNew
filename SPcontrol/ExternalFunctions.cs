using System;
using System.Globalization;

namespace SPcontrol
{
    public class ExternalFunctions
    {
        public static string returnCompactString(string str)
        {
            string answer = "";
            answer = str.Trim().Replace(" ", "");
            return answer;
        }
        public static byte[] HexStringToBytes(string hexString)
        {
            if (hexString == null)
                throw new ArgumentNullException("hexString");
            if (hexString.Length % 2 != 0)
                throw new ArgumentException("hexString must have an even length", "hexString");
            var bytes = new byte[hexString.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                string currentHex = hexString.Substring(i * 2, 2);
                bytes[i] = Convert.ToByte(currentHex, 16);
            }
            return bytes;
        }
        public static string byteArrayToHexString(byte[] bytes)
        {
            string answer = String.Empty;
            foreach (byte b in bytes)
            {
                answer = answer + b.ToString().ToLower() + " ";
            }
            return answer;
        }
        public static string returnStringfromArray(object[] array)
        {
            string answer = "";
            foreach (var i in array)
            {
                answer = answer + i.ToString() + " ";
            }
            return answer;
        }
        public static string returnStringfromArray(byte[] array)
        {
            string answer = "";
            foreach (var i in array)
            {
                answer = answer + i.ToString() + " ";
            }
            return answer;
        }

        public static byte[] getIr(decimal ir_decimal)          
        {
            //Šita funkcija neaški - kokia Ir vertė, skirtingose skalėse, ribos, etc?
            //max Iin vertė - 0xFFF(4095);
            uint y = (uint)(ir_decimal * 4095M / 100.0M);//Iin vertė čia yra procentinė išraiška; 
            byte[] Ir = new byte[2] { 0, 0 };
            Ir = BitConverter.GetBytes(y);
            return Ir;
        }

        public static byte returnCthVoltage(decimal volts)
        {
            //arba tiesiog dauginti iš 77,5757 pateiktą reikšmę;
            byte voltage = 0;
            if (volts <= 3.3M)
            {
                voltage =(byte)( 256.0M / 3.3M * volts);
            }
            return voltage;
        }
        public static string getErrorCode(byte code)
        {
            string err = String.Empty;
            switch (code)
            {
                case 0:
                    {
                        err = "Err:0 - OK.";
                        break;
                    }
                case 1:
                    {
                        err = "Err:1 - CRC klaida.";
                        break;
                    }
                case 2:
                    {
                        err = "Err:2 - bloga komanda";
                        break;
                    }
                case 3:
                    {
                        err = "Err:3 - blogas parametras";
                        break;
                    }
                case 4:
                    {
                        err = "Err:4 - protokolo time out'as";
                        break;
                    }
                case 5:
                    {
                        err = "Err:5 - U (įtampa) per maža";
                        break;
                    }
                case 6:
                    {
                        err = "Err:6 - U (įtampa) per didelė";
                        break;
                    }
                default:
                    {
                        err = "Err:-1 - " + code.ToString();
                        break;
                    }
            }
            return err;
        }
        /*Funkcijos, skirtos apdoroti informaciją, prašymui pradėti skaičiavimą*/
        public static byte getTs(double Ts_double)
        {
            byte Ts = 0;
            /*kas 100 ms, max 25,5 s kas atitinka 0xff , 0 - nepertraukiamas skaičiavimas*/
            //Ts - sekundėmis, dauginant iš 10, gaunamas skaičius kas 100 ms
            Ts = (byte)(Ts_double * 10.0d);
            return Ts;
        }
        public static byte getTz(double tz_double)
        {
            /*baitas, kas 10 us, 0xff atitinka 2,55 ms*/
            //paduodama ms, dauginant iš 100, gaunama skaičius kas 10 us;
            byte Tz = 0;
            Tz = (byte)(tz_double * 100.0d);
            return Tz;
        }
        public static byte[] getTq ( double tq_double)
        {
            //Padauginus iš 100, gaunamas skaičius, rodantis laiko vertę kas 10 us
            //0xFFFF atitinka ~ 0,65 s = 650 ms;
            byte[] Tq = new byte[2] { 0, 0 };
            Tq = BitConverter.GetBytes(tq_double*100.0d);
            return Tq;
        }
        public static byte getVq(double voltage)
        {
            /*Gesinimo įtampa Vq, 0-0xFF, 0-100 V*/
            byte Vq;
            Vq =(byte) Convert.ToUInt16(voltage/0.392d); //0xFF atitinka 100 V;
            return Vq;
        }

        public static byte[] get42Vset35array(double voltageIn42Vset35, bool useAorB)
        {
            double volts2 = 0;
            double volts = 0;
            //0 atitinka 4,2 kV, 0xFFF atitinka 2 kV;
            //konversija:
            //x = 4095*(4,2kV-U kV)/2,2kV;
            //=====Šita f-ja iki korekcijos
            // double volts = 4095 * (4200 - voltageIn42Vset35*1000) / 2200; //gaunama norimas įrašyti skaičius
            //Šita funkcija po korekcijos:
            if (useAorB)
            {
                volts2 = (voltageIn42Vset35 * 1000.0d + 143.66d) / 1.0158d;
                volts = 4095 * (4200.0d - volts2) / 2200.0d; //μController A
            }
            if (!useAorB)
            {
                volts2 = (voltageIn42Vset35 * 1000.0d + 2.5435d) / 1.012d;
                volts = 4095 * (4100.0d - volts2) / 2200.0d; //μController B
            }
            //volts = 4095 * (4200.0d - volts2) / 2200.0d;
            int U = (int)volts;
            byte[] voltage = new byte[2];
            voltage = BitConverter.GetBytes(Convert.ToUInt16(U));
            return voltage;
        }

        public static string retStringEqualSize(string msg, int length = 12)
        {
            string cmd=String.Empty;
            int same_length = length;
            //ilgis visąlaik turės būti 12 simbolių;
            int size = msg.Length;
            //Console.WriteLine(msg.Length);
            if(msg.Contains("Err"))
            {
                length = 30;
            }
            else
            {
                length = same_length; //nekeičiam
            }
            
            if (size < length)
            {
                cmd = msg;
                //Console.WriteLine(msg + " čia tas msg 176 EF");
                for(int a = 0; a<(length-size); a++)
                {
                    cmd = cmd + " ";
                }
            }
            else
            {

                cmd = msg;

            }
            //Console.WriteLine("cmd naujas ilgis EF 181 : " + cmd.Length.ToString()+" "+cmd+"|");
            cmd = cmd + "|";
            return cmd;
        }

        public static string getNiPerMinute(double Ni, double t_in_seconds)
        {
            string NiPerminute = String.Empty;
            double x =Math.Round(( 60.0d * Ni / t_in_seconds), 0, MidpointRounding.AwayFromZero); //Ni per minutę, jei t - sekundėmis! tik sveiki skaičiai;
            NiPerminute = Convert.ToString(x, CultureInfo.InvariantCulture);
            return NiPerminute;
        }
        public static double getDoubleNIperMinute (double Ni, double t_in_seconds)
        {
            double x = 60.0d * Ni / t_in_seconds; //Ni per minutę, jei t - sekundėmis!
            return x;
        }

        public static string SqrtNiPerMinute(double Ni, double t_in_seconds)
        {
            string ats = String.Empty;
            double x =Math.Round((Math.Sqrt( 60.0d * Ni / t_in_seconds)), 0); //Ni per minutę, jei t - sekundėmis!
            ats = Convert.ToString(x, CultureInfo.InvariantCulture);
            return ats;
        }

        public static string GetLongStringFromObjects(params object[] obj)
        {
            string ats = String.Empty;
            foreach ( var i in obj)
            {
                ats = ats + Convert.ToString(i, CultureInfo.InvariantCulture) + "  ";
            }
            return ats+";"+Environment.NewLine;
        }

        public static double[] getStepStartingEndingEnergy(bool Forward, double startingEnergy, double endingEnergy, double stepSize)
        {
            double[] answ = new double[3] { 0, 0, 0 };
            double strE=0;
            double endE=0;
            double step = stepSize;
            if(Forward)
            {
                strE = startingEnergy;
                endE = endingEnergy;
                step = stepSize;
            }
            else if (!Forward)
            {
                strE = endingEnergy;
                endE = startingEnergy;
                step = -stepSize;
            }
            answ[0] = step;
            answ[1] = strE;
            answ[2] = endE;
            return answ;
        }
        public static bool Continue (double strE, double endE, double CurrentE, bool Forward, double step=0.1d)
        {
            bool Continue = false;
            if(Forward)
            {
                if(CurrentE > endE)
                {
                    //stabdom ciklą:
                    Continue = false;
                }
                else
                {
                    Continue = true;
                }
            }
            else if(!Forward)
            {
                if (CurrentE < strE)
                {
                    //stabdom ciklą:
                    Continue = false;
                }
                else
                {
                    Continue = true;
                }
            }
            return Continue;
        }
        public static double returnCounts(double kiekis, double nuo, double iki, double step)
        {
            double counts = 0;
            counts = kiekis * 2 + ((iki - nuo) / step+1) * kiekis;
            return counts;
        }
    }

    
}
