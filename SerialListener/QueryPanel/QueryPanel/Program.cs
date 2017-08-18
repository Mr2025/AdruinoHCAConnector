using System;
using System.Configuration;
using System.Text;
using System.Linq;
using System.Threading;
using OpenNETCF.IO.Ports;
using System.Diagnostics;


namespace QueryPanel
{
    class Program
    {
        static int m_LowPin = 0;
        static int m_HighPin = 0;
        static string m_TruthText = "true";
        static string m_FalseText = "false";

        static void Main(string[] args)
        {
            //Step 0. Load some Configurations
            m_LowPin = int.Parse(ConfigurationSettings.AppSettings.Get("LowPin"));
            m_HighPin = int.Parse(ConfigurationSettings.AppSettings.Get("HighPin"));
            m_TruthText = ConfigurationSettings.AppSettings.Get("TrueText");
            m_FalseText = ConfigurationSettings.AppSettings.Get("FalseText");

            //Step 1. GEt the Com Port
            string comport = ConfigurationSettings.AppSettings.Get("COMPort");
            //byte[] bufout = buildCmd();
            byte[] bufout = buildQueryCmd();

            //STep 2. Attach to the given port, use windows trick to get com ports > 8
            SerialPort target = new SerialPort(@"\\.\" + comport, 9600);
            //target.ReadBufferSize = 2048; 
            target.Encoding = ASCIIEncoding.ASCII;

            string comSource = string.Empty;
            try
            {
                if (target.IsOpen) target.Close();
                target.Open();
                Thread.Sleep(1000);	// wait for device to boot up - include flash sequence.
                
                //Step 3. Send Query "q" 
                target.Write(bufout, 0, bufout.Length);
                Thread.Sleep(1000);	// wait for device to receive and process command
                comSource = target.ReadExisting();                               
            }
            finally
            {
                //Step 5. Shut down the Port
                target.Close();
            }

            if (string.IsNullOrEmpty(comSource)) throw new ApplicationException("No Response from Device");

            Debug.WriteLine(string.Format("ComSource:={0}",comSource));
            //Step 4. Convert Response to HCA settings and slam to the console
            Console.WriteLine(
                replacePinNames(
                    comSource,
                    buildPinTitleList()));


            //STep 6. Debug statement
            //Console.ReadKey();
        }

        static byte[] buildCmd()
        {
            string cmd = "r\0";
            return System.Text.Encoding.ASCII.GetBytes(cmd); 

        }

        static byte[] buildQueryCmd()
        {
            string cmd = "qAA\0";
            byte[] rVal = System.Text.Encoding.ASCII.GetBytes(cmd);

            rVal[1] = Convert.ToByte(m_LowPin);
            rVal[2] = Convert.ToByte(m_HighPin);

            Debug.WriteLine(string.Format("Query:=[{0}{1}{2}]", Convert.ToChar(rVal[0]),Convert.ToChar(rVal[1]),Convert.ToChar(rVal[2])));
            return rVal;

        }

        static string[] buildPinTitleList()
        {
            //Step 1. load and parse the give settings items.
            string csv = ConfigurationSettings.AppSettings.Get("PinTitles");
            return csv.Split(",".ToCharArray()).Select((string s)=> s.Trim()).ToArray();
        }

        static byte[] buildSourceValues(string hexString)
        {
            //Step 1. load and parse the give settings items.
            return hexString.Trim().Split(' ').Select((string s) => Convert.ToByte(s.Trim(),16)).ToArray();
        }

        static bool[] buildSourceValues(byte[] compressedValue)
        {
            //Step 1. Cross Product for compressedValue * BIT_MASK
            byte[] BIT_MASK = new byte[] { 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01 };
            return compressedValue.SelectMany((byte val) => BIT_MASK.Select((byte mask) => ( (val & mask) > 0 ))).ToArray();
        }

        static string replacePinNames(string hexString, string[] PinList)
        {
            
            string rVal = string.Empty;
            bool[] pinValues = buildSourceValues(buildSourceValues(hexString));
            //Step 1. use the parallel nature of the of the input string and the pinlist to name the given strings. 
            
            for(int index = m_LowPin; index < m_HighPin; index++){
                
                string pinName = string.Format("Pin{0}", index);
                if (PinList.Length > index) pinName = PinList[index];

                string isPinHigh = (pinValues[index] ? m_TruthText : m_FalseText);
                
                if (index > m_LowPin) rVal += "\r\n";
                rVal += String.Format("{0}={1}", pinName, isPinHigh); 
            }
            return rVal;
        }
    }
}
