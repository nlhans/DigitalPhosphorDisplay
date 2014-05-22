using System;

namespace PhosphorDisplay
{
    public class Waveform
    {
        public float[][] Data;
        public float[] Horizontal;
        public float TriggerTime;
        public int Samples;
        public int Channels;
        public float SampleTime = -1f;
        public float LastSampleTime = 0.0f;
        private int MemDepth;

        public Waveform(int channels, int memDepth)
        {
            Channels = channels;
            Data = new float[channels][];
            for (int ch = 0; ch < channels; ch++)
                Data[ch] = new float[memDepth];

            MemDepth = memDepth;
            Horizontal = new float[memDepth];
        }

        public void Store(float time, float[] ch)
        {
            if (Samples >= MemDepth) return;
            Horizontal[Samples] = time;
            for (int i = 0; i < Channels; i++)
                Data[i][Samples] = ch[i];
            Samples++;


            if (SampleTime < 0)
                SampleTime = time-LastSampleTime;
            else
                SampleTime = (time-LastSampleTime)/2.0f + SampleTime/2.0f;

            LastSampleTime = time;
        }

        public void Process(int requiredWidth)
        {
            if (requiredWidth >= Samples*2)
            {
                // We're going to interpolate signals
                var scaleFactor = requiredWidth/Samples;
                var newMemDepth = scaleFactor*Samples;
                var NewData = new float[Channels][];
                var NewTime = new float[newMemDepth];
                for (int ch = 0; ch < Channels; ch++)
                    NewData[ch] = new float[newMemDepth];

                for (int ch = 0; ch < Channels; ch++)
                {
                    var lastInpl = 0.0f;
                    var nextInpl = 0.0f;
                    var lastTime = 0.0f;
                    var nextTime = 0.0f;
                    for (int i = 0; i < newMemDepth; i++)
                    {
                        var val = 0.0f;
                        var part = i%scaleFactor*1.0f;
                        if (i%scaleFactor == 0)
                        {
                            lastInpl = Data[ch][i/scaleFactor];
                            nextInpl = i/scaleFactor + 1 == Samples ? lastInpl : Data[ch][i/scaleFactor + 1];
                            lastTime = Horizontal[i/scaleFactor];
                            nextTime = i/scaleFactor + 1 == Samples ? lastTime : Horizontal[i/scaleFactor + 1];
                        }
                        var time = nextTime * (part / scaleFactor) + lastTime * (1 - part / scaleFactor);
                        
                        // Linear:
                        val = nextInpl*(part/scaleFactor) + lastInpl*(1 - part/scaleFactor);

                        // SINC:
                        /* INCREDIBLY slow
                        var xt = 0.0f;
                        for (var n = 0; n < Samples; n++)
                        {
                            float x = time - n * SampleTime;
                            x /= SampleTime;

                            if (x == 0) x = 1;
                            else x = (float)(Math.Sin(Math.PI * x) / (Math.PI * x));
                            xt += Data[ch][n] * x;
                        }
                        val = xt;
                       */
                        NewData[ch][i] = val;
                        NewTime[i] = time;
                    }
                }

                Samples = newMemDepth;
                Horizontal = NewTime;
                Data = NewData;
            }

        }
    }
}