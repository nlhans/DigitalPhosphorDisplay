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

        public List<ITrigger> TriggerSources { get; set; } 

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

            TriggerSources = new List<ITrigger>();
            TriggerSources.Add(new FreeRunning());
            TriggerSources.Add(new Edge());

            Trigger = new Edge(); // TODO: Temporary trigger

            Source = source;
            Source.Data += ProcessWaveform;
            Source.Data += Source_Data;
            Source.HighresVoltage += Source_HighresVoltage;

            Source.Connect(null);
            var dummyCfg = new NetStreamConfiguration();
            dummyCfg.AdcSpeed = 0;
            dummyCfg.AfeGain = 0;
            dummyCfg.UseFastAdc = false;

            Source.Configure(dummyCfg);
            

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
            var divs = 5;
            var minAcqLength = 4*divs + 1;

            AcquisitionLength = 1+(int)(AcquisitionTime*Source.SampleRate);
            if (AcquisitionLength < minAcqLength) AcquisitionLength = minAcqLength;

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
                var samplesAsArray = samplesOverflowSink.ToArray();
                var samplesTotal = samplesAsArray.Length;
                var walkingOffset = 0;
                var removeSize = 0;

                while (triggerInfo.Triggered && samplesTotal - walkingOffset >= AcquisitionLength + Pretrigger) // always keep 1 waveform extra in memory
                {
                    triggerInfo = Trigger.IsTriggered(samplesAsArray, walkingOffset);

                    // Skip the waveform (if we keep trigger) till we satisfy the pretrigger requirement.
                    while (triggerInfo.Triggered && triggerInfo.TriggerPoint-walkingOffset < Pretrigger)
                    {
                        triggerInfo = Trigger.IsTriggered(samplesAsArray, triggerInfo.TriggerPoint);
                    }
                    // Only process when actually triggered.
                    if (!triggerInfo.Triggered) break;

                    // We take the waveform from the buffer at the triggerpoint, but also subtracting the pre-trigger sample depth.
                    var waveformStart = triggerInfo.TriggerPoint - Pretrigger;

                    if (waveformStart + AcquisitionLength >= samplesTotal) break;

                    // Get these samples, and put them into the waveform.
                    var waveformSamples = new float[AcquisitionLength];
                    Array.Copy(samplesAsArray, waveformStart, waveformSamples, 0, AcquisitionLength);
                    //var waveformSamples = samplesOverflowSink.Skip(waveformStart).Take(AcquisitionLength).ToArray();
                    if (waveformSamples.Length != AcquisitionLength) break;
                    var waveform = new Waveform(1, AcquisitionLength);
                    var timestamp = -Pretrigger*data.TimeInterval;
                    foreach(var s in waveformSamples)
                    {
                        timestamp += data.TimeInterval;
                        waveform.Store(timestamp, new float[1] {s});
                    }

                    FireTriggeredWaveform(waveform);

                    var step = (waveformStart - walkingOffset) + AcquisitionLength;
                    if (step < minAcqLength)
                    {
                        step = minAcqLength;
                    }

                    if (step > samplesOverflowSink.Count)
                    {
                        walkingOffset = 0;
                        samplesOverflowSink.Clear();
                        break;
                    }
                    //samplesOverflowSink.RemoveRange(0, step);
                    walkingOffset += step;
                }

                if(walkingOffset>0)
                {
                    samplesOverflowSink.RemoveRange(0, walkingOffset);
                }
            }

        }

        public void SetTrigger(ITrigger tr)
        {
            Trigger = tr;
        }
    }
}
