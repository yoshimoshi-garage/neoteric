using Meadow;
using Meadow.Devices;
using Meadow.Units;
using System;
using System.Threading;

namespace Neoteric
{
    public class TCFApp : App<F7Micro, TCFApp>
    {
        private CoolantTempSensor _sensor;

        public TCFApp()
        {
            Console.WriteLine($"Creating a Coolant Temp Sensor...");

            _sensor = new CoolantTempSensor(
                Device,
                Device.Pins.A00,
                new Resistance(330, Resistance.UnitType.Ohms),
                CoolantTempSensor.SensorCurve.GM);

            ShowTemperatures();
        }

        void ShowTemperatures()
        {
            while (true)
            {
                Console.WriteLine($"{_sensor.Temperature.Fahrenheit}F");

                Thread.Sleep(1000);
            }
        }
    }
}
