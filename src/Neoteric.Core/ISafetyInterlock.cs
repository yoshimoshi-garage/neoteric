using System;

namespace Neoteric;

public interface ISafetyInterlock
{
    event EventHandler<bool>? Changed;
    public bool IsSafe { get; }
}
