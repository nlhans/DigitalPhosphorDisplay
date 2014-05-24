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
using Timer = System.Windows.Forms.Timer;

namespace PhosphorDisplay
{
    public partial class Form1 : Form
    {
        private bool artificialData = false;
        private ucPhosphorDisplay display;
        private Timer _mUpdateWaveforms = new Timer();

        private DataStream paDataStream;

        private int framesAcquired = 0;
        private Mutex framesHistoryMutex = new Mutex();
        private List<Waveform> framesHistory = new List<Waveform>();
        public Form1()
        {
            InitializeComponent();


            if (!artificialData)
            {
                paDataStream = new DataStream();
                paDataStream.WaveformDone += paDataStream_WaveformDone;
                paDataStream.Start();
            }
            display = new ucPhosphorDisplay();
            display.Dock = DockStyle.Fill;
            Controls.Add(display);

            this.FormClosing += Form1_FormClosing;

            _mUpdateWaveforms = new Timer();
            _mUpdateWaveforms.Interval = 10;
            if(artificialData)
                _mUpdateWaveforms.Tick += _mUpdateWaveforms_Tick_FakeData;
            else
                _mUpdateWaveforms.Tick += _mUpdateWaveforms_Tick_FromDatastream;
            _mUpdateWaveforms.Start();

            if(artificialData)
            _mUpdateWaveforms_Tick_FakeData(null, new EventArgs());

            this.SizeChanged += Form1_SizeChanged;
        }

        void Form1_SizeChanged(object sender, EventArgs e)
        {
            display.Invalidate();
        }

        void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(!artificialData)
            paDataStream.Stop();
        }

        void paDataStream_WaveformDone(object sender, EventArgs e)
        {
            lock (framesHistoryMutex)
            {
                framesAcquired++;
                framesHistory.Add((Waveform) sender);
            }
        }

        void _mUpdateWaveforms_Tick_FromDatastream(object sender, EventArgs e)
        {
            if (framesAcquired > 0)
            {
                display.channels = 1;

                // Add all frames to display
                lock (framesHistoryMutex)
                {
                    display.AddRange(framesHistory);
                    framesHistory.Clear();
                    //framesHistory.RemoveRange(0, framesHistory.Count/10);
                }
                display.Invalidate();

                framesAcquired = 0;
            }
        }

        private int frames = 5;
        private int counter = 0;
        private int lastMeasurement = -1;
        void _mUpdateWaveforms_Tick_FakeData(object sender, EventArgs e)
        {
            if (display.measurements == lastMeasurement) return;
            lastMeasurement = display.measurements;
            if (display.measurements >= 20)
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
            var sampleLength = this.Width;
            sampleLength = 800;
            //frames = 125*1920/sampleLength;
            frames = 500;
            //if (frames > 500) frames = 500;
            var channels = 3;

            Waveform f = default(Waveform);
            var r = new Random();

            for (int m = 0; m < frames; m++)
            {
                f = new Waveform(channels, sampleLength);

                var modFunc = 1.0f; // +m * 0.05f / frames;
                if (counter <= 0)
                {
                    counter = 25;
                    //counter = r.Next(1, 5000) ;
                    modFunc = 0.25f;
                }
                counter--;

                for (int i = 0; i < sampleLength; i++)
                {

                    float ch1 = 0, ch2 = 0, ch3 = 0;

                    //modFunc = (float)Math.Sin(i*2*Math.PI/2048);
                    ch1 = (float) (modFunc*Math.Sin(i*2*Math.PI/sampleLength*2));
                    ch3 = r.Next(-5000, 5000) * 1.0f / r.Next(60000, 2500000);
                    ch1 += ch3;
                    ch2 = -ch1 - r.Next(3,10)*ch3;

                    var ch = new[] {ch1, ch2, ch3};
                    var time = i*0.05f/sampleLength - 0.05f/2; // 70ms

                    f.Store(time, ch);
                }
                f.TriggerTime = 0;


                display.Add(f);
            }

            display.channels = channels;
            display.Invalidate();
            ws.Stop();
        }
    }
}
