using System;

namespace PhosphorDisplay.Widget
{
    public class Waveform
    {        
        public int Samples;
        public int Channels;
        public float[][] Data;
        public float[] Horizontal;
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
            if (Samples >= MemDepth)
                return;
            Horizontal[Samples] = time;
            for (int i = 0; i < Channels; i++)
                Data[i][Samples] = ch[i];
            Samples++;
        }
    }
}

