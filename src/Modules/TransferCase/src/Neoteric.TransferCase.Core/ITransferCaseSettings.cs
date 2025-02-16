namespace Neoteric.TransferCase;

public interface ITransferCaseSettings
{
    bool InterlockEnabled { get; }
    ISelectorSwitchVoltageSettings SwitchVoltageSettings { get; }
    ITransferCaseVoltageSettings TransferCaseVoltageSettings { get; }
}
