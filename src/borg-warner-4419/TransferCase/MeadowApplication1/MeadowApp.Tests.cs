using Meadow;
using Meadow.Simulation;
using Meadow.Units;
using System.Threading.Tasks;

namespace Neoteric;

public partial class MeadowApp
{
    public bool SimulateMotor { get; set; } = false;
    public bool SimulateGearSelector { get; set; } = false;

    private async Task TestPositionSwitches()
    {
        while (true)
        {
            Resolver.Log.Info($"Current gear: {_tcase.CurrentGear} {(_sw1.State ? 1 : 0)}{(_sw2.State ? 1 : 0)}{(_sw3.State ? 1 : 0)}{(_sw4.State ? 1 : 0)}");
            await Task.Delay(1000);
        }
    }

    private async Task TestMotor()
    {
        while (true)
        {
            _motor.StartClockwise();
            Resolver.Log.Info($"CW");
            await Task.Delay(1000);
            _motor.Stop();
            Resolver.Log.Info($"STOP");
            await Task.Delay(1000);
            _motor.StartCounterClockwise();
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
}