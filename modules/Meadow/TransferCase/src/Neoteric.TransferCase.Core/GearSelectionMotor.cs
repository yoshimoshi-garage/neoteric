using Meadow;
using Meadow.Foundation.Motors;
using Meadow.Hardware;
using System;
using System.Threading;
using static Meadow.Foundation.Motors.BidirectionalDcMotor;

namespace Neoteric.TransferCase;

public class GearSelectionMotor : IGearSelectionMotor
{
    public event EventHandler<MotorState>? StateChanged = default!;

    private readonly BidirectionalDcMotor _motor;

    private IDigitalOutputPort? _lockRelease;
    private TimeSpan _lockReleaseDelay;

    public GearSelectionMotor(IPin pinA, IPin pinB, IPin? lockRelease, TimeSpan? releaseDelay)
    {
        _motor = new BidirectionalDcMotor(
            pinA.CreateDigitalOutputPort(false),
            pinB.CreateDigitalOutputPort(false));

        if (lockRelease != null)
        {
            _lockRelease = lockRelease.CreateDigitalOutputPort(false);
        }
        _lockReleaseDelay = releaseDelay ?? TimeSpan.Zero;

        _motor.StateChanged += OnMotorStateChanged;
    }

    private void OnMotorStateChanged(object sender, MotorState e)
    {
        StateChanged?.Invoke(this, e);
    }

    public bool IsMoving => _motor.State != MotorState.Stopped;

    public void BeginShiftUp()
    {
        if (_lockRelease != null)
        {
            _lockRelease.State = true;
            Thread.Sleep(_lockReleaseDelay);
        }
        _motor.StartCounterClockwise();
    }

    public void BeginShiftDown()
    {
        if (_lockRelease != null)
        {
            _lockRelease.State = true;
            Thread.Sleep(_lockReleaseDelay);
        }
        _motor.StartClockwise();
    }

    public void StopShift()
    {
        _motor.Stop();
        if (_lockRelease != null)
        {
            Thread.Sleep(_lockReleaseDelay);
            _lockRelease.State = false;
        }
    }
}
