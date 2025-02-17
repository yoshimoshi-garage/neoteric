using Meadow;
using System.Collections.Generic;

namespace Neoteric.TransferCase;

public class TransferCaseSettings<TSwitchSetting, TTCaseSettings>
    : ITransferCaseSettings
    where TSwitchSetting : ISelectorSwitchVoltageSettings, new()
    where TTCaseSettings : ITransferCaseVoltageSettings, new()
{
    public bool InterlockEnabled { get; set; } = true;
    public bool GearLockEnabled { get; set; } = true;
    public int GearLockDelay { get; set; } = 0;

    public ISelectorSwitchVoltageSettings SwitchVoltageSettings { get; set; }
    public ITransferCaseVoltageSettings TransferCaseVoltageSettings { get; set; }

    public TransferCaseSettings(Dictionary<string, string> settings)
    {
        string v;

        if (settings.TryGetValue("Interlock.Enabled", out v))
        {
            if (bool.TryParse(v, out bool b))
            {
                Resolver.Log.Info($"Interlock.Enabled from app settings: {b}");
                InterlockEnabled = b;
            }
        }

        if (settings.TryGetValue("GearLock.Enabled", out v))
        {
            if (bool.TryParse(v, out bool b))
            {
                Resolver.Log.Info($"GearLock.Enabled from app settings: {b}");
                GearLockEnabled = b;
            }
        }

        if (settings.TryGetValue("GearLock.Delay", out v))
        {
            if (int.TryParse(v, out int b))
            {
                Resolver.Log.Info($"GearLock.Delay from app settings: {b}");
                GearLockDelay = b;
            }
        }

        TransferCaseVoltageSettings = new TTCaseSettings();
        TransferCaseVoltageSettings.ApplySettings(settings);

        SwitchVoltageSettings = new TSwitchSetting();
        SwitchVoltageSettings.ApplySettings(settings);
    }

}
