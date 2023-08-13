using Meadow;
using Meadow.Foundation.Leds;
using System.Threading;

namespace Neoteric;

public class NotificationService
{
    private readonly RgbLed _led;
    private SystemState _state;
    private AutoResetEvent _stateEvent = new AutoResetEvent(false);
    private ErrorCodes _currentError = ErrorCodes.None;

    public enum SystemState
    {
        Startup,
        Idle,
        DrivingMotorCW,
        DrivingMotorCCW,
        GearSelectionRequested,
        GearChangeDetected
    }

    public enum ErrorCodes
    {
        None = 0,
        SelectionSwitchNotDetected = 1,
    }

    public NotificationService(RgbLed led)
    {
        _led = led;

        new Thread(StateMonitorProc).Start();
    }

    public void ClearError()
    {
        _currentError = ErrorCodes.None;
        _stateEvent.Set();
    }

    public void SetError(ErrorCodes error)
    {
        Resolver.Log.Info($"ERROR: {error}");
        _currentError = error;
        _stateEvent.Set();
    }

    public void SetState(SystemState state, int? data = null)
    {
        Resolver.Log.Info($"New State: {state} {(data.HasValue ? data.Value.ToString() : string.Empty)}");

        _state = state;
        _stateEvent.Set();
    }

    private void StateMonitorProc(object o)
    {
        while (true)
        {
            if (_stateEvent.WaitOne(1000))
            {
                if (_currentError == ErrorCodes.None)
                {
                    switch (_state)
                    {
                        case SystemState.Startup:
                            _led.SetColor(RgbLedColors.Blue);
                            break;
                        case SystemState.DrivingMotorCW:
                            _led.SetColor(RgbLedColors.Cyan);
                            break;
                        case SystemState.DrivingMotorCCW:
                            _led.SetColor(RgbLedColors.Magenta);
                            break;
                        case SystemState.Idle:
                            _led.SetColor(RgbLedColors.Green);
                            break;
                    }
                }
                else
                {
                    _led.SetColor(RgbLedColors.Red);
                }
            }
        }
    }
}

