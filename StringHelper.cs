using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace diode_Test_Beta
{

   
    public class StringHelper
      
    {
        string parsedString = "";

        // Removes all spaces and characters other than the BME_NO from 2D barcode
        public string RemoveLaggingChars(string boardNumber)
        {

            string trim = boardNumber.Trim(' ', ',');
            string result = trim;
            int count;
            for (int i = 0; i < trim.Length; i++)
            {

                if (trim.ElementAt(i) == ',')
                {
                    count = (trim.Length) - i;
                    result = trim.Remove(i, count);

                    break;
                }
            }


            return result;
        }

        public string RemoveLeadingChars(string boardNumber, int iterable)
        {
            string trim = boardNumber.Trim(' ', ',');
            int count;
            

            if (iterable == 0)
            {
                return boardNumber;
            }
            

            for (int i = 0; i < trim.Length; i++)
            {

                if (trim.ElementAt(i) == ',')
                {
                    count = (trim.Length) - i;
                    parsedString = trim.Substring(i, count);

                    break;
                }
            }

            iterable--;
            
            if(iterable >0)
            {
                RemoveLeadingChars(parsedString, iterable);
            }



            return RemoveLaggingChars(parsedString);

        }

        public bool NumberComplete(string boardNumber)
        {

            string trim = boardNumber.Trim(' ', ',');
            int lastindex = trim.Length - 1;



            int count = 0;
            for (int i = lastindex; i >= 0; i--)
            {
                count++;

                if (trim.ElementAt(i) == ',')
                {
                    break;
                }


            }

            if (count == 6)
            {
                return true;
            }
            else
                return false;

        }
    }
}
