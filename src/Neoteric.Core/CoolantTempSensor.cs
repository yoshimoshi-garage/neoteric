using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Units;
using System;

namespace Neoteric
{
    public class CoolantTempSensor
    {
        private SensorCurve _curve;
        private IAnalogInputPort _input;
        private VoltageDividerPort _divider;
        private double _exponent = 0d;
        private double _factor = 0d;

        public TimeSpan RefreshPeriod => _input.SampleInterval;

        public enum SensorCurve
        {
            Mopar,
            GM,
            Custom
        }

        public CoolantTempSensor(IMeadowDevice device, IPin pin, Resistance fixedResistor, SensorCurve curve)
        {
            if (curve == SensorCurve.Custom)
            {
                throw new Exception("Custom curves require a factor and exponent.  Use another constructor");
            }

            Curve = curve;

            CreateDivider(device, pin, fixedResistor);
        }

        public CoolantTempSensor(IMeadowDevice device, IPin pin, Resistance fixedResistor, double factor, double exponent)
        {
            Curve = SensorCurve.Custom;
            _factor = factor;
            _exponent = exponent;

            CreateDivider(device, pin, fixedResistor);
        }

        private void CreateDivider(IMeadowDevice device, IPin pin, Resistance r1)
        {
            if (!pin.Supports<IAnalogChannelInfo>(i => i.InputCapable))
            {
                throw new Exception("Pin must be analog input capable");
            }

            _input = device.CreateAnalogInputPort(pin, 1, TimeSpan.FromSeconds(5), new Voltage(3.3, Voltage.UnitType.Volts));
            _input.StartUpdating();

            _divider = new VoltageDividerPort(_input, r1);
        }

        public SensorCurve Curve
        {
            get => _curve;
            private set
            {
                switch (value)
                {
                    case SensorCurve.GM:
                        _factor = 945;
                        _exponent = -0.287;
                        break;
                    case SensorCurve.Mopar:
                        _factor = 945;
                        _exponent = -0.287;
                        break;
                    default:
                        _factor = 0;
                        _exponent = 0;
                        break;
                }
                _curve = value;
            }
        }

        public Temperature Temperature
        {
            get => new Temperature(_factor * Math.Pow(_divider.ReadR2Resistance().Ohms, _exponent), Temperature.UnitType.Fahrenheit);
        }
    }
}
