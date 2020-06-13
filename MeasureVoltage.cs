using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Agilent.Ag34450.Interop;
namespace diode_Test_Beta
{
    public class MeasureVoltage

        // multimeter interface 
  
    {
        public  double Measure()
        {
            Ag34450 dmm = new Ag34450();
            double reading = 0;
            try
            {

                dmm.Initialize("USB0::0x2A8D::0xB318::MY59150006::0::INSTR", false, true);
                //dmm.Initialize("GPIB::23", false, true,"simulate = true");

                dmm.Status.Clear();
                dmm.Status.Preset();
                dmm.DCVoltage.AutoRangeEnabled = true;
                dmm.DCVoltage.ConfigureAuto();
                dmm.Trigger.Delay = 0.01;
                reading = dmm.Measurement.Read(2000);

                if (CheckDMMError(dmm))
                {

                    return reading;
                }




            }

            catch (Exception e)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() => MessageBox.Show("Error occured: " + e.Message));
               
            }

            

                try { dmm.Close(); }
                catch { }
                try { System.Runtime.InteropServices.Marshal.ReleaseComObject(dmm); }
                catch { }

            return reading;
                
            

           
        }

        public bool CheckDMMError(Ag34450 dmm)
        {

            dmm.System2.WriteString("SYST:ERR?");
            string errStr = dmm.System2.ReadString();
            

            if (errStr.Contains("No error")) //If no error, then return
                return true;
            //If there is an error, read out all of the errors and return them in an exception
            else
            {
                Application.Current.Dispatcher.Invoke(() => MessageBox.Show(errStr));
                string errStr2 = "";
                do
                {
                    dmm.System2.WriteString("SYST:ERR?");
                    errStr2 = dmm.System2.ReadString();
                    if (!errStr2.Contains("No error")) errStr = errStr + "\n" + errStr2;

                } while (!errStr2.Contains("No error"));
                
                throw new Exception("Exception: Encountered system error(s)\n" + errStr);

                
            }

        }

    }







    
}
