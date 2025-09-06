using Meadow;
using Meadow.Foundation.Sensors;
using Meadow.Logging;

namespace Neoteric.TransferCase.Core.Tests;

public class SelectorSwitchTests
{
    public SelectorSwitchTests()
    {
        Resolver.Services.Add(new Logger(new DebugLogProvider()) { LogLevel = LogLevel.Trace });
    }

    [Fact]
    public async Task MP3023InputVoltageDecodesToProperGear()
    {
        //var motor = new SimulatedGearSelectionMotor();
        //var input = new SimulatedAnalogInputPort();
        //var sw = new ThreePositionFordTransferCaseSwitch(input, new FordSwitchSettings());
        //var settings = new TransferCaseSettings<FordSwitchSettings, MP3023Settings>();

        //var tcase = new MP3023NQH(motor, input, null, null, settings);
        //tcase.GearChanging += (s, e) => Debug.WriteLine("shifting...");

        //input.SetSensorValue(new Meadow.Units.Voltage(1.4));

        //tcase.RequestShiftTo(sw.RequestedPosition);
        //tcase.StartControlLoop();

        //while (true)
        //{
        //    Debug.WriteLine(tcase.CurrentGear);
        //    await Task.Delay(1000);
        //}
    }

    [Fact]
    public async Task ChangingVoltageSelectsProperGear()
    {
        Resolver.Log.LogLevel = LogLevel.Trace;
        var simulatedAnalog = new SimulatedAnalogInputPort();

        var selector = new ThreePositionFordTransferCaseSwitch(simulatedAnalog, new FordSwitchSettings());

        TransferCasePosition? eventPosition = null;

        selector.RequestedPositionChanged += (s, position) =>
        {
            eventPosition = position;
        };

        foreach (var position in selector)
        {
            eventPosition = null;

            // set the voltage
            simulatedAnalog.SetSensorValue(position.MidRangeVoltage);

            // wait for at least the check period
            await Task.Delay(selector.CheckPeriod * 2);

            // make sure the event fired
            Assert.NotNull(eventPosition);

            // make sure the proper position is selected
            Assert.Equal(position.Position, eventPosition);
        }
    }
}