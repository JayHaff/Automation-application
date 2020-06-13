using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;
using System.ComponentModel;
using WECOControlWrapper;
using System.Collections.ObjectModel;

namespace diode_Test_Beta
{
    /// <summary>
    /// Jay Hafiani 6/2018
    /// 
    /// This program was produce to automate a diode test for KV meters
    /// </summary>
    public partial class MainWindow : Window

    {

        public ObservableCollection<string> Voltages { get; set; }
        public ObservableCollection<string> Voltages2 { get; set; }
        MeasureVoltage myDmm;
        bool powerOn = false;
        bool initialize = false;
        bool pass;
        bool fail;
        bool fail240;
        bool coverOpen;
        bool power480;
        bool meterConnected;
        string form;
        string base_;
        double dcVoltage1;
        double dcVoltage2;


        PowerUp power;

        StringHelper _stringHelper;
        List<WecoInputs> result1;
        List<RelaySWInfo> results;
        Settings secWindow;



        public MainWindow()
        {

            InitializeComponent();

            _stringHelper = new StringHelper();

            secWindow = new Settings();
            secWindow.Close();
            SerialNumber.IsEnabled = true;
            Voltages = new ObservableCollection<string> { };
            Voltages2 = new ObservableCollection<string> { };
            DataContext = this;

           
        }

