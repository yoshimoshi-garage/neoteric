using Meadow;
using Meadow.Hardware;

namespace Neoteric.TransferCase;

public class SafetyInterlockSwitch : ISafetyInterlock
{
    private readonly IDigitalInputPort _inputPort;
    private readonly bool _safeState;

    public bool IsSafe => _inputPort.State == _safeState;

    public SafetyInterlockSwitch(IPin input, bool safeState = false)
    {
        _inputPort = input.CreateDigitalInputPort(ResistorMode.ExternalPullUp);
        _safeState = safeState;
    }
}
