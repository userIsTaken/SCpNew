using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SPcontrol
{
    public class MeasurementParameters
    {
        //parametrai matavimams
        public double Ts;
        public double Tz;
        public double Tq;
        public double Cth;
        public double Vq;
        public double StartingEnergy;
        public double EndingEnergy;
        public double Counts;

        //kryptis:
        public bool Forward;

        //žingsnis:
        public double Step;
        public bool StepDirection;

        //parametrai atsakymo grąžinimui:
        public double EnergyValue;
        public double Ni;
        public string CurrentLambda;
        public double CurrentMeasurementTime; //sekundėmis;

        public string LambdaFromCs;

        //Klaidų grąžinimui
        public string ExceptionMessage;
        public int ExceptionCode; //0 - OK, 1 - BAD...;


        //CRC kontrolei
        public int AnswerCRCCode;
        public string AnswerCRCError;


        public bool UseLPT;
        public int LEDWorkingTime;
        public int LEDWaitAfterWorkTime;
        public int DataLPT; //duomenys rašymui į LPT;

        public bool useLEDorHalogen; //true : led; false : halogenas

        public bool useAutoDrop;
        public decimal AutoDropVoltageValue;
        public double currentVoltage; //kVolts!

        //Rigol DP832 valdymui:

        public decimal rigol_start_current;
        public decimal rigol_end_current;

        public decimal rigol_limit_current;

        public string rigol_channel;

        public int rigol_sleepTime;

        public decimal rigol_stepCurrent;

        public decimal rigol_maximum_voltage;

        //Rigol atsakymai:
        public double Actual_current;

        //HW dalis neperduodama.

        public double StandbyTime;

        public int _Points;


        public MeasurementParameters()
        {
            /*Default constructor*/
        }
    }
}
