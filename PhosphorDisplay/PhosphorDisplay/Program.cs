using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PhosphorDisplay.Data;

namespace PhosphorDisplay
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            /*bool adcMcp = true;
            float volt = 3.3f;

            StringBuilder sb = new StringBuilder();

            for (int i = -0x8000; i < 0x7FFF; i+=16)
            {
                sb.Append(i + "," +
                    NetStream.ConvertCodeToAmp(i, volt, 1, 0, adcMcp) + "," +
                    NetStream.ConvertCodeToAmp(i, volt, 10, 0, adcMcp) + "," +
                    NetStream.ConvertCodeToAmp(i, volt, 100, 0, adcMcp) + "," +
                    NetStream.ConvertCodeToAmp(i, volt, 1000, 0, adcMcp) + "," +
                          "\r\n");
            }
            File.WriteAllText("./cal_adc.csv", sb.ToString());

            return;*/
                Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new PhosphorDisplay());
        }
    }
}
