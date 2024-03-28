using Meadow.Hardware;
using Meadow.Units;
using System;

namespace Neoteric;

public class VoltageDividerPort : VoltageDivider
{
    public IAnalogInputPort Input { get; }

    public VoltageDividerPort(IAnalogInputPort input, Resistance fixedResistor)
        : base(input.ReferenceVoltage, fixedResistor)
    {
        Input = input;
    }

    public Resistance ReadR2Resistance()
    {
        var ohms = Input.Voltage.Volts * R1.Ohms / (Vin.Volts - Input.Voltage.Volts);
        Console.WriteLine($"Resistance: {ohms:0.00}ohms");
        return new Resistance(ohms, Resistance.UnitType.Ohms);
    }
}
