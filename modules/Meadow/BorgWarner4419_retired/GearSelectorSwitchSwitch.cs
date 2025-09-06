using Meadow;
using Meadow.Hardware;
using System;
using System.Threading;

namespace Neoteric;

public class GearSelectorSwitchSwitch : IDisposable
{
    public event EventHandler<TransferCasePosition> SwitchPositionChanged;

    private struct SelectionBounds
    {
        public TransferCasePosition Position { get; set; }
        public double SwitchResistance { get; set; }
        public double MinVoltage { get; set; }
        public double MaxVoltage { get; set; }
    }

    private Thread _switchCheckThread;
    private IAnalogInputPort _analogInputPort;
    private TransferCasePosition? _currentPosition;
    private double _lastReading;

    public bool IsDisposed { get; private set; }

    private readonly SelectionBounds[] _selectionBounds;

    public GearSelectorSwitchSwitch(IAnalogInputPort inputPort)
    {
        _analogInputPort = inputPort;

        _selectionBounds = new SelectionBounds[]
        {
             new SelectionBounds
             {
                  Position = TransferCasePosition.Low4,
                  SwitchResistance = 130,
                  MinVoltage = 0.40, // lab-measured voltage 0.57
                  MaxVoltage = 0.80
             },
             new SelectionBounds
             {
                  Position = TransferCasePosition.High4,
                  SwitchResistance = 270,
                  MinVoltage = 0.80, // lab-measured voltage 0.98
                  MaxVoltage = 1.20
             },
             new SelectionBounds
             {
                  Position = TransferCasePosition.High2,
                  SwitchResistance = 620,
                  MinVoltage = 1.20, // lab-measured voltage 1.53
                  MaxVoltage = 1.80
             }
        };

        _switchCheckThread = new Thread(SwitchCheckProc);
        _switchCheckThread.Start();
    }

    public TransferCasePosition CurrentSwitchPosition
    {
        get => _currentPosition ?? TransferCasePosition.Unknown;
        private set
        {
            if (value == _currentPosition) return;

            Resolver.Log.Info($"Setting switch position to {value} ({_lastReading:0.00} volts)");

            _currentPosition = value;
            SwitchPositionChanged?.Invoke(this, CurrentSwitchPosition);
        }
    }

    private void SwitchCheckProc(object state)
    {
        while (!IsDisposed)
        {
            try
            {
                _lastReading = _analogInputPort.Read().Result.Volts;

                var detectedPosition = TransferCasePosition.Unknown;

                foreach (var range in _selectionBounds)
                {
                    if (_lastReading > range.MinVoltage && _lastReading <= range.MaxVoltage)
                    {
                        detectedPosition = range.Position;
                        break;
                    }
                }

                CurrentSwitchPosition = detectedPosition;

                if (detectedPosition == TransferCasePosition.Unknown)
                {
                    Resolver.Log.Info($"ADC: {_lastReading} volts");
                    Thread.Sleep(2000);
                }
                else
                {
                    Thread.Sleep(500);
                }
            }
            catch (Exception ex)
            {
                Resolver.Log.Info($"Failed reading selector switch: {ex.Message}");
                Thread.Sleep(1000);
            }
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            if (disposing)
            {
            }

            IsDisposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
