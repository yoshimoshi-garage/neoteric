using Meadow.Hardware;
using System.Collections.Generic;

namespace Neoteric.TransferCase;

/// <summary>
/// The Magna Power MP3023NQH 3-position transfer case
/// </summary>
public class MP3023NQH : TransferCaseBase
{
    private readonly Dictionary<int, TransferCasePosition> _positionTruthTable = new();
    private readonly IAnalogInputPort _positionSensor;

    public MP3023NQH(
        GearSelectionMotor motor,
        IAnalogInputPort positionSensorPort,
        ISafetyInterlock? safetyInterlock)
        : base(motor, safetyInterlock)
    {
        _positionSensor = positionSensorPort;
    }

    public override TransferCasePosition[] SupportedGears => new TransferCasePosition[]
        {
            TransferCasePosition.Low4,
            TransferCasePosition.High2,
            TransferCasePosition.High4
        };

    // The MP3023NQH gear position sensor requires a 5V signal.  It returns the following voltages based on gear selection:
    // 4LOW: 1.3V
    //  2HI: 2.0V
    //  4HI: 3.0V
    // The TCC v1b uses a voltage divider to pull 0-5 down to 0-3.3.  Mapped values:
    // INPUT  THEORETICAL   MEASURED
    //  1.0V     0.66V       0.72V
    //  1.3      0.86        0.95V
    //  2.0      1.32        1.40
    //  3.0      1.98        2.07
    public override TransferCasePosition CurrentGear
    {
        get
        {
            if (_positionSensor == null) return TransferCasePosition.Unknown;

            var sensorReading = _positionSensor.Read().Result.Volts;

            if (sensorReading > 0.90 && sensorReading < 1.05)       // 0-5V input ~1.3
            {
                return TransferCasePosition.Low4;
            }
            else if (sensorReading > 1.3 && sensorReading < 1.5)    // 0-5V input ~2.0
            {
                return TransferCasePosition.High2;
            }
            else if (sensorReading > 1.95 && sensorReading < 2.15)  // 0-5V input ~3.0
            {
                return TransferCasePosition.High4;
            }

            return TransferCasePosition.Unknown;
        }
    }
}
