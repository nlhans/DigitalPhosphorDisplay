﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhosphorDisplay.Triggers
{

    public class FreeRunning : ITrigger
    {
        #region Implementation of ITrigger

        public string Name { get { return "Free running"; } }
        public TriggerInfo IsTriggered(IEnumerable<float> samples, int start)
        {
            return new TriggerInfo(true, start + 1);
        }

        #endregion
    }

    public class Edge : ITrigger
    {
        public Edge()
        {
            TriggerLevelL = 1.0f/1000;
            TriggerLevelH = 5.0f/1000;
            HistoryEffect = 6;

            RisingEdge = false;
            FallingEdge = true;

        }

        public bool RisingEdge { get; private set; }
        public bool FallingEdge { get; private set; }
        public float TriggerLevelL { get; private set; }
        public float TriggerLevelH { get; private set; }
        public int HistoryEffect { get; private set; }

        #region Implementation of ITrigger

        public string Name { get { return "Edge"; } }
        public TriggerInfo IsTriggered(IEnumerable<float> s, int start)
        {
            TriggerLevelL = -5.5f / 1000;
            TriggerLevelH = 5.5f / 1000;

            RisingEdge = true;
            FallingEdge = false;
            //return new TriggerInfo(true, start+1);

            if (start > s.Count() - 1) return new TriggerInfo(false, -1);
            var samples = s.Take(1000000).ToArray();

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
                            return new TriggerInfo(true, k);
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

        #endregion
    }
}
