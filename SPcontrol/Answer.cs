using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPcontrol
{
    public class Answer
    {
        //parametrai, kuriais perduodamas bus atsakymas
        public double Answer42Vset35;//ribinei įtampai rasti, 42Vset35 kintamasis iš įtampos radimo komandos;
        public byte[] Begining = new byte[2];
        public double Ilgis;
        public byte[] AnswerCRC32 = new byte[4];
        public byte ErrorCode; //nuo -1 iki 6 (0-6 Viliūno, -1 - mano dėl tikrinimo)
        public string AnswerErrorCode; //nuo -1 iki 6 (0-6 Viliūno, -1 - mano dėl tikrinimo)
        public string AnswerException; //mano kode kylančioms problemoms
        public byte CmdNumber;// komandos numeris
        public byte CmdRepeated;//Pakartota komanda
        public byte ScaleStatus;//50nA/500nA 0/1;
        public byte[] CmdCrc = new byte[2]; //kodo crc vietoj versijos numerio;
        public double Iin; //Iin vidutinė vertė;
        public double Answer00HV45_15; //00HV45_15 šaltinio įtampa 0-4,5kV skalėje;
        public double Tm;//Tm dabar vykstančio skaičiavimo trukmė po 10 ms;
        public double Ni; //einama impulsų skaitiklio vertė (4-ri baitai)
        public string AnswerInBytes; //Atsakymas baitų pavidalu, grąžinama visada pasitikrinimui;
        public string AnswerInHexString; //Atsakymas Hex frmatu, grąžinama visada visas;
        public int Ts;//skaičiavimo trukmės saugojimas;
        public int AnswerCRCerrorCode; //0 - ok, -1 blogai ...
        public string AnswerCRC_code;
        public string CRC_code_fromMicroController;
        public string CRC_code_should_be;
        public double Iin_ampere; //Iin vertė nA?

        //Iš Rigol DP832:
        public double CurrentCurrent;


        public Answer()
        {
            //default constructor;
        }

        public void returnAutoVoltageAnswer(byte[] data)
        {
            //=========================================
            //nuskaitoma įtampa iš atsakymo
            // pagal dokumentaciją, pradžia yra 4-tas bitas {start nuo 0, 1, 2, 3 ... }
            double uAnsw = (double)BitConverter.ToInt16(data, 4); //Grąžinta įtampa 0-4095 skaičių skalėje
            double uAnswer42Vset35 =( 2200.0d + ((4095.0d- uAnsw)/4095.0d)*(4200.0d-2200.0d)); //42Vset35 vertė Voltais, 2,0 -4,2 kV skalėje;//SU PAKLAIDA
            this.Answer42Vset35 = Math.Round((uAnswer42Vset35 + 143.66d) / 1.0158d, 3); //PAKLAIDOS eliminavimas?
            //užpildoma statuso informacija;BitConverter.ToInt16(data, 4)
            returnLastFourteenBytes(data);
            //visada grąžinama informacija apie atsakymą baitais ir Hex:
            getAnswerbytesAndHex(data);
            checkAnswerCrcCode(data);

        }

        public void returnCounterValue(byte[] data)
        {
            //==========================================
            Ni = BitConverter.ToInt32(data, 4); //impulsų skaitiklio vertė;
            returnLastFourteenBytes(data);
            checkAnswerCrcCode(data);
            getAnswerbytesAndHex(data);
        }
        
        public void returnBigMemoryData(byte[] data)
        {
            //==========================================
        }

        public  void returnLastFourteenBytes(byte[] data)
        {
            //=======================================
            //14-ka paskutinių baitų visada grąžinama:
            int ilgis = data.Length; //nuo paskutinių elementų atsiskaičiuojama;
            //be to ilgis - elemntų skaičiuos, skaičiuojama nuo 1-to;
            //indeksas gi - nuo 0!;
            AnswerCRC32[0] = data[ilgis - 4];
            AnswerCRC32[1] = data[ilgis - 3];
            AnswerCRC32[2] = data[ilgis - 2];
            AnswerCRC32[3] = data[ilgis - 1];
            ErrorCode = data[ilgis - 5];
            AnswerErrorCode = ExternalFunctions.getErrorCode(ErrorCode);//erro, nuo -1 (mano kodas, rodantis, kad nepataikyta su array index) iki 6 (Viliūno kodai);
            CmdNumber = data[ilgis - 6];
            CmdRepeated = data[ilgis - 7];
            ScaleStatus = data[ilgis - 8];//čia bus grąžinama 0 arba 1 (50nA/500nA);
            CmdCrc[0] = data[ilgis - 9];
            CmdCrc[1] = data[ilgis - 10];//kodo crc 2 baitai;
            Iin = BitConverter.ToInt16(data, ilgis - 12);//Iin vidutinė vertė, jau skaičius, 12-tas+13tas baitai nuo galo?
            Answer00HV45_15 =Math.Round(( 4500.0d * BitConverter.ToInt16(data, ilgis - 14) / (double)0x7FFF), 3);// įtampa 0-4,5kV skalėje randama taip U = 4500 V * 00HV45_15 vertė / 0x7FFF;
            Tm = BitConverter.ToInt32(data, ilgis - 18); //Atsakymas bus milisekundėmis ?;
            //=============================================
            checkAnswerCrcCode(data);
            if(ScaleStatus == 0) //50nA;
            {
                Iin_ampere =Math.Round(( (Iin / 4095.0d) * 50.0d), 2); //Skaičius nanoamperais;
            }
            else if (ScaleStatus == 1)//500 nA
            {
                Iin_ampere = Math.Round(((Iin / 4095.0d) * 500.0d), 2); //Skaičius nanoamperais, iki 500nA;
            }

        }

        public void getAnswerbytesAndHex(byte[] data)
        {
            this.AnswerInBytes = ExternalFunctions.returnStringfromArray(data);
            this.AnswerInHexString = BitConverter.ToString(data).Replace("-", " ");
        }

        public void returnNifromMemory(byte[] data)
        {
            /*Iš principo, čia tokia pat funkcija, kaip ir tiesiog Counter'io nuskaitymas;*/
            //==========================================
            Ni = BitConverter.ToInt32(data, 4); //impulsų skaitiklio vertė;
            returnLastFourteenBytes(data);
            checkAnswerCrcCode(data);
            getAnswerbytesAndHex(data);
        }

        public void returnInScaleStatusFromMemory(byte[] data)
        {
            //nuskaito Iin skalęv;
            returnLastFourteenBytes(data);
            checkAnswerCrcCode(data);
            //statusas nustatomas iš paskutinių 14-kos baitų
            //this.ScaleStatus = data[4];


        }

        public  void returnAnswerFromMemoryWriteCMD(byte[] data)
        {
            returnLastFourteenBytes(data);
            //visada grąžinama informacija apie atsakymą baitais ir Hex:
            getAnswerbytesAndHex(data);
            checkAnswerCrcCode(data);

        }

        public void rdAnswerAnalyzer(byte[] data)
        {
            //======================================
            //paima Ni vertę bei kitas vertes iš rd.Answer, gauto iš BackGroundWorker;
            returnLastFourteenBytes(data);
            uint Ni_value = BitConverter.ToUInt32(data, 4);
            this.Ni = (double)Ni_value;
            checkAnswerCrcCode(data);
            getAnswerbytesAndHex(data);//nebuvo šio lauko
        }

        public void checkAnswerCrcCode(byte[] data)
        {
            byte[] crc_from_controller = new byte[4];
            byte[] answer = new byte[data.Length - 4];
            //?
            for( int i = 0; i<data.Length-4; i++)
            {
                answer[i] = data[i];
            }
            for(int i = 0; i<4; i++)
            {
                crc_from_controller[i] = data[data.Length - 4 + i];
            }
            string got_crc_code = BitConverter.ToString(crc_from_controller);
            Crc32 crc = new Crc32();
            byte[] crc_code_answ = crc.ComputeHash(answer);
            string crc_should_be = BitConverter.ToString(crc_code_answ);
            if(crc_should_be == got_crc_code)
            {
                this.AnswerCRCerrorCode = 0; //OK
                this.AnswerCRC_code = "CRC kodas gautas geras";
                Console.WriteLine("CRC OK");
            }
            else if (crc_should_be != got_crc_code)
            {
                this.AnswerCRCerrorCode = -1; //bad sad;
                this.AnswerCRC_code = "CRC kodas gautas BLOGAS";
                Console.WriteLine("WRONG CRC!!!");
                this.CRC_code_fromMicroController = got_crc_code;
                this.CRC_code_should_be = crc_should_be;
            }
            
            Console.WriteLine(BitConverter.ToString(data));
            Console.WriteLine(BitConverter.ToString(crc_from_controller));
            Console.WriteLine(BitConverter.ToString(answer));
           
        }
    }
}
