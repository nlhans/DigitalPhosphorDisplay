using System.Collections.Generic;

namespace PhosphorDisplay.Triggers
{
    public interface ITrigger
    {
        string Name { get; }

        TriggerInfo IsTriggered(float[] samples, int start);
        void SetOption<T>(TriggerOption option, T value);
    }
}