using Meadow;
using System;
using System.Threading.Tasks;

namespace Neoteric.TransferCase;

public abstract class TransferCaseBase : ITransferCase
{
    public event EventHandler<TransferCasePosition>? GearChanged;

    private readonly GearSelectionMotor _motor;
    private readonly ISafetyInterlock? _safetyInterlock;

    public abstract TransferCasePosition CurrentGear { get; }

    public bool IsShifting => _motor.IsMoving;

    protected GearSelectionMotor SelectionMotor => _motor;
    protected ISafetyInterlock? SafetyInterlock => _safetyInterlock;

    protected TransferCaseBase(GearSelectionMotor selectionMotor, ISafetyInterlock? safetyInterlock)
    {
        _motor = selectionMotor;
        _safetyInterlock = safetyInterlock;
    }

    protected async Task<TransferCasePosition> VerifyCurrentGear()
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
        GearChanged?.Invoke(this, position);

    }
}
