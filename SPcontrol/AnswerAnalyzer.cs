using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPcontrol
{
    public class AnswerAnalyzer
    {

        public static string getBigAnswer(byte[] bytes)
        {
            string getAnswer = String.Empty;
            int indexShift = 4;
            /**/
            string newLine = Environment.NewLine;
            string empty = "\t";
            uint Ni =BitConverter.ToUInt32(bytes, 0x0+indexShift);
            uint Tn = BitConverter.ToUInt16(bytes, 0x4+indexShift); //kas 100 ms čia, 0xFFFF - riba, po kurios stoja
            double Tn_in_seconds = (double)Tn * 0.1; //Tn dabar čia bus sekundėmis, jį grąžiname;
            uint HV45 = BitConverter.ToUInt16(bytes, 0x6 + indexShift); //HV45, dauginant 1,0498V gaunama įtampa nuo 0 iki 4,5 kV;
            double HV45_d = (double)HV45 * 1.0498; //00HV45 šaltinio įtampa 0-4,5kV skalėje;
            uint HV35 = BitConverter.ToUInt16(bytes, 0x8 + indexShift); //00HV35, 3,5-4,5 kV skalėje;
            double HV35_d = 3490 - HV35 * 0.1813; //00HV35 vertė voltais, 3,5-4,5 kV skalėje;
            uint PWM = bytes[0xa + indexShift]; //PWM vertė?
            uint Ti = bytes[0xb + indexShift]; //
            /**/
            uint Tm = BitConverter.ToUInt32(bytes, 0x60 + indexShift); //tm trukmė milisekundėmis; be konvertavimo grąžinama;
            double HV45_15 = BitConverter.ToUInt16(bytes, 0x64 + indexShift) * 0.1813; //įtampa 00HV45_15 voltais 0-4,5kV skalėje; jau konvertuota;
            uint Iin = BitConverter.ToUInt16(bytes, 0x66 + indexShift); //iin vertė, kokiais vienetais?

            //Viskas sujungiama į string'ą:
            getAnswer = Ni.ToString() + empty + Tn_in_seconds.ToString() + empty +  HV45_d.ToString() + empty + HV35_d.ToString() + empty + PWM.ToString() + empty +  Ti.ToString() + empty + Tm.ToString() + empty + HV45_15.ToString() + empty + Iin.ToString() + newLine;

            return getAnswer;
        }

        public static List<string> getLastpartOfAnswer(byte[] data)
        {
            List<string> answer = new List<string>();
            /*Čia bandom ištraukti informaciją apie 14-ką paskutinių baitų*/
            //============================================================

            return answer;
        }

    }
}
