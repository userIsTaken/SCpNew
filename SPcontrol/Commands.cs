using System;
using System.Globalization;

namespace SPcontrol
{
    public class Commands
    {

        public static byte[] CmdFindThresholdVoltage(uint commandNumber, decimal Cth_d, decimal Ir_d, int cmd_ilgis=12)
        {
            /* Prašymas automatiškai nustatyti įtampą*/
            //============================================
            //esc v nr P1 P2 Cth Ir(du baitai) CRC32
            
            //================================================
            byte esc = 0x1b; //Esc
            byte v = 0x76; //v(oltage)
            byte nr = (byte)commandNumber;
            byte P1 = 0x33;
            byte P2 = 0x33;
            //P1 P2 nenaudojami, hardcode'inama 0x33 vertės į juos;
            byte Cth = ExternalFunctions.returnCthVoltage(Cth_d);
            byte[] Ir = ExternalFunctions.getIr(Ir_d);
            //================================================
            byte[] cmd = new byte[8];
            cmd[0] = esc;
            cmd[1] = v;
            cmd[2] = (byte)nr;
            cmd[3] = P1;
            cmd[4] = P2;
            cmd[5] = Cth;
            cmd[6] = Ir[0];
            cmd[7] = Ir[1];   //jei grąžinama 0xFF 0x0F, turi būti tada gerai...
            Crc32 crc = new Crc32();
            byte[] crc_code = crc.ComputeHash(cmd);
            byte[] full_cmd_to_write = new byte[cmd.Length + crc_code.Length];
            cmd.CopyTo(full_cmd_to_write, 0);
            crc_code.CopyTo(full_cmd_to_write, cmd.Length);
            return full_cmd_to_write;
        }

        public static byte[] CmdAskStartCounting(uint commandNumber, double Ts_double, double tz_double, double tq_double, decimal cth_double, double Vq_double,int cmd_length = 13)
        {
            //===================================
            //
            /*Prašymas pradėti skaičiavimą*/
            /*Esc s nr Ts Tz Tq(2 baitai) Cth Vq CRC32*/
            byte esc = 0x1b; //Esc
            byte s = 0x73; //s(tart)
            byte nr = (byte)commandNumber;
            byte Ts = ExternalFunctions.getTs(Ts_double);
            byte Tz = ExternalFunctions.getTz(tz_double);
            byte[] Tq = ExternalFunctions.getTq(tq_double);
            byte Cth = ExternalFunctions.returnCthVoltage(cth_double);
            byte Vq = ExternalFunctions.getVq(Vq_double);
            byte[] cmd = new byte[9];
            cmd[0] = esc;
            cmd[1] = s;
            cmd[2] = nr;
            cmd[3] = Ts;
            cmd[4] = Tz;
            cmd[5] = Tq[0];
            cmd[6] = Tq[1];
            cmd[7] = Cth;
            cmd[8] = Vq;
            Crc32 crc = new Crc32();
            byte[] crc_code = crc.ComputeHash(cmd);
            byte[] cmd_ret = new byte[cmd.Length + crc_code.Length];
            cmd.CopyTo(cmd_ret, 0);
            crc_code.CopyTo(cmd_ret, cmd.Length);
            return cmd_ret;
        }

        public static byte[] ReadValueFromMemory(int Kiek, int Address, int cmd_length = 9)
        {
            //not implemented yet
            throw new NotImplementedException();
            
        }

        public static byte[] WriteInStatustoMemory(uint cmdnr, int StatusasIinSkales, int cmd_length = 11)
        {
            //not implemented yet;
            byte[] cm = new byte[6];
            
            cm[0] = 0x1b;
            cm[1] = 0x77;
            cm[2] = (byte)cmdnr;
            cm[3] = 2;//kiek
            cm[4] = 0x82;
            cm[5] = (byte)StatusasIinSkales;
            Crc32 crc = new Crc32();
            byte[] crccode = crc.ComputeHash(cm);
            byte[] cmd = retCmdSuCrc32(cm, crccode);
            return cmd;
            
        }

        public Commands()
        {
            //default constructor;
        }

        public static byte[] Write42Vset35IntoMemory(uint CmdNumber, int kiek, double value42Vset35, int Address, bool useAorB, int cmd_length = 13)
        {
            //naudojama įrašyti tiesiogiai 42Vset35 darbinei įtampai ( 3,5 - 4,2 kV ribos);
            //Upd: pasikeitė ribos - 2- 4 kV:
            byte[] cmd_w_crc = new byte[7];
            cmd_w_crc[0] = 0x1b;
            cmd_w_crc[1] = 0x77;
            cmd_w_crc[2] = (byte)CmdNumber;
            cmd_w_crc[3] = (byte)(kiek+1);
            cmd_w_crc[4] = (byte)Address;
            //duomenys:
            byte[] Uarray = ExternalFunctions.get42Vset35array(value42Vset35, useAorB);
            cmd_w_crc[5] = Uarray[0]; //tikrinti, koks yra vyr jaun bitai;
            cmd_w_crc[6] = Uarray[1];
            //crc skaičiavimas
            Crc32 crc = new Crc32();
            byte[] crc_code = crc.ComputeHash(cmd_w_crc);

            //======cmd section
            byte[] cmd = new byte[crc_code.Length+cmd_w_crc.Length];
            cmd_w_crc.CopyTo(cmd, 0);
            crc_code.CopyTo(cmd, cmd_w_crc.Length);
            //==grąžinama komanda vykdymui:
            return cmd;
        }

        public static byte[] ReadIinStatusFrommemory(uint cmdNumeris)
        {
            //statinė komanda, be pakeitimų vykdoma, visada nuskaito 0x82 baito vertę;
            //tiksliau, liepianti grąžinti 0x82 baito vertę;
            byte esc = 0x1b;
            byte r = 0x72;
            byte cmdNr = (byte)cmdNumeris;
            byte kiek = 2;
            byte address = 0x82;
            byte[] cmd = new byte[5] { esc, r, cmdNr, kiek, address };
            Crc32 crc = new Crc32();
            byte[] crc_code = crc.ComputeHash(cmd);
            byte[] cmd_ret = retCmdSuCrc32(cmd, crc_code);
            return cmd_ret;
        }

        public static byte[] retCmdSuCrc32(byte[] cmd, byte[] crc)
        {
            byte[] ans = new byte[cmd.Length + crc.Length];
            cmd.CopyTo(ans, 0);
            crc.CopyTo(ans, cmd.Length);
            return ans;
        }

        public static string retLambdaStringFromEv(double eV)
        {
            string lambda = String.Empty;
            double lmb =Math.Round( (1239.75d / eV), 2); //nanometrais
            lambda = Convert.ToString(lmb, CultureInfo.InvariantCulture);
            return lambda;
        }
        public static string returnCurrentLambda(double eV)
        {
            string lambda = string.Empty;
            lambda =Convert.ToString( Math.Round((1239.75d / eV), 2)); //nanometrais
            return lambda;
        }
    }
}
