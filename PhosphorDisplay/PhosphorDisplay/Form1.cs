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
        private bool triggerOk = true;
        private bool overviewRefresh = false;

        public ucPhosphorDisplay displayTrig;
        public ucPhosphorDisplay displayOverview;
        public ucRmsMeter dmm;

        public AcquisitionEngine acqEngine;


        public Form1()
        {
            InitializeComponent();

            acqEngine = new AcquisitionEngine(new NetStream());

            acqEngine.AcquisitionTime = 100.0f/1000;

            // Zoom waveform
            displayTrig = new ucPhosphorDisplay();
            displayTrig.channels = 1;

            displayTrig.horizontalScale = (float) (acqEngine.AcquisitionTime/displayTrig.horizontalDivisions/2);
            displayTrig.verticalScale = new float[3] { 25f/1000000, 1, 1};

            Controls.Add(displayTrig);
            
            // Triggered waveform
            displayOverview = new ucPhosphorDisplay();
            displayOverview.channels = 1;
            displayOverview.verticalScale= displayTrig.verticalScale;
            displayOverview.horizontalScale = 1.0f;

            Controls.Add(displayOverview);

            // Measurement "DMM"
            dmm = new ucRmsMeter();
            Controls.Add(dmm);

            this.Layout += Form1_Layout;

            acqEngine.OverviewWaveform += acqEngine_OverviewWaveform;
            acqEngine.TriggeredWaveform += acqEngine_Waveform;

            var t = new Timer();
            t.Tick += (sender, args) =>
                          {
                              if (triggerOk)
                              {
                                  triggerOk = false;

                                  displayTrig.Invalidate();
                              }
                              if(overviewRefresh)
                              {
                                  overviewRefresh = false;

                                  dmm.Invalidate();
                                  displayOverview.Invalidate();
                              }
                          };
            
            t.Interval = 40; // 25 fps
            t.Start();

            this.SizeChanged += Form1_SizeChanged;
        }

        void Form1_Layout(object sender, LayoutEventArgs e)
        {
            var h = this.Height;
            var h1 = (int)(h*0.3);
            var h2 = (int) (h*0.7);
            dmm.Size = new Size(150, h1);

            displayOverview.Size = new Size(this.Width-dmm.Width, h1);
            displayTrig.Size = new Size(this.Width, h2);

            displayOverview.Location = new Point(0, 0);
            dmm.Location = new Point(displayOverview.Width, 0);
            displayTrig.Location = new Point(0, h1);
        }

        void acqEngine_OverviewWaveform(Waveform f)
        {
            displayOverview.horizontalScale = f.Horizontal[f.Samples - 1]/10;
            displayOverview.horizontalOffset = displayOverview.horizontalScale * displayOverview.horizontalDivisions;
            
            displayOverview.Add(f);

            // Update dmm:
            float currentMin = float.MaxValue;
            float currentMax = float.MinValue;
            float currentRms = 0;
            float currentMean = 0;

            for(int k = 0; k < f.Samples;k ++)
            {
                var c = f.Data[0][k];
                currentMin = Math.Min(currentMin, c);
                currentMax = Math.Max(currentMax, c);

                currentRms += c*c;
                currentMean += c;
            }

            currentMean /= f.Samples;

            currentRms *= f.Horizontal[1] - f.Horizontal[0];
            currentRms /= f.Horizontal[f.Samples - 1];
            currentRms = (float)Math.Sqrt(currentRms);

            dmm.meanCurrent = currentMean;
            dmm.rmsCurrent = currentRms;
            dmm.minCurrent = currentMin;
            dmm.maxCurrent = currentMax;

            dmm.sixDigitVoltage = acqEngine.LastVoltageMeasurent;

            overviewRefresh = true;
        }

        void acqEngine_Waveform(Waveform f)
        {
            displayTrig.Add(f);
            triggerOk = true;
        }



        void Form1_SizeChanged(object sender, EventArgs e)
        {
            displayTrig.Invalidate();
        }
    }
}
