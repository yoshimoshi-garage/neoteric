using Meadow;
using Meadow.Hardware;
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
    private readonly IDigitalOutputPort? _hubLockEnable;
    private int _destinationGearIndex = -1;

    public abstract TransferCasePosition CurrentGear { get; }
    public abstract TransferCasePosition[] SupportedGears { get; }

    public bool IsShifting => _motor.IsMoving;

    protected GearSelectionMotor SelectionMotor => _motor;
    protected ISafetyInterlock? SafetyInterlock => _safetyInterlock;

    /// <summary>
    /// Override this method to provide data for the state machine on which direction to shift from current position to achieve a desired gear
    /// </summary>
    /// <param name="position">The gear desired</param>
    /// <returns>Whether the shift needs to be up or down to reach the gear.  Return Idle if unknown or unable to determine.</returns>
    protected virtual TransferCaseState GetDirectionTo(TransferCasePosition position) => TransferCaseState.Idle;

    protected TransferCaseBase(
        GearSelectionMotor selectionMotor,
        ISafetyInterlock? safetyInterlock,
        IDigitalOutputPort? hubLockEnable
    )
    {
        _motor = selectionMotor;
        _safetyInterlock = safetyInterlock;
        _hubLockEnable = hubLockEnable;
    }

    private int ConvertGearToIndex(TransferCasePosition position)
    {
        for (var i = 0; i < SupportedGears.Length; i++)
        {
            if (position == SupportedGears[i]) return i;
        }

        return -1;
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

    private int _lastCheckedIndex = -1;
    private int _lastKnownValidGearIndex = -1;
    private TransferCaseState _lastShiftDirection;

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
                else if (_destinationGearIndex == -1)
                {
                    // unknown destination (we're just starting up) so do nothing
                }
                else
                {
                    var currentGear = CurrentGear;
                    // convert the current to an index
                    var currentIndex = ConvertGearToIndex(currentGear);

                    if (currentIndex >= 0)
                    {
                        _lastKnownValidGearIndex = currentIndex;
                    }

                    // Resolver.Log.Info($"Current Gear: {currentGear} ({currentIndex}): destination {_destinationGearIndex}");

                    if (currentIndex == _destinationGearIndex)
                    {
                        // we're at the destination
                        // is this a state change?  if so, raise an event
                        if (State != TransferCaseState.Idle)
                        {
                            GearChanged?.Invoke(this, currentGear);
                        }

                        _motor.StopShift();
                        State = TransferCaseState.Idle;
                    }
                    else
                    {
                        if (currentIndex >= 0)
                        {// we're in a gear, but it's not the destination
                         // need to keep moving
                            if (currentIndex != _lastCheckedIndex)
                            {
                                GearChanged?.Invoke(this, currentGear);
                                State = TransferCaseState.Idle;
                            }
                        }

                        switch (currentGear)
                        {
                            case TransferCasePosition.BeforeLowest:
                                if (State == TransferCaseState.Idle)
                                {
                                    GearChanging?.Invoke(this, SupportedGears[_destinationGearIndex]);
                                }
                                State = _lastShiftDirection = TransferCaseState.ShiftingUp;
                                _motor.BeginShiftUp();
                                break;
                            case TransferCasePosition.AboveHighest:
                                if (State == TransferCaseState.Idle)
                                {
                                    GearChanging?.Invoke(this, SupportedGears[_destinationGearIndex]);
                                }
                                State = _lastShiftDirection = TransferCaseState.ShiftingDown;
                                _motor.BeginShiftDown();
                                break;
                            default:
                                if (_destinationGearIndex < _lastKnownValidGearIndex)
                                {
                                    if (State == TransferCaseState.Idle)
                                    {
                                        GearChanging?.Invoke(this, SupportedGears[_destinationGearIndex]);
                                    }
                                    State = _lastShiftDirection = TransferCaseState.ShiftingDown;
                                    _motor.BeginShiftDown();
                                }
                                else if (_destinationGearIndex > _lastKnownValidGearIndex)
                                {
                                    if (State == TransferCaseState.Idle)
                                    {
                                        GearChanging?.Invoke(this, SupportedGears[_destinationGearIndex]);
                                    }
                                    State = _lastShiftDirection = TransferCaseState.ShiftingUp;

                                    _motor.BeginShiftUp();
                                }
                                else if ((_destinationGearIndex == _lastKnownValidGearIndex) && currentGear == TransferCasePosition.BetweenGears)
                                {
                                    // we were in gear, but have "slipped" out
                                    // see if the transfer case knows which way to go
                                    var direction = GetDirectionTo(SupportedGears[_destinationGearIndex]);
                                    // if it doesn't, keep going in the last known direction to find a gear
                                    if (direction == TransferCaseState.Idle) direction = _lastShiftDirection;

                                    if (State == TransferCaseState.Idle)
                                    {
                                        GearChanging?.Invoke(this, SupportedGears[_destinationGearIndex]);
                                    }

                                    if (direction == TransferCaseState.ShiftingUp)
                                    {
                                        State = _lastShiftDirection = TransferCaseState.ShiftingUp;
                                        _motor.BeginShiftUp();
                                    }
                                    else
                                    {
                                        State = _lastShiftDirection = TransferCaseState.ShiftingDown;
                                        _motor.BeginShiftDown();
                                    }
                                }

                                break;
                        }
                    }

                    _lastCheckedIndex = currentIndex;
                }
            }
            catch (Exception ex)
            {
                Resolver.Log.Warn(ex.Message);
            }

            await Task.Delay(100);
        }
    }

    private async Task StateMachine2()
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
                    var currentIndex = ConvertGearToIndex(CurrentGear);
                    if (currentIndex >= 0) _lastKnownValidGearIndex = currentIndex;

                    if (currentIndex < 0 && _destinationGearIndex < 0)
                    {
                        // both are unknown - we're likely just starting up, so do nothing
                    }
                    else if (_destinationGearIndex < _lastKnownValidGearIndex)
                    {
                        if (_lastCheckedIndex != currentIndex)
                        {
                            GearChanging?.Invoke(this, SupportedGears[_destinationGearIndex]);
                        }

                        _motor.BeginShiftDown();
                        State = TransferCaseState.ShiftingDown;
                    }
                    else if (_destinationGearIndex > _lastKnownValidGearIndex)
                    {
                        if (_lastCheckedIndex != currentIndex)
                        {
                            GearChanging?.Invoke(this, SupportedGears[_destinationGearIndex]);
                        }

                        _motor.BeginShiftUp();
                        State = TransferCaseState.ShiftingUp;
                    }
                    else
                    {
                        if (_lastCheckedIndex != currentIndex)
                        {
                            GearChanged?.Invoke(this, SupportedGears[currentIndex]);
                        }

                        _motor.StopShift();
                        State = TransferCaseState.Idle;
                    }

                    _lastCheckedIndex = currentIndex;
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

        if (_hubLockEnable != null)
        {
            switch (position)
            {
                case TransferCasePosition.Low4:
                case TransferCasePosition.High4:
                    _hubLockEnable.State = true;
                    break;
                default:
                    _hubLockEnable.State = false;
                    break;
            }
        }

        _destinationGearIndex = destinationIndex;
    }
}
