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

        public event AcquiredWaveform Waveform;

        public IDataSource Source { get; private set; }
        public ITrigger Trigger { get; private set; }

        public int AcquisitionLength { get; private set; }
        public int Pretrigger { get; private set; }

        public AcquisitionEngine(IDataSource source)
        {
            if (source is ArtificialStream)
                AcquisitionLength = (source as ArtificialStream).samplesPerSecond / (source as ArtificialStream).freq;
            else
                AcquisitionLength = 400;
            Pretrigger = AcquisitionLength/2;

            Source = source;
            Source.Connect(null);
            Source.Start();

            samplesOverflowSink = new List<float>();
            
            Trigger = new Edge(); // TODO: Temporary trigger

            Source.Data += Source_Data;
        }
        
        private void FireWaveform(Waveform f)
        {
            if (Waveform != null)
                Waveform(f);
        }
        private List<float> samplesOverflowSink; 
    

        void Source_Data(DataPacket data)
        {
            lock (samplesOverflowSink)
            {
                samplesOverflowSink.AddRange(data.Samples);

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

                    FireWaveform(waveform);

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
