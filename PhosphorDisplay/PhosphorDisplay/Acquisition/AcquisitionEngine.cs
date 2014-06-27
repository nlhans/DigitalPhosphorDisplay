using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhosphorDisplay.Data;
using PhosphorDisplay.Triggers;

namespace PhosphorDisplay.Acquisition
{
    public class AcquisitionEngine
    {
        public delegate void AcquiredWaveform(Waveform f);

        public event AcquiredWaveform TriggeredWaveform;
        public event AcquiredWaveform OverviewWaveform;

        public IDataSource Source { get; private set; }
        public ITrigger Trigger { get; private set; }

        public int AcquisitionLength { get; private set; }
        public int Pretrigger { get; private set; }

        public float LastVoltageMeasurent { get; private set; }

        public double LongAcquisitionTime { get; set; }
        public double AcquisitionTime { get; set; }
        public double PretriggerTime { get; set; }


        public AcquisitionEngine(IDataSource source)
        {
            samplesOverflowSink = new List<float>();

            overviewWfLastCapture = DateTime.Now;
            overviewWf = new Waveform(1, 6000000);

            Trigger = new Edge(); // TODO: Temporary trigger

            Source = source;
            Source.Data += ProcessWaveform;
            Source.Data += Source_Data;
            Source.HighresVoltage += Source_HighresVoltage;

            Source.Connect(null);
            Source.Start();

        }

        void Source_HighresVoltage(float voltage)
        {
            LastVoltageMeasurent = voltage;
        }

        // Processes for overall view,.
        private Waveform overviewWf;
        private float overviewTimestamp = 0;
        private DateTime overviewWfLastCapture;
        void Source_Data(DataPacket data)
        {
            // Store each sample.
            foreach (var s in data.Samples)
            {
                overviewTimestamp += data.TimeInterval;
                overviewWf.Store(overviewTimestamp, new float[1] { s });
            }

            if (DateTime.Now.Subtract(overviewWfLastCapture).TotalMilliseconds >= LongAcquisitionTime *1000)
            {
                if (OverviewWaveform != null)
                    OverviewWaveform(overviewWf);

                overviewWf = new Waveform(1, 6000000);
                overviewTimestamp = 0.0f;
                overviewWfLastCapture = DateTime.Now;

            }
        }

        private void FireTriggeredWaveform(Waveform f)
        {
            if (TriggeredWaveform != null)
                TriggeredWaveform(f);
        }
        private List<float> samplesOverflowSink; 
    

        void ProcessWaveform(DataPacket data)
        {
            AcquisitionLength = 1+(int)(AcquisitionTime*Source.SampleRate);
            if (AcquisitionLength < 15) AcquisitionLength = 15;

            Pretrigger = (int) (PretriggerTime*Source.SampleRate) + AcquisitionLength / 2;

            lock (samplesOverflowSink)
            {
                samplesOverflowSink.AddRange(data.Samples);

                var pretriggerSampleDepth = (int) Source.SampleRate;

                if (samplesOverflowSink.Count > pretriggerSampleDepth)
                {
                    samplesOverflowSink.RemoveRange(0, samplesOverflowSink.Count - pretriggerSampleDepth);
                }

                var triggerInfo = new TriggerInfo(true, 0); // dummy

                while (triggerInfo.Triggered && samplesOverflowSink.Count >= AcquisitionLength + Pretrigger) // always keep 1 waveform extra in memory
                {
                    triggerInfo = Trigger.IsTriggered(samplesOverflowSink, 0);

                    // Skip the waveform (if we keep trigger) till we satisfy the pretrigger requirement.
                    while (triggerInfo.Triggered && triggerInfo.TriggerPoint < Pretrigger)
                    {
                        triggerInfo = Trigger.IsTriggered(samplesOverflowSink, triggerInfo.TriggerPoint );
                    }
                    // Only process when actually triggered.
                    if (!triggerInfo.Triggered) break;

                    // We take the waveform from the buffer at the triggerpoint, but also subtracting the pre-trigger sample depth.
                    var waveformStart = triggerInfo.TriggerPoint - Pretrigger;

                    // Get these samples, and put them into the waveform.
                    var waveformSamples = samplesOverflowSink.Skip(waveformStart).Take(AcquisitionLength).ToArray();
                    if (waveformSamples.Length != AcquisitionLength) break;
                    var waveform = new Waveform(1, AcquisitionLength);
                    var timestamp = -Pretrigger*data.TimeInterval;
                    foreach(var s in waveformSamples)
                    {
                        timestamp += data.TimeInterval;
                        waveform.Store(timestamp, new float[1] {s});
                    }

                    FireTriggeredWaveform(waveform);

                    samplesOverflowSink.RemoveRange(0, waveformStart+AcquisitionLength-1);

                }
            }

        }

        public void SetTrigger(ITrigger tr)
        {
            Trigger = Trigger;
        }
    }
}
