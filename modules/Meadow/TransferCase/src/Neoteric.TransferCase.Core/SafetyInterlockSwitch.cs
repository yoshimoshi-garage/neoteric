using Meadow;
using Meadow.Hardware;
using System;

namespace Neoteric.TransferCase;

public class SafetyInterlockSwitch : ISafetyInterlock
{
    public event EventHandler<bool>? Changed;

    private readonly IDigitalInterruptPort _inputPort;
    private readonly bool _safeState;

    public bool IsSafe => _inputPort.State == _safeState;

    public SafetyInterlockSwitch(IPin input, bool safeState = false)
    {
        _inputPort = input.CreateDigitalInterruptPort(InterruptMode.EdgeBoth, ResistorMode.ExternalPullUp, TimeSpan.FromMilliseconds(500));
        _inputPort.Changed += OnInputPortChanged;
        _safeState = safeState;
    }

    private void OnInputPortChanged(object sender, DigitalPortResult e)
    {
        Changed?.Invoke(this, IsSafe);
    }
}
