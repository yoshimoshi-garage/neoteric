using Meadow.Units;

namespace Neoteric;

public class VoltageDivider
{
    public Resistance R1 { get; }
    public Voltage Vin { get; }

    public VoltageDivider(Voltage inputVoltage, Resistance fixedResistor)
    {
        Vin = inputVoltage;
        R1 = fixedResistor;
    }

    public Voltage CalcOutputVoltage(Resistance r2)
    {
        return new Voltage(Vin.Volts * (r2.Ohms / (r2.Ohms + R1.Ohms)));
    }

    public Resistance CalcR2Resistance(Voltage vOut)
    {
        return new Resistance(vOut.Volts * R1.Ohms / (Vin.Volts - vOut.Volts));
    }
}
