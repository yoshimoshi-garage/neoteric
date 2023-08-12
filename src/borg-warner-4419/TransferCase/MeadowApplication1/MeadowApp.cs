using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Motors;
using Meadow.Hardware;
using Meadow.Simulation;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Neoteric;

// Change F7FeatherV2 to F7FeatherV1 for V1.x boards
public class MeadowApp : App<F7FeatherV2>
{
    private BidirectionalDcMotor _motor;
    private BorgWarner4419 _tcase;
    private GearSelectorSwitchSwitch _switch;
    private NotificationService _notificationService;

    public bool SimulateMotor { get; set; } = false;
    public bool SimulateGearSelector { get; set; } = true;

    private IAnalogInputPort _analog;

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
            _expander = new SimulatedIOExpander(1);
            _analog = _expander.GetPin(0).CreateAnalogInputPort(1);
            (_analog as SimulatedAnalogInputPort).Voltage = new Voltage(1.75, Voltage.UnitType.Volts);
        }
        else
        {
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
            motorA = Device.Pins.D00.CreateDigitalOutputPort(false);
            motorB = Device.Pins.D01.CreateDigitalOutputPort(false);
        }

        Resolver.Log.Info($" Motor");
        _motor = new BidirectionalDcMotor(motorA, motorB);
        _motor.StateChanged += OnMotorStateChanged;

        Resolver.Log.Info($" Transfer case");
        _tcase = new BorgWarner4419(
            _motor,
            Device.Pins.D11.CreateDigitalInterruptPort(
                InterruptMode.EdgeRising,
                ResistorMode.InternalPullDown,
                TimeSpan.FromMilliseconds(100),
                TimeSpan.FromMilliseconds(1)),
            Device.Pins.D12.CreateDigitalInterruptPort(
                InterruptMode.EdgeRising,
                ResistorMode.InternalPullDown,
                TimeSpan.FromMilliseconds(100),
                TimeSpan.FromMilliseconds(1)),
            Device.Pins.D13.CreateDigitalInterruptPort(
                InterruptMode.EdgeRising,
                ResistorMode.InternalPullDown,
                TimeSpan.FromMilliseconds(100),
                TimeSpan.FromMilliseconds(1)),
            Device.Pins.D10.CreateDigitalInterruptPort(
                InterruptMode.EdgeRising,
                ResistorMode.InternalPullDown,
                TimeSpan.FromMilliseconds(100),
                TimeSpan.FromMilliseconds(1)));

        _tcase.GearChanged += OnTransferCaseGearChanged;

        // TODO: make sure transfer case is in gear matching switch

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
                break;
            case TransferCasePosition.High4: // 4H
                _ = _tcase.ShiftTo(TransferCasePosition.High4);
                break;
            case TransferCasePosition.High2:
                _ = _tcase.ShiftTo(TransferCasePosition.High2);
                break; // 2H
            default:
                _notificationService.SetState(NotificationService.SystemState.Error, (int)NotificationService.ErrorCodes.SelectionSwitchNotDetected);
                break;
        }
    }

    private async Task TestPositionSwitches()
    {
        while (true)
        {
            await Task.Delay(1000);
        }
    }

    private async Task TestMotor()
    {
        while (true)
        {
            _motor.Clockwise();
            Resolver.Log.Info($"CW");
            await Task.Delay(1000);
            _motor.Stop();
            Resolver.Log.Info($"STOP");
            await Task.Delay(1000);
            _motor.CounterClockwise();
            Resolver.Log.Info($"CCW");
            await Task.Delay(1000);
            _motor.Stop();
            Resolver.Log.Info($"STOP");
            await Task.Delay(1000);
        }
    }

    private async Task DisconnectedGearSwitchTest()
    {
        while (true)
        {
            (_analog as SimulatedAnalogInputPort).Voltage = new Voltage(3.3, Voltage.UnitType.Volts);
            await Task.Delay(5000);
            (_analog as SimulatedAnalogInputPort).Voltage = new Voltage(0.5, Voltage.UnitType.Volts);
            await Task.Delay(5000);
        }
    }

    private async Task ShiftCycleTest2()
    {
        bool up = true;

        while (true)
        {
            switch (_tcase.CurrentGear)
            {
                case TransferCasePosition.High2:
                    (_analog as SimulatedAnalogInputPort).Voltage = new Voltage(0.932, Voltage.UnitType.Volts);
                    up = false;
                    break;
                case TransferCasePosition.High4:
                    (_analog as SimulatedAnalogInputPort).Voltage = new Voltage((up ? 1.75 : 0.50), Voltage.UnitType.Volts);
                    break;
                case TransferCasePosition.Low4:
                    (_analog as SimulatedAnalogInputPort).Voltage = new Voltage(0.932, Voltage.UnitType.Volts);
                    up = true;
                    break;
            }

            await Task.Delay(5000);
        }
    }

    private async Task ShiftCycleTest()
    {
        while (true)
        {
            Resolver.Log.Info($"Current gear: {_tcase.CurrentGear}");

            if (_tcase.CurrentGear == TransferCasePosition.Low4)
            {
                Resolver.Log.Info($"Shifting down....");
                await _tcase.ShiftTo(TransferCasePosition.High2);
            }
            else if (_tcase.CurrentGear == TransferCasePosition.High2)
            {
                Resolver.Log.Info($"Shifting up....");
                await _tcase.ShiftTo(TransferCasePosition.Low4);
            }
            await Task.Delay(500);
        }
    }

    public override async Task Run()
    {
        Resolver.Log.Info($"Current gear is {_tcase.CurrentGear}");
        Resolver.Log.Info($"Gear selector is in {_switch.CurrentSwitchPosition}");

        await ShiftCycleTest2();
    }

}