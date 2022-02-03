using Meadow;
using Meadow.Devices;
using Meadow.Units;
using System;
using System.Threading;

namespace Neoteric
{
    public class SensorTestApp : App<F7Micro, SensorTestApp>
    {
        private Voltage _referenceVoltage = new Voltage(3.3, Voltage.UnitType.Volts);
        private Resistance _dividerResistor = new Resistance(330, Resistance.UnitType.Ohms);

        public SensorTestApp()
        {
            Console.WriteLine("Create input...");
            var adc = Device.CreateAnalogInputPort(Device.Pins.A00, 1, TimeSpan.FromSeconds(1), _referenceVoltage);
            adc.StartUpdating(null);

            while (true)
            {
                Console.WriteLine("Reading voltage...");
                var volts = adc.Voltage;
                Console.WriteLine($"voltage: {volts.Volts}V");

                Console.WriteLine("Calculating resistance...");
                var resistance = new Resistance(
                    volts.Volts * _dividerResistor.Ohms / (_referenceVoltage.Volts - volts.Volts),
                    Resistance.UnitType.Ohms);
                Console.WriteLine($"resistance: {resistance.Ohms}Ohms");

                Console.WriteLine("Calculating temperature (GM curve)...");
                var temp = new Temperature(
                    945 * Math.Pow(resistance.Ohms, -0.287),
                    Temperature.UnitType.Fahrenheit);
                Console.WriteLine($"temperature: {temp.Fahrenheit}Ohms");

                // wait 5 seconds and repeat
                Thread.Sleep(5000);
            }
        }
    }
}
