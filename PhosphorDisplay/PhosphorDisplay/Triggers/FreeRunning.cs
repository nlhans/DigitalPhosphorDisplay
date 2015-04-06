namespace PhosphorDisplay.Triggers
{
    public class FreeRunning : ITrigger
    {
        #region Implementation of ITrigger

        public string Name { get { return "Free running"; } }
        public TriggerInfo IsTriggered(float[] samples, int start)
        {
            return new TriggerInfo(true, start + 1);
        }

        public void SetOption<T>(TriggerOption option, T value)
        {
            // Do nothing with it.
        }

        #endregion
    }
}