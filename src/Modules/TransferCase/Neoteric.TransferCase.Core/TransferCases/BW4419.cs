using Meadow;
using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Neoteric.TransferCase;

/// <summary>
/// The Borg Warner BW4419 3-position transfer case
/// </summary>
public class BW4419 : ITransferCase
{
    public event EventHandler<TransferCasePosition>? GearChanged;

    private readonly Dictionary<int, TransferCasePosition> _positionTruthTable = new();
    private readonly GearSelectionMotor _motor;
    private readonly IDigitalInputPort _positionSwitch1;
    private readonly IDigitalInputPort _positionSwitch2;
    private readonly IDigitalInputPort _positionSwitch3;
    private readonly IDigitalInputPort _positionSwitch4;
    private readonly ISafetyInterlock? _safetyInterlock;

    public BW4419(
        GearSelectionMotor motor,
        IDigitalInputPort positionSwitch1,
        IDigitalInputPort positionSwitch2,
        IDigitalInputPort positionSwitch3,
        IDigitalInputPort positionSwitch4,
        ISafetyInterlock? safetyInterlock)
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
        _safetyInterlock = safetyInterlock;

        _ = VerifyCurrentGear();
    }

    public bool IsShifting => _motor.IsMoving;

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

        _motor.BeginShiftUp();
        while (CurrentGear == current || CurrentGear == TransferCasePosition.Unknown)
        {
            await Task.Delay(10);
        }
        Resolver.Log.Info($"{CurrentGear}");
        _motor.StopShift();
    }

    private async Task ShiftDown()
    {
        if (CurrentGear == TransferCasePosition.Low4)
        {
            // we're already in the lowest location.  Do nothing.
            return;
        }

        var current = CurrentGear;

        _motor.BeginShiftDown();
        while (CurrentGear == current || CurrentGear == TransferCasePosition.Unknown)
        {
            await Task.Delay(10);
        }
        Resolver.Log.Info($"{CurrentGear}");
        _motor.StopShift();
    }


    public async Task ShiftTo(TransferCasePosition position)
    {
        Resolver.Log.Info($"Request shift to {position}");

        if (_safetyInterlock != null && !_safetyInterlock.IsSafe)
        {
            Resolver.Log.Warn($"Interlock is not safe.  Shift request ignored.");
            return;
        }

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
