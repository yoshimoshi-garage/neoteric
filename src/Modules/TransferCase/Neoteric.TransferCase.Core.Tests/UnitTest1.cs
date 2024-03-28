using Meadow;
using Meadow.Hardware;
using Meadow.Logging;
using Meadow.Peripherals.Sensors;
using Meadow.Units;

namespace Neoteric.TransferCase.Core.Tests;

public class SimulatedAnalogInputPort : IAnalogInputPort, ISimulatedSensor
{
    public event EventHandler<IChangeResult<Voltage>>? Updated;

    private readonly Voltage referenceVoltage;
    private Voltage currentVoltage;

    public Voltage Voltage => currentVoltage;
    public Voltage ReferenceVoltage => referenceVoltage;
    public int SampleCount => 1;

    public SimulatedAnalogInputPort()
        : this(Voltage.Zero, 3.3.Volts())
    {
    }

    public SimulatedAnalogInputPort(Voltage initialVoltage)
        : this(initialVoltage, 3.3.Volts())
    {
    }

    public SimulatedAnalogInputPort(Voltage initialVoltage, Voltage referenceVoltage)
    {
        this.referenceVoltage = referenceVoltage;
        this.referenceVoltage = referenceVoltage;
        currentVoltage = initialVoltage;
    }

    public void Dispose()
    {
    }

    public void SetVoltage(Voltage voltage)
    {
        var previous = currentVoltage;

        if (voltage < Voltage.Zero)
        {
            voltage = Voltage.Zero;
        }
        else if (voltage > referenceVoltage)
        {
            voltage = referenceVoltage;
        }

        if (voltage == currentVoltage) { return; }

        currentVoltage = voltage;
        Updated?.Invoke(this, new ChangeResult<Voltage>(currentVoltage, previous));
    }

    public void SetSensorValue(object value)
    {
        if (value is Voltage v)
        {
            SetVoltage(v);
        }
        throw new ArgumentException("Value must be a voltage");
    }

    public Task<Voltage> Read()
    {
        return Task.FromResult(currentVoltage);
    }

    //------------------------------ TODO: implement below here
    public Voltage[] VoltageSampleBuffer => throw new NotImplementedException();

    public TimeSpan UpdateInterval => throw new NotImplementedException();

    public TimeSpan SampleInterval => throw new NotImplementedException();

    public IAnalogChannelInfo Channel => throw new NotImplementedException();

    public IPin Pin => throw new NotImplementedException();

    public SimulationBehavior[] SupportedBehaviors => throw new NotImplementedException();

    public Type ValueType => throw new NotImplementedException();

    public void StartUpdating(TimeSpan? updateInterval = null)
    {
        throw new NotImplementedException();
    }

    public void StopUpdating()
    {
        throw new NotImplementedException();
    }

    public IDisposable Subscribe(IObserver<IChangeResult<Voltage>> observer)
    {
        throw new NotImplementedException();
    }

    public void StartSimulation(SimulationBehavior behavior)
    {
        throw new NotImplementedException();
    }
}

public class SelectorSwitchTests
{
    public SelectorSwitchTests()
    {
        Resolver.Services.Add(new Logger(new DebugLogProvider()) { LogLevel = LogLevel.Trace });
    }

    [Fact]
    public async Task ChangingVoltageSelectsProperGear()
    {
        Resolver.Log.LogLevel = LogLevel.Trace;
        var simulatedAnalog = new SimulatedAnalogInputPort();

        var selector = new ThreePositionFordTransferCaseSwitch(simulatedAnalog);

        TransferCasePosition? eventPosition = null;

        selector.RequestedPositionChanged += (s, position) =>
        {
            eventPosition = position;
        };

        foreach (var position in selector)
        {
            eventPosition = null;

            // set the voltage
            simulatedAnalog.SetVoltage(position.MidRangeVoltage);

            // wait for at least the check period
            await Task.Delay(selector.CheckPeriod * 2);

            // make sure the event fired
            Assert.NotNull(eventPosition);

            // make sure the proper position is selected
            Assert.Equal(position.Position, eventPosition);
        }
    }
}