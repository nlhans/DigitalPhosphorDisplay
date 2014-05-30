using System.Collections.Generic;

namespace PhosphorDisplay.Triggers
{
    public interface ITrigger
    {
        string Name { get; }

        TriggerInfo IsTriggered(IEnumerable<float> samples, int start);

    }
}