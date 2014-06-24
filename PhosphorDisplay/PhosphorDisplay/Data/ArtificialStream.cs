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

        private MMTimer generateData;

        #region Implementation of IDataSource

        public float SampleRate { get; private set; }
        public event DataSourceEvent Data;
        public event HighresEvent HighresVoltage;
        public event EventHandler Connected;
        public event EventHandler Disconnected;
        public void Connect(object target)
        {
            generateData = new MMTimer(10);
            generateData.Tick += GenerateDataElapse;
            generateData.Start();

            IsConnected = true;
            if(Connected!=null) Connected(this, new EventArgs());
        }

        public void Disconnect()
        {
            IsConnected = false;
            if (Disconnected != null) Disconnected(this, new EventArgs());
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
        public int samplesPerSecond = 400000;
        public int freq = 1000;

        private void GenerateDataElapse(object sender, EventArgs eventArgs)
        {
            var r = new Random();
            int generatingSamples = samplesPerSecond /100;

            float[] samples = new float[generatingSamples];
            var timeInterval = 1.0/samplesPerSecond;

            for (int i = 0; i < generatingSamples; i++)
            {
                var k = i + sampleCounter;

                var t = timeInterval*k;

                samples[i] = (float) (Math.Sin(2*Math.PI*t*(freq )) + (r.Next(2005, 5005) / 100000.0f));
            }
            sampleCounter += generatingSamples;

            var dpk = new DataPacket(samples, DataType.DutCurrent, (float)timeInterval);

            if (Data != null)
                Data(dpk);

        }
    }
}
