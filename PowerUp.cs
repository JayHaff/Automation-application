using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace diode_Test_Beta
{
     public class PowerUp
    {
        



        public string wrs_routing_brd { get; set; }
        public string wrs_form { get; set; }
        public string wrs_base { get; set; }
        public string wrs_phase { get; set; }
        public string comPort { get; set; }
        public ushort wrs_pa_volt_sw_1 { get; set; }
        public ushort wrs_pa_volt_sw_2 { get; set; }

        public string fail = "";


        protected static string[] eMessages = new string[20]
        {
            "No_Error",
            "No_Meter_Present",
            "Invalid_Voltage",
            "Invalid_Current",
            "Invalid_Model",
            "Invalid_Potential_Routing_Brd",
            "Invalid_Interface",
            "Invalid_Option",
            "Invalid_Current_Relays",
            "Invalid_DeEnergize",
            "USB_Open_Failed",
            "Invalid_Comport",
            "Serial_Port_Not_Connected",
            "DLL_Not_Initialized",
            "Voltage_Was_Denergized",
            "SMUSB_DLL_NOT_FOUND",
            "Invalid_Station_Number",
            "PCB_Does_Not_Support_2Phase",
            "PCB_Supports_2Phase_Not_Configured",
            "Fully_Suports_2Phase"

        };

        public PowerUp()
        {



        }
        public PowerUp(string wrs_routing_brd, string wrs_form, string wrs_base, string wrs_phase, string comPort)
        {
            this.wrs_routing_brd = wrs_routing_brd;
            this.wrs_form = wrs_form;
            this.wrs_base = wrs_base;
            this.wrs_phase = wrs_phase;
            this.comPort = comPort;


        }

        [DllImport("M3204.dll")]
        public extern static ushort DeEnergize(ushort delayseconds, ushort current);

        [DllImport("M3204.dll")]
        public extern static ushort Energize(ushort voltage, ushort current);

        [DllImport("M3204.dll")]
        public extern static ushort Init_DLL(ushort model, ushort prt, ushort tbi, ushort option1, ushort option2, ushort option3, ref ushort dll_rev);

        [DllImport("M3204.dll")]
        public extern static ushort Set_Relays(ushort curent, ushort v_r_1, ushort v_r_2, ushort kyz);


        public bool Initialize_Powerup()
        {
            ushort intRetValue = 0;
            ushort model = 1;
            ushort prb = 1;                                 // Potential_Routing_Board
            ushort tbi = 1;                                 // Test_Board_Interface (Com port #)

            ushort opt1 = 0;
            ushort dll_rev = 0;

            

            //string tstr = comPort.Remove(0, 41);
            tbi = (ushort)int.Parse(comPort);
            intRetValue = Init_DLL(model, prb, tbi, opt1, opt1, opt1, ref dll_rev);
            //Console.WriteLine($"SEE MEE {intRetValue}");
            if (intRetValue != 0)
            {
                fail = eMessages[intRetValue];
                // System.IO.Ports.SerialPort serial = new System.IO.Ports.SerialPort();
                // serial.Close();

                // comPort = null;



                return false;
            }


            return true;


        }

        public bool Energize_Powerup(ushort voltage)
        {
            ushort intRetValue = 0;
            ushort current = 0;
            ushort kyz = 0;


            intRetValue = Set_Relays(current, wrs_pa_volt_sw_1, wrs_pa_volt_sw_2, kyz);

            if (0 != intRetValue)
            {
                fail = eMessages[intRetValue];
                return false;
            }

            intRetValue = Energize(voltage, current);

            if (0 != intRetValue)
            {
                fail = eMessages[intRetValue];
                return false;

            }


            return true;
        }




        public bool deEnergize_Powerup()
        {
            ushort uintRetVal = 0;

            uintRetVal = DeEnergize(1, 1);

            if (0 != uintRetVal)
            {
                if (13 == uintRetVal)
                {
                    Initialize_Powerup();

                    fail = eMessages[uintRetVal];
                    return false;
                }
                else
                {

                    return false;
                }
            }
            Thread.Sleep(1000);
            return true;



        }



        ~PowerUp()
        {
          
        }


    

    }
}
