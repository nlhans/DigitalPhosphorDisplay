using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhosphorDisplay.Triggers
{
    public class Edge : ITrigger
    {
        public Edge()
        {
            TriggerLevelL = 100.0f/1000000;
            TriggerLevelH = 159.0f/1000000;
            HistoryEffect = 45;

            RisingEdge = true;
            FallingEdge = false;

        }

        public bool RisingEdge { get; private set; }
        public bool FallingEdge { get; private set; }
        public float TriggerLevelL { get; private set; }
        public float TriggerLevelH { get; private set; }
        public int HistoryEffect { get; private set; }

        #region Implementation of ITrigger

        public string Name { get { return "Edge"; } }
        public TriggerInfo IsTriggered(float[] samples, int start)
        {
            if (start > samples.Length - 1) return new TriggerInfo(false, -1);
            if (start < 0) return new TriggerInfo(false, -1);
            float lastSample = samples[start];

            for(int k = start+HistoryEffect; k < samples.Length; k++)
            {
                if (samples[k] >= TriggerLevelH && RisingEdge)
                {
                    for (int m = -HistoryEffect; m < 0; m++)
                    {
                        // Rising edge
                        if (samples[k + m] < TriggerLevelL)
                        {
                            var duty = 0.0f;
                            // Search for the latest that is just under..
                            for (int m2 = 0; m2 >= m; m2--)
                            {
                                if (samples[k + m2] <= TriggerLevelL)
                                {
                                    duty = (TriggerLevelL - samples[k + m2]) / (samples[k + m2] - samples[k]);
                                    break;
                                }
                            }

                            return new TriggerInfo(true, k, duty);
                        }
                    }
                }
                if (samples[k] < TriggerLevelL && FallingEdge)
                {
                    for (int m = -HistoryEffect; m < 0; m++)
                    {
                        // Falling edge
                        if (samples[k + m] >= TriggerLevelH)
                        {
                            return new TriggerInfo(true, k);
                        }
                    }
                }
                lastSample = samples[k];
            }

            return new TriggerInfo(false, -1);
        }

        public void SetOption<T>(TriggerOption option, T value)
        {
            switch (option)
            {
                case TriggerOption.Edge:
                    var edges = (TriggerEdge)Enum.Parse(typeof(TriggerEdge), value.ToString());
                    RisingEdge = edges == TriggerEdge.Both || edges == TriggerEdge.Rising;
                    RisingEdge = edges == TriggerEdge.Both || edges == TriggerEdge.Rising;
                    break;

                case TriggerOption.LevelL:
                    TriggerLevelL = Convert.ToSingle(value);
                    break;
                case TriggerOption.LevelH:
                    TriggerLevelH = Convert.ToSingle(value);
                    break;

                case TriggerOption.EdgeWidth:
                    HistoryEffect = Convert.ToInt32(value);
                    break;
            }
        }

        #endregion
    }
}
