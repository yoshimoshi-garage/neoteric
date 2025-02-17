﻿using Meadow;
using System;
using System.Collections.Generic;

namespace Neoteric.TransferCase;

public interface ISelectorSwitchVoltageSettings
{
    public double Low4Min { get; set; }
    public double Low4Max { get; set; }
    public double High4Min { get; set; }
    public double High4Max { get; set; }
    public double High2Min { get; set; }
    public double High2Max { get; set; }

    private static readonly Dictionary<string, Action<ISelectorSwitchVoltageSettings, double>> _setters = new()
    {
        ["Switch.Low4.Min"] = (settings, value) => settings.Low4Min = value,
        ["Switch.Low4.Max"] = (settings, value) => settings.Low4Max = value,
        ["Switch.High4.Min"] = (settings, value) => settings.High4Min = value,
        ["Switch.High4.Max"] = (settings, value) => settings.High4Max = value,
        ["Switch.High2.Min"] = (settings, value) => settings.High2Min = value,
        ["Switch.High2.Max"] = (settings, value) => settings.High2Max = value
    };

    private bool ParseVoltage(string name, string candidate, out double value)
    {
        if (double.TryParse(candidate, out double d))
        {
            if (d < 0 || d > 3.3)
            {
                Resolver.Log.Info($"{name} setting out of bounds: {d:N2}");
            }
            else
            {
                Resolver.Log.Info($"{name} from app settings: {d:N2}");
                value = d;
                return true;
            }
        }
        else
        {
            Resolver.Log.Info($"{name} setting not parseable: {candidate}");
        }
        value = 0;
        return false;
    }

    public void ApplySettings(Dictionary<string, string> settings)
    {
        foreach (var setting in settings)
        {
            if (_setters.TryGetValue(setting.Key, out var setter) &&
                ParseVoltage(setting.Key, setting.Value, out double value))
            {
                setter(this, value);
            }
        }
    }
}
