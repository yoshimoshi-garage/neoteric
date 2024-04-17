using Meadow;
using Meadow.Foundation.Motors;
using Meadow.Hardware;
using System;
using static Meadow.Foundation.Motors.BidirectionalDcMotor;

namespace Neoteric.TransferCase;

public class GearSelectionMotor
{
    public event EventHandler<MotorState>? StateChanged = default!;

    private readonly BidirectionalDcMotor _motor;

    public GearSelectionMotor(IPin pinA, IPin pinB)
    {
        _motor = new BidirectionalDcMotor(
            pinA.CreateDigitalOutputPort(false),
            pinB.CreateDigitalOutputPort(false));

        _motor.StateChanged += OnMotorStateChanged;
    }

    private void OnMotorStateChanged(object sender, MotorState e)
    {
        StateChanged?.Invoke(this, e);
    }

    public bool IsMoving => _motor.State != MotorState.Stopped;

    public void BeginShiftUp()
    {
        _motor.StartCounterClockwise();
    }

    public void BeginShiftDown()
    {
        _motor.StartClockwise();
    }

    public void StopShift()
    {
        _motor.Stop();
    }
}
