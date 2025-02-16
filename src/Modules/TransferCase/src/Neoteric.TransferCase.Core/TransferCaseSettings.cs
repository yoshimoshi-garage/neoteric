using Meadow;
using System.Collections.Generic;

namespace Neoteric.TransferCase;

public class TransferCaseSettings<TSwitchSetting, TTCaseSettings>
    : ITransferCaseSettings
    where TSwitchSetting : ISelectorSwitchVoltageSettings, new()
    where TTCaseSettings : ITransferCaseVoltageSettings, new()
{
    public bool InterlockEnabled { get; set; } = true;
    public ISelectorSwitchVoltageSettings SwitchVoltageSettings { get; set; }
    public ITransferCaseVoltageSettings TransferCaseVoltageSettings { get; set; }

    public TransferCaseSettings(Dictionary<string, string> settings)
    {
        if (settings.TryGetValue("Interlock:Enabled", out string v))
        {
            if (bool.TryParse(v, out bool b))
            {
                Resolver.Log.Info($"Interlock.Enabled from app settings: {b}");
                InterlockEnabled = b;
            }
        }

        TransferCaseVoltageSettings = new TTCaseSettings();
        TransferCaseVoltageSettings.ApplySettings(settings);

        SwitchVoltageSettings = new TSwitchSetting();
        SwitchVoltageSettings.ApplySettings(settings);
    }

}
