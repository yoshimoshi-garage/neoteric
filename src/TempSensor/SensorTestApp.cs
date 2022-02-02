using Meadow;
using Meadow.Devices;
using Meadow.Units;
using System;
using System.Threading;

namespace Neoteric
{
    public class SensorTestApp : App<F7Micro, SensorTestApp>
    {
        private CoolantTempSensor _sensor;

        public SensorTestApp()
        {
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
