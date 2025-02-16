using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Hardware;
using System;
using System.Reflection;
using static Meadow.Foundation.Motors.BidirectionalDcMotor;

namespace Neoteric.TransferCase.F7;

/// <summary>
/// Controller for either the BW4419 or MP3023NQH Transfer cases using the Ford 3-position selector switch
/// </summary>
public class NTCCv1b : ITransferCaseController
{
    /*
    F7Featherv2 pin map for the NTCv1b

    A00 TC Gear Selector Switch
    A01 MP3023 Analog Position Sensor

    D00 TC Selector Motor Relay CW
    D01 TC Selector Motor Relay CCW

    D05 Safety Interlock Enable (enable == low)
    D06 Transfer Case Selection (BOTTOM POS (high)==BW4419, TOP POS(low)==MP3023)

    D07 I2C CLK for Optional debug display
    D08 I2C DAT for Optional debug display

    D10 4419 POS SW 4
    D11 4419 POS SW 3
    D12 4419 POS SW 2
    D13 4419 POS SW 1
    */

    private readonly ITransferCase _transferCase;
    private readonly ITransferCaseGearSelector _gearSelector;
    private readonly DisplayService? _displayService;

    public NTCCv1b(F7FeatherV2 device, ITransferCaseSettings settings)
    {
        // do we have a display attached?
        var i2c = device.CreateI2cBus(I2cBusSpeed.High);
        try
        {
            var display = new Ssd1306(i2c);

            _displayService = new DisplayService(display);
        }
        catch (Exception ex)
        {
            // TODO: blink a warning
            Resolver.Log.Info("No Debug Display found");
        }

        // what transfer case are we selecting (J1)

        using var selectionPort = device.Pins.D06.CreateDigitalInputPort(Meadow.Hardware.ResistorMode.Disabled);

        var motor = new GearSelectionMotor(device.Pins.D01, device.Pins.D00);
        motor.StateChanged += OnMotorStateChanged;

        ISafetyInterlock? interlock = null;

        if (settings.InterlockEnabled)
        {
            interlock = new SafetyInterlockSwitch(device.Pins.D05);
            _displayService?.Report($"ENA: {(interlock.IsSafe ? "safe" : "not safe")}");
            interlock.Changed += (s, e) =>
            {
                _displayService?.Report($"ENA: {(interlock.IsSafe ? "safe" : "not safe")}");
            };
        }
        else
        {
            _displayService?.Report($"ENA: disabled");
        }

        string versionInfo;

        if (selectionPort.State)
        {
            versionInfo = $"BW4419 v{Assembly.GetExecutingAssembly().GetName().Version.ToString(3)}";

            var sw1 = device.Pins.D13.CreateDigitalInputPort(ResistorMode.InternalPullDown);
            var sw2 = device.Pins.D12.CreateDigitalInputPort(ResistorMode.InternalPullDown);
            var sw3 = device.Pins.D11.CreateDigitalInputPort(ResistorMode.InternalPullDown);
            var sw4 = device.Pins.D10.CreateDigitalInputPort(ResistorMode.InternalPullDown);

            _transferCase = new BW4419(motor, sw1, sw2, sw3, sw4, interlock);

        }
        else
        {
            versionInfo = $"MP3023NQH v{Assembly.GetExecutingAssembly().GetName().Version.ToString(3)}";

            _transferCase = new MP3023NQH(
                motor,
                device.Pins.A01.CreateAnalogInputPort(3),
                interlock,
                device.Pins.D04.CreateDigitalOutputPort(false));
        }

        Resolver.Log.Info(versionInfo);
        _displayService?.Report(versionInfo);

        _transferCase.GearChanged += OnTransferCaseGearChanged;
        _transferCase.GearChanging += OnTransferCaseGearChanging;

        _gearSelector = new ThreePositionFordTransferCaseSwitch(device.Pins.A00.CreateAnalogInputPort());
        _gearSelector.RequestedPositionChanged += OnGearSelectorRequestedPositionChanged;

        _transferCase.RequestShiftTo(_gearSelector.RequestedPosition);
        _transferCase.StartControlLoop();

        Resolver.Log.Info($"Selected gear: {_gearSelector.CurrentSwitchPosition}");
        Resolver.Log.Info($"Transfer case in: {_transferCase.CurrentGear}");
        _displayService?.Report($"SW: {_gearSelector.CurrentSwitchPosition}");
        _displayService?.Report($"TC: {_transferCase.CurrentGear}");
    }

    private void OnTransferCaseGearChanging(object sender, TransferCasePosition e)
    {
        Resolver.Log.Info($"Transfer case shifting to: {e}");
        _displayService?.Report($"--> {e}");
    }

    private void OnTransferCaseGearChanged(object sender, TransferCasePosition e)
    {
        Resolver.Log.Info($"Transfer case in: {e}");
        _displayService?.Report($"TC: {e}");
    }

    private void OnGearSelectorRequestedPositionChanged(object sender, TransferCasePosition e)
    {
        Resolver.Log.Info($"Selected gear: {e}");
        _transferCase.RequestShiftTo(e);
        _displayService?.Report($"SW: {e}");
    }

    private void OnMotorStateChanged(object sender, MotorState e)
    {
        Resolver.Log.Info($"Motor {e}");
        var state = e switch
        {
            MotorState.RunningClockwise => "MO: CW",
            MotorState.RunningCounterclockwise => "MO: CCW",
            _ => "MO: STOP"
        };
        _displayService?.Report(state);
    }
}
