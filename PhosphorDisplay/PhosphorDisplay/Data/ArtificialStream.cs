using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using PhosphorDisplay.Acquisition;

namespace PhosphorDisplay.Data
{
    class ArtificialStream : IDataSource
    {
        public bool IsConnected = false;
        public bool IsRunning = false;
        #if __MonoCS__
        private Timer generateData;
        #else
        private MMTimer generateData;
        
        
        
        
        
        
        #endif
        #region Implementation of IDataSource
        public float SampleRate { get { return samplesPerSecond; } }
        public event DataSourceEvent Data;
        public event HighresEvent HighresVoltage;
        public event EventHandler Connected;
        public event EventHandler Disconnected;
        public void Connect(object target)
        {
#if __MonoCS__
            
            generateData = new Timer();
            generateData.Interval = 10;
            generateData.Elapsed += GenerateDataElapse;
            generateData.Start();
#else
                generateData = new MMTimer(10);
            generateData.Tick += GenerateDataElapse;
            generateData.Start();
#endif

            IsConnected = true;
            if (Connected != null)
                Connected(this, new EventArgs());
        }
        public void Disconnect()
        {
            IsConnected = false;
            if (Disconnected != null)
                Disconnected(this, new EventArgs());
        }
        public void Configure(object configuration)
        {
            // No configuration yet!
        }
        public void Start()
        {
            IsRunning = true;
        }
        public void Stop()
        {
            IsRunning = false;
        }
        #endregion
        private int sampleCounter = 0;
        public int samplesPerSecond = 2500000;
        private float accumulatedPhase = 0.0f;
        private float accumulatedAmplitudeModulation = 0.0f;
        private float amModulation = 0.0f;
        public int freq = 2500;
        private void GenerateDataElapse(object sender, EventArgs eventArgs)
        {
            freq = 10000;
            samplesPerSecond = 5000000;
            var r = new Random();
            int generatingSamples = samplesPerSecond / 100;

            float[] samples = new float[generatingSamples];
            var timeInterval = 1.0f / samplesPerSecond;

            for (int i = 0; i < generatingSamples; i++)
            {
                var k = i + sampleCounter;

                var t = timeInterval * k;

                accumulatedPhase += timeInterval * 1.0f * freq;
                accumulatedPhase %= 1;

                accumulatedAmplitudeModulation++;
                accumulatedAmplitudeModulation %= samplesPerSecond*2;

                var s = Math.Sin(2 * Math.PI * accumulatedPhase);
                //s *= 1 / Math.Sqrt(2);
                //s *= Math.Cos(2.0*Math.PI*accumulatedAmplitudeModulation/samplesPerSecond);

                s += r.Next(-1005, 1005) / 100000.0f;

                samples[i] = (float)(s);
            }
            sampleCounter += generatingSamples;

            var dpk = new DataPacket(samples, DataType.DutCurrent, (float)timeInterval);

            if (Data != null)
                Data(dpk);

        }
    }
}
