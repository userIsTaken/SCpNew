using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO.Ports;
using System.Globalization;
using System.IO;
using System.Threading;
using CornerstoneDll; // use for control Oriel monochromator;
using CyUSB;
using System.Drawing;


using InstrumentDrivers; //ok, prisidėta;
using System.Text;

using System.Collections.Generic;
using Ivi.Visa;
using Ivi.Visa.Interop;
using  System.Runtime.InteropServices;

namespace SPcontrol
{
    public partial class SPControl : Form
    {
        public uint commandNumber = 0; //sinchronizavimui, kad žinotume, kokiai komandai gautas atsakymas;
        public SerialPort port;
        public SerialPort arduinoPort;
        public string newLine = Environment.NewLine;
        public int responseWaitTime;
        public int waitBetweenQuestionResponse;

        public int gass_counter = 0;

        public int ScaleStatus;

        public Answer atsakymas = new Answer();//ar turi būti globalus?

        public bool canChange50nA = false;
        public bool canChange500nA = false;

        public bool powerSupplyOnOff = false;

        public int MatavimoNr = 0;

        public bool useLPT = false;
        public bool useLEDorHalogen = true; //LED - true, halogenas - false;

        public hmp4000 powerSupply = null;
        //Visa list:
        ResourceManager ioMgr = new ResourceManager();
        FormattedIO488 instrument = new FormattedIO488();

        IVisaSession session = null;
        //END of SCOPE
        //Global device list:
        USBDeviceList usbDevices;
        CyUSBDevice cornerstone;
        Cornerstone cs;
        //end of list
        double requiredWavelength;
        string terminator = "\r\n";

        public double AutoItampa42set35;

        public double stepSize;

        public int dataLPT = 0;

        public int counter_rigol_started = 0;
        public bool rigol_started = false;
        

        public SPControl()
        {
            InitializeComponent();
            setupEverything();
        }

        private void uždarytiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //this.Close();
            /*Code to release all COMs, save data*/
            //Programos uždarymas
            try
            {
                port.Close();//uždaromas port'as
                cs.disconnect();//Oriel atjungiamas
            }
            catch (Exception ex)
            {
                //nieko nedarom, reiškia, kad port'as net neegzistavo, tad išeinam iš app'so ir tiek;
                Console.WriteLine(ex.ToString()); //Tiksliau, kad išvengtume Warning "ex not used";
            }
            Application.Exit();
            //this.Close();
        }

        private void rescanButton_Click(object sender, EventArgs e)
        {
            /**/
            string[] portNames = SerialPort.GetPortNames();
            foreach(string i in portNames)
            {
                if(comConnectionsListBox.Items.IndexOf(i) == -1)
                {
                    comConnectionsListBox.Items.Add(i);
                }
                if (comForArduinoPortBox.Items.IndexOf(i) == -1)
                {
                    comForArduinoPortBox.Items.Add(i);
                }
            }
        }
        

        private void connectButton_Click(object sender, EventArgs e)
        {
            changePicture(0);
            responseWaitTime = (int)maximumWaitTimeBox.Value;
            waitBetweenQuestionResponse = (int)waitTimeBetweenCommandAndResponseBox.Value;
            /*same button for connect and disconnect*/
            if (connectButton.Text == "Atsijungti")
            {
                port.Close();
                connectButton.Text = "Prisijungti";
            }
            else if (connectButton.Text == "Prisijungti")
            {
                /*Connect function:*/
                port = new SerialPort(comConnectionsListBox.Text);
                if (handshakeBox.Text == "None")
                {
                    port.Handshake = Handshake.None;
                }
                if (parityBitBox.Text == "None")
                {
                    port.Parity = Parity.None;
                }
                string stbits = stopBitsBox.Text;
                switch (stbits)
                {
                    case ("1"):
                        {
                            port.StopBits = StopBits.One;
                            errorAndInfoBox.AppendText("Case 1" + newLine);
                            break;
                        }
                    case ("2"):
                        {
                            port.StopBits = StopBits.Two;
                            errorAndInfoBox.AppendText("Case 2" + newLine);
                            break;
                        }
                    case ("1.5"):
                        {
                            port.StopBits = StopBits.OnePointFive;
                            errorAndInfoBox.AppendText("Case 1.5" + newLine);
                            break;
                        }
                    default:
                        {
                            port.StopBits = StopBits.One;
                            errorAndInfoBox.AppendText("Default case value - StopBits.One" + Environment.NewLine);
                            break;
                        }


                } //end switch;
                port.WriteTimeout = responseWaitTime;
                port.ReadTimeout = responseWaitTime;
                /*Baudrate*/
                port.BaudRate = Convert.ToInt32(baudRateBox.Text, CultureInfo.InvariantCulture);
                try
                {
                    port.Open();
                    showportinfo(port);
                    
                }
                catch (Exception ex)
                {
                    errorAndInfoBox.AppendText("Exception ex has been trown" + Environment.NewLine + ex.GetType().ToString()+newLine);
                }
                if (port.IsOpen)
                {
                    connectButton.Text = "Atsijungti";
                    //Čia bus kodas, nuskaitantis skales, etc - reikia, kad prietaisas visada būtų prijungtas nuo šio momento:
                    checkAndSetcurrentScale();
                    
                }
            }
        }

        private void sendCustommanualCommand_Click(object sender, EventArgs e)
        {
            /*send custom command to a connected Serial Port*/
            /*there should be a command without crc32 hash*/
            string[] commandString;
            try
            {
                if (commandManualBox.Text.Length != 0)
                {
                    //int index = 0;
                    commandString = commandManualBox.Text.Split(' ');
                    Crc32 crc_calculator = new Crc32();
                    string cmd = ExternalFunctions.returnStringfromArray(commandString);
                    errorAndInfoBox.AppendText("cmd buvo (turi būti tas pats string, hex formatas) " + cmd + newLine);
                    byte[] bytes = ExternalFunctions.HexStringToBytes(ExternalFunctions.returnCompactString(commandManualBox.Text));
                    /**/
                    cmd = ExternalFunctions.returnStringfromArray(bytes);

                    errorAndInfoBox.AppendText("cmd buvo (bytes[] kodas) " + cmd + newLine);
                    string hash = String.Empty; //store hash
                    string hashHex = String.Empty;
                    byte[] crc32_of_bytes = crc_calculator.ComputeHash(bytes);
                    var fullDataToSend = new byte[bytes.Length + crc32_of_bytes.Length];
                    bytes.CopyTo(fullDataToSend, 0);
                    crc32_of_bytes.CopyTo(fullDataToSend, bytes.Length);
                    //uint crcMano = crc32calculator.crc32_code_custom(bytes, 0x00000000u);
                    foreach (byte b in crc32_of_bytes)
                    {
                        hash += b.ToString().ToLower();

                    }
                    hashHex = BitConverter.ToString(crc32_of_bytes);
                    errorAndInfoBox.AppendText("hash crc23 (DSC_crc32) bus toks: " + hash + newLine);
                    errorAndInfoBox.AppendText("hashHex crc23 (DSC_crc32) bus toks: " + hashHex + newLine);
                    
                    if (port.IsOpen)
                    {
                        port.Write(fullDataToSend, 0, fullDataToSend.Length);
                    }
                    


                }
            }
            catch(Exception ex)
            {
                appendText("CustomCMD:101: Problema su komanda/portu. Klaidos pranešimas: ", ex.Message.ToString());
            }

        }

        private void getManualAnswer_Click(object sender, EventArgs e)
        {
            int answerLength = (int)answerLengthBox.Value;
            byte[] bytes = new byte[answerLength];//koks iš tiesų ilgis atsakymo?
            try
            {
                port.ReadTimeout = responseWaitTime;
                port.Read(bytes, 0, answerLength);
                string answer = ExternalFunctions.byteArrayToHexString(bytes);
                string errCde = ExternalFunctions.getErrorCode(bytes[answerLength - 5]);
                answerStatusCodeBox.Text = errCde;
                answerBox.Text = answer;
                /*Čia errorBoxe išvesti visą atsakymą bytes[] ir hex formatu*/
                appendText("CMD manual; Atsakymas buvo " + answer); //bytes
                appendText("CMD Manual; Atsakymas Hex formatu buvo " + (BitConverter.ToString(bytes)).Replace('-', ' '));
                port.DiscardInBuffer(); //kad nesiakumuliuotų data?
                port.DiscardOutBuffer(); //kad nebūtų siunčiama seni duomenys?
                updateCmdNumber();


            }
            catch (Exception ex)
            {
                errorAndInfoBox.AppendText(ex.GetType().ToString() + newLine);
                errorAndInfoBox.AppendText(ex.ToString() + newLine);
            }
        }
        
        
        
        
       
        private void findthresholdVoltageBtn_Click(object sender, EventArgs e)
        {
            changePicture(0); //visada nusiunčiant komandą būna viskas gerai, blogai - tik vėliau;
            /* Prašymas automatiškai nustatyti įtampą*/
            //============================================
            //esc v nr P1 P2 Cth Ir(du baitai) CRC32
            //============================================
            decimal Cth = cthThresholdValueBox.Value;
            decimal Ir = currentThresholdValueBox.Value;
            //==========================================================
            //pasidaromas byte[] array kurio crc skaičiuosim ir sudarysim pilną komandą;
            byte[] cmd = Commands.CmdFindThresholdVoltage(commandNumber, Cth, Ir );
            
            //===============================================
            //full_cmd_to_write <-- komanda kurią įrašysim:
            try
            {
                //=======================================
                //Išvedama siunčiama komanda:
                showData_sent(cmd, "Nusiųsta komanda "+commandNumber.ToString()+ " cl 283");
                //=======================================
                port.Write(cmd, 0, cmd.Length);
                
                Thread.Sleep(waitBetweenQuestionResponse);  //laukiama kol bus sugeneruotas atsakymas, dė viso pikto 0,5 s;
                //=======================================
                int cmd_length = (int)answerIcmdBox.Value;
                byte[] data = new byte[cmd_length];
                int bts = port.Read(data, 0, data.Length);
                if (bts > 0)
                {
                    
                    showData_sent(data, "Atsakymas į "+ atsakymas.CmdNumber.ToString()+" Cl 295");
                    atsakymas.returnAutoVoltageAnswer(data);
                    if (atsakymas.AnswerCRCerrorCode == 0)
                    {
                        //atsakymo atvaizdavimas:
                        displayAnswerAutoVoltage();
                        appendText();
                        appendText("Atsakymas į Autoįtampos komandą", atsakymas.CmdNumber);
                        appendText(atsakymas.AnswerInBytes);
                        appendText(atsakymas.AnswerInHexString);
                        appendText("42HV35 :", atsakymas.Answer42Vset35);
                        appendText(atsakymas.Answer42Vset35);
                        appendText();
                        changePicture(0);
                    }
                    else if (atsakymas.AnswerCRCerrorCode == -1)
                    {
                        appendText();
                        appendText("blogas CRC atsakyme");
                        appendText(atsakymas.AnswerCRC_code);
                        appendText(atsakymas.CRC_code_fromMicroController, "Tai gautas iš μContr");
                        appendText(atsakymas.CRC_code_should_be, "Turėjo būti gautas");
                        appendText(atsakymas.AnswerInBytes);
                        appendText(atsakymas.AnswerInHexString);
                        appendText();
                        changePicture(1);
                    }
                    port.DiscardInBuffer();
                    port.DiscardOutBuffer();
                    //==Atnaujinamas siųstų komandų skaičius;
                    updateCmdNumber();

                }
                else
                {
                    port.DiscardInBuffer();
                    port.DiscardOutBuffer();
                    changePicture(1);
                    appendText(" e ?");
                    updateCmdNumber();

                }
                
            }
            catch(Exception ex)
            {
                try
                {
                    appendText();
                    appendText("Problema su įtampos automatinio radimo komanda, rašymu į port'ą.", ex.Message);
                    appendText(ex.ToString());
                    changePicture(1);
                    appendText();
                    port.DiscardInBuffer();
                    port.DiscardOutBuffer();
                }
                catch (Exception ez)
                {
                    changePicture(1);
                    appendText();
                    appendText(ez.ToString());
                    appendText();

                }
            }
        }

        private void startCountingButton_Click(object sender, EventArgs e)
        {
            //===============================
            //Čia reikia uždaryti sklendę:
            changePicture(0);
            StartCounting();
            //naudojama išorinė funkcija, kad kodas būtų aiškesnis
            //+ turim galimybę tą pačią f-ją iškviesti ir kitur;

        }

        private void readMemoryButton_Click(object sender, EventArgs e)
        {
            //BE CRC kodo tikrinimo čia!
            /*Atminties nuskaitymas*/
            /*Nuskaitoma nuo 0 iki 111-to baito, iš viso 112-ka baitų*/
            updateCmdNumber();
            try
            {
                if (port.IsOpen) /*kas nebūtų nusiųsta komanda į uždarytą portą;*/
                {
                    try
                    {
                        /*maksimalus nuskaitomų baitų kiekis - 128 - 0x80;*/
                        /*Užklausimas nuskaityti atmintį*/
                        byte[] cmd_byte = new byte[5];
                        cmd_byte[0] = 0x1b; //Esc simbolis;
                        cmd_byte[1] = 0x72; //r (read) simbolis;
                        cmd_byte[2] = (byte)commandNumber;
                        cmd_byte[3] = 0x6f; //111-ka baitų nuskaitoma;
                        cmd_byte[4] = 0x0; //atminties adresas;
                                           /*crc kodo skaičiavimas bei pilno byte[] array sudarymas rašymui į portą;*/
                        Crc32 getCrc = new Crc32();
                        byte[] crc_of_cmd = getCrc.ComputeHash(cmd_byte);
                        byte[] full_cmd_to_write = new byte[cmd_byte.Length + crc_of_cmd.Length];
                        cmd_byte.CopyTo(full_cmd_to_write, 0);
                        crc_of_cmd.CopyTo(full_cmd_to_write, cmd_byte.Length);
                        /*rašymas į portą:*/
                        /*ir siunčiamos komandos išvedimas*/
                        port.Write(full_cmd_to_write, 0, full_cmd_to_write.Length);
                        string cmdSent = ExternalFunctions.returnStringfromArray(full_cmd_to_write);
                        string cmdsentHex = BitConverter.ToString(full_cmd_to_write);
                        appendText("===========CMD=================");
                        appendText(cmdSent);
                        appendText(cmdsentHex.Replace('-', ' '));
                        /*Atminties nuskaitymas*/
                        Thread.Sleep(waitBetweenQuestionResponse);
                        byte[] bytesInMemory = new byte[133]; //buffer;
                        int bytesRead = port.Read(bytesInMemory, 0, bytesInMemory.Length);
                        if (bytesRead >= 0)
                        {
                            
                                /*jei bytesRead daugiau nei nulis, buvo nuskaityta kažkas, analizuojama*/
                                string answer = AnswerAnalyzer.getBigAnswer(bytesInMemory);
                            
                                string answerHexformat = BitConverter.ToString(bytesInMemory);
                                string justAnswer = ExternalFunctions.returnStringfromArray(bytesInMemory);
                                appendText("================================================");
                                appendText("====Atsakymas===========");
                                //==========================================================
                                showAnswerInAutoAnswerBox("Ni", "Tn", "HV45", "HV35", "PWMT", "Tm", "HV45_15", "I_in");
                                appendText(answer);
                                appendText(justAnswer);
                                appendText(answerHexformat);
                                port.DiscardInBuffer(); //panaikinami data, kad nesiakumuoliuotų?
                                port.DiscardOutBuffer();
                                
                            

                        }
                    }
                    catch (Exception ex)
                    {
                        appendText("Atminties nuskaitymo problema: " + ex.Message.ToString() + newLine);
                    }
                }
                else
                {
                    appendText("Port'as yra uždarytas!");
                }
            }
            catch (Exception ex)
            {
                appendText("Klaida nsukaitant atmintį, kažkas su port'u? " + ex.Message.ToString());
            }
        }

        private void maximumWaitTimeBox_ValueChanged(object sender, EventArgs e)
        {
            /**/
            responseWaitTime = (int)maximumWaitTimeBox.Value;
            try
            {
                port.WriteTimeout = responseWaitTime;
                port.ReadTimeout = responseWaitTime;
            }
            catch (Exception ex)
            {
                appendText("TimeOut vertės nustatymo klaida" + newLine + ex.Message.ToString() + newLine);
            }
        }

        private void waitTimeBetweenCommandAndResponseBox_ValueChanged(object sender, EventArgs e)
        {
            waitBetweenQuestionResponse = (int)waitTimeBetweenCommandAndResponseBox.Value;
        }
        

        private void readAnswerInBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //=================matavimų thread'as?======================================

