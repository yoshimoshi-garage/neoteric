using Meadow;
using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Neoteric.TransferCase;

public abstract class AnalogTransferCaseGearSelectorSwitch : ITransferCaseGearSelector, IDisposable
{
    public event EventHandler<TransferCasePosition>? RequestedPositionChanged;

    private readonly IAnalogInputPort _inputPort;
    private readonly TransferCaseSwitchSelectionBounds[] _switchPositions;
    private readonly Timer _switchPollTimer;

    private Voltage _lastReading;
    private TransferCasePosition _currentPosition = TransferCasePosition.Unknown;

    public static TimeSpan DefaultCheckPeriod = TimeSpan.FromMilliseconds(500);

    public bool IsDisposed { get; private set; }
    public TransferCasePosition RequestedPosition { get; }
    public TimeSpan CheckPeriod { get; }

    public AnalogTransferCaseGearSelectorSwitch(IAnalogInputPort inputPort, TransferCaseSwitchSelectionBounds[] switchPositions)
        : this(inputPort, switchPositions, DefaultCheckPeriod)
    {
    }

    public AnalogTransferCaseGearSelectorSwitch(IAnalogInputPort inputPort, TransferCaseSwitchSelectionBounds[] switchPositions, TimeSpan checkPeriod)
    {
        _inputPort = inputPort;
        _switchPositions = switchPositions;
        CheckPeriod = checkPeriod;
        _switchPollTimer = new Timer(SwitchCheckTimerProc, null, CheckPeriod, TimeSpan.FromMilliseconds(-1));
    }

    public TransferCasePosition CurrentSwitchPosition
    {
        get => _currentPosition;
        private set
        {
            if (value == _currentPosition) return;

            Resolver.Log.Info($"Setting switch position to {value} ({_lastReading.Volts:0.00} volts)");

            _currentPosition = value;
            RequestedPositionChanged?.Invoke(this, CurrentSwitchPosition);
        }
    }

    private void SwitchCheckTimerProc(object _)
    {
        if (IsDisposed) return;


        try
        {
            _lastReading = _inputPort.Read().Result;

            var detectedPosition = TransferCasePosition.Unknown;

            foreach (var position in _switchPositions)
            {
                if (position.IsActive(_lastReading))
                {
                    detectedPosition = position.Position;
                    break;
                }
            }

            CurrentSwitchPosition = detectedPosition;

            // Resolver.Log.Info($"ADC: {_lastReading} volts");

            if (detectedPosition == TransferCasePosition.Unknown)
            {
                _switchPollTimer.Change(TimeSpan.FromSeconds(2), TimeSpan.FromMilliseconds(-1));
            }
            else
            {
                _switchPollTimer.Change(CheckPeriod, TimeSpan.FromMilliseconds(-1));
            }
        }
        catch (Exception ex)
        {
            Resolver.Log.Info($"Failed reading selector switch: {ex.Message}");
            _switchPollTimer.Change(TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(-1));
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

    public IEnumerator<TransferCaseSwitchSelectionBounds> GetEnumerator()
    {
        return _switchPositions.Cast<TransferCaseSwitchSelectionBounds>().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
