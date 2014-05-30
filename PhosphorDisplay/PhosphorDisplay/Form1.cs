using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using PhosphorDisplay.Acquisition;
using PhosphorDisplay.Data;
using Timer = System.Windows.Forms.Timer;

namespace PhosphorDisplay
{
    public partial class Form1 : Form
    {
        private bool hadStuffAdded = true;
        public ucPhosphorDisplay display;
        public AcquisitionEngine acqEngine;
        public Form1()
        {
            InitializeComponent();

            acqEngine = new AcquisitionEngine(new ArtificialStream());

            display = new ucPhosphorDisplay();
            display.channels = 1;
            display.horizontalScale = 0.1f/(acqEngine.Source as ArtificialStream).freq;
            display.Dock = DockStyle.Fill;
            Controls.Add(display);

            acqEngine.Waveform += acqEngine_Waveform;

            var t = new Timer();
            t.Tick += (sender, args) =>
                          {
                              if (hadStuffAdded)
                              {
                                  hadStuffAdded = false;

                                  display.Invalidate();
                              }
                          };
            t.Interval = 1;
            t.Start();

            this.SizeChanged += Form1_SizeChanged;
        }

        void acqEngine_Waveform(Waveform f)
        {
            //Debug.WriteLine("waveform");
            display.Add(f);
            hadStuffAdded = true;
        }



        void Form1_SizeChanged(object sender, EventArgs e)
        {
            display.Invalidate();
        }
    }
}