        void Launch()
        {
           
            pass = false;
            fail = false;
            
        
             myDmm = new MeasureVoltage();

            dcVoltage1 = Math.Abs(myDmm.Measure());

            if (dcVoltage1 == 0.00000000)
            {
                
                meterConnected = false;
                SerialNumber.Text = "";
                
                MessageBox.Show("Connect Meter USB");
         
            }
            
            else
            {
                
                meterConnected = true;
                BackgroundWorker bg = new BackgroundWorker();
                bg.DoWork += Bg_DoWork;
                bg.RunWorkerCompleted += Bg_RunWorkerCompleted;
                bg.RunWorkerAsync();

            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {


            Settings secWindow = new Settings();

            if (secWindow.ShowActivated == true)
            {
                SettingsButton.IsEnabled = false;

            }
            secWindow.Show();
            Close();
        }


        public bool TurnOffPower(PowerUp power)
        {


            bool powerDown = power.deEnergize_Powerup();
            if (powerOn == false)
            {
                return true;
            }



            if (powerDown == false)
            {
                Application.Current.Dispatcher.Invoke(() => MessageBox.Show("POWER DOWN FAIL: " + power.fail));
                return false;
            }


            return true;
        }

        public bool PowerWeco(PowerUp power, ushort voltage)
        {



            if (power.Energize_Powerup(voltage))
            {

                return true;
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() => MessageBox.Show("POWER UP FAILED: " + power.fail));
                return false;
            }



        }

       

        bool Measure1(MeasureVoltage dmm)
        {

            double reading;
            int i;
            dcVoltage1 = 0.0;
            for (i = 1; i <= 5; i++)
            {
                reading = Math.Abs(dmm.Measure());
                

                dcVoltage1 = (dcVoltage1 + reading);

                Application.Current.Dispatcher.Invoke(() => Voltages.Add(Convert.ToString(reading + " Volts")));
                if (reading == 0.00)
                {
                    Application.Current.Dispatcher.Invoke(() => MessageBox.Show("Connect Meter USB"));
                    TurnOffPower(power);
                    meterConnected = false;
                    return false;


                }
                if (reading < 300 || reading > 340)
                {
                    dcVoltage1 = reading;
                    
                    return false;

                }
      

            }
            dcVoltage1 = dcVoltage1 /(i-1);
            


            return true;
        }

        bool Measure2(MeasureVoltage dmm)
        {
            dcVoltage2 = 0.0;
            double reading;
            int i;

            for (i = 1; i <= 5; i++)
            {
                reading = Math.Abs(dmm.Measure());

                dcVoltage2 = (dcVoltage2 + reading);
                Application.Current.Dispatcher.Invoke(() => Voltages2.Add(Convert.ToString(reading + " Volts")));
                if (reading == 0.00)
                {

                    Application.Current.Dispatcher.Invoke(() => MessageBox.Show("Connect Meter USB"));
                    TurnOffPower(power);
                    meterConnected = false;
                    return false;
                }
                if (reading < 340 || reading > 375)
                {
                    dcVoltage2 = reading;
                    return false;

                }
         

            }
            dcVoltage2 = dcVoltage2 / (i-1);
            return true;
        }



        private void SerialNumber_TextChanged(object sender, TextChangedEventArgs e)

        {
            if (SerialNumber.Text.Length >= 26)
            {
                ID.Text = "";
                Fail.Text = "";
                Pass.Text = "";
                Test240.Text = "";
                Test480.Text = "";
                fail = false;
                pass = false;
                fail240 = false;
                coverOpen = false;
                dcVoltage1 = double.NaN;
                dcVoltage2 = double.NaN;

                Application.Current.Dispatcher.Invoke(() => Voltages.Clear());
                Application.Current.Dispatcher.Invoke(() => Voltages2.Clear());

               

                string trim = SerialNumber.Text.Trim();

                if (_stringHelper.NumberComplete(trim) == true)
                {

                    string baseNumber = _stringHelper.RemoveLaggingChars(SerialNumber.Text);
                    
                    


                    result1 = DatabaseHelper.SetForm_Base(baseNumber);

                    foreach (var x in result1)
                    {
                        form = x.form_no;
                        base_ = x.base_;
                    }


                    if (form != null && base_ != null)
                    {


                        Launch();

                    }

                    else
                    {


                        MessageBox.Show("INVAILD BASE NUMBER");

                        SerialNumber.Text = "";
                        form = null;
                        base_ = null;


                    }
                }
                else if (trim.Length >= 28)
                {


                    MessageBox.Show("INVAILD BASE NUMBER");

                    SerialNumber.Text = "";
                }


            }
        }


        private void Bg_DoWork(object sender, DoWorkEventArgs e)
        {
            string routingBoard = Properties.Settings.Default.board.ToString().Trim();
            string com = (Properties.Settings.Default.com + 1).ToString();

            power = new PowerUp(routingBoard, form, base_, "1", com);
            //power = new WecoControl(form, base_);

            results = DatabaseHelper.ReadWECORelayTable(power.wrs_routing_brd, power.wrs_form, power.wrs_base, power.wrs_phase);
            Thread.Sleep(200);


            foreach (var x in results)
            {
                power.wrs_pa_volt_sw_1 = (ushort)x.pa_volt_sw_1;
                power.wrs_pa_volt_sw_2 = (ushort)x.pa_volt_sw_2;
            }


            if (powerOn == false)
            {


                if (power != null)
                {

                    if (initialize == false)
                    {
                        bool init = power.Initialize_Powerup();

                        if (init == false)
                        {
                            Application.Current.Dispatcher.Invoke(() => MessageBox.Show("INITIALIZATION FAILED: " + power.fail));
                            initialize = false;

                        }
                        else if (init == true)
                        {
                            initialize = true;
                        }


                    }


                    if (initialize == true && PowerWeco(power, 240))
                    {

                        powerOn = true;
                    }
                    else
                    {
                        powerOn = false;
                    }

                }
            }
        }

        private void Bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {


            Thread.Sleep(100);
            if (powerOn == true && initialize == true)
            {
                IsOn.Foreground = Brushes.Red;
                Measage1.Foreground = Brushes.Red;
                Measage2.Foreground = Brushes.Red;

                BackgroundWorker bg2 = new BackgroundWorker();
                bg2.DoWork += Bg2_DoWork;
                bg2.RunWorkerCompleted += Bg2_RunWorkerCompleted;
                bg2.RunWorkerAsync();

            }

            else
            {
                
                SerialNumber.Text = "";

                form = null;
                base_ = null;
            }
        }

        private void Bg2_DoWork(object sender, DoWorkEventArgs e)
        {



           // myDmm = new MeasureVoltage();

            double volts;
          
            volts = Math.Abs(myDmm.Measure());

            // this is a aprroximated voltage that will in theory is to low to be measured on the cap when charge is applied . hopefully this will check if leads are connected or the saftey cover is closed
            //this value was determined experimentally
            if (volts > 5)
            {

               
                if (Measure1(myDmm) == true )
                {

                    Application.Current.Dispatcher.Invoke(() => Test240.Text = "240V Test Passed");

                    if (TurnOffPower(power))
                    {



                        if (PowerWeco(power, 480))
                        {

                            power480 = true;
                            
                            if (Measure2(myDmm) == true)
                            {
                                pass = true;


                            }
                            else
                            {
                                fail = true;


                            }
                        }
                        else
                        {
                            power480 = false;
                        }
                    }

                    else
                    {
                        powerOn = true;
                    }

                }

                else
                {
                    fail240 = true;


                }

            }

            else
            {
                coverOpen = true;
            }

            if (TurnOffPower(power))
            {

                powerOn = false;


            }
        }


        private void Bg2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
            if (powerOn == false && meterConnected == true)

            {
                
                if (pass == true)
                {
                    ID.Text = SerialNumber.Text;
                    Test480.Text = "480V Test Passed";
                    DateTime aDate = DateTime.Now;
                  


                    //string time = aDate.ToString("dddd, dd MMMM yyyy HH:mm:ss");
                    
                    string result = "Pass";
                    string result240 = "Pass";
                    string result480 = "Pass";
                    string barCode = SerialNumber.Text;
                    string supplierCode = _stringHelper.RemoveLeadingChars(SerialNumber.Text, 1);
                    string dateCode = _stringHelper.RemoveLeadingChars(SerialNumber.Text, 2);
                    string serialNumber = _stringHelper.RemoveLeadingChars(SerialNumber.Text, 3);
                    string partNumber = _stringHelper.RemoveLaggingChars(SerialNumber.Text);
                    string leadsCover = "Yes";
                    DatabaseHelper.AddData(barCode, partNumber, supplierCode, dateCode, serialNumber, leadsCover, result240, dcVoltage1.ToString(), result480, dcVoltage2.ToString(), result, aDate);

                    Fail.Text = "";
                    Pass.Text = "PASS";
                    Pass.Foreground = Brushes.Green;

                }

                if (fail == true)
                {
                    ID.Text = SerialNumber.Text;
                    Test480.Text = "480V Test Failed";
                    DateTime aDate = DateTime.Now;
                    //string time = aDate.ToString("dddd, dd MMMM yyyy HH:mm:ss");
                    string result = "Fail";
                    string result240 = "Pass";
                    string result480 = "Fail";
                    string barCode = SerialNumber.Text;
                    string supplierCode = _stringHelper.RemoveLeadingChars(SerialNumber.Text, 1);
                    string dateCode = _stringHelper.RemoveLeadingChars(SerialNumber.Text, 2);
                    string serialNumber = _stringHelper.RemoveLeadingChars(SerialNumber.Text, 3);
                    string partNumber = _stringHelper.RemoveLaggingChars(SerialNumber.Text);
                    string leadsCover = "Yes";
                    DatabaseHelper.AddData(barCode, partNumber, supplierCode, dateCode, serialNumber, leadsCover, result240, dcVoltage1.ToString(), result480, dcVoltage2.ToString(), result, aDate);
                    Pass.Text = "";
                    Fail.Text = "FAIL";
                    Fail.Foreground = Brushes.Red;

                }




                if (fail240 == true)
                {
                    ID.Text = SerialNumber.Text;

                    Test240.Text = "240V Test Failed";
                    DateTime aDate = DateTime.Now;
                    //string time = aDate.ToString("dddd, dd MMMM yyyy HH:mm:ss");
                    string result = "Fail";
                    string result240 = "Fail";
                    string result480 = "";
                    string barCode = SerialNumber.Text;
                    string supplierCode = _stringHelper.RemoveLeadingChars(SerialNumber.Text, 1);
                    string dateCode = _stringHelper.RemoveLeadingChars(SerialNumber.Text, 2);
                    string serialNumber = _stringHelper.RemoveLeadingChars(SerialNumber.Text, 3);
                    string partNumber = _stringHelper.RemoveLaggingChars(SerialNumber.Text);
                    string leadsCover = "Yes";
                    DatabaseHelper.AddData(barCode, partNumber, supplierCode, dateCode, serialNumber, leadsCover, result240, dcVoltage1.ToString(), result480, dcVoltage2.ToString(), result, aDate);


                    Pass.Text = "";
                    Fail.Text = "FAIL";
                    Fail.Foreground = Brushes.Red;
                }

                if (coverOpen == true)
                {
                    MessageBox.Show("Check if Cover is closed. Check if leads are connected. Check if Base is assembled properly.");
                    DateTime aDate = DateTime.Now;
                    //string time = aDate.ToString("dddd, dd MMMM yyyy HH:mm:ss");
                    string result = "";
                    string result240 = "";
                    string result480 = "";
                    string barCode = SerialNumber.Text;
                    string supplierCode = _stringHelper.RemoveLeadingChars(SerialNumber.Text, 1);
                    string dateCode = _stringHelper.RemoveLeadingChars(SerialNumber.Text, 2);
                    string serialNumber = _stringHelper.RemoveLeadingChars(SerialNumber.Text, 3);
                    string partNumber = _stringHelper.RemoveLaggingChars(SerialNumber.Text);
                    string leadsCover = "No";
                    dcVoltage1 = double.NaN;
                    DatabaseHelper.AddData(barCode, partNumber, supplierCode, dateCode, serialNumber, leadsCover, result240, dcVoltage1.ToString(), result480, dcVoltage2.ToString(), result, aDate);
                }

                SerialNumber.Text = "";
                IsOn.Foreground = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#C0C0C0"));
                IsOn.Text = "Power ON";
                form = null;
                base_ = null;
                while(Math.Abs(myDmm.Measure()) > 5.0)
                {

                }
                Measage1.Foreground = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#C0C0C0"));
                Measage2.Foreground = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#C0C0C0"));



            }

            else if (powerOn == true)
            {
                Test240.Text = "";
                Test480.Text = "";
                Voltages.Clear();
                Voltages2.Clear();
                SerialNumber.Text = "";
                form = null;
                base_ = null;
                while (Math.Abs(myDmm.Measure()) > 5.0)
                {

                }
                Measage1.Foreground = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#C0C0C0"));
                Measage2.Foreground = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#C0C0C0"));

            }
            else if (meterConnected == false)
            {
                IsOn.Foreground = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#C0C0C0"));
                IsOn.Text = "Power ON";
                SerialNumber.Text = "";
                Test240.Text = "";
                Test480.Text = "";
                Voltages.Clear();
                Voltages2.Clear();
                

                
                form = null;
                base_ = null;

            }

            else if (power480 == false)
            {
                
                SerialNumber.Text = "";
                Voltages.Clear();
                Voltages2.Clear();
                IsOn.Foreground = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#C0C0C0"));
                IsOn.Text = "Power ON";
                MessageBox.Show("480V POWER UP FAIL: " + power.fail);
                form = null;
                base_ = null;
                while (Math.Abs(myDmm.Measure()) > 5.0)
                {

                }
                Measage1.Foreground = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#C0C0C0"));
                Measage2.Foreground = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#C0C0C0"));


            }
        }
    }
}
