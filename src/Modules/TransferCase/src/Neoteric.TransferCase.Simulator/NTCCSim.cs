using Meadow;
using Meadow.Foundation.Motors;
using Meadow.Foundation.Sensors;
using Meadow.Logging;
using Meadow.Units;
using Neoteric;
using Neoteric.TransferCase;

namespace Neaoteric.TransferCase.Simulator;

public class NTCCSim : ITransferCaseController
{
    private SimulatedAnalogInputPort _switchInput;
    private SimulatedAnalogInputPort _tcaseInput;
    private FordSwitchSettings _switchSettings = new FordSwitchSettings();

    public NTCCSim()
    {
        Resolver.Services.Add(new Logger());
        Resolver.Log.AddProvider(new ConsoleLogProvider());
        Resolver.Log.LogLevel = LogLevel.Debug;

        var motor = new SimulatedGearSelectionMotor();

        _switchInput = new SimulatedAnalogInputPort();
        _tcaseInput = new SimulatedAnalogInputPort();

        var sw = new ThreePositionFordTransferCaseSwitch(_switchInput, _switchSettings);
        var settings = new TransferCaseSettings<FordSwitchSettings, MP3023Settings>();
        var tcase = new MP3023NQH(motor, _tcaseInput, null, null, settings);

        tcase.GearChanged += (s, e) => Resolver.Log.Info($"TCase is in {e}");
        tcase.GearChanging += (s, e) => Resolver.Log.Info($"TCase headed to {e}");

        sw.RequestedPositionChanged += (s, e) =>
        {
            Resolver.Log.Info($"Switch changed to {e}");
            tcase.RequestShiftTo(e);
        };

        _switchInput.SetSensorValue(1.4.Volts()); // 4H
        _tcaseInput.SetSensorValue(2.3.Volts()); // 4H
        Resolver.Log.Info($"Start Gear: {tcase.CurrentGear}");
        Resolver.Log.Info($"Start Switch: {sw.CurrentSwitchPosition}");

        motor.StateChanged += (s, e) =>
        {
            switch (e)
            {
                case BidirectionalDcMotor.MotorState.RunningClockwise:
                    _tcaseInput.SetSensorValue(_tcaseInput.Voltage - 0.01.Volts());
                    Resolver.Log.Info($"{_tcaseInput.Voltage.Volts:N2}V");
                    break;
                case BidirectionalDcMotor.MotorState.RunningCounterclockwise:
                    _tcaseInput.SetSensorValue(_tcaseInput.Voltage + 0.01.Volts());
                    Resolver.Log.Info($"{_tcaseInput.Voltage.Volts:N2}V");
                    break;
            }
        };

        tcase.RequestShiftTo(sw.RequestedPosition);
        tcase.StartControlLoop();
    }

    public void RequestGear(TransferCasePosition position)
    {
        Resolver.Log.Info($"REQUESTING {position}");

        var factor = 1;

        switch (position)
        {
            case TransferCasePosition.High2:
                var m = (_switchSettings.High2Max + _switchSettings.High2Min) / 2;
                m *= factor;
                _switchInput.SetSensorValue(m.Volts());
                break;
            case TransferCasePosition.High4:
                var m2 = (_switchSettings.High4Max + _switchSettings.High4Min) / 2;
                m2 *= factor;
                _switchInput.SetSensorValue(m2.Volts());
                break;
            case TransferCasePosition.Low4:
                var m3 = (_switchSettings.Low4Max + _switchSettings.Low4Min) / 2;
                m3 *= factor;
                _switchInput.SetSensorValue(m3.Volts());
                break;
        }
    }
}
