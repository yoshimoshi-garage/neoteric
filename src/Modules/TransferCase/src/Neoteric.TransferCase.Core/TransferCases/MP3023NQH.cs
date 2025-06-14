﻿using Meadow;
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
    private readonly GearSensorConfig _gearSensorConfig;

    public MP3023NQH(
        IGearSelectionMotor motor,
        IAnalogInputPort positionSensorPort,
        ISafetyInterlock? safetyInterlock,
        IDigitalOutputPort? hubLockEnable,
        ITransferCaseSettings settings)
        : base(motor, safetyInterlock, hubLockEnable)
    {
        _positionSensor = positionSensorPort;
        _gearSensorConfig = new GearSensorConfig(settings.TransferCaseVoltageSettings);
    }

    public override TransferCasePosition[] SupportedGears => new TransferCasePosition[]
    {
        TransferCasePosition.Low4,
        TransferCasePosition.High2,
        TransferCasePosition.High4
    };

    public class GearSensorConfig
    {
        // The MP3023NQH gear position sensor requires a 5V signal.  It returns the following voltages based on gear selection:
        // 4LOW: 1.45V
        //  2HI: 2.45V
        //  4HI: 3.45V
        // 
        // Since the transfer case uses 5V, but the ADC on the hardware can only read from 0-3.3,
        // the TCC v1b uses a voltage divider (1.7k and 3.3k) to scale the 0-5V input down to 0-3.3V
        // 
        // Mapped values
        //  - column 1 is the raw (0-5V) voltage from the TC
        //  - column 2 is the theoretical value read by the ADC after going through the voltage divider
        //  - column 3 are actual measured values on hardware
        //  - column 4 is the back-calculation from the measured.  These are the default input settings
        // INPUT  THEORETICAL   MEASURED        BACK-CALC FROM MEASURED
        //  1.0V     0.66V       0.72V
        //  1.3      0.86        0.95V
        //  1.4      0.92        0.90   <-- 4LO   1.36
        //  1.5      0.99        0.96   <-- 4LO   1.45
        //  2.0      1.32        1.40
        //  2.4      1.58        1.59   <-- 2HI   2.41
        //  2.5      1.65        1.66   <-- 2HI   2.52
        //  3.0      1.98        2.07
        //  3.4      2.24        2.23   <-- 4HI   3.38
        //  3.5      2.31        2.35   <-- 4HI   3.56

        public double Min4Low { get; set; } = 0.90;
        public double Max4Low { get; set; } = 0.96;
        public double Min2High { get; set; } = 1.59;
        public double Max2High { get; set; } = 1.66;
        public double Min4High { get; set; } = 2.23;
        public double Max4High { get; set; } = 2.35;

        public GearSensorConfig(ITransferCaseVoltageSettings settings)
        {
            // CONVERT THE SETTINGS, WHICH ARE 5V-BASED, TO ADC VALUES, WHICH ARE 3.3-BASED
            var R1 = 1700d;
            var R2 = 3300d;
            var scale = R2 / (R1 + R2);

            Min4Low = settings.Low4Min * scale;
            Max4Low = settings.Low4Max * scale;
            Min4High = settings.High4Min * scale;
            Max4High = settings.High4Max * scale;
            Min2High = settings.High2Min * scale;
            Max2High = settings.High2Max * scale;

            ReportSettings();
        }

        public void ReportSettings()
        {
            Resolver.Log.Info($"MP3023 Voltage Settings:");
            Resolver.Log.Info($"  Min4Low: {Min4Low}");
            Resolver.Log.Info($"  Max4Low: {Max4Low}");
            Resolver.Log.Info($"  Min4High: {Min4High}");
            Resolver.Log.Info($"  Max4High: {Max4High}");
            Resolver.Log.Info($"  Min2High: {Min2High}");
            Resolver.Log.Info($"  Max2High: {Max2High}");
        }
    }

    protected override TransferCaseState GetDirectionTo(TransferCasePosition position)
    {
        if (_positionSensor == null) return TransferCaseState.Idle;

        var sensorReading = _positionSensor.Read().Result.Volts;

        switch (position)
        {
            case TransferCasePosition.Low4:
                if (sensorReading < _gearSensorConfig.Min4Low) return TransferCaseState.ShiftingUp;
                if (sensorReading > _gearSensorConfig.Max4Low) return TransferCaseState.ShiftingDown;
                return TransferCaseState.Idle;
            case TransferCasePosition.High2:
                if (sensorReading < _gearSensorConfig.Min2High) return TransferCaseState.ShiftingUp;
                if (sensorReading > _gearSensorConfig.Max2High) return TransferCaseState.ShiftingDown;
                return TransferCaseState.Idle;
            case TransferCasePosition.High4:
                if (sensorReading < _gearSensorConfig.Min4High) return TransferCaseState.ShiftingUp;
                if (sensorReading > _gearSensorConfig.Max4High) return TransferCaseState.ShiftingDown;
                return TransferCaseState.Idle;
        }

        return TransferCaseState.Idle;
    }

    public override TransferCasePosition CurrentGear
    {
        get
        {
            if (_positionSensor == null) return TransferCasePosition.Unknown;

            var sensorReading = _positionSensor.Read().Result.Volts;

            Resolver.Log.Info($"TCASE VOLTAGE: {sensorReading} volts");

            if (sensorReading < _gearSensorConfig.Min4Low)
            {
                return TransferCasePosition.BeforeLowest;
            }
            else if (sensorReading >= _gearSensorConfig.Min4Low && sensorReading <= _gearSensorConfig.Max4Low)
            { // 0-5V input 1.4-1.5
                return TransferCasePosition.Low4;
            }
            else if (sensorReading >= _gearSensorConfig.Min2High && sensorReading <= _gearSensorConfig.Max2High)
            { // 0-5V input 2.4-2.5
                return TransferCasePosition.High2;
            }
            else if (sensorReading >= _gearSensorConfig.Min4High && sensorReading <= _gearSensorConfig.Max4High)
            { // 0-5V input 3.4-3.5
                return TransferCasePosition.High4;
            }
            else if (sensorReading > _gearSensorConfig.Max4High)
            {
                return TransferCasePosition.AboveHighest;
            }

            return TransferCasePosition.BetweenGears;
        }
    }
}
