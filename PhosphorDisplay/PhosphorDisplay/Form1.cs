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
            _mUpdateWaveforms.Interval = 25;
            _mUpdateWaveforms.Tick += _mUpdateWaveforms_Tick;
            _mUpdateWaveforms.Start();
            _mUpdateWaveforms_Tick(null, new EventArgs());

            this.SizeChanged += Form1_SizeChanged;
        }

        void Form1_SizeChanged(object sender, EventArgs e)
        {
            display.Invalidate();
        }

        private int frames = 5;

        void _mUpdateWaveforms_Tick(object sender, EventArgs e)
        {
            if (display.measurements >= 20 && frames <= 500)
            {
                Debug.WriteLine("Drawing " + frames + " frames takes on average " + display.speed + " (+" + display.overhead +
                                "ms) -> " + (frames*1000/display.speed*display.channels) + "wfms");
                display.speed = -1;
                display.measurements = 0;
                display.overhead = -1;

                if (frames >= 100) frames += 10;
                else frames += 5;
            }

            Stopwatch ws = new Stopwatch();
            ws.Start();
            frames = 250;
            var channels = 1;

            Waveform f = default(Waveform);
            var r = new Random();

            for (int m = 0; m < frames; m++)
            {
                var sampleLength = 1920; // this.Width;
                f = new Waveform(channels, sampleLength);

                for (int i = 0; i < sampleLength; i++)
                {

                    float ch1 = 0, ch2 = 0, ch3 = 0;

                    var modFunc = 1.0f; // +m * 0.05f / frames;
                    if (m % 25 == 0) modFunc /= 2.0f;
                    ch1 = (float) (modFunc*Math.Sin(i*2*Math.PI/sampleLength*2));
                    ch3 = r.Next(-1000, 1000) * 1.0f / r.Next(25000, 500000);
                    ch1 += ch3;
                    ch2 = -ch1;
                    
                    var ch = new[] {ch1, ch2, ch3};
                    var time = i*0.07f/sampleLength - 0.07f/2; // 70ms

                    f.Store(time, ch);
                }
                f.TriggerTime = 0;


                display.Add(f);
                display.Add(f);
                display.Add(f);
                display.Add(f);
                display.Add(f);
                m += 4;
            }

            display.channels = channels;
            display.Invalidate();
            ws.Stop();
        }
    }
}
