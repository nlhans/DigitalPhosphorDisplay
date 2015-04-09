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
using PhosphorDisplay.Triggers;
using Timer = System.Windows.Forms.Timer;
using PhosphorDisplay.Widget;

namespace PhosphorDisplay
{
    public partial class PhosphorDisplay : Form
    {
        private bool triggerOk = true;
        private bool overviewRefresh = false;
        public ucScopeControls controls;
        public ucPhosphorDisplay displayTrig;
        public ucPhosphorDisplay displayOverview;
        public ucRmsMeter dmm;
        public AcquisitionEngine acqEngine;

        private Timer waveformUpdateTimer;

        public PhosphorDisplay()
        {
            InitializeComponent();

            acqEngine = new AcquisitionEngine(new NetStream());
            acqEngine.AcquisitionTime = 0.15f / 1000;
            acqEngine.PretriggerTime = 0.0f / 1000;

            // Zoom waveform
            displayTrig = new ucPhosphorDisplay();
            displayTrig.Channels = 1;
            displayTrig.DoubleClicked += displayTrig_DoubleClicked;

            displayTrig.HorizontalScale = (float)(acqEngine.AcquisitionTime / displayTrig.HorizontalDivisions / 2);
            displayTrig.VerticalScale = new float[3] { 0.25f, 1, 1 };

            Controls.Add(displayTrig);
            
            // Triggered waveform
            displayOverview = new ucPhosphorDisplay();
            displayOverview.Channels = 1;
            displayOverview.VerticalScale = displayTrig.VerticalScale;
            displayOverview.HorizontalScale = 1.0f;
            displayOverview.lowContrast = true;

            Controls.Add(displayOverview);

            var takeZeroTimer = new System.Timers.Timer();
            takeZeroTimer.Interval = 500;
            takeZeroTimer.AutoReset = false;
            takeZeroTimer.Elapsed += (sender, args) =>
                                     {
                                         // Calculate zero from autocal values
                                         lock (autoCalValues)
                                         {
                                             var meanZero = autoCalValues.Average();
                                             acqEngine.Source.Zero(meanZero);
                                         }
                                     };

            // Measurement "DMM"
            dmm = new ucRmsMeter();
            dmm.ClearZero += (sender, args) => acqEngine.Source.Zero(0);
            dmm.Zero += (sender, args) =>
                        {
                            acqEngine.Source.Zero(0);
                            lock (autoCalValues)
                            {
                                Thread.Sleep(500);
                                autoCalValues.Clear();
                            }
                            takeZeroTimer.Start();
                        };

            Controls.Add(dmm);

            // Control panel
            controls = new ucScopeControls(acqEngine, displayTrig, displayOverview);
            controls.UpdateRateChanged += controls_UpdateRateChanged;
            Controls.Add(controls);

            this.Layout += Form1_Layout;

            acqEngine.OverviewWaveform += acqEngine_OverviewWaveform;
            acqEngine.TriggeredWaveform += acqEngine_Waveform;

            waveformUpdateTimer = new Timer();
            waveformUpdateTimer.Tick += (sender, args) =>
            {
                if (triggerOk)
                {
                    triggerOk = false;

                    displayTrig.Invalidate();
                }
                if (overviewRefresh)
                {
                    overviewRefresh = false;

                    dmm.Invalidate();
                    displayOverview.Invalidate();
                }
            };
            
            waveformUpdateTimer.Interval = 40; // 25 fps
            waveformUpdateTimer.Start();

            acqEngine.Source.HighresVoltage += voltage =>
            {
                dmm.sixDigitVoltage = voltage;
                dmm.Invalidate();
            };
            this.SizeChanged += Form1_SizeChanged;
        }

        private void controls_UpdateRateChanged(float period)
        {
            waveformUpdateTimer.Interval = (int)period;

        }

        private void displayTrig_DoubleClicked(double time, double scalarCh1)
        {
            var half = displayTrig.VerticalScale[0]/100.0f;

            // Set trigger level:
            acqEngine.Trigger.SetOption(TriggerOption.LevelL, scalarCh1 - half);
            acqEngine.Trigger.SetOption(TriggerOption.LevelH, scalarCh1 + half);

            Console.WriteLine("Trigger level set at {0:0.000000}A ({1:000.000} uA)", scalarCh1, (scalarCh1*1000000));
        }


        void Form1_Layout(object sender, LayoutEventArgs e)
        {
            var h = Height-40;
            var h1 = (int)(h * 0.3);
            var h2 = (int)(h * 0.7);
            dmm.Size = new Size(200, h1);
            controls.Size = new Size(200, h2);

            displayOverview.Size = new Size(this.Width - dmm.Width, h1);
            displayTrig.Size = new Size(this.Width - dmm.Width, h2);

            displayOverview.Location = new Point(0, 0);
            dmm.Location = new Point(displayOverview.Width, 0);
            displayTrig.Location = new Point(0, h1);
            controls.Location = new Point(displayOverview.Width, h1);
        }

        private List<float> autoCalValues = new List<float>();

        private void acqEngine_OverviewWaveform(Waveform f)
        {
            if (f.Error) return;
            if (f.Samples == 0) return;
            displayOverview.HorizontalScale = f.Horizontal[f.Samples - 1]/10;
            displayOverview.HorizontalOffset = displayOverview.HorizontalScale*displayOverview.HorizontalDivisions;

            displayOverview.Add(f);

            // Update dmm:
            float currentMin = float.MaxValue;
            float currentMax = float.MinValue;
            float currentRms = 0;
            float currentMean = 0;

            for (int k = 0; k < f.Samples; k ++)
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
            currentRms = (float) Math.Sqrt(currentRms);

            dmm.meanCurrent = currentMean;
            dmm.rmsCurrent = currentRms;
            dmm.minCurrent = currentMin;
            dmm.maxCurrent = currentMax;

            dmm.sixDigitVoltage = acqEngine.LastVoltageMeasurent;

            overviewRefresh = true;

            // Autocal values;
            lock (autoCalValues)
            {
                autoCalValues.AddRange(f.Data[0].Take(f.Samples));
                if (autoCalValues.Count > 10000)
                    autoCalValues.RemoveRange(0, autoCalValues.Count - 10000);
            }
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

        private void PhosphorDisplay_FormClosing(object sender, FormClosingEventArgs e)
        {
            acqEngine.Stop();
        }
    }
}
