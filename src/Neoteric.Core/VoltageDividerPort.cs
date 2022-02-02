using Meadow.Hardware;
using Meadow.Units;

namespace Neoteric
{
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
            return new Resistance(Input.Voltage.Volts * R1.Ohms / (Vin.Volts - Input.Voltage.Volts));
        }
    }
}
