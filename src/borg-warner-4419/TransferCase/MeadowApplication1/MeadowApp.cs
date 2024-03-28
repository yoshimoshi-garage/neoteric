using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Motors;
using Meadow.Hardware;
using Meadow.Simulation;
using Meadow.Units;
using System.Threading.Tasks;

namespace Neoteric;

public partial class MeadowApp : App<F7FeatherV1>
{
    private BidirectionalDcMotor _motor;
    private BorgWarner4419 _tcase;
    private GearSelectorSwitchSwitch _switch;
    private NotificationService _notificationService;
    private IAnalogInputPort _analog;
    private IDigitalInputPort _sw1;
    private IDigitalInputPort _sw2;
    private IDigitalInputPort _sw3;
    private IDigitalInputPort _sw4;

    private SimulatedIOExpander? _expander;

    public override Task Initialize()
    {
        Resolver.Log.Info("Initialize...");

        IDigitalOutputPort motorA;
        IDigitalOutputPort motorB;

        Resolver.Log.Info($" LED");

        var led = new RgbLed(
            Device.Pins.OnboardLedRed,
            Device.Pins.OnboardLedGreen,
            Device.Pins.OnboardLedBlue);

        Resolver.Log.Info($" notification service");
        _notificationService = new NotificationService(led);
        Resolver.Services.Add(_notificationService);

        _notificationService.SetState(NotificationService.SystemState.Startup);

        Resolver.Log.Info($" Analog");
        if (SimulateGearSelector)
        {
            Resolver.Log.Info($"USING SIMULATED ANALOG");
            _expander = new SimulatedIOExpander(1);
            _analog = _expander.GetPin(0).CreateAnalogInputPort(1);
            (_analog as SimulatedAnalogInputPort).Voltage = new Voltage(1.75, Voltage.UnitType.Volts);
        }
        else
        {
            Resolver.Log.Info($"Using A00 for switch");
            _analog = Device.Pins.A00.CreateAnalogInputPort(1);
        }

        Resolver.Log.Info($" Gear selector");
        _switch = new GearSelectorSwitchSwitch(_analog);

        _switch.SwitchPositionChanged += OnSwitchPositionChanged;

        if (SimulateMotor)
        {
            motorA = Device.Pins.OnboardLedRed.CreateDigitalOutputPort(false);
            motorB = Device.Pins.OnboardLedGreen.CreateDigitalOutputPort(false);
        }
        else
        {
            motorA = Device.Pins.D01.CreateDigitalOutputPort(false);
            motorB = Device.Pins.D00.CreateDigitalOutputPort(false);
        }

        Resolver.Log.Info($" Motor");
        _motor = new BidirectionalDcMotor(motorA, motorB);
        _motor.StateChanged += OnMotorStateChanged;

        Resolver.Log.Info($" Transfer case");
        _sw1 = Device.Pins.D13.CreateDigitalInputPort(ResistorMode.InternalPullDown);
        _sw2 = Device.Pins.D12.CreateDigitalInputPort(ResistorMode.InternalPullDown);
        _sw3 = Device.Pins.D11.CreateDigitalInputPort(ResistorMode.InternalPullDown);
        _sw4 = Device.Pins.D10.CreateDigitalInputPort(ResistorMode.InternalPullDown);

        _tcase = new BorgWarner4419(
            _motor,
            _sw1, _sw2, _sw3, _sw4);

        _tcase.GearChanged += OnTransferCaseGearChanged;

        return base.Initialize();
    }

    private void OnTransferCaseGearChanged(object sender, TransferCasePosition e)
    {
        Resolver.Log.Info($"In gear: {e}");
    }

    private void OnMotorStateChanged(object sender, BidirectionalDcMotor.MotorState e)
    {
        Resolver.Log.Info($"Motor {e}");

        switch (e)
        {
            case BidirectionalDcMotor.MotorState.Stopped:
                _notificationService.SetState(NotificationService.SystemState.Idle);
                break;
            case BidirectionalDcMotor.MotorState.RunningClockwise:
                _notificationService.SetState(NotificationService.SystemState.DrivingMotorCW);
                break;
            case BidirectionalDcMotor.MotorState.RunningCounterclockwise:
                _notificationService.SetState(NotificationService.SystemState.DrivingMotorCCW);
                break;

        }
    }

    private void OnSwitchPositionChanged(object sender, TransferCasePosition position)
    {
        Resolver.Log.Info($"OnSwitchPositionChanged");

        switch (position)
        {
            case TransferCasePosition.Low4: // 4L
                _ = _tcase.ShiftTo(TransferCasePosition.Low4);
                _notificationService.ClearError();
                break;
            case TransferCasePosition.High4: // 4H
                _ = _tcase.ShiftTo(TransferCasePosition.High4);
                _notificationService.ClearError();
                break;
            case TransferCasePosition.High2:
                _ = _tcase.ShiftTo(TransferCasePosition.High2);
                _notificationService.ClearError();
                break; // 2H
            default:
                _notificationService.SetError(NotificationService.ErrorCodes.SelectionSwitchNotDetected);
                break;
        }
    }

    private async Task ReleaseMode()
    {
        while (true)
        {
            if (!_tcase.IsShifting)
            {
                // verify the selected gear is the gear we're in
                if (_tcase.CurrentGear != _switch.CurrentSwitchPosition)
                {
                    await _tcase.ShiftTo(_switch.CurrentSwitchPosition);
                }
            }

            await Task.Delay(1000);
        }
    }

    public override async Task Run()
    {
        Resolver.Log.Info($"Current gear is {_tcase.CurrentGear}");
        Resolver.Log.Info($"Gear selector is in {_switch.CurrentSwitchPosition}");

        await ReleaseMode();
    }

}