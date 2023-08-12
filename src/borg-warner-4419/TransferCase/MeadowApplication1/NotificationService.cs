using Meadow;
using Meadow.Foundation.Leds;
using System.Threading;

namespace Neoteric;

public class NotificationService
{
    private readonly RgbLed _led;
    private SystemState _state;
    private AutoResetEvent _stateEvent = new AutoResetEvent(false);

    public enum SystemState
    {
        Startup,
        Idle,
        Error,
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

    public void SetState(SystemState state, int? data = null)
    {
        Resolver.Log.Info($"State: {state} {(data.HasValue ? data.Value.ToString() : string.Empty)}");

        _state = state;
        _stateEvent.Set();
    }

    private void StateMonitorProc(object o)
    {

        while (true)
        {
            if (_stateEvent.WaitOne(1000))
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
                    case SystemState.Error:
                        _led.SetColor(RgbLedColors.Red);
                        break;
                    case SystemState.Idle:
                        _led.SetColor(RgbLedColors.Green);
                        break;
                }
            }
        }
    }
}

