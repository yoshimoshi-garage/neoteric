using Meadow;
using System;
using System.Threading.Tasks;

namespace Neoteric.TransferCase;

public enum TransferCaseState
{
    Idle,
    ShiftingUp,
    ShiftingDown
}

public abstract class TransferCaseBase : ITransferCase
{
    public event EventHandler<TransferCasePosition>? GearChanging;
    public event EventHandler<TransferCasePosition>? GearChanged;

    private readonly GearSelectionMotor _motor;
    private readonly ISafetyInterlock? _safetyInterlock;
    private int _destinationGearIndex = -1;

    public abstract TransferCasePosition CurrentGear { get; }
    public abstract TransferCasePosition[] SupportedGears { get; }

    public bool IsShifting => _motor.IsMoving;

    protected GearSelectionMotor SelectionMotor => _motor;
    protected ISafetyInterlock? SafetyInterlock => _safetyInterlock;

    protected TransferCaseBase(GearSelectionMotor selectionMotor, ISafetyInterlock? safetyInterlock)
    {
        _motor = selectionMotor;
        _safetyInterlock = safetyInterlock;
    }

    private int CurrentGearIndex
    {
        get
        {
            if (CurrentGear == TransferCasePosition.Unknown) return -1;

            for (var i = 0; i < SupportedGears.Length; i++)
            {
                if (CurrentGear == SupportedGears[i]) return i;
            }

            return -1;
        }
    }

    private TransferCaseState _state;
    public TransferCaseState State
    {
        get => _state;
        private set
        {
            if (_state == value) return;
            _state = value;
            Resolver.Log.Info($"New state: {_state.ToString()}");
        }
    }

    public void StartControlLoop()
    {
        Task.Factory.StartNew(StateMachine, TaskCreationOptions.LongRunning);
    }

    private async Task StateMachine()
    {
        while (true)
        {
            try
            {
                if (_safetyInterlock != null && !_safetyInterlock.IsSafe)
                {
                    _motor.StopShift();
                    State = TransferCaseState.Idle;
                }
                else
                {
                    var currentIndex = CurrentGearIndex;

                    if (CurrentGear == TransferCasePosition.Unknown && _destinationGearIndex < 0)
                    {
                        // both are unknown - we're likely just starting up, so do nothing
                    }
                    else if (_destinationGearIndex < currentIndex)
                    {
                        _motor.BeginShiftDown();
                        State = TransferCaseState.ShiftingDown;
                    }
                    else if (_destinationGearIndex > currentIndex)
                    {
                        _motor.BeginShiftUp();
                        State = TransferCaseState.ShiftingUp;
                    }
                    else
                    {
                        _motor.StopShift();
                        State = TransferCaseState.Idle;
                    }
                }
            }
            catch (Exception ex)
            {
                Resolver.Log.Warn(ex.Message);
            }

            await Task.Delay(100);
        }
    }

    public void RequestShiftTo(TransferCasePosition position)
    {
        if (_safetyInterlock != null && !_safetyInterlock.IsSafe)
        {
            Resolver.Log.Warn($"Interlock is not safe.  Shift request ignored.");
        }

        var destinationIndex = -1;
        for (var i = 0; i < SupportedGears.Length; i++)
        {
            if (position == SupportedGears[i]) destinationIndex = i;
        }

        if (destinationIndex < 0)
        {
            // nonsense.  we won't shift to an unknown
            return;
        }

        _destinationGearIndex = destinationIndex;
    }
}
