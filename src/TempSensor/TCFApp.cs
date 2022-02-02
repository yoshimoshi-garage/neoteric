using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using Meadow.Units;
using System;
using System.Threading;

namespace Neoteric
{
    public class TCFApp : App<F7Micro, TCFApp>
    {
        private VoltageDividerPort _divider;

        public TCFApp()
        {
            Initialize();

            ShowTemperatures();
        }

        void ShowVoltages()
        {
            while (true)
            {
                Console.WriteLine($"{_divider.Input.Voltage}V");
                Thread.Sleep(1000);
            }
        }

        void ShowTemperatures()
        {
            while (true)
            {
                var volts = _divider.Input.Voltage;

                var resistance = _divider.ReadR2Resistance();
                var temp = new Temperature(945 * Math.Pow(resistance.Ohms, -0.287), Temperature.UnitType.Fahrenheit);

                Console.WriteLine($"{volts.Volts}V = {resistance.Ohms}ohms = {temp.Fahrenheit}");

                Thread.Sleep(1000);
            }
        }

        private Voltage _referenceVoltage = new Voltage(3.3, Voltage.UnitType.Volts);
        private Resistance _dividerResistor = new Resistance(330, Resistance.UnitType.Ohms);

        Resistance GetResistanceForVoltage(Voltage volts, Resistance knownResistor)
        {
            return new Resistance((_referenceVoltage.Volts * knownResistor.Ohms / volts.Volts) - knownResistor.Ohms);
        }

        Temperature GetTempForGMVoltage(Voltage volts)
        {
            // temp = 945 * resistance ^ -0.287
            // resistance = 
            var resistance = GetResistanceForVoltage(volts, _dividerResistor);
            return new Temperature( 945 * Math.Pow(resistance.Ohms, -0.287), Temperature.UnitType.Fahrenheit);
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            var adc = Device.CreateAnalogInputPort(Device.Pins.A00, 1, TimeSpan.FromSeconds(1), _referenceVoltage);
            adc.StartUpdating(null);

            _divider = new VoltageDividerPort(adc, _dividerResistor);
        }
    }
}