            ReadingParameters rd = e.Argument as ReadingParameters; //pasiimam argumentus:
            //Šis workeris veikia šiuo metu tik užlaikant Thread'ą
            int wait =(int) (rd.Ts*1000.0d);
            int deltaT = (wait+1000) / 100;
            for (int i = 0; i<=100; i=i + 1)
            {
                Thread.Sleep(deltaT); //laukiama Ts+100 milisekundžių laiko
                readAnswerInBackgroundWorker.ReportProgress(i);
            }
            
        }

        private void readAnswerInBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            
            this.workerProgressBar.Value=e.ProgressPercentage;
            this.progressLabelText.Text = e.ProgressPercentage.ToString() + "%";
        }

        private void readAnswerInBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            /*baigtas workerio darbas. ką daryti?*/
            ReadingParameters rd = e.Result as ReadingParameters;
            
            byte[] answ = new byte[26];
            try
            {
                int i = port.Read(answ, 0, answ.Length);
                if(i>0)
                {
                    
                        atsakymas.rdAnswerAnalyzer(answ);
                    if (atsakymas.AnswerCRCerrorCode == 0)
                    {
                        //atnaujinam informaciją apie Ni vertes bei kitus duomenis:
                        showAnswerInAutoAnswerBox();
                        showAnswerInAutoAnswerBox("Cmd. Nr.", "Ni vertė", "Tm", "00HV45_15", "Iin", "Iin, nA", "Statusas", "Err.code");
                        showAnswerInAutoAnswerBox(atsakymas.CmdNumber, atsakymas.Ni, atsakymas.Tm, atsakymas.Answer00HV45_15, atsakymas.Iin, atsakymas.Iin_ampere, atsakymas.ScaleStatus, atsakymas.AnswerErrorCode);
                        //===============================
                        showAnswerInAutoAnswerBox();
                        this.workerProgressBar.Value = 0;
                        this.progressLabelText.Text = "0 %";
                        //Čia pridedam info į lauką, skirtą atsakymo Ni atvaizdavimui:
                        //========================================
                        double l = (double)this.lambdaOrEnergyInputBox.Value;
                        double eV = (double)this.energyvalueElectronVoltsBox.Value;
                        //dujų skaitiklio momentinė vertė:
                        string curr_gass_counter = gassIndicatorLabel.Text.Replace("s.v.", string.Empty);
                        this.outputBox.AppendText(ForInfoViewer.AddString(MatavimoNr,eV, atsakymas.Ni, this.voltage42Vset35_in_kiloVolts.Value.ToString(), "kV" , curr_gass_counter, "s.v.")); //Prirašyta kV reikšė, kuri buvo nustatyta;
                        //atvaizduojame grafike:
                        //this.resultsViewChart.Series["DATA"].Points.AddXY(eV, atsakymas.Ni);
                        updateGraphWithPoint(atsakymas.Ni, eV);
                        MatavimoNr++;
                        //========================================
                        //pakeista iš λ į eV, kad grafikas normalus būtų
                        //===============================
                        //atsakymo išvedimas, pasitikrinimui, nes nebuvo išvedama:
                        appendText();
                        appendText("Gautas atsakymas į " + atsakymas.CmdNumber);
                        appendText(atsakymas.AnswerInBytes);
                        appendText(atsakymas.AnswerInHexString);
                        changePicture(0);
                        appendText();
                        //======Kažkodėl išveda tuščius laukus??
                        closeOpenShutter("C"); //Uždaroma sklendė
                    }
                    else if (atsakymas.AnswerCRCerrorCode == -1)
                    {
                        appendText();
                        appendText("blogas CRC atsakyme");
                        appendText(atsakymas.AnswerCRC_code);
                        appendText("CRC turėjo būti", atsakymas.CRC_code_should_be);
                        appendText("CRC iš μContr", atsakymas.CRC_code_fromMicroController);
                        appendText(atsakymas.AnswerInBytes);
                        appendText(atsakymas.AnswerInHexString);
                        appendText();
                        changePicture(1);
                    }
                }
                else
                {
                    appendText();
                    appendText("Nuskaitymo problema, I<=0 CS531 eilutė");
                    changePicture(1);
                    appendText();

                }
            }
            catch (Exception ex)
            {
                appendText();
                appendText("Skaičiavimų atsakymo nuskaitymo problema");
                appendText("RunWorker_completed funkcijoje");
                changePicture(1);
                appendText(ex.ToString());
                appendText();
                this.workerProgressBar.Value = 0;
                this.progressLabelText.Text = "0 %";
                closeOpenShutter("C"); //Uždaroma sklendė
            }
        }

        private void displayAnswerAutoVoltage()
        {
            showAnswerInAutoAnswerBox();
            showAnswerInAutoAnswerBox("Cmd. Nr.", "42Vset35", "Tm", "00HV45_15", "Iin", "Iin [nA]", "Statusas", "Err.code");
            showAnswerInAutoAnswerBox(atsakymas.CmdNumber, atsakymas.Answer42Vset35, atsakymas.Tm, atsakymas.Answer00HV45_15, atsakymas.Iin, atsakymas.Iin_ampere, atsakymas.ScaleStatus, atsakymas.AnswerErrorCode);
            answerStatusCodeBox.Text = atsakymas.AnswerErrorCode;
            labelAuto42set35value.Text = "Įtampa " + atsakymas.Answer42Vset35.ToString();
            this.AutoItampa42set35 = atsakymas.Answer42Vset35;
            showAnswerInAutoAnswerBox();
            

        }

        
        private void updateCmdNumber()
        {
            commandNumber = commandNumber + 1;
            commandNumberLabel.Text = commandNumber.ToString();
        }

        private void directlySet42Vset35_value_Click(object sender, EventArgs e)
        {
            bool useA = true;
            if(this.uController_A.Checked)
            {
                useA = true;
            }
            else if (this.uController_B.Checked)
            {
                useA = false;
            }
            else
            {
                useA = true;
            }
            //if false, use B - the new μcontroller;
            
            //===============================================
            //====Tiesiogiai įrašoma įtampa be automatinio nustatymo;
            //byte[] voltage = ExternalFunctions.get42Vset35array((double)voltage42Vset35_in_kiloVolts.Value);

            byte[] cmd = Commands.Write42Vset35IntoMemory(commandNumber, 2, (double)voltage42Vset35_in_kiloVolts.Value, 0x70, useA);
            try
            {
                //bandoma rašyti į portą:
                showData_sent(cmd, "Nusiųsta komanda");
                port.Write(cmd, 0, cmd.Length);
                Thread.Sleep(waitBetweenQuestionResponse);
                //bandoma gauti atsakymą:
                byte[] answerIgot = new byte[22];
                int i = port.Read(answerIgot, 0, answerIgot.Length);
                if ( i > 0)
                {
                    //analizuojamas atsakymas:
                    atsakymas.returnAnswerFromMemoryWriteCMD(answerIgot);
                    if (atsakymas.AnswerCRCerrorCode == 0)
                    {
                        //atvaziduojama
                        showAnswerWhetVoltagehasBeenSet();
                        port.DiscardInBuffer(); //panaikinami data, kad nesiakumuoliuotų?
                        port.DiscardOutBuffer();
                        appendText();
                        appendText("gautas atsakymas į komandą" + commandNumber.ToString());
                        appendText(atsakymas.AnswerInBytes);
                        appendText(atsakymas.AnswerInHexString);
                        appendText();
                        
                    }
                    else if (atsakymas.AnswerCRCerrorCode == -1)
                    {
                        appendText();
                        appendText("blogas CRC atsakyme");
                        appendText("CRC turėjo būti", atsakymas.CRC_code_should_be);
                        appendText("CRC iš μContr", atsakymas.CRC_code_fromMicroController);
                        appendText(atsakymas.AnswerInBytes);
                        appendText(atsakymas.AnswerInHexString);
                        appendText(atsakymas.AnswerCRC_code);
                        appendText();
                    }
                }
                else
                {
                    appendText("Klaida rašant į atmintį F577, gautų baitų kiekis mažiau nei 1-nas");
                    port.DiscardInBuffer(); //panaikinami data, kad nesiakumuoliuotų?
                    port.DiscardOutBuffer();
                }


            }
            catch(Exception ex)
            {
                appendText();
                appendText("Klaida rašant į atmintį arba nuskaitant, F571", ex.Message);
                appendText(ex.ToString());
                appendText();

            }
            updateCmdNumber();

        }

        private void showAnswerWhetVoltagehasBeenSet()
        {
            showAnswerInAutoAnswerBox();
            showAnswerInAutoAnswerBox("Cmd numeris", "Tm", "00HV45_15", "Iin", "Iin [nA]", "ErrCode");
            showAnswerInAutoAnswerBox(atsakymas.CmdNumber, atsakymas.Tm, atsakymas.Answer00HV45_15, atsakymas.Iin, atsakymas.Iin_ampere, atsakymas.AnswerErrorCode);
            this.AutoItampa42set35 = (double)this.voltage42Vset35_in_kiloVolts.Value * 1000.0d;
            //=============================================
            this.labelAuto42set35value.Text = "Įtampa " + AutoItampa42set35.ToString();
            showAnswerInAutoAnswerBox();
        }

        private void currentThresholdValueBox_ValueChanged(object sender, EventArgs e)
        {
            //
            double I_percent = (double)currentThresholdValueBox.Value;
            double scale = 0;
            if(scale50nAradioBtn.Checked)
            {
                scale = 50;
            }
            else if (scale500nARadioButton.Checked)
            {
                scale = 500;
            }
            double I = I_percent / 100.0d * scale;
            string I_str = "I : " + I.ToString() + " nA";
            string txt = "Nustatytas srovės stipris " + I_str;
            scaleLabelInfo.Text = txt;
        }

        

        private void scale50nAradioBtn_CheckedChanged(object sender, EventArgs e)
        {
            //======Įrašoma skalės vertė:
            try
            {
                if(this.scale50nAradioBtn.Checked && canChange50nA) 
                {
                    byte[] cmd = Commands.WriteInStatustoMemory(commandNumber, 0);
                    appendText();
                    appendText("Nusiųsta skalės keitimo į 50nA komanda:");
                    appendText(ExternalFunctions.returnStringfromArray(cmd));
                    appendText(BitConverter.ToString(cmd));
                    appendText();
                    port.Write(cmd, 0, cmd.Length);
                    Thread.Sleep(responseWaitTime);
                    byte[] answ = new byte[22];
                    int i = port.Read(answ, 0, answ.Length);
                    appendText();
                    appendText("Gautas atsakymas į 50nA komandą:");
                    appendText(ExternalFunctions.returnStringfromArray(answ));
                    appendText(BitConverter.ToString(answ));
                    appendText();
                    if (i > 0)
                    {
                        atsakymas.returnLastFourteenBytes(answ);
                        //skalių atnaujinimas pagal atsakymo informaciją:
                        if (atsakymas.AnswerCRCerrorCode == 0)
                        {
                            if (atsakymas.ScaleStatus == 0)
                            {
                                this.ScaleStatus = 0;
                                this.scale50nAradioBtn.Checked = true;
                                this.scale500nARadioButton.Checked = false;
                                canChange50nA = true;
                                canChange500nA = true;
                            }
                            else if (atsakymas.ScaleStatus == 1)
                            {
                                this.ScaleStatus = 1;
                                this.scale500nARadioButton.Checked = true;
                                this.scale50nAradioBtn.Checked = false;
                                //kad veiktų radiobuttonai;
                                canChange50nA = true;
                                canChange500nA = true;
                            }
                            else
                            {
                                //======================================
                                appendText();
                                appendText("Problemos su skalės statusu CS797");
                                appendText(atsakymas.ScaleStatus, "Skalė nusttyta?");
                                appendText();
                            }
                        }
                        else if (atsakymas.AnswerCRCerrorCode == -1)
                        {
                            appendText();
                            appendText("blogas CRC atsakyme");
                            appendText(atsakymas.AnswerCRC_code);
                            appendText();
                        }
                    }
                    else
                    {
                        appendText();
                        appendText("Problema su skalės keitimu, i<=0 CS798");
                        appendText();
                    }
                }
            }
            catch(Exception ex)
            {
                appendText();
                appendText("Problema su skalės keitimu rašant į atmintį cs737");
                appendText(ex.ToString());
                appendText();
            }
        }

        private void scale500nARadioButton_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.scale500nARadioButton.Checked && canChange500nA)
                {
                    byte[] cmd = Commands.WriteInStatustoMemory(commandNumber, 1);
                    appendText();
                    appendText("Nusiųsta skalės keitimo į 500nA komanda:");
                    appendText(ExternalFunctions.returnStringfromArray(cmd));
                    appendText(BitConverter.ToString(cmd));
                    appendText();
                    port.Write(cmd, 0, cmd.Length);
                    Thread.Sleep(responseWaitTime);
                    byte[] answ = new byte[22];
                    int i = port.Read(answ, 0, answ.Length);
                    appendText();
                    appendText("Gautas atsakymas į 500nA komandą:");
                    appendText(ExternalFunctions.returnStringfromArray(answ));
                    appendText(BitConverter.ToString(answ));
                    appendText();                    
                    if (i > 0)
                        {
                            atsakymas.returnLastFourteenBytes(answ);
                        if (atsakymas.AnswerCRCerrorCode == 0)
                        {
                            //info atnaujinimas
                            if (atsakymas.ScaleStatus == 0)
                            {
                                this.ScaleStatus = 0;
                                this.scale50nAradioBtn.Checked = true;
                                this.scale500nARadioButton.Checked = false;
                                canChange50nA = true;
                                canChange500nA = true;
                            }
                            else if (atsakymas.ScaleStatus == 1)
                            {
                                this.ScaleStatus = 1;
                                this.scale500nARadioButton.Checked = true;
                                this.scale50nAradioBtn.Checked = false;
                                //kad veiktų radiobuttonai;
                                canChange50nA = true;
                                canChange500nA = true;
                            }
                            else
                            {
                                //======================================
                                appendText();
                                appendText("Problemos su skalės statusu CS854");
                                appendText(atsakymas.ScaleStatus, "Skalė nusttyta?");
                                appendText();
                            }
                        }
                        else if (atsakymas.AnswerCRCerrorCode == -1)
                        {
                            appendText();
                            appendText("blogas CRC atsakyme");
                            appendText(atsakymas.AnswerCRC_code);
                            appendText();
                        }
                    }
                    else
                    {
                        appendText();
                        appendText("Problema su skalės keitimu, i<=0 CS798");
                        appendText();
                    }

                }
                    
                
            }
            catch (Exception ex)
            {
                appendText();
                appendText("Problema su skalės keitimu rašant į atmintį cs751");
                appendText(ex.ToString());
                appendText();
            }

        }

        //===================================================================================
        //Funkcijos, kurios yra pagalbinės:
        //naudojamos atvaizdoti informaciją etc ...
        //===================================================================================
        private void activateTab_onErrorAndInfoBox(int i = 1)
        {
            errorAndInfoTabs.SelectedIndex = i;
        }
        private void appendText(string text)
        {
            errorAndInfoBox.AppendText(text + newLine);
            activateTab_onErrorAndInfoBox();
        }
        private void appendText(string text, string msg)
        {
            errorAndInfoBox.AppendText(text + " " + msg + newLine);
            activateTab_onErrorAndInfoBox();
        }
        private void appendText(string text, object obj)
        {
            errorAndInfoBox.AppendText(text + obj.ToString() + " "  + newLine);
            activateTab_onErrorAndInfoBox();
        }
        private void appendText()
        {
            errorAndInfoBox.AppendText("==================================" + newLine);
            activateTab_onErrorAndInfoBox();

        }
        private void appendText(params object[] obj)
        {
            string txt = String.Empty;
            foreach (var i in obj)
            {
                txt = txt + i.ToString();

            }
            txt = txt + Environment.NewLine;
            errorAndInfoBox.AppendText(txt);
        }

        private void showAnswerInAutoAnswerBox(string msg)
        {
            automaticAnswerTextBox.AppendText(msg + newLine);
            activateTab_onErrorAndInfoBox(0);
        }
        private void showAnswerInAutoAnswerBox()
        {
            string line = "___________________________________" + newLine;
            automaticAnswerTextBox.AppendText(line);
            activateTab_onErrorAndInfoBox(0);
        }
        private void showAnswerInAutoAnswerBox(params object[] msgs)
        {
            string txt = String.Empty;
            string entrySep = String.Empty; ;
            foreach (var i in msgs)
            {
                txt = txt + ExternalFunctions.retStringEqualSize(i.ToString()) + entrySep;
            }
            txt = txt + newLine;
            automaticAnswerTextBox.AppendText(txt);
            activateTab_onErrorAndInfoBox(0);
        }

        private void showData_sent(byte[] data, string msg = "Nusiųsta komanda")
        {
            appendText();
            appendText(msg + commandNumber.ToString());
            appendText(ExternalFunctions.returnStringfromArray(data));
            appendText("Hex formatu");
            appendText(BitConverter.ToString(data).Replace("-", " "));
            appendText();

        }

        private void showportinfo(SerialPort port)
        {
            errorAndInfoBox.AppendText(port.Handshake.ToString() + " Handshake" + newLine + port.Parity.ToString() + " Parity Bits" + newLine);
            errorAndInfoBox.AppendText(port.StopBits.ToString() + " StopBits" + newLine + port.DataBits.ToString() + " DataBits" + newLine);
            errorAndInfoBox.AppendText(port.BaudRate.ToString() + " BaudRate" + newLine);
            activateTab_onErrorAndInfoBox();
        }

        private void setupEverything()
        {
            /*Fill data, set default values:*/
            int com4index = 0;
            dataBitsBox.SelectedIndex = 7;
            handshakeBox.SelectedIndex = 0;
            parityBitBox.SelectedIndex = 0;
            baudRateBox.SelectedIndex = 11;
            stopBitsBox.SelectedIndex = 0;
            //Arduino code:
            dataBitsArduinoBox.SelectedIndex = 7;
            handshakeArduinoBox.SelectedIndex = 0;
            parityArduinoBox.SelectedIndex = 0;
            baudRateArduinoBox.SelectedIndex = 4;
            stopBitsArduino.SelectedIndex = 0;
            string[] portNames = SerialPort.GetPortNames();
            foreach (string i in portNames)
            {
                comConnectionsListBox.Items.Add(i);
                comForArduinoPortBox.Items.Add(i);
            }
            try
            {
                com4index = comConnectionsListBox.Items.IndexOf("COM4");
                errorAndInfoBox.AppendText("COM index is " + com4index.ToString() + newLine);
                if (com4index == -1)
                {
                    com4index = 0;
                }
            }
            catch (ArgumentNullException ex)
            {
                com4index = 0;
                errorAndInfoBox.AppendText(ex.ToString() + newLine);
            }
            comConnectionsListBox.SelectedIndex = com4index;
            string comNr = commandNumberLabel.Text;
            commandNumber = Convert.ToUInt32(comNr, CultureInfo.InvariantCulture);
            commandManualBox.Text = "1b 76 18 33 33 c0 00 03";
            //appendText("testavimas BIG or LITTLE Endian " + BitConverter.ToInt16(new byte[2] { 0xff, 0x0f }, 0));
            //appendText("testavimas BIG or LITTLE Endian " + BitConverter.ToInt16(new byte[2] { 0x0f, 0xff }, 0));
            this.outputBox.Text = ForInfoViewer.FirstLine();
            this.fileNameFieldBox.Text = "AA1";
            this.dateBoxEntry.Text = ForInfoViewer.GetLongDate();
            this.channelsListBox.SelectedIndex = 0;
            liveArduinoChart.ChartAreas[0].AxisX.Title = "Sekundės nuo stebėjimo pradžios [s]";
            liveArduinoChart.ChartAreas[0].AxisY.Title = "Dujų kiekis [s.v.]";
            changePicture();

        }

        private void checkAndSetcurrentScale()
        {
            try
            {
                byte[] cmd = Commands.ReadIinStatusFrommemory(commandNumber);
                port.Write(cmd, 0, cmd.Length);
                Thread.Sleep(waitBetweenQuestionResponse);
                byte[] ans = new byte[24];
                int i = port.Read(ans, 0, ans.Length);
                appendText();
                appendText("Skalės nustatymo fn-s atsakymas");
                appendText(ExternalFunctions.returnStringfromArray(ans));
                appendText(BitConverter.ToString(ans));
                atsakymas.checkAnswerCrcCode(ans);
                appendText();
                atsakymas.returnInScaleStatusFromMemory(ans);
                //atsakymo laukuose bus naujausia informacija apie skalę 0/1;
                if(atsakymas.AnswerCRCerrorCode == 0)
                {
                    if (atsakymas.ScaleStatus == 0)
                    {
                        this.ScaleStatus = 0;
                        this.scale50nAradioBtn.Checked = true;
                        this.scale500nARadioButton.Checked = false;
                        canChange50nA = true;
                        canChange500nA = true;
                        changePicture(0);
                    }
                    else if (atsakymas.ScaleStatus == 1)
                    {
                        this.ScaleStatus = 1;
                        this.scale500nARadioButton.Checked = true;
                        this.scale50nAradioBtn.Checked = false;
                        //kad veiktų radiobuttonai;
                        canChange50nA = true;
                        canChange500nA = true;
                        changePicture(0);
                    }
                    else
                    {
                        //======================================
                        appendText();
                        appendText("Problemos su skalės statusu C662");
                        appendText(atsakymas.ScaleStatus, "Skalė nustyta?");
                        appendText();
                    }
                }
                else if(atsakymas.AnswerCRCerrorCode == -1)
                {
                    appendText();
                    appendText("Problemos su skalės statuso atsakymu, blogas CRC");
                    changePicture(1);
                    //appendText(atsakymas.ScaleStatus, "Skalė nusttyta?");
                    appendText();
                }
                else
                {
                    //????
                    appendText("ELSE šaka 928");
                    changePicture(1);
                }

            }
            catch (Exception ex)
            {
                appendText();
                appendText("Problema nuskaitant srovės skalės vertę");
                appendText(ex.ToString());
                changePicture(1);
                appendText();
            }
        }

        private void saveFileButton_Click(object sender, EventArgs e)
        {
            changePicture(0);
            //=============================================
            //Failo išsaugojimo funkcija;
            Random rnd = new Random();
            string filestr = fileNameFieldBox.Text;
            string filename = String.Empty;
            string FileName = String.Empty;
            string DataNow = ForInfoViewer.GetLongDate();
            this.dateBoxEntry.Text = DataNow;
            if(filestr.Contains(".txt")|| filestr.Contains(".TXT")|| filestr.Contains(".dat"))
            {
                filename = filestr;
            }
            else
            {
                filename = filestr+ "_"+dateBoxEntry.Text + ".txt";
            }
            try
            {
                if (File.Exists(filename))
                {
                    FileName =DataNow+"_"+ filename;
                }
                else
                {
                    FileName = filename;
                }
            }
            catch (Exception ex)
            {
                appendText();
                appendText("Failo egzistavimo tikrinimo problema, failo pavadinimas nepakeistas");
                appendText(ex.ToString());
                appendText(DataNow);
                appendText();
                FileName = filename;
                changePicture(1);
            }
            try
            {
                string allData = outputBox.Text;
                string ALL_Tex = allData.Replace("|", ";");
                //===Pridedam prie failo galo matavimo parametrus:
                string Parameters = ExternalFunctions.GetLongStringFromObjects("? Ts :",Ts_box.Value, " Tz :", Tz_box.Value, " Tq : ", Tq_box.Value, "  Vq :", Vq_box.Value, " Cth :", Cth_box.Value, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));//Klaustukas - neimportuoti tų eilučių, kas su ? prasideda.
                StreamWriter writer = new StreamWriter(FileName);
                writer.Write(ALL_Tex+newLine+Parameters); //Importuojant geriau, kai skiriamasis simbolis yra ; o ne |, | naudojamas pseudolentelės formavimui;
                writer.Close();
                //===Jei iki čia nenulūžo; tai viskas gerai ir statuse rodoma informacija:
                appendText();
                appendText("Išsaugota į failą " + FileName);
                appendText();
                changePicture(0);
                //====Išvaloma imformacija matavimų:
                this.outputBox.Text = String.Empty;
                this.outputBox.Text = ForInfoViewer.FirstLine();
                MatavimoNr = 0; //Kad iš naujo keliautų matavimo numeriai;
                //this.fileNameFieldBox.Text = "AA1"; //Paliekam senąjį pavadinimą - svarbu kartais, kad nepakistų visgi.
                this.dateBoxEntry.Text = ForInfoViewer.GetLongDate();
                
            }
            catch(Exception ex)
            {
                appendText();
                appendText("Klaida saugant duomenis į failą");
                appendText(ex.ToString());
                appendText();
                appendText(DataNow);
                changePicture(1);
            }
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            //Programos uždarymas
            try
            {
                port.Close();//uždaromas port'as
                cs.disconnect();//Oriel atjungiamas
                //Rigol atjungimas
                instrument.IO.Close();
            }
            catch (Exception ex)
            {
                //nieko nedarom, reiškia, kad port'as net neegzistavo, tad išeinam iš app'so ir tiek;
                Console.WriteLine(ex.ToString()); //Tiksliau, kad išvengtume Warning "ex not used";
            }
            Application.Exit();
            //this.Close();
        }

        private void StartCounting()
        {

            /*Prašymas pradėti skaičiavimą*/
            /*Esc s nr Ts Tz Tq(2 baitai) Cth Vq CRC32*/
            cthThresholdValueBox.Value = Cth_box.Value; //suvienodinimas verčių;
            //==========================================
            updateCmdNumber(); //didinama vienetu, nes anksčiau nebuvo realizuota?
            //closeOpenShutter("O"); // atidaroma sklendė
             //Parametrų nuskaitymas:
            decimal Cth = Cth_box.Value;
            double Ts = (double)Ts_box.Value; //skaičiavimo trukmė sekundėmis, s!;
            double Tz = (double)Tz_box.Value; //trumpinimo trukmė, kas 10 us;
            double Tq = (double)Tq_box.Value;//gesinimo trukmė, kas 10 us;
            double Vq = (double)Vq_box.Value;//įtampa Vq, 1-100 V;
            //==========cmd pasidarymas, kurią siųsime:
            byte[] cmd = Commands.CmdAskStartCounting(commandNumber, Ts, Tz, Tq, Cth, Vq);
            //=========Išvedimas pasitikrinimui, ką siunčiame:
            string cmd_sent = ExternalFunctions.returnStringfromArray(cmd);
            string cmd_sent_hex = BitConverter.ToString(cmd).Replace("-", " ");
            appendText("Skaičiavimo pradėjimo komanda Nr" + commandNumber.ToString());
            appendText(cmd_sent);
            appendText(cmd_sent_hex);
            appendText();
            ReadingParameters rd = new ReadingParameters();
            //======================================================
            //Čia visgi reiktų daryti background workerį, arba laukiantį Ts, arba nenutrūkstamai
            //nuskaitantį skaitiklio reikšmes:
            //======================================================
            //Čia jei Ts == 0; nuskaitomas atsakymas tiesiog?
            //O jei  Ts > 0; paleidžiamas backgroundWorkeris su laukimo paramtru ir progreso 
            //indikacija?
            //======================================================
            rd.Cmd = cmd;
            rd.Ts = Ts; //sekundėmis;
            //padidinam vienetu bet kokiu atveju;s
            if (Ts == 0.0d)
            {
                //Nuskaitomas atsakymas?
                try
                {
                    port.Write(cmd, 0, cmd.Length);
                    Thread.Sleep(responseWaitTime);
                    //TODO:
                    //padaryti nuskaitymą reikia!
                    byte[] answ = new byte[26];
                    int i = port.Read(answ, 0, answ.Length);
                    if (i > 0)
                    {

                        atsakymas.rdAnswerAnalyzer(answ);
                        if (atsakymas.AnswerCRCerrorCode == 0)
                        {
                            //atnaujinam informaciją apie Ni vertes bei kitus duomenis:
                            showAnswerInAutoAnswerBox();
                            showAnswerInAutoAnswerBox("Cmd. Nr.", "Ni vertė", "Tm", "00HV45_15", "Iin", "Iin [nA]", "Statusas", "Err.code");
                            showAnswerInAutoAnswerBox(atsakymas.CmdNumber, atsakymas.Ni, atsakymas.Tm, atsakymas.Answer00HV45_15, atsakymas.Iin, atsakymas.Iin_ampere, atsakymas.ScaleStatus, atsakymas.AnswerErrorCode);
                            //===============================
                            showAnswerInAutoAnswerBox();
                            //===============================
                            //atsakymo išvedimas, pasitikrinimui, nes nebuvo išvedama:
                            appendText();
                            appendText("Gautas atsakymas į " + atsakymas.CmdNumber);
                            appendText(atsakymas.AnswerInBytes);
                            appendText(atsakymas.AnswerInHexString);
                            appendText();
                        }
                        else if (atsakymas.AnswerCRCerrorCode == -1)
                        {
                            appendText();
                            appendText("blogas CRC atsakyme");
                            appendText(atsakymas.AnswerCRC_code);
                            appendText(atsakymas.CRC_code_should_be, "CRC turėjo būti");
                            appendText(atsakymas.CRC_code_fromMicroController, "gautas CRC iš μContr");
                            appendText(atsakymas.AnswerInBytes, "Atsakymas");
                            appendText(atsakymas.AnswerInHexString);
                            appendText();
                            changePicture(1);
                        }
                        port.DiscardInBuffer();
                        port.DiscardOutBuffer();

                    }
                    else
                    {
                        appendText();
                        appendText("I<=0, problema su nuskaitymu atsakymo CS 377 eilutė");
                        appendText();
                        port.DiscardInBuffer();
                        port.DiscardOutBuffer();
                    }
                }
                catch (Exception ex)
                {
                    appendText();
                    appendText("Klaida C369 eilutėje, su porto Rašymu/skaitymu");
                    appendText(ex.Message.ToString());
                    appendText(ex.ToString());
                    appendText();
                    changePicture(1);
                    port.DiscardInBuffer();
                    port.DiscardOutBuffer();
                }
            }
            else
            {
                //Paleidžiamas BackGroundWorker Thread, laukiantis Ts, nuskaitantis atsakymą ir jį grąžinantis?
                try
                {
                    port.Write(cmd, 0, cmd.Length);
                    readAnswerInBackgroundWorker.RunWorkerAsync(rd);
                }
                catch (Exception ex)
                {
                    appendText();
                    appendText(ex.ToString());
                    changePicture(1);
                    appendText();
                }

            }
        }

        private void energyvalueElectronVoltsBox_ValueChanged(object sender, EventArgs e)
        {
            //perskaičiuojama energija, bangos ilgis ir atvaizduojama
            double energy = (double)energyvalueElectronVoltsBox.Value;
            double lambda =Math.Round(( 1239.75d / energy),1);
            this.lambdaOrEnergyInputBox.Value = (decimal)lambda;
        }

        private void pluseVbutton_Click(object sender, EventArgs e)
        {
            //prideda 0,1 eV;
            decimal d = 0.1M;
            decimal u = energyvalueElectronVoltsBox.Value;
            energyvalueElectronVoltsBox.Value = u + d;
        }

        private void minuseVbutton_Click(object sender, EventArgs e)
        {
            //atima 0,1 eV;
            decimal d = 0.1M;
            decimal u = energyvalueElectronVoltsBox.Value;
            energyvalueElectronVoltsBox.Value = u - d;

        }

        private void setWaveLengthButton_Click(object sender, EventArgs e)
        {
            //nustato monochromatoriuje bangos ilgį, nm;
            changePicture(0);
            requiredWavelength = (double)lambdaOrEnergyInputBox.Value;
            //This function sets a required wavelength:
            try
            {

                bool status = cs.sendCommand("GOWAVE " + Convert.ToString(requiredWavelength, CultureInfo.InvariantCulture) + terminator);
                if (status)
                {
                    Thread.Sleep(500); //Slow device.
                    string lambda = cs.getStringResponseFromCommand("WAVE?\r\n");
                    orielResponseStatus.Text = "Atsakas " + lambda;
                }
                else
                {
                    orielResponseStatus.Text = "Something wrong";
                    changePicture(1);
                }
            }
            catch (Exception ex)
            {
                errorAndInfoBox.AppendText("==========================" + newLine);
                errorAndInfoBox.AppendText("bangos ilgio nustatymo problema" + newLine);
                errorAndInfoBox.AppendText(ex.ToString());
                appendText();
                changePicture(1);
            }
       }
            
        

        private void findMonochromator_Click(object sender, EventArgs e)
        {
            //randa ir aktyvina Oriel monochromatorių
            try
            {
                cs = new Cornerstone(true);
                connect();
            }
            catch (Exception ex)
            {
                errorAndInfoBox.AppendText("=======================================" + newLine);
                errorAndInfoBox.AppendText("CyUSB.dll durniuoja?" + newLine);
                errorAndInfoBox.AppendText(ex.ToString() + newLine);
                errorAndInfoBox.AppendText("=======================================" + newLine);
                errorAndInfoBox.AppendText("Patikrinti, ar USB prievadas yra" + newLine + "tas pats, kaip ir diegimo metu" + newLine);
                changePicture(1);
            }
        }

        private void shutterStatusButton_Click(object sender, EventArgs e)
        {
            try {
                //keičia sklendės uždarymą, atidarymą:
                string statusShutter = cs.getStringResponseFromCommand("SHUTTER?" + terminator).Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)[0];
                if (statusShutter == "O")
                {
                    appendText();
                    appendText("Sklendė buvo atidaryta, uždaroma");
                    appendText();
                    cs.sendCommand("SHUTTER C" + terminator);
                    shutterStatusLabel.Text = "C (Uždaryta)";
                }
                if (statusShutter == "C")
                {
                    appendText();
                    appendText("Sklendė uždaryta, atidaroma");
                    appendText();
                    cs.sendCommand("SHUTTER O" + terminator);
                    shutterStatusLabel.Text = "O (Atidaryta)";
                }
                changePicture(0);
            }
            catch (Exception ex)
            {
                changePicture(1);
                appendText();
                appendText("Problema su sklende");
                appendText(ex.ToString());
                appendText();
            }
        }
        void connect()
        {

            cornerstone = cs.device;
            usbDevices = cs.usbDevices;

            int deviceCount = usbDevices.Count;
            orielStatusLabel.Text = "Rasta prietaisų" + " " + deviceCount.ToString();
            //toggleButtons(deviceCount > 0);

            errorAndInfoBox.AppendText(cs.getLastMessage()+newLine);
        }

        private void shutterControlWithdelay_DoWork(object sender, DoWorkEventArgs e)
        {
            ShutterTime shutter = e.Argument as ShutterTime;
            int time = shutter.Time;
            for (int i = 0; i< time; i=i+time/100)
            {
                Thread.Sleep(time / 100);
                double percents = (double)i /(double) time * 100.0d;
                shutterControlWithdelay.ReportProgress((int)percents);
            }

        }

        private void shutterControlWithdelay_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.shutterProgress.Value = e.ProgressPercentage;
        }

        private void shutterControlWithdelay_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                cs.sendCommand("SHUTTER C"+terminator);
                this.shutterProgress.Value = 0;
                changePicture(0);
            }
            catch (Exception ex)
            {
                appendText();
                appendText("Sklendės uždarymas neveikia Thread'e");
                appendText(ex.ToString());
                appendText();
                this.shutterProgress.Value = 0;
                changePicture(1);
            }
        }

        private void timeDelayedShutterControlBtn_Click(object sender, EventArgs e)
        {
            changePicture(0);
            int time_s = (int)timeToBeOpenedShutter.Value;
            int time = time_s * 1000;
            ShutterTime shutterTime = new ShutterTime();
            shutterTime.Time = time;
            try
            {
                cs.sendCommand("SHUTTER O" + terminator);
                shutterControlWithdelay.RunWorkerAsync(shutterTime);
            }
            catch(Exception ex)
            {
                appendText();
                appendText("Problemos su sklende");
                appendText(ex.ToString());
                appendText();
                changePicture(1);
            }
            

        }

        private void closeOpenShutter(string Par)
        {
            try
            {
                cs.sendCommand("SHUTTER "+Par+ terminator);
                changePicture(0);
                //shutterControlWithdelay.RunWorkerAsync(shutterTime);
            }
            catch (Exception ex)
            {
                appendText();
                appendText("Problemos su sklende");
                changePicture(1);
                appendText(ex.ToString());
                appendText();
            }
        }

        private void changePicture(int err=2)
        {
            if(err == 0)
            {
                statusPictureBox.Image = Properties.Resources.Good;
                statusPictureBox.Refresh();
                statusPictureBox.Visible = true;
            }
            else if (err == 1)
            {
                statusPictureBox.Image = Properties.Resources.Wrong;
                statusPictureBox.Refresh();
                statusPictureBox.Visible = true;
            }
            else
            {
                statusPictureBox.Image = Properties.Resources.Quest;
                statusPictureBox.Refresh();
                statusPictureBox.Visible = true;
            }
        }

        private void clearErrorBoxButton_Click(object sender, EventArgs e)
        {
            try
            {
                errorAndInfoBox.Text = String.Empty;
                port.DiscardInBuffer();
                port.DiscardOutBuffer();
            }
            catch(Exception ex)
            {
                appendText();
                appendText("Buferio išvalymas nepavyko");
                appendText(ex.ToString());
                appendText();
            }
        }

        

        private void preesToRunBigCycleButton_Click(object sender, EventArgs e)
        {
            //bool status = cs.sendCommand("GOWAVE " + Convert.ToString(requiredWavelength, CultureInfo.InvariantCulture) + terminator)
            //Čia bus galingas ciklas su thread'ais kuris matuos viską. Vat.
            //===Pradžiai užteks ConsoleWriteLn:
            //Sukuriamas naujas kintamasis dėl parametrų:
            changePicture(0);
            MeasurementParameters mPar = new MeasurementParameters();
            mPar.Tq = (double)Tq_box.Value;
            mPar.Tz = (double)Tz_box.Value;
            mPar.Cth = (double)Cth_box.Value;
            mPar.Vq = (double)Vq_box.Value;
            mPar.Ts = (double)Ts_box.Value;
            mPar.Forward = fromSmallestBox.Checked;
            mPar.LEDWorkingTime = (int)(timeForLEDBox.Value * 1000); //laikas ms jau
            mPar.LEDWaitAfterWorkTime = (int)(waitBetweenLEDandScanTime.Value * 1000); //laikas irgi ms jau
            mPar.UseLPT = useLPT; //ar naudoti LPT ar ne prieš matavimų ciklą?
            mPar.DataLPT = dataLPT;//ką įrašinėti?
            mPar.useLEDorHalogen = useLEDorHalogen;
            stepSize = (double)stepValueBox.Value;
            if(setStepDirect05eVbox.Checked)
            {
                mPar.Step = stepSize;
                mPar.StepDirection = true; //nustatoma matavimų kryptis;
            }
            else
            {
                mPar.Step = stepSize; //teoriškai bus galima skenuoti ir kas mažiau?
                mPar.StepDirection = false;
            }
            //Problema su viršuje esančių parametrų pasiėmimu:
            //Ne, toliau buvo Ts su Tz supainiotas; =Pataisyta=;
            appendText();
            appendText(mPar.Tq,mPar.Tz, mPar.Cth, mPar.Vq, mPar.Ts);
            appendText("Tq, Tz, Cth, Vq, Ts");
            appendText();
            mPar.StartingEnergy = (double)energyDownToValueBox.Value;
            mPar.EndingEnergy = (double)energyUpToValueBox.Value;
            mPar.Counts = (double)measureTimesforOnePointBox.Value;
            //======Autodrop voltage:
            mPar.useAutoDrop = useAutoDropVoltageBox.Checked;
            mPar.AutoDropVoltageValue = autoDropDownVoltageBox.Value; //Voltai!
            //======
            mPar.currentVoltage = (double)voltage42Vset35_in_kiloVolts.Value; //kV!
            //==================================
            appendText();
            double time = (double)Ts_box.Value * 1.1;
            double counts = ExternalFunctions.returnCounts((double)measureTimesforOnePointBox.Value, (double)energyDownToValueBox.Value,
                (double)energyUpToValueBox.Value, stepSize);
            appendText("Matavimų taškų bus apytiksliai ", counts);
            appendText("Matavimai truks maždaug [s]", counts * time);
            double minutes = (counts * time - (counts * time) % 60) / 60;
            appendText("Matavimai truks maždaug  ", minutes, " min. ", (int)((counts * time) % 60), " sekund.");
            appendText();
            //============================================
            longMeasurementsThreadWorker.RunWorkerAsync(mPar);
            //===================================
            //Išjungiami valdikliai, skirti prietaisų kontrolei, 
            //kad nebūtų paspaudimų atsitiktinių: (RUN button'as);
            //===================================
            this.preesToRunBigCycleButton.Enabled = false;
            this.cancelButton.Enabled = true;
            //Keičiamos ašių ribos, kad nebūtų kitų verčių:
            changeGraphAxis((double)energyDownToValueBox.Value, (double)energyUpToValueBox.Value);
            removePointsFromGraph();//Kad nesimaišytų su ktais matavimais;
            


        }
        //==================================================
        //Čia aktyvuojamas thread'as, matuojantis nuo iki ir tamsą;
        //==================================================
        private void longMeasurementsThreadWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            MeasurementParameters mP = e.Argument as MeasurementParameters; //Paimame argumentus darbui atlikti
            //Skaičiavimo komandos pasidarymas:
            Answer longAts = new Answer(); //Atsakymų analizei;
            //======================
            //procentinio progreso skaičiavimui:
            //======================
            double x = (mP.EndingEnergy - mP.StartingEnergy) / 0.05d * mP.Counts + 3 * mP.Counts+3;
            //ConsoleWriter.WriteOutput(mP.Counts, mP.EndingEnergy, mP.StartingEnergy, " mP Counts, Ending, starting", x);
            //======================
            double en = 0;
            //double currEnergy = 0;
            //???
            uint CmdNmb = 0;
            byte[] cmd = Commands.CmdAskStartCounting(CmdNmb, mP.Ts, mP.Tz, mP.Tq, (decimal)mP.Cth, mP.Vq);
            //Turim komandą, kurią siuntinėsim pastoviai cikle
            //Pirmasis ciklas skirtas tamsos skaičiavimui:
            int Count = (int)mP.Counts;
            double step = mP.Step;//koks step'as???
            bool Continue = true;
            double[] uffVertes = new double[3] { 0, 0, 0 };//kintamųjų pasiėmimui?;
            uffVertes = ExternalFunctions.getStepStartingEndingEnergy(mP.Forward, mP.StartingEnergy, mP.EndingEnergy, mP.Step);
            bool direction = mP.Forward;
            if(mP.StepDirection)//if true
            {
                //====Matavimai nuo iki kas 0,05d; trumpai;
                //========START======
                try
                {
                    //Vienas tamsos matavimų ciklas
                    //====
                    //Tamsos matavimas;
                    cs.sendCommand("SHUTTER C" + terminator);//kad tikrai būtų tamsa?
                    Thread.Sleep(1000);
                    //tamsos matavimo ciklas
                    for (int i = 0; i < Count; i++)
                    {
                        //Ilgas IF tikrinantis threado nutraukimą:
                        if (!longMeasurementsThreadWorker.CancellationPending)
                        {
                            port.Write(cmd, 0, cmd.Length);
                            Thread.Sleep((int)(mP.Ts * 1000 * 1.1));//laukiama  kol išmatuos 10% ilgiau laiko;
                            byte[] answ = new byte[26];
                            int status = port.Read(answ, 0, answ.Length);
                            if ((status > 0))
                            {
                                //kažką pavyko nuskaityti, apdorojam
                                longAts.returnCounterValue(answ);
                                if (longAts.AnswerCRCerrorCode == 0)
                                {
                                    mP.CurrentLambda = "Tamsa"; //Tamsa pas mus visada žymima 0-liu;
                                    mP.EnergyValue = -1.0d;
                                    mP.Ni = longAts.Ni; //Grąžinama reikiama vertė;
                                                        //atnaujina imformacija:
                                    mP.ExceptionCode = 0;
                                    mP.AnswerCRCCode = 0; //Būtina atnaujinti laukus, nes gali būti likę iš seniau;
                                    mP.CurrentMeasurementTime = mP.Ts;
                                    int percentage = (int)((CmdNmb) / x * 100);
                                    longMeasurementsThreadWorker.ReportProgress(percentage, mP);
                                    CmdNmb++;
                                }
                                else
                                {
                                    //grąžinama info apie CRC kodų problemas:
                                    mP.AnswerCRCCode = longAts.AnswerCRCerrorCode;
                                    mP.ExceptionCode = 1; // kad nebūtų vaizduojama atsakymai
                                    longMeasurementsThreadWorker.ReportProgress(100, mP);
                                }
                            }
                        }
                    }
                    //Vienas ciklas siganlui:
                    //=====
                    double energy = mP.StartingEnergy;
                    while((energy <= mP.EndingEnergy)&& (longMeasurementsThreadWorker.CancellationPending != true))
                    {
                        //====Matavimų ciklas:
                        string lmbd = Commands.retLambdaStringFromEv(energy);
                        bool statusas = cs.sendCommand("GOWAVE " + lmbd + terminator); //nustatomas reikiamas bangos ilgis;
                        Thread.Sleep(1000); //kad spėtų nustatyti bangos ilgį;
                        string lambda = cs.getStringResponseFromCommand("WAVE?\r\n");
                        if (statusas)
                        {
                            for (int i = 0; i < Count; i++) //atliekami matavimai Count kartų 
                            {
                                //====Čia įterpiamas LED LPT kodas:
                                if(mP.UseLPT)
                                {
                                    if (mP.useLEDorHalogen) //LED kodas
                                    {
                                        if (mP.useAutoDrop)
                                        {
                                            double dropValue = (double)mP.AutoDropVoltageValue;
                                            AutoVoltageDirectlySet42Vset35(mP.currentVoltage, -1.0d * dropValue, longAts);
                                        }
                                        LPTLibraryCMDS.Output(0x0378, mP.DataLPT);
                                        Thread.Sleep(mP.LEDWorkingTime);
                                        LPTLibraryCMDS.Output(0x0378, 0);//išjungiama;
                                        Thread.Sleep(mP.LEDWaitAfterWorkTime);//laukiama efekto?
                                        if (mP.useAutoDrop)
                                        {
                                            double dropValue = (double)mP.AutoDropVoltageValue;
                                            AutoVoltageDirectlySet42Vset35(mP.currentVoltage, 0, longAts);
                                        }
                                    }
                                    else //halogeninės lemputės kodas
                                    {
                                        if (mP.useAutoDrop)
                                        {
                                            double dropValue = (double)mP.AutoDropVoltageValue;
                                            AutoVoltageDirectlySet42Vset35(mP.currentVoltage, -1.0d * dropValue, longAts);
                                        }
                                        //Įjungiama halogeninė lempute
                                        powerSupply.ConfigureGeneralOutputState(true);
                                        Thread.Sleep(mP.LEDWorkingTime);
                                        //Išjungiama halogeninė lemputė
                                        powerSupply.ConfigureGeneralOutputState(false);
                                        Thread.Sleep(mP.LEDWaitAfterWorkTime);//laukiama efekto?
                                        if (mP.useAutoDrop)
                                        {
                                            double dropValue = (double)mP.AutoDropVoltageValue;
                                            AutoVoltageDirectlySet42Vset35(mP.currentVoltage, 0, longAts);
                                        }
                                    }
                                }
                                //===LPT kodo pabaiga
                                cs.sendCommand("SHUTTER O" + terminator); //atidaroma
                                Thread.Sleep(1000);
                                port.Write(cmd, 0, cmd.Length); //matuojama
                                Thread.Sleep((int)(mP.Ts * 1000 * 1.1));//laukiama  kol išmatuos 10% ilgiau laiko;
                                byte[] answ = new byte[26];
                                int status = port.Read(answ, 0, answ.Length);
                                if (status > 0)
                                {
                                    //kažką pavyko nuskaityti, apdorojam
                                    longAts.returnCounterValue(answ);
                                    if (longAts.AnswerCRCerrorCode == 0)
                                    {
                                        //mP.CurrentLambda = Commands.returnCurrentLambda(en); //dabartinė λ
                                        mP.EnergyValue = energy;//dabartinė energija;
                                        mP.Ni = longAts.Ni; //Grąžinama reikiama vertė;
                                        mP.CurrentLambda = Commands.returnCurrentLambda(energy); //atnaujina imformacija:
                                        mP.LambdaFromCs = lambda;
                                        mP.ExceptionCode = 0;
                                        mP.AnswerCRCCode = 0; //Būtina atnaujinti laukus, nes gali būti likę iš seniau;
                                        mP.CurrentMeasurementTime = mP.Ts;
                                        int percentage = (int)((CmdNmb + 1) / x * 100);
                                        longMeasurementsThreadWorker.ReportProgress(percentage, mP);
                                        CmdNmb++;
                                    }
                                    else
                                    {
                                        //grąžinama info apie CRC kodų problemas:
                                        mP.AnswerCRCCode = longAts.AnswerCRCerrorCode;
                                        mP.ExceptionCode = 1; // kad nebūtų vaizduojama atsakymai
                                        longMeasurementsThreadWorker.ReportProgress(100, mP);
                                    }
                                }
                                cs.sendCommand("SHUTTER C" + terminator); //uždaroma;
                                Thread.Sleep(2000);
                            }
                        }
                        else
                        {
                            ConsoleWriter.WriteOutput("Problemos su cs bangos ilgio nustatymu");
                        }
                        //====Matavmų ciklo pabaiga====
                        energy = energy + step;
                    }
                    //Pakartota tamsa:
                    //=====
                    //Tamsos matavimas;
                    cs.sendCommand("SHUTTER C" + terminator);//kad tikrai būtų tamsa?
                    Thread.Sleep(1000);
                    //tamsos matavimo ciklas
                    for (int i = 0; i < Count; i++)
                    {
                        //Ilgas IF tikrinantis threado nutraukimą:
                        if (!longMeasurementsThreadWorker.CancellationPending)
                        {
                            port.Write(cmd, 0, cmd.Length);
                            Thread.Sleep((int)(mP.Ts * 1000 * 1.1));//laukiama  kol išmatuos 10% ilgiau laiko;
                            byte[] answ = new byte[26];
                            int status = port.Read(answ, 0, answ.Length);
                            if ((status > 0))
                            {
                                //kažką pavyko nuskaityti, apdorojam
                                longAts.returnCounterValue(answ);
                                if (longAts.AnswerCRCerrorCode == 0)
                                {
                                    mP.CurrentLambda = "Tamsa"; //Tamsa pas mus visada žymima 0-liu;
                                    mP.EnergyValue = -1.0d;
                                    mP.Ni = longAts.Ni; //Grąžinama reikiama vertė;
                                                        //atnaujina imformacija:
                                    mP.ExceptionCode = 0;
                                    mP.AnswerCRCCode = 0; //Būtina atnaujinti laukus, nes gali būti likę iš seniau;
                                    mP.CurrentMeasurementTime = mP.Ts;
                                    int percentage = (int)((CmdNmb) / x * 100);
                                    longMeasurementsThreadWorker.ReportProgress(percentage, mP);
                                    CmdNmb++;
                                }
                                else
                                {
                                    //grąžinama info apie CRC kodų problemas:
                                    mP.AnswerCRCCode = longAts.AnswerCRCerrorCode;
                                    mP.ExceptionCode = 1; // kad nebūtų vaizduojama atsakymai
                                    longMeasurementsThreadWorker.ReportProgress(100, mP);
                                }
                            }
                        }
                    }

                }
                catch(Exception ex)
                {
                    mP.ExceptionCode = 1;
                    mP.ExceptionMessage = ex.ToString();
                    longMeasurementsThreadWorker.ReportProgress(100, mP);
                }
                //========END========
            }
            else if (!mP.StepDirection)//if !false=true
            {
                //Matavimai nuo iki kas 0,1d
                //======START=====
                try
                {
                    //Tamsos matavimas;
                    cs.sendCommand("SHUTTER C" + terminator);//kad tikrai būtų tamsa?
                    Thread.Sleep(1000);
                    //tamsos matavimo ciklas
                    for (int i = 0; i < Count; i++)
                    {
                        //Ilgas IF tikrinantis threado nutraukimą:
                        if (!longMeasurementsThreadWorker.CancellationPending)
                        {
                            port.Write(cmd, 0, cmd.Length);
                            Thread.Sleep((int)(mP.Ts * 1000 * 1.1));//laukiama  kol išmatuos 10% ilgiau laiko;
                            byte[] answ = new byte[26];
                            int status = port.Read(answ, 0, answ.Length);
                            if ((status > 0))
                            {
                                //kažką pavyko nuskaityti, apdorojam
                                longAts.returnCounterValue(answ);
                                if (longAts.AnswerCRCerrorCode == 0)
                                {
                                    mP.CurrentLambda = "Tamsa"; //Tamsa pas mus visada žymima 0-liu;
                                    mP.EnergyValue = -1.0d;
                                    mP.Ni = longAts.Ni; //Grąžinama reikiama vertė;
                                                        //atnaujina imformacija:
                                    mP.ExceptionCode = 0;
                                    mP.AnswerCRCCode = 0; //Būtina atnaujinti laukus, nes gali būti likę iš seniau;
                                    mP.CurrentMeasurementTime = mP.Ts;
                                    int percentage = (int)((CmdNmb) / x * 100);
                                    longMeasurementsThreadWorker.ReportProgress(percentage, mP);
                                    CmdNmb++;
                                }
                                else
                                {
                                    //grąžinama info apie CRC kodų problemas:
                                    mP.AnswerCRCCode = longAts.AnswerCRCerrorCode;
                                    mP.ExceptionCode = 1; // kad nebūtų vaizduojama atsakymai
                                    longMeasurementsThreadWorker.ReportProgress(100, mP);
                                }
                            }
                        }
                    }
                    //tamsos matavimo ciklo pabaiga
                    //taškų matavimas nuo eV max iki eV min kas 0,1 ev:
                    //for(double en = mP.EndingEnergy; en<= mP.StartingEnergy; en=en-0.1d)//while ciklu reikia keisti!
                    //===================================
                    //Nusistatom pradines energijas:

                    en = uffVertes[1]; //starting energy;
                    step = uffVertes[0]; //turėtų jau veikti ir step'o kontrolė
                    while ((Continue) && (longMeasurementsThreadWorker.CancellationPending != true))
                    {
                        string lmbd = Commands.retLambdaStringFromEv(en);
                        bool statusas = cs.sendCommand("GOWAVE " + lmbd + terminator); //nustatomas reikiamas bangos ilgis;
                        Thread.Sleep(1000); //kad spėtų nustatyti bangos ilgį;
                        string lambda = cs.getStringResponseFromCommand("WAVE?\r\n");
                        if (statusas)
                        {
                            for (int i = 0; i < Count; i++) //atliekami matavimai Count kartų 
                            {
                                //====Čia įterpiamas LED LPT kodas:
                                if (mP.UseLPT)
                                {
                                    if (mP.useLEDorHalogen) //LED kodas
                                    {
                                        if (mP.useAutoDrop)
                                        {
                                            double dropValue = (double)mP.AutoDropVoltageValue;
                                            AutoVoltageDirectlySet42Vset35(mP.currentVoltage, -1.0d * dropValue, longAts);
                                        }
                                        LPTLibraryCMDS.Output(0x0378, mP.DataLPT);
                                        Thread.Sleep(mP.LEDWorkingTime);
                                        LPTLibraryCMDS.Output(0x0378, 0);//išjungiama;
                                        Thread.Sleep(mP.LEDWaitAfterWorkTime);//laukiama efekto?
                                        if (mP.useAutoDrop)
                                        {
                                            double dropValue = (double)mP.AutoDropVoltageValue;
                                            AutoVoltageDirectlySet42Vset35(mP.currentVoltage, 0, longAts);
                                        }
                                    }
                                    else //halogeninės lemputės kodas
                                    {
                                        if (mP.useAutoDrop)
                                        {
                                            double dropValue = (double)mP.AutoDropVoltageValue;
                                            AutoVoltageDirectlySet42Vset35(mP.currentVoltage, -1.0d * dropValue, longAts);
                                        }
                                        //Įjungiama halogeninė lempute
                                        powerSupply.ConfigureGeneralOutputState(true);
                                        Thread.Sleep(mP.LEDWorkingTime);
                                        //Išjungiama halogeninė lemputė
                                        powerSupply.ConfigureGeneralOutputState(false);
                                        Thread.Sleep(mP.LEDWaitAfterWorkTime);//laukiama efekto?
                                        if (mP.useAutoDrop)
                                        {
                                            double dropValue = (double)mP.AutoDropVoltageValue;
                                            AutoVoltageDirectlySet42Vset35(mP.currentVoltage, 0, longAts);
                                        }
                                    }
                                }
                                //===LPT kodo pabaiga
                                cs.sendCommand("SHUTTER O" + terminator); //atidaroma
                                Thread.Sleep(1000);
                                port.Write(cmd, 0, cmd.Length); //matuojama
                                Thread.Sleep((int)(mP.Ts * 1000 * 1.1));//laukiama  kol išmatuos 10% ilgiau laiko;
                                byte[] answ = new byte[26];
                                int status = port.Read(answ, 0, answ.Length);
                                if (status > 0)
                                {
                                    //kažką pavyko nuskaityti, apdorojam
                                    longAts.returnCounterValue(answ);
                                    if (longAts.AnswerCRCerrorCode == 0)
                                    {
                                        //mP.CurrentLambda = Commands.returnCurrentLambda(en); //dabartinė λ
                                        mP.EnergyValue = en;//dabartinė energija;
                                        mP.Ni = longAts.Ni; //Grąžinama reikiama vertė;
                                        mP.CurrentLambda = Commands.returnCurrentLambda(en); //atnaujina imformacija:
                                        mP.LambdaFromCs = lambda;
                                        mP.ExceptionCode = 0;
                                        mP.AnswerCRCCode = 0; //Būtina atnaujinti laukus, nes gali būti likę iš seniau;
                                        mP.CurrentMeasurementTime = mP.Ts;
                                        int percentage = (int)((CmdNmb + 1) / x * 100);
                                        longMeasurementsThreadWorker.ReportProgress(percentage, mP);
                                        CmdNmb++;
                                    }
                                    else
                                    {
                                        //grąžinama info apie CRC kodų problemas:
                                        mP.AnswerCRCCode = longAts.AnswerCRCerrorCode;
                                        mP.ExceptionCode = 1; // kad nebūtų vaizduojama atsakymai
                                        longMeasurementsThreadWorker.ReportProgress(100, mP);
                                    }
                                }
                                cs.sendCommand("SHUTTER C" + terminator); //uždaroma;
                                Thread.Sleep(2000);
                            }
                        }
                        else
                        {
                            ConsoleWriter.WriteOutput("Problemos su cs bangos ilgio nustatymu");
                        }
                        en = en + step;
                        Continue = ExternalFunctions.Continue(mP.StartingEnergy, mP.EndingEnergy, en, direction, step);

                    }
                    //taškų matavimo nuo eV max iki eV min kas 0,1 ev pabaiga
                    //Tamsos matavimas;
                    cs.sendCommand("SHUTTER C" + terminator);//kad tikrai būtų tamsa?
                    Thread.Sleep(1000);
                    //tamsos matavimo ciklas
                    for (int i = 0; i < Count; i++)
                    {
                        if (longMeasurementsThreadWorker.CancellationPending != true)
                        {

                            port.Write(cmd, 0, cmd.Length);
                            Thread.Sleep((int)(mP.Ts * 1000 * 1.1));//laukiama  kol išmatuos 10% ilgiau laiko;
                            byte[] answ = new byte[26];
                            int status = port.Read(answ, 0, answ.Length);
                            if (status > 0)
                            {
                                //kažką pavyko nuskaityti, apdorojam
                                longAts.returnCounterValue(answ);
                                if (longAts.AnswerCRCerrorCode == 0)
                                {
                                    mP.CurrentLambda = "Tamsa"; //Tamsa pas mus visada žymima 0-liu;
                                    mP.EnergyValue = -1.0d;
                                    mP.Ni = longAts.Ni; //Grąžinama reikiama vertė;
                                                        //atnaujina imformacija:
                                    mP.ExceptionCode = 0;
                                    mP.AnswerCRCCode = 0; //Būtina atnaujinti laukus, nes gali būti likę iš seniau;
                                    mP.CurrentMeasurementTime = mP.Ts;
                                    int percentage = (int)((CmdNmb) / x * 100);
                                    longMeasurementsThreadWorker.ReportProgress(percentage, mP);
                                    CmdNmb++;
                                }
                                else
                                {
                                    //grąžinama info apie CRC kodų problemas:
                                    mP.AnswerCRCCode = longAts.AnswerCRCerrorCode;
                                    mP.ExceptionCode = 1; // kad nebūtų vaizduojama atsakymai
                                    longMeasurementsThreadWorker.ReportProgress(100, mP);
                                }
                            }
                        }

                    }
                    //tamsos matavimo ciklo pabaiga
                    //==========================
                    //Antroji matavimų pusė:
                    //====================================================
                    //for (double en = mP.EndingEnergy-0.05d; en <= mP.StartingEnergy-0.05d; en = en - 0.1d)
                    Continue = true;
                    en = uffVertes[1] - 0.05d; //turėtų veikti matavimas ir reversu, ir aversu;
                    while ((Continue) && (longMeasurementsThreadWorker.CancellationPending != true))
                    {
                        string lmbd = Commands.retLambdaStringFromEv(en);
                        bool statusas = cs.sendCommand("GOWAVE " + lmbd + terminator); //nustatomas reikiamas bangos ilgis;
                        Thread.Sleep(1000); //kad spėtų nustatyti bangos ilgį;
                        if (statusas)
                        {
                            string lambda = cs.getStringResponseFromCommand("WAVE?\r\n");
                            Thread.Sleep(100);
                            for (int i = 0; i < Count; i++) //atliekami matavimai Count kartų 
                            {
                                //====Čia įterpiamas LED LPT kodas:
                                if (mP.UseLPT)
                                {
                                    if (mP.useLEDorHalogen) //LED kodas
                                    {
                                        if (mP.useAutoDrop)
                                        {
                                            double dropValue = (double)mP.AutoDropVoltageValue;
                                            AutoVoltageDirectlySet42Vset35(mP.currentVoltage, -1.0d * dropValue, longAts);
                                        }
                                        LPTLibraryCMDS.Output(0x0378, mP.DataLPT);
                                        Thread.Sleep(mP.LEDWorkingTime);
                                        LPTLibraryCMDS.Output(0x0378, 0);//išjungiama;
                                        Thread.Sleep(mP.LEDWaitAfterWorkTime);//laukiama efekto?
                                        if (mP.useAutoDrop)
                                        {
                                            double dropValue = (double)mP.AutoDropVoltageValue;
                                            AutoVoltageDirectlySet42Vset35(mP.currentVoltage, 0, longAts);
                                        }
                                    }
                                    else //halogeninės lemputės kodas
                                    {
                                        if (mP.useAutoDrop)
                                        {
                                            double dropValue = (double)mP.AutoDropVoltageValue;
                                            AutoVoltageDirectlySet42Vset35(mP.currentVoltage, -1.0d * dropValue, longAts);
                                        }
                                        //Įjungiama halogeninė lempute
                                        powerSupply.ConfigureGeneralOutputState(true);
                                        Thread.Sleep(mP.LEDWorkingTime);
                                        //Išjungiama halogeninė lemputė
                                        powerSupply.ConfigureGeneralOutputState(false);
                                        Thread.Sleep(mP.LEDWaitAfterWorkTime);//laukiama efekto?
                                        if (mP.useAutoDrop)
                                        {
                                            double dropValue = (double)mP.AutoDropVoltageValue;
                                            AutoVoltageDirectlySet42Vset35(mP.currentVoltage, 0, longAts);
                                        }
                                    }
                                }
                                //===LPT kodo pabaiga
                                cs.sendCommand("SHUTTER O" + terminator); //atidaroma
                                Thread.Sleep(1000);
                                port.Write(cmd, 0, cmd.Length); //matuojama
                                Thread.Sleep((int)(mP.Ts * 1000 * 1.1));//laukiama  kol išmatuos 10% ilgiau laiko;
                                byte[] answ = new byte[26];
                                int status = port.Read(answ, 0, answ.Length);
                                if (status > 0)
                                {
                                    //kažką pavyko nuskaityti, apdorojam
                                    longAts.returnCounterValue(answ);
                                    if (longAts.AnswerCRCerrorCode == 0)
                                    {
                                        mP.CurrentLambda = Commands.returnCurrentLambda(en); //dabartinė λ
                                        mP.EnergyValue = en;//dabartinė energija;
                                        mP.Ni = longAts.Ni; //Grąžinama reikiama vertė;
                                        mP.LambdaFromCs = lambda;                //atnaujina imformacija:
                                        mP.ExceptionCode = 0;
                                        mP.AnswerCRCCode = 0; //Būtina atnaujinti laukus, nes gali būti likę iš seniau;
                                        mP.CurrentMeasurementTime = mP.Ts;
                                        int percentage = (int)((CmdNmb + 1) / x * 100);
                                        longMeasurementsThreadWorker.ReportProgress(percentage, mP);
                                        CmdNmb++;
                                    }
                                    else
                                    {
                                        //grąžinama info apie CRC kodų problemas:
                                        mP.AnswerCRCCode = longAts.AnswerCRCerrorCode;
                                        mP.ExceptionCode = 1; // kad nebūtų vaizduojama atsakymai
                                        longMeasurementsThreadWorker.ReportProgress(100, mP);
                                    }
                                }
                                cs.sendCommand("SHUTTER C" + terminator); //uždaroma;
                                Thread.Sleep(2000);
                            }
                        }
                        en = en + step;
                        Continue = ExternalFunctions.Continue(mP.StartingEnergy, mP.EndingEnergy, en, direction);
                    }
                    //antrosios matavimų pusės pabaiga;
                    //Kartojamas tamsos matavimas:
                    //Tamsos matavimas;
                    cs.sendCommand("SHUTTER C" + terminator);//kad tikrai būtų tamsa?
                    Thread.Sleep(1000);
                    //tamsos matavimo ciklas
                    for (int i = 0; i < Count; i++)
                    {
                        if (longMeasurementsThreadWorker.CancellationPending != true)
                        {
                            port.Write(cmd, 0, cmd.Length);
                            Thread.Sleep((int)(mP.Ts * 1000 * 1.1));//laukiama  kol išmatuos 10% ilgiau laiko;
                            byte[] answ = new byte[26];
                            int status = port.Read(answ, 0, answ.Length);
                            if (status > 0)
                            {
                                //kažką pavyko nuskaityti, apdorojam
                                longAts.returnCounterValue(answ);
                                if (longAts.AnswerCRCerrorCode == 0)
                                {
                                    mP.CurrentLambda = "Tamsa"; //Tamsa pas mus visada žymima -1-liu;
                                    mP.EnergyValue = -1.0d;
                                    mP.Ni = longAts.Ni; //Grąžinama reikiama vertė;
                                                        //atnaujina imformacija:
                                    mP.ExceptionCode = 0;
                                    mP.AnswerCRCCode = 0; //Būtina atnaujinti laukus, nes gali būti likę iš seniau;
                                    mP.CurrentMeasurementTime = mP.Ts;
                                    int percentage = (int)((CmdNmb) / x * 100);
                                    longMeasurementsThreadWorker.ReportProgress(percentage, mP);
                                    CmdNmb++;
                                }
                                else
                                {
                                    //grąžinama info apie CRC kodų problemas:
                                    mP.AnswerCRCCode = longAts.AnswerCRCerrorCode;
                                    mP.ExceptionCode = 1; // kad nebūtų vaizduojama atsakymai
                                    longMeasurementsThreadWorker.ReportProgress(100, mP);
                                }
                            }

                        }
                    }
                    //tamsos matavimo ciklo pabaiga
                }
                catch (Exception ex)
                {
                    mP.ExceptionCode = 1;
                    mP.ExceptionMessage = ex.ToString();
                    longMeasurementsThreadWorker.ReportProgress(100, mP);
                }
                //======END=======

            }
            else
            {
                //Čia visai šūdas, jei čia patenkama
                Console.WriteLine("ŠŪDAS (CS:1801 eilutė CTRL+F)");
            }
            


        }
        //==================================================
        //Čia atnaujinama informacija su matavimo rezultatais
        //==================================================
        private void longMeasurementsThreadWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            MeasurementParameters mP = e.UserState as MeasurementParameters;
            int percents = e.ProgressPercentage;
            if(mP.ExceptionCode == 0)
            {
                //Geras kodas
                updateGraphWithPoint(mP.Ni, mP.EnergyValue);//grafikas atnaujintas
                //Normuojam į minutę:
                string NiPerMinute = ExternalFunctions.getNiPerMinute(mP.Ni, mP.CurrentMeasurementTime);
                string SqrtNi = ExternalFunctions.SqrtNiPerMinute(mP.Ni, mP.CurrentMeasurementTime);
                double CorrNi = LightCorrections.GetNiDividedByLight(mP.Ni, mP.EnergyValue, mP.CurrentMeasurementTime);
                //dujų skaitiklio muskaitymas:
                string gass_value = gassIndicatorLabel.Text.Replace("s.v.", "");
                string curVoltage = voltage42Vset35_in_kiloVolts.Value.ToString();
                if (mP.EnergyValue <= 0.0d)
                {
                    this.outputBox.AppendText(ForInfoViewer.AddString(MatavimoNr, mP.CurrentLambda, mP.Ni, NiPerMinute, SqrtNi,  curVoltage, gass_value, DateTime.Now.ToString("HH:mm:ss")));
                }
                else
                {
                    this.outputBox.AppendText(ForInfoViewer.AddString(MatavimoNr, Math.Round(mP.EnergyValue, 4), mP.Ni, NiPerMinute, SqrtNi,  curVoltage, gass_value, DateTime.Now.ToString("HH:mm:ss")));
                }
                MatavimoNr++;//didinamas matavimo nr vienetu ir atvaziduojama infobloke;
                //tnaujinamas atsakas iš CS:
                this.orielResponseStatus.Text = mP.LambdaFromCs;
                //atnaujinamas progressbar'as:

                try
                {
                    this.workerProgressBar.Value = percents;
                    this.progressLabelText.Text = percents.ToString() + " %";
                }
                catch (Exception ex)
                {
                    //Do nothing.
                    //Do something:
                    this.workerProgressBar.Value = 100;//Vistiek virš suskaičiuoto progreso;
                    this.progressLabelText.Text = percents.ToString() + "%!";
                    this.anotherTexBoxErrorsOnly.AppendText("Vėl daugiau nei 100" + newLine+ex.Message.ToString()+newLine);
                }

            }
            else if (mP.ExceptionCode == 1 && mP.AnswerCRCCode == 0)
            {
                //klaidų apdorojimas
                appendText();
                appendText("PROBLEMOS su ilgu cikliniu matavimu");
                changePicture(1);
                appendText(mP.ExceptionMessage);
                appendText();
                //Jei klaida, tai gal buvo Exception, tai RUN reikia įjungti:
                this.preesToRunBigCycleButton.Enabled = true;
                this.workerProgressBar.Value = 0;
                this.progressLabelText.Text = "0 %";
            }
            else if (mP.ExceptionCode == 1 && mP.AnswerCRCCode == 1)
            {
                //klaidų apdorojimas, čia CRC bloga šaka
                appendText();
                appendText("Blogas CRC atsakyme, ilgame cikle");
                appendText();
                //this.workerProgressBar.Value = 0;

            }
            

        }
        //==================================================
        //Čia pabaigus darbą vykdomas kodas
        //==================================================
        private void longMeasurementsThreadWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                appendText();
                appendText("Matavimų gija buvo nutraukta");
                appendText();
            }

            /*Kol kas nieko specifinio nereikia daryti*/
            //Net ir nutraukus kodą, šie veiksmai turi būti atlikti:
            this.preesToRunBigCycleButton.Enabled = true;
            this.workerProgressBar.Value = 0; //grąžinama išmatavus atgal į pradinę 0 reikšmę
            this.progressLabelText.Text = "0 %";
            this.cancelButton.Enabled = false;

        }
        /*ilgų matavimų threado pabaiga*/

        //Grafiko atnaujinimui funkcija, kad kodas trumpesnis būtų?
        private void updateGraphWithPoint(double Ni, double eV)
        {
            if (eV >= 0.0d) {
                resultsViewChart.Series["DATA"].Points.AddXY(eV, Ni);
            }
            else
            {
                appendText("Tamsinis rezultatas");
            }
        }

        private void changeGraphAxis(double min, double max)
        {
            //+/- 10% nuo verčių:
            double minX = min-0.1d;
            double maxX = 0.1d+max;
            this.resultsViewChart.ChartAreas[0].AxisX.Minimum = minX;
            this.resultsViewChart.ChartAreas[0].AxisX.Maximum = maxX;
            this.resultsViewChart.ChartAreas[0].AxisX2.Minimum = minX;
            this.resultsViewChart.ChartAreas[0].AxisX2.Maximum = maxX;
            this.resultsViewChart.ChartAreas[0].AxisX.Interval = 0.1;
            this.resultsViewChart.ChartAreas[0].AxisX2.Interval = 0.1;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            //===Bandymas stabdyti BackgroundWorkerį dėl kažkokių problemų:=====
            try
            {
                this.longMeasurementsThreadWorker.CancelAsync();
                changePicture(2);
            }
            catch (Exception ex)
            {
                appendText();
                appendText("Bandymas nutraukti matavimų giją nepavyko");
                changePicture(1);
                appendText(ex.ToString());
                appendText();
            }

        }

        private void removePointsFromGraph()
        {
            this.resultsViewChart.Series[0].Points.Clear();
        }



        private void setStepDirect05eVbox_CheckedChanged(object sender, EventArgs e)
        {
            if(setStepDirect05eVbox.Checked)
            {
                stepSize = (double)stepValueBox.Value;
            }
        }

        private void tryUseLPTButton_Click(object sender, EventArgs e)
        {
            //bandysim naudoti LPT portą (kurį?)
            try
            {
                bool c = tryUseLPTButton.Text.Contains("Off");
                if (tryUseLPTButton.Text.Contains("Off"))
                {
                    LPTLibraryCMDS.Output(0x0378, dataLPT);
                    appendText("LPT1 įrašyta ", dataLPT);
                    tryUseLPTButton.Text = "LPT On";
                    changePicture(0);
                }
                else if (tryUseLPTButton.Text.Contains("On"))
                {
                    LPTLibraryCMDS.Output(0x0378, 0);
                    appendText("LPT1 įrašyta ", dataLPT);
                    tryUseLPTButton.Text = "LPT Off";
                    changePicture(0);
                }
                else
                {
                    appendText(tryUseLPTButton.Text);
                    appendText(c);
                }
            }
            catch (Exception ex)
            {
                appendText();
                appendText("DLL problemos, LPT problemos");
                appendText(ex.ToString());
                changePicture(1);
                appendText();
            }
        }

        private void setLPT1_Click(object sender, EventArgs e)
        {
            int i = 1;
            if(setLPT1.Text == "0")
            {
                setLPT1.Text = "1";
                dataLPT = dataLPT + i;
                setLPT1.BackColor = Color.Green;
            }
            else if (setLPT1.Text == "1")
            {
                setLPT1.Text = "0";
                dataLPT = dataLPT - i;
                setLPT1.BackColor = SystemColors.Control;
            }
            appendText("Rašymui skaičius " + dataLPT.ToString());
        }

        private void setLPT2_Click(object sender, EventArgs e)
        {
            int i = 2;
            if (setLPT2.Text == "0")
            {
                setLPT2.Text = "1";
                dataLPT = dataLPT + i;
                setLPT2.BackColor = Color.Green;
            }
            else if (setLPT2.Text == "1")
            {
                setLPT2.Text = "0";
                dataLPT = dataLPT - i;
                setLPT2.BackColor = SystemColors.Control;
            }
            appendText("Rašymui skaičius " + dataLPT.ToString());
        }

        private void setLPT3_Click(object sender, EventArgs e)
        {
            int i = 4;
            if (setLPT3.Text == "0")
            {
                setLPT3.Text = "1";
                dataLPT = dataLPT + i;
                setLPT3.BackColor = Color.Green;
            }
            else if (setLPT3.Text == "1")
            {
                setLPT3.Text = "0";
                dataLPT = dataLPT - i;
                setLPT3.BackColor = SystemColors.Control;
            }
            appendText("Rašymui skaičius " + dataLPT.ToString());
        }

        private void setLPT4_Click(object sender, EventArgs e)
        {
            int i = 8;
            if (setLPT4.Text == "0")
            {
                setLPT4.Text = "1";
                dataLPT = dataLPT + i;
                setLPT4.BackColor = Color.Green;
            }
            else if (setLPT4.Text == "1")
            {
                setLPT4.Text = "0";
                dataLPT = dataLPT - i;
                setLPT4.BackColor = SystemColors.Control;
            }
            appendText("Rašymui skaičius " + dataLPT.ToString());
        }

        private void setLPT5_Click(object sender, EventArgs e)
        {
            int i = 16;
            if (setLPT5.Text == "0")
            {
                setLPT5.Text = "1";
                dataLPT = dataLPT + i;
                setLPT5.BackColor = Color.Green;
            }
            else if (setLPT5.Text == "1")
            {
                setLPT5.Text = "0";
                dataLPT = dataLPT - i;
                setLPT5.BackColor = SystemColors.Control;
            }
            appendText("Rašymui skaičius " + dataLPT.ToString());
        }

        private void setLPT6_Click(object sender, EventArgs e)
        {
            int i = 32;
            if (setLPT6.Text == "0")
            {
                setLPT6.Text = "1";
                dataLPT = dataLPT + i;
                setLPT6.BackColor = Color.Green;
            }
            else if (setLPT6.Text == "1")
            {
                setLPT6.Text = "0";
                dataLPT = dataLPT - i;
                setLPT6.BackColor = SystemColors.Control;
            }
            appendText("Rašymui skaičius " + dataLPT.ToString());
        }

        private void setLPT7_Click(object sender, EventArgs e)
        {
            int i = 64;
            if (setLPT7.Text == "0")
            {
                setLPT7.Text = "1";
                dataLPT = dataLPT + i;
                setLPT7.BackColor = Color.Green;
            }
            else if (setLPT7.Text == "1")
            {
                setLPT7.Text = "0";
                dataLPT = dataLPT - i;
                setLPT7.BackColor = SystemColors.Control;
            }
            appendText("Rašymui skaičius " + dataLPT.ToString());
        }

        private void setLPT8_Click(object sender, EventArgs e)
        {
            int i = 128;
            if (setLPT8.Text == "0")
            {
                setLPT8.Text = "1";
                dataLPT = dataLPT + i;
                setLPT8.BackColor = Color.Green;
            }
            else if (setLPT8.Text == "1")
            {
                setLPT8.Text = "0";
                dataLPT = dataLPT - i;
                setLPT8.BackColor = SystemColors.Control;
            }
            appendText("Rašymui skaičius " + dataLPT.ToString());
        }

        private void useLptBox_CheckedChanged(object sender, EventArgs e)
        {
            useLPT = !useLPT; //invertavimas
        }

        //====Autoįrašymas:
        private void AutoVoltageDirectlySet42Vset35(double currentVoltage, double dropVoltage, Answer atsakymas)
        {
            bool useA = true;
            if(uController_A.Checked)
            {
                useA = true;
            }
            else if (uController_B.Checked)
            {
                useA = false;
            }
            else
            {
                useA = true;
            }
            //===============================================
            //====Tiesiogiai įrašoma įtampa be automatinio nustatymo;
            //byte[] voltage = ExternalFunctions.get42Vset35array((double)voltage42Vset35_in_kiloVolts.Value);
            double voltageInKV = currentVoltage + dropVoltage/1000.0d;
            byte[] cmd = Commands.Write42Vset35IntoMemory(commandNumber, 2, voltageInKV, 0x70, useA);
            try
            {
                //bandoma rašyti į portą:
                //showData_sent(cmd, "Nusiųsta komanda");
                port.Write(cmd, 0, cmd.Length);
                Thread.Sleep(waitBetweenQuestionResponse);
                //bandoma gauti atsakymą:
                byte[] answerIgot = new byte[22];
                int i = port.Read(answerIgot, 0, answerIgot.Length);
                if (i > 0)
                {
                    //analizuojamas atsakymas:
                    atsakymas.returnAnswerFromMemoryWriteCMD(answerIgot);
                    if (atsakymas.AnswerCRCerrorCode == 0)
                    {
                        //atvaziduojama
                        //showAnswerWhetVoltagehasBeenSet();
                        port.DiscardInBuffer(); //panaikinami data, kad nesiakumuoliuotų?
                        port.DiscardOutBuffer();
                       

                    }
                    else if (atsakymas.AnswerCRCerrorCode == -1)
                    {
                        appendText();
                        appendText("blogas CRC atsakyme");
                        appendText("CRC turėjo būti", atsakymas.CRC_code_should_be);
                        appendText("CRC iš μContr", atsakymas.CRC_code_fromMicroController);
                        appendText(atsakymas.AnswerInBytes);
                        appendText(atsakymas.AnswerInHexString);
                        appendText(atsakymas.AnswerCRC_code);
                        appendText();
                    }
                }
                else
                {
                    appendText("Klaida rašant į atmintį F577, gautų baitų kiekis mažiau nei 1-nas");
                    port.DiscardInBuffer(); //panaikinami data, kad nesiakumuoliuotų?
                    port.DiscardOutBuffer();
                }


            }
            catch (Exception ex)
            {
                appendText();
                appendText("Klaida rašant į atmintį arba nuskaitant, F571", ex.Message);
                appendText(ex.ToString());
                appendText();

            }
           // updateCmdNumber();

        }

        private void useLEDorHalogenButton_Click(object sender, EventArgs e)
        {
            double currentPowerSupply = (double)currentPowerSupplyBox.Value;
            double voltagePowerSupply = (double)voltagePowerSupplyBox.Value;
            //nustatoma, ar naudojamas LED ar Halogeninė lemputė
            string text = useLEDorHalogenButton.Text;
            if(text.Contains("LED?"))
            {
                useLEDorHalogenButton.Text = "Halogenas?";
                useLEDorHalogen = false;
                //inicializacija driverio:
                if(powerSupply != null)
                {
                    appendText("Lyg ir rijungtas, veikia HMP4040?");
                    changePicture(0);
                }
                else
                {
                    try
                    {
                        //iniciliazijuojaas draiveris:
                        string address = aslrNumberBox.Text;
                        powerSupply = new hmp4000(address, true, false);
                        var idnQuery = new StringBuilder(1024);
                        powerSupply.IDQueryResponse(idnQuery);
                        //string txt = ("Success. *IDN? query: '{0}'", idnQuery);
                        appendText(idnQuery, "OK");
                        try
                        {
                            //RemoteMode Mix doesn't block the manual operation
                            powerSupply.RemoteMode(hmp4000Constants.RemoteMix);

                            //Channel 1 configuration
                            powerSupply.ConfigureChannel(hmp4000Constants.Ch1);
                            powerSupply.ConfigureVoltageValue(voltagePowerSupply); //3 Volts
                            powerSupply.ConfigureCurrentValue(currentPowerSupply); //0.2 Amps
                            powerSupply.ConfigureOutputStateChannelOnly(true); //only the channel 1 state changes, not the general output state

                            
                            //now change the general output state - all channels will be switched on synchronously
                            //driver.ConfigureGeneralOutputState(true);
                        }
                        catch (Exception ex)
                        {
                            appendText();
                            appendText(ex.ToString());
                            changePicture(1);
                            appendText();

                            
                        }
                    }
                    catch (Exception ex)
                    {
                        appendText();
                        appendText(ex.ToString());
                        appendText(aslrNumberBox.Text);
                        appendText();
                    }
                }
            }
            else if (text.Contains("Halogenas?"))
            {
                useLEDorHalogenButton.Text = "LED?";
                useLEDorHalogen = true;
                changePicture(0);
            }
        }

        private void justOnOffButton_Click(object sender, EventArgs e)
        {
            powerSupply.ConfigureGeneralOutputState(!powerSupplyOnOff);
            powerSupplyOnOff = !powerSupplyOnOff;
            
        }

        private void findRigolPowerSupply_Click(object sender, EventArgs e)
        {
            try
            {
                string[] names = ioMgr.FindRsrc("USB?*");
                foreach (string i in names)
                {
                    appendText(i);
                }
                session = ioMgr.Open(visaAddrressRigol.Text, AccessMode.NO_LOCK, 2000, "");
                instrument.IO = (IMessage)session;
                instrument.IO.Timeout = 3000;                       //in milliseconds  
                instrument.WriteString("*IDN?", true);
                string idnString = instrument.ReadString();
                appendText(idnString);
                //instrument.IO.Close();
            }
            catch(Exception ex)
            {
                appendText();
                appendText(ex.ToString());
                appendText();
            }
        }

        private void n_vs_currentButton_Click(object sender, EventArgs e)
        {
            decimal rigol_start_current = rigol_startCurrentBox.Value;
            decimal rigol_end_current = rigol_endCurrentBox.Value;
            string rigol_channel = channelsListBox.SelectedText;
            int rigol_wait_time = (int)rigol_waitBetweenLED_box.Value;
            decimal rigol_step_current = rigol_currentStepBox.Value;
            decimal rigol_limit_current = rigol_limitCurrentBox.Value;
            decimal rigol_voltage = rigol_maxVoltageBox.Value;
            MeasurementParameters mPar = new MeasurementParameters();
            mPar.Tq = (double)Tq_box.Value;
            mPar.Tz = (double)Tz_box.Value;
            mPar.Cth = (double)Cth_box.Value;
            mPar.Vq = (double)Vq_box.Value;
            mPar.Ts = (double)Ts_box.Value;
            //μValdikliui valdyti
            //Supildoma kita informacija dėl Rigol'o:
            mPar.rigol_channel = rigol_channel;
            mPar.rigol_end_current = rigol_end_current;
            mPar.rigol_limit_current = rigol_limit_current;
            mPar.rigol_sleepTime = rigol_wait_time;
            mPar.rigol_start_current = rigol_start_current;
            mPar.rigol_stepCurrent = rigol_step_current;
            mPar.rigol_maximum_voltage = rigol_voltage;
            //Paleidžiamas thread'as:
            this.currentLEDthread.RunWorkerAsync(mPar);
            Console.WriteLine("Thread is started?");
            changePicture(0);
            //Išvalyti grafiką ir tekstą?
            //grafiką tiktai:
            removePointsFromGraph();//Kad nesimaišytų su ktais matavimais;
            this.n_vs_currentButton.Enabled = false;
            this.cancelLEDThreadButton.Enabled = true;
            //aktyvinam outputo lauką, idant kursorius tikrai ten būtų:
            //dar reikia, jog ir pačiame gale būtų:
            this.outputBox.Focus();
            this.outputBox.SelectionStart=this.outputBox.Text.Length;


        }

        private void currentLEDthread_DoWork(object sender, DoWorkEventArgs e)
        {
            //čia atliekami matavimai
            MeasurementParameters mP = e.Argument as MeasurementParameters;
            Answer answer = new Answer(); //atsakymų analizei;
            //ciklas matavimų:
            double rigol_start_current = (double)mP.rigol_start_current;
            double rigol_end_current = (double)mP.rigol_end_current;
            double rigol_step = (double)mP.rigol_stepCurrent;
            int rigol_sleep_time = mP.rigol_sleepTime;
            string rigol_channel = mP.rigol_channel;
            double rigol_limit_current = (double)mP.rigol_limit_current;
            string rigol_voltage = Convert.ToString(mP.rigol_maximum_voltage, CultureInfo.InvariantCulture);
            //komanda mikrovaldikliui, matavimams:
            uint CmdNmb = 0;
            byte[] cmd = Commands.CmdAskStartCounting(CmdNmb, mP.Ts, mP.Tz, mP.Tq, (decimal)mP.Cth, mP.Vq);
            for (double i = rigol_start_current; i < rigol_end_current; i = i + rigol_step)
            {
                //matavimų ciklas:
                if (!currentLEDthread.CancellationPending)
                {
                    try
                    {
                        //Įjungiams LED'as:
                        instrument.WriteString(":INST "+rigol_channel, true);
                        instrument.WriteString(":CURR "+Convert.ToString((i/1000.0d), CultureInfo.InvariantCulture));
                        instrument.WriteString(":CURR:PROT "+Convert.ToString((rigol_limit_current/1000.0d), CultureInfo.InvariantCulture), true);
                        instrument.WriteString(":CURR:PROT:STAT ON", true);
                        instrument.WriteString(":VOLT " + rigol_voltage, true);
                        instrument.WriteString("OUTP "+rigol_channel+",ON");
                        port.Write(cmd, 0, cmd.Length); //matuojama
                        Thread.Sleep((int)(mP.Ts*1000.0d+100.0d)); //laukiama atsakymo tiek, kiek Ts, ir tiek pat šviečiama su LED'u.
                        //nuskaitomas atsakymas iš mikrovaldiklio:
                        byte[] answ = new byte[26];
                        int status = port.Read(answ, 0, answ.Length);
                        instrument.WriteString("OUTP " + rigol_channel + ",OFF");//Išjungiamas Rigol'o outputas;
                        if (status > 0)
                        {
                            answer.returnCounterValue(answ);
                            if (answer.AnswerCRCerrorCode == 0)
                            {
                                mP.CurrentLambda = "ZERO"; //dabartinė λ
                                mP.EnergyValue = 0;//dabartinė energija;
                                mP.Ni = answer.Ni; //Grąžinama reikiama vertė;
                                mP.LambdaFromCs = "UNUSED";                //atnaujina imformacija:
                                mP.ExceptionCode = 0;
                                mP.AnswerCRCCode = 0; //Būtina atnaujinti laukus, nes gali būti likę iš seniau;
                                mP.CurrentMeasurementTime = mP.Ts;
                                mP.Actual_current = i;
                                int percentage = 100;
                                currentLEDthread.ReportProgress(percentage, mP);
                                CmdNmb++;
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        mP.ExceptionMessage = ex.ToString();
                        mP.ExceptionCode = 1;
                        currentLEDthread.ReportProgress(100, mP);
                    }
                    Thread.Sleep(rigol_sleep_time*1000);//laukiama tarp švietimų nurodytą laiką;
                }
            }

        }

        private void currentLEDthread_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //atnaujinti informaciją TODO;
            MeasurementParameters mP = e.UserState as MeasurementParameters;
            if (mP.ExceptionCode != 1)
            {
                string curr_gass_c = gassIndicatorLabel.Text.Replace("s.v.", string.Empty);
                double Ni = mP.Ni;
                double actualCurrent = mP.Actual_current;
                this.outputBox.AppendText(ForInfoViewer.AddString(actualCurrent, Ni, curr_gass_c, "d."));
                updateGraphWithPoint(Ni, actualCurrent);
            }
            else
            {
                appendText();
                appendText(mP.ExceptionMessage);
                appendText();
                changePicture(1);
            }
        }

        private void currentLEDthread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //pabaigti matavimus TODO
            this.n_vs_currentButton.Enabled = true;
            this.cancelLEDThreadButton.Enabled = false;
        }

        private void cancelLEDThreadButton_Click(object sender, EventArgs e)
        {
            //===Bandymas stabdyti BackgroundWorkerį dėl kažkokių problemų:=====
            try
            {
                this.currentLEDthread.CancelAsync();
                changePicture(2);
            }
            catch (Exception ex)
            {
                appendText();
                appendText("Bandymas nutraukti LEDcurrent matavimų giją  nepavyko");
                changePicture(1);
                appendText(ex.ToString());
                appendText();
            }
        }

        private void connectButtonArduinoBox_Click(object sender, EventArgs e)
        {
            /*Prisijungimas prie arduino COM*/
            changePicture(0);
            //responseWaitTime = (int)maximumWaitTimeBox.Value;
            //waitBetweenQuestionResponse = (int)waitTimeBetweenCommandAndResponseBox.Value;
            /*same button for connect and disconnect*/
            if (connectButtonArduinoBox.Text == "Atsijungti")
            {
                arduinoPort.Close();
                arduinoPort.DataReceived -= ArduinoPort_DataReceived;
                connectButtonArduinoBox.Text = "Prisijungti";
                continiuosOutputfromArduino.Clear();

            }
            else if (connectButtonArduinoBox.Text == "Prisijungti")
            {
                if (!string.IsNullOrEmpty(comForArduinoPortBox.Text))
                {
                    /*Connect function:*/
                    arduinoPort = new SerialPort(comForArduinoPortBox.Text);
                    if (handshakeArduinoBox.Text == "None")
                    {
                        arduinoPort.Handshake = Handshake.None;
                    }
                    if (parityArduinoBox.Text == "None")
                    {
                        arduinoPort.Parity = Parity.None;
                    }
                    string stbits = stopBitsArduino.Text;
                    switch (stbits)
                    {
                        case ("1"):
                            {
                                arduinoPort.StopBits = StopBits.One;
                                errorAndInfoBox.AppendText("Case 1" + newLine);
                                break;
                            }
                        case ("2"):
                            {
                                arduinoPort.StopBits = StopBits.Two;
                                errorAndInfoBox.AppendText("Case 2" + newLine);
                                break;
                            }
                        case ("1.5"):
                            {
                                arduinoPort.StopBits = StopBits.OnePointFive;
                                errorAndInfoBox.AppendText("Case 1.5" + newLine);
                                break;
                            }
                        default:
                            {
                                arduinoPort.StopBits = StopBits.One;
                                errorAndInfoBox.AppendText("Default case value - StopBits.One" + Environment.NewLine);
                                break;
                            }


                    } //end switch;
                      // port.WriteTimeout = responseWaitTime;
                      // port.ReadTimeout = responseWaitTime;
                      /*Baudrate*/
                    arduinoPort.BaudRate = Convert.ToInt32(baudRateArduinoBox.Text, CultureInfo.InvariantCulture);

                    try
                    {
                        arduinoPort.DataReceived += ArduinoPort_DataReceived;
                        arduinoPort.Open();
                        continiuosOutputfromArduino.Clear();
                        string date = ForInfoViewer.GetLongDate();
                        gassFileName.Text = "dujos" + date + ".txt";
                        //?????


                    }
                    catch (Exception ex)
                    {
                        errorAndInfoBox.AppendText("Exception ex has been trown" + Environment.NewLine + ex.GetType().ToString() + newLine);
                    }
                    if (arduinoPort.IsOpen)
                    {
                        connectButtonArduinoBox.Text = "Atsijungti";
                    }
                }
                else
                {
                    anotherTexBoxErrorsOnly.AppendText("Empty Arduino COM port!");
                    ShowTab(2);
                }
            }
        }

        private void ArduinoPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                //?
                string curTime = DateTime.Now.ToString("HH:mm:ss");
                    //continiuosOutputfromArduino.AppendText(e.ToString() + newLine);
                    string data = arduinoPort.ReadExisting().Replace("\r\n", "");
                arduinoPort.DiscardInBuffer(); //turėtų pagelbėti nusikratyti dvigubų verčių
                int mod = (int)saveGassValueeverySecondBox.Value;
                int reminder = gass_counter % mod;
                if (reminder == 0)
                {
                    continiuosOutputfromArduino.AppendText(gass_counter.ToString() + "  " + curTime.Replace(":","o") + "   " + data + newLine);
                    liveArduinoChart.Series["Dujos"].Points.AddXY(gass_counter, Convert.ToDouble(data));
                }
                this.gassIndicatorLabel.Text = data.Replace("\r\n", "") + " s.v.";
                //*******Dujų kontrolės kodas:
                int valueToKeep = (int)tryKeepThisGassValueBox.Value;
                int deltaGass = (int)deltaGassValueBox.Value;
                int gass_shutter_time = (int)gassShutterTimeBox.Value;
                int checkGassTime = (int)watchgassEverySecondBox.Value;
                if (tryControlGassFlowBox.Checked)
                {
                    if ((gass_counter % checkGassTime)==0) {
                        OnOffGassShutter(valueToKeep, deltaGass, gass_shutter_time, checkGassTime, Convert.ToInt32(data));
                    }
                }
               
                //*******
                gass_counter = gass_counter + 1;
                //
                if(gass_counter >= (int)gass_counter_limits_entry.Value)
                {
                    gass_counter = 0;
                    if(gassAutoSaveBox.Checked)
                    {
                        SaveGassFile(); 
                    }
                    continiuosOutputfromArduino.Clear();
                    liveArduinoChart.Series["Dujos"].Points.Clear();
                }
                
            }
            catch (Exception ex)
            {
                appendText();
                appendText(ex.ToString());
                /*Tai aotomatiškai parodys klaidų tab'ą*/
                appendText();
            }
        }

        private void OnOffGassShutter(int valueToKeep, int deltaGass, int gass_shutter_time, int checkGassTime, int data)
        {
            try
            {
                //instrument.WriteString("OUTP "+rigol_channel+",ON");
                if((valueToKeep-data) >= deltaGass)
                {
                    if(!rigol_started)
                    {
                        //TODO fix 
                        instrument.WriteString("OUTP " + "CH2" + ",ON");
                        counter_rigol_started = gass_counter;//išsaugome laiką, kai prasidėjo skaičiavimas 
                        rigol_started = true;
                        //*****Čia thread'o stabdymas*****///
                        /*Thread.Sleep((int)gassShutterSleepTimeBox.Value);
                        instrument.WriteString("OUTP " + "CH2" + ",OFF");
                        rigol_started = false;*/
                        //****Jei to nereiki, IKI čia galima užkomentuoti

                    }
                    if(rigol_started && ((gass_counter - counter_rigol_started)>=gass_shutter_time))
                    {
                        //uždarome sklendę dujų:
                        instrument.WriteString("OUTP " + "CH2" + ",OFF");
                        rigol_started = false;
                    }
                    //turi veikti
                }
                if (((valueToKeep - data) < deltaGass) && rigol_started)
                {
                    //uždarome sklendę dujų:
                    instrument.WriteString("OUTP " + "CH2" + ",OFF");
                    rigol_started = false;
                }


            }
            catch (Exception ex)
            {

                appendText();
                appendText(ex.ToString());
                appendText();
            }
        }

        private void ShowTab(int obj)
        {
            errorAndInfoTabs.SelectTab(obj);
        }
        private void ShowTab(string obj)
        {
            errorAndInfoTabs.SelectTab(obj);
        }
        private void ShowTab(TabPage obj)
        {
            errorAndInfoTabs.SelectTab(obj);
        }

        private void saveGassInfoButton_Click(object sender, EventArgs e)
        {
            SaveGassFile();
        }

        public void SaveGassFile()
        {
            //==================================
            //===Išsaugo matavimus į failą:
            try
            {
                string allGassData = continiuosOutputfromArduino.Text;
                string fileName = gassFileName.Text;
                StreamWriter writer = new StreamWriter(fileName);
                writer.Write(allGassData);
                writer.Close();
                string datanow = ForInfoViewer.GetLongDate();
                gassFileName.Text = "dujos" + datanow + ".txt";
                changePicture(0);
            }
            catch (Exception ex)
            {
                appendText();
                appendText(ex.ToString());
                appendText();
                changePicture(1);
            }
        }

        private void tryControlGassFlowBox_CheckedChanged(object sender, EventArgs e)
        {
            //nustato Rigolo antrojo kanalo parametrus - 12 V, 500 mA;
            try
            {
                if (tryControlGassFlowBox.Checked)
                {
                    instrument.WriteString(":INST " + "CH2", true);
                    instrument.WriteString(":CURR " + "0.6");
                    instrument.WriteString(":CURR:PROT " + "0.7", true);
                    instrument.WriteString(":CURR:PROT:STAT ON", true);
                    instrument.WriteString(":VOLT " + "12.0", true);
                    instrument.WriteString("OUTP " + "CH2" + ",OFF"); //tikrai išjungiame kanalą
                }
                else if (!tryControlGassFlowBox.Checked)
                {
                    instrument.WriteString(":VOLT " + "3.0", true);
                    instrument.WriteString("OUTP " + "CH2" + ",OFF");
                }
            }
            catch (Exception ex)
            {
                appendText();
                appendText(ex.ToString());
                appendText();
            }

        }

        private void lightEnergyBox_ValueChanged(object sender, EventArgs e)
        {
            double energy = (double)lightEnergyBox.Value;
            double lambda = Math.Round((1239.75d / energy), 1);
            lightLambdaBox.Value = (decimal)lambda;
        }

        private void minusEnergyButton_Click(object sender, EventArgs e)
        {
            lightEnergyBox.Value = lightEnergyBox.Value - 0.1M;
        }

        private void plusEnergyButton_Click(object sender, EventArgs e)
        {
            lightEnergyBox.Value = lightEnergyBox.Value + 0.1M;
        }

        private void setLambdaOnOriel_Click(object sender, EventArgs e)
        {
            //nustato monochromatoriuje bangos ilgį, nm;
            changePicture(0);
            requiredWavelength = (double)lightLambdaBox.Value;
            //This function sets a required wavelength:
            try
            {

                bool status = cs.sendCommand("GOWAVE " + Convert.ToString(requiredWavelength, CultureInfo.InvariantCulture) + terminator);
                if (status)
                {
                    Thread.Sleep(500); //Slow device.
                    string lambda = cs.getStringResponseFromCommand("WAVE?\r\n");
                    anotherOrielResponseLabel.Text = "Atsakas " + lambda;
                }
                else
                {
                    anotherOrielResponseLabel.Text = "Something wrong";
                    changePicture(1);
                }
            }
            catch (Exception ex)
            {
                errorAndInfoBox.AppendText("==========================" + newLine);
                errorAndInfoBox.AppendText("bangos ilgio nustatymo problema" + newLine);
                errorAndInfoBox.AppendText(ex.ToString());
                appendText();
                changePicture(1);
            }
        }

        private void stopAnotherThreadButton_Click(object sender, EventArgs e)
        {
            //===Bandymas stabdyti BackgroundWorkerį dėl kažkokių problemų:=====
            try
            {
                this.slightlyDifferentWorkerThread.CancelAsync();
                changePicture(2);
            }
            catch (Exception ex)
            {
                appendText();
                appendText("Bandymas nutraukti matavimų giją nepavyko");
                changePicture(1);
                appendText(ex.ToString());
                appendText();
            }
        }

        private void runAnotherThreadButton_Click(object sender, EventArgs e)
        {
            changePicture(0);
            MeasurementParameters mPar = new MeasurementParameters();
            mPar.Tq = (double)Tq_box.Value;
            mPar.Tz = (double)Tz_box.Value;
            mPar.Cth = (double)Cth_box.Value;
            mPar.Vq = (double)Vq_box.Value;
            mPar.Ts = (double)Ts_box.Value;
            mPar.Step = (double)stepBoxForAnotherThread.Value;
            mPar.StepDirection = fromMinimumCheckBox.Checked;
            appendText();
            appendText(mPar.Tq, mPar.Tz, mPar.Cth, mPar.Vq, mPar.Ts);
            appendText("Tq, Tz, Cth, Vq, Ts");
            appendText();
            mPar.StartingEnergy = (double)startEnergyBox.Value;
            mPar.EndingEnergy = (double)stopEnergyBox.Value;
            mPar.Counts = (double)measureTimesBox.Value;
            //paleidžiamas thread'as
            slightlyDifferentWorkerThread.RunWorkerAsync(mPar);
            runAnotherThreadButton.Enabled = false;
            stopAnotherThreadButton.Enabled = true;

        }

        private void slightlyDifferentWorkerThread_DoWork(object sender, DoWorkEventArgs e)
        {
            MeasurementParameters mP = e.Argument as MeasurementParameters;
            Answer longAts = new Answer();
            int x = 0;
            //skaičiavimo komanda:
            uint CmdNmb = 0;
            byte[] cmd = Commands.CmdAskStartCounting(CmdNmb, mP.Ts, mP.Tz, mP.Tq, (decimal)mP.Cth, mP.Vq);
            byte[] answ = new byte[26];
            //Tamsa:
            try
            {
                //==================================
                if (!slightlyDifferentWorkerThread.CancellationPending)
                {
                    cs.sendCommand("SHUTTER C" + terminator);//kad tikrai būtų tamsa?
                    Thread.Sleep(1000);
                    port.Write(cmd, 0, cmd.Length);
                    Thread.Sleep((int)(mP.Ts * 1000 * 1.1));//laukiama  kol išmatuos 10% ilgiau laiko;

                    int status = port.Read(answ, 0, answ.Length);
                    if ((status > 0))
                    {
                        //kažką pavyko nuskaityti, apdorojam
                        longAts.returnCounterValue(answ);
                        if (longAts.AnswerCRCerrorCode == 0)
                        {
                            mP.CurrentLambda = "Tamsa"; //Tamsa pas mus visada žymima 0-liu;
                            mP.EnergyValue = -1.0d;
                            mP.Ni = longAts.Ni; //Grąžinama reikiama vertė;
                                                //atnaujina imformacija:
                            mP.ExceptionCode = 0;
                            mP.AnswerCRCCode = 0; //Būtina atnaujinti laukus, nes gali būti likę iš seniau;
                            mP.CurrentMeasurementTime = mP.Ts;
                            int percentage = (int)((CmdNmb) / x * 100);
                            slightlyDifferentWorkerThread.ReportProgress(percentage, mP);
                            CmdNmb++;
                        }
                        else
                        {
                            //grąžinama info apie CRC kodų problemas:
                            mP.AnswerCRCCode = longAts.AnswerCRCerrorCode;
                            mP.ExceptionCode = 1; // kad nebūtų vaizduojama atsakymai
                            slightlyDifferentWorkerThread.ReportProgress(100, mP);
                        }
                    }
                    //=============ENDofTamsa===========
                }
            }
            catch (Exception ex)
            {
                mP.AnswerCRCCode = 0;
                mP.ExceptionCode = 1;
                mP.ExceptionMessage = "Tamsos blokas 0" + ex.ToString();
                slightlyDifferentWorkerThread.ReportProgress(100, mP);

            }
            //====Matavimai cikle:
            //Vienas ciklas siganlui:
            try { 
            //=====
            double Count = mP.Counts;
            double energy = mP.StartingEnergy;
                for (double i = 0; i <= Count; i++)
                {
                    while ((energy <= mP.EndingEnergy) && (!slightlyDifferentWorkerThread.CancellationPending))
                    {
                        //====Matavimų ciklas:
                        string lmbd = Commands.retLambdaStringFromEv(energy);
                        bool statusas = cs.sendCommand("GOWAVE " + lmbd + terminator); //nustatomas reikiamas bangos ilgis;
                        Thread.Sleep(1000); //kad spėtų nustatyti bangos ilgį;
                        string lambda = cs.getStringResponseFromCommand("WAVE?\r\n");
                        if (statusas && !slightlyDifferentWorkerThread.CancellationPending)
                        {

                            cs.sendCommand("SHUTTER O" + terminator); //atidaroma
                            Thread.Sleep(1000);
                            port.Write(cmd, 0, cmd.Length); //matuojama
                            Thread.Sleep((int)(mP.Ts * 1000 * 1.1));//laukiama  kol išmatuos 10% ilgiau laiko;
                            answ = new byte[26];
                            int status = port.Read(answ, 0, answ.Length);
                            if (status > 0)
                            {
                                //kažką pavyko nuskaityti, apdorojam
                                longAts.returnCounterValue(answ);
                                if (longAts.AnswerCRCerrorCode == 0)
                                {
                                    //mP.CurrentLambda = Commands.returnCurrentLambda(en); //dabartinė λ
                                    mP.EnergyValue = energy;//dabartinė energija;
                                    mP.Ni = longAts.Ni; //Grąžinama reikiama vertė;
                                    mP.CurrentLambda = Commands.returnCurrentLambda(energy); //atnaujina imformacija:
                                    mP.LambdaFromCs = lambda;
                                    mP.ExceptionCode = 0;
                                    mP.AnswerCRCCode = 0; //Būtina atnaujinti laukus, nes gali būti likę iš seniau;
                                    mP.CurrentMeasurementTime = mP.Ts;
                                    int percentage = 100;
                                    slightlyDifferentWorkerThread.ReportProgress(percentage, mP);
                                    CmdNmb++;
                                }
                                else
                                {
                                    //grąžinama info apie CRC kodų problemas:
                                    mP.AnswerCRCCode = longAts.AnswerCRCerrorCode;
                                    mP.ExceptionCode = 1; // kad nebūtų vaizduojama atsakymai
                                    slightlyDifferentWorkerThread.ReportProgress(100, mP);
                                }
                            }
                            cs.sendCommand("SHUTTER C" + terminator); //uždaroma;
                            Thread.Sleep(2000);
                        }

                        else
                        {
                            ConsoleWriter.WriteOutput("Problemos su cs bangos ilgio nustatymu");
                        }
                        //====Matavmų ciklo pabaiga====
                        energy = energy + mP.Step;
                        //====END of matavimai:
                    }
                }
            }
            catch (Exception ex)
            {
                mP.AnswerCRCCode = 0;
                mP.ExceptionCode = 1;
                mP.ExceptionMessage = "Šviesos blokas 1" + ex.ToString();
                slightlyDifferentWorkerThread.ReportProgress(100, mP);
            }
            //Tamsa paskutinis matavimas:
            //Tamsa:
            try
            {
                //==================================
                if (!slightlyDifferentWorkerThread.CancellationPending)
                {
                    cs.sendCommand("SHUTTER C" + terminator);//kad tikrai būtų tamsa?
                    Thread.Sleep(1000);
                    port.Write(cmd, 0, cmd.Length);
                    Thread.Sleep((int)(mP.Ts * 1000 * 1.1));//laukiama  kol išmatuos 10% ilgiau laiko;

                    int status = port.Read(answ, 0, answ.Length);
                    if ((status > 0))
                    {
                        //kažką pavyko nuskaityti, apdorojam
                        longAts.returnCounterValue(answ);
                        if (longAts.AnswerCRCerrorCode == 0)
                        {
                            mP.CurrentLambda = "Tamsa"; //Tamsa pas mus visada žymima 0-liu;
                            mP.EnergyValue = -1.0d;
                            mP.Ni = longAts.Ni; //Grąžinama reikiama vertė;
                                                //atnaujina imformacija:
                            mP.ExceptionCode = 0;
                            mP.AnswerCRCCode = 0; //Būtina atnaujinti laukus, nes gali būti likę iš seniau;
                            mP.CurrentMeasurementTime = mP.Ts;
                            int percentage = (int)((CmdNmb) / x * 100);
                            slightlyDifferentWorkerThread.ReportProgress(percentage, mP);
                            CmdNmb++;
                        }
                        else
                        {
                            //grąžinama info apie CRC kodų problemas:
                            mP.AnswerCRCCode = longAts.AnswerCRCerrorCode;
                            mP.ExceptionCode = 1; // kad nebūtų vaizduojama atsakymai
                            slightlyDifferentWorkerThread.ReportProgress(100, mP);
                        }
                    }
                    //=============ENDofTamsa===========
                }
            }
            catch (Exception ex)
            {
                mP.AnswerCRCCode = 0;
                mP.ExceptionCode = 1;
                mP.ExceptionMessage = "Tamsos blokas 2" +ex.ToString();
                slightlyDifferentWorkerThread.ReportProgress(100, mP);

            }

        }
      
        private void slightlyDifferentWorkerThread_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            MeasurementParameters mP = e.UserState as MeasurementParameters;
            int percents = e.ProgressPercentage;
            if (mP.ExceptionCode == 0)
            {
                //Geras kodas
                updateGraphWithPoint(mP.Ni, mP.EnergyValue);//grafikas atnaujintas
                //Normuojam į minutę:
                string NiPerMinute = ExternalFunctions.getNiPerMinute(mP.Ni, mP.CurrentMeasurementTime);
                string SqrtNi = ExternalFunctions.SqrtNiPerMinute(mP.Ni, mP.CurrentMeasurementTime);
                double CorrNi = LightCorrections.GetNiDividedByLight(mP.Ni, mP.EnergyValue, mP.CurrentMeasurementTime);
                //dujų skaitiklio muskaitymas:
                string gass_value = gassIndicatorLabel.Text.Replace("s.v.", "");
                string curVoltage = voltage42Vset35_in_kiloVolts.Value.ToString();
                if (mP.EnergyValue <= 0.0d)
                {
                    this.outputBox.AppendText(ForInfoViewer.AddString(MatavimoNr, mP.CurrentLambda, mP.Ni, NiPerMinute, SqrtNi, curVoltage, gass_value, DateTime.Now.ToString("HH:mm:ss")));
                }
                else
                {
                    this.outputBox.AppendText(ForInfoViewer.AddString(MatavimoNr, Math.Round(mP.EnergyValue, 4), mP.Ni, NiPerMinute, SqrtNi, curVoltage, gass_value, DateTime.Now.ToString("HH:mm:ss")));
                }
                MatavimoNr++;//didinamas matavimo nr vienetu ir atvaziduojama infobloke;
                //tnaujinamas atsakas iš CS:
                this.anotherOrielResponseLabel.Text = mP.LambdaFromCs;
                //atnaujinamas progressbar'as:

                try
                {
                    this.workerProgressBar.Value = percents;
                    this.progressLabelText.Text = percents.ToString() + " %";
                }
                catch (Exception ex)
                {
                    //Do nothing.
                    //Do something:
                    this.workerProgressBar.Value = 100;//Vistiek virš suskaičiuoto progreso;
                    this.progressLabelText.Text = percents.ToString() + "%!";
                    this.anotherTexBoxErrorsOnly.AppendText("Vėl daugiau nei 100" + newLine + ex.Message.ToString() + newLine);
                }

            }
            else if (mP.ExceptionCode == 1 && mP.AnswerCRCCode == 0)
            {
                //klaidų apdorojimas
                appendText();
                appendText("PROBLEMOS su ilgu cikliniu matavimu");
                changePicture(1);
                appendText(mP.ExceptionMessage);
                appendText();
                //Jei klaida, tai gal buvo Exception, tai RUN reikia įjungti:
                this.runAnotherThreadButton.Enabled = true;
                this.workerProgressBar.Value = 0;
                this.progressLabelText.Text = "0 %";
            }
            else if (mP.ExceptionCode == 1 && mP.AnswerCRCCode == 1)
            {
                //klaidų apdorojimas, čia CRC bloga šaka
                appendText();
                appendText("Blogas CRC atsakyme, ilgame cikle");
                appendText();
                //this.workerProgressBar.Value = 0;

            }
        }


        private void slightlyDifferentWorkerThread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                appendText();
                appendText("Matavimų gija buvo nutraukta");
                appendText();
            }

            /*Kol kas nieko specifinio nereikia daryti*/
            //Net ir nutraukus kodą, šie veiksmai turi būti atlikti:
            this.runAnotherThreadButton.Enabled = true;
            this.workerProgressBar.Value = 0; //grąžinama išmatavus atgal į pradinę 0 reikšmę
            this.progressLabelText.Text = "0 %";
            this.stopAnotherThreadButton.Enabled = false;

        }

        private void label43_Click(object sender, EventArgs e)
        {
            continiuosOutputfromArduino.Clear();
            liveArduinoChart.Series["Dujos"].Points.Clear();
            gass_counter = 0;
        }
    }
}

