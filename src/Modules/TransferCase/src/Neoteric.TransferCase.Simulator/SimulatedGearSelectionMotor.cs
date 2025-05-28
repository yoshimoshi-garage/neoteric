using Meadow;
using Meadow.Foundation.Motors;
using Neoteric.TransferCase;

namespace Neaoteric.TransferCase.Simulator;

public class SimulatedGearSelectionMotor : IGearSelectionMotor
{
    private BidirectionalDcMotor.MotorState _state;
    private BidirectionalDcMotor.MotorState? _lastState;

    public bool IsMoving => _state != BidirectionalDcMotor.MotorState.Stopped;

    public event EventHandler<BidirectionalDcMotor.MotorState>? StateChanged;

    private BidirectionalDcMotor.MotorState State
    {
        get => _state;
        set
        {
            if (_lastState != value)
            {
                Resolver.Log.Info($"GearSelectionMotor changing to: {value}");
                _lastState = value;
            }
            _state = value;
            StateChanged?.Invoke(this, _state);
        }
    }

    public void BeginShiftDown()
    {
        State = BidirectionalDcMotor.MotorState.RunningClockwise;
    }

    public void BeginShiftUp()
    {
        State = BidirectionalDcMotor.MotorState.RunningCounterclockwise;
    }

    public void StopShift()
    {
        State = BidirectionalDcMotor.MotorState.Stopped;
    }
}
