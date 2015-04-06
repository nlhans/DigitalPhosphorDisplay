namespace PhosphorDisplay.Triggers
{
    public class TriggerInfo
    {
        public bool Triggered { get; private set; }
        public int TriggerPoint { get; private set; }
        public float TriggerDuty { get; private set; }

        public TriggerInfo(bool triggered, int triggerPoint) : this(triggered, triggerPoint, 0)
        {
            
        }

        public TriggerInfo(bool triggered, int triggerPoint, float triggerDuty)
        {
            Triggered = triggered;
            TriggerPoint = triggerPoint;
            TriggerDuty = triggerDuty;
        }
    }
}