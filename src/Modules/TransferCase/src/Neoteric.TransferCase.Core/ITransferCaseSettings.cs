namespace Neoteric.TransferCase;

public interface ITransferCaseSettings
{
    bool InterlockEnabled { get; }
    bool GearLockEnabled { get; }
    int GearLockDelay { get; }

    ISelectorSwitchVoltageSettings SwitchVoltageSettings { get; }
    ITransferCaseVoltageSettings TransferCaseVoltageSettings { get; }
}
