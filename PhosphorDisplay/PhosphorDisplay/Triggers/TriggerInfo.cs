namespace PhosphorDisplay.Triggers
{
    public class TriggerInfo
    {
        public bool Triggered { get; private set; }
        public int TriggerPoint { get; private set; }

        public TriggerInfo(bool triggered, int triggerPoint)
        {
            Triggered = triggered;
            TriggerPoint = triggerPoint;
        }
    }
}