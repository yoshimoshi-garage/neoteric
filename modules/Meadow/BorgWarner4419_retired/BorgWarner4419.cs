using Meadow;
using Meadow.Foundation.Motors;
using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Neoteric;

public class BorgWarner4419
{
    public event EventHandler<TransferCasePosition> GearChanged = default!;

    private Dictionary<int, TransferCasePosition> _positionTruthTable = new();
    private BidirectionalDcMotor _motor;
    private IDigitalInputPort _positionSwitch1;
    private IDigitalInputPort _positionSwitch2;
    private IDigitalInputPort _positionSwitch3;
    private IDigitalInputPort _positionSwitch4;

    public BorgWarner4419(
        BidirectionalDcMotor motor,
        IDigitalInputPort positionSwitch1,
        IDigitalInputPort positionSwitch2,
        IDigitalInputPort positionSwitch3,
        IDigitalInputPort positionSwitch4)
    {
        _positionTruthTable.Add((1 << 0) | (1 << 2), TransferCasePosition.High2);     // 1 + 3
        _positionTruthTable.Add((1 << 1) | (1 << 2) | (1 << 3), TransferCasePosition.High4); // 2 + 3 + 4
        _positionTruthTable.Add((1 << 2) | (1 << 3), TransferCasePosition.Neutral);   // 3 + 4
        _positionTruthTable.Add((1 << 0) | (1 << 1) | (1 << 3), TransferCasePosition.Low4);  // 1 + 2 + 4

        _motor = motor;
        _positionSwitch1 = positionSwitch1;
        _positionSwitch2 = positionSwitch2;
        _positionSwitch3 = positionSwitch3;
        _positionSwitch4 = positionSwitch4;

        _ = VerifyCurrentGear();
    }

    public bool IsShifting => _motor.State != BidirectionalDcMotor.MotorState.Stopped;

    public TransferCasePosition CurrentGear
    {
        get
        {
            // switches combine to give you a gear - it's not 1 switch == 1 gear
            var position = 0;

            if (_positionSwitch1.State) position |= (1 << 0);
            if (_positionSwitch2.State) position |= (1 << 1);
            if (_positionSwitch3.State) position |= (1 << 2);
            if (_positionSwitch4.State) position |= (1 << 3);

            if (_positionTruthTable.ContainsKey(position))
            {
                return _positionTruthTable[position];
            }
            else
            {
                return TransferCasePosition.Unknown;
            }
        }
    }

    private async Task<TransferCasePosition> VerifyCurrentGear()
    {
        // in still unknown shift down to find a gear
        if (CurrentGear == TransferCasePosition.Unknown)
        {
            Resolver.Log.Info("Not in gear, searching down...");
            await ShiftDown();
        }
        else
        {
            Resolver.Log.Info($"Verified in {CurrentGear}");
        }

        return CurrentGear;
    }

    private async Task ShiftUp()
    {
        if (CurrentGear == TransferCasePosition.High2)
        {
            // we're already in the highest location.  Do nothing.
            return;
        }

        var current = CurrentGear;

        _motor.StartCounterClockwise();
        while (CurrentGear == current || CurrentGear == TransferCasePosition.Unknown)
        {
            await Task.Delay(10);
        }
        Resolver.Log.Info($"{CurrentGear}");
        _motor.Stop();
    }

    private async Task ShiftDown()
    {
        if (CurrentGear == TransferCasePosition.Low4)
        {
            // we're already in the lowest location.  Do nothing.
            return;
        }

        var current = CurrentGear;

        _motor.StartClockwise();
        while (CurrentGear == current || CurrentGear == TransferCasePosition.Unknown)
        {
            await Task.Delay(10);
        }
        Resolver.Log.Info($"{CurrentGear}");
        _motor.Stop();
    }


    public async Task ShiftTo(TransferCasePosition position)
    {
        Resolver.Log.Info($"Request shift to {position}");

        // verify current gear
        await VerifyCurrentGear();

        if (position == TransferCasePosition.Unknown)
        {
            // nonsense.  we won't shift to an unknown
            return;
        }

        if (position == CurrentGear) return;

        // if the desired gear is above us, move up
        if (position > CurrentGear)
        {
            while (CurrentGear != position)
            {
                await ShiftUp();
            }
        }
        // else if desired gear is below us shift down
        else if (position < CurrentGear)
        {
            while (CurrentGear != position)
            {
                await ShiftDown();
            }
        }

        Resolver.Log.Info($"Completed shift to {position}");
    }
}