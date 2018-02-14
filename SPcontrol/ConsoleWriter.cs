using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SPcontrol
{
    public static class ConsoleWriter
    {
        public static void WriteOutput(params object[] obj)
        {
            try
            {
                string PrintIt = String.Empty;
                foreach (var i in obj)
                {
                    string t = checkTypereturnString(i);
                    PrintIt = PrintIt + t + " ";
                }
                Console.WriteLine("ConsoleWriter : " + PrintIt);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Problema su ConsoleWriter'iu");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("==============");
            }
        }

        public static string checkTypereturnString(object obj)
        {
            string answer = string.Empty;
            string ats = string.Empty;
            if(obj.GetType() == typeof(byte[]))
            {
                foreach(byte b in (IEnumerable<byte>)obj)
                {
                    ats = ats + b.ToString() + " ";
                }
                answer = ats;
            }
            else
            {
                answer = obj.ToString();
            }
            return answer;
        }

        
    }
}
