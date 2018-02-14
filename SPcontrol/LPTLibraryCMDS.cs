using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices; //for dll imports;
using System.Text;

namespace SPcontrol
{
    public static class LPTLibraryCMDS
    {

        //[DllImport("inpoutx64.dll", EntryPoint = "Out32")]//64 bit version;
        [DllImport("inpout32.dll", EntryPoint = "Out32")]//32-bit version
        public static extern void Output(int address, int value); // decimal?

    }
}
