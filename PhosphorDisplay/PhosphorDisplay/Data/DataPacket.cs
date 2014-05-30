namespace PhosphorDisplay.Data
{
    public class DataPacket
    {
        public DataType Type;

        public float[] Samples;
        public int Count;
        public float TimeInterval;

        public DataPacket(float[] samples, DataType type, float timeInterval)
        {
            Type = type;
            Count = samples.Length;
            Samples = samples;
            TimeInterval = timeInterval;
        }
    }
}