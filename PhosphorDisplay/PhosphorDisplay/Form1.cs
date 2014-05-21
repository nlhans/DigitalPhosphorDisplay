using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhosphorDisplay
{
    public partial class Form1 : Form
    {
        private ucPhosphorDisplay display;
        private Timer _mUpdateWaveforms = new Timer();

        public Form1()
        {
            InitializeComponent();

            display = new ucPhosphorDisplay();
            display.Dock = DockStyle.Fill;
            Controls.Add(display);

            _mUpdateWaveforms = new Timer();
            _mUpdateWaveforms.Interval = 100;
            _mUpdateWaveforms.Tick += _mUpdateWaveforms_Tick;
            _mUpdateWaveforms.Start();
            _mUpdateWaveforms_Tick(null, new EventArgs());

            this.SizeChanged += Form1_SizeChanged;
        }

        void Form1_SizeChanged(object sender, EventArgs e)
        {
            display.Invalidate();
        }

        private DateTime lastFrameIncrease = DateTime.Now;
        private int frames = 5;

        void _mUpdateWaveforms_Tick(object sender, EventArgs e)
        {
            if (display.measurements>=20 && frames <= 200)
            {
                var dt = DateTime.Now.Subtract(lastFrameIncrease).TotalMilliseconds/display.measurements;

                Debug.WriteLine("Drawing "+frames+" takes on average " + display.speed + " (+"+display.overhead+"ms)/"+dt+"ms -> " + (frames*1000/display.speed) + "wfms");
                display.speed = -1;
                display.measurements = 0;
                display.overhead = -1;
                lastFrameIncrease = DateTime.Now;

                if (frames >= 100) frames += 10;
                else frames += 5;
            }

            Stopwatch ws = new Stopwatch();
            ws.Start();
            frames = 1;
            var r = new Random();
            for (int m = 0; m < frames; m++)
            {
                var sampleLength = 28000;
                Waveform f = new Waveform(3, sampleLength);

                for (int i = 0; i < sampleLength; i++)
                {
                    float ch1, ch2,ch3;
                    /*
                    if (false)
                    {
                        ch1 = (i / 1.0f / (sampleLength/16)) % 2 - 1;
                        ch2 = (-i / 1.0f/(sampleLength/32)) % 2+1;
                    }else
                    {
                        ch1 = (float)Math.Sin(i*4*Math.PI/sampleLength);
                        ch2 = 0.0f;
                    }

                    */
                    var modFunc = (float)Math.Sin(i*2.0f*Math.PI/32);
                    //var modFunc = (i%64)/32.0f-1;
                    modFunc = 1;
                    ch1 = (float)(modFunc*Math.Sin(i*2*Math.PI/7500));
                    ch2 = i /(sampleLength/5.0f)%1-0.5f;
                    ch3 = -i / (sampleLength/10.0f)% 2 + 1.0f;
                    var noise = r.Next(-50, 50) * 1.0f / r.Next(1500, 10000);

                    //ch1 += noise;
                    ch2 -= noise;
                    ch3 -= noise*5;

                    var ch = new float[] { ch1, ch2, ch3 };
                    var time = i*0.05f / sampleLength - 0.05f / 2; //50ms

                    f.Store(time, ch);
                }
                f.TriggerTime = 0;
                display.Add(f);
            }

            display.channels = 3;
            display.Invalidate();
            ws.Stop();
        }
    }
}
