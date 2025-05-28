namespace Neoteric.TransferCase;

public interface ITransferCaseSettings
{
    bool InterlockEnabled { get; }
    bool GearUnlockEnabled { get; }
    int GearUnlockDelay { get; }

    ISelectorSwitchVoltageSettings SwitchVoltageSettings { get; }
    ITransferCaseVoltageSettings TransferCaseVoltageSettings { get; }
}
