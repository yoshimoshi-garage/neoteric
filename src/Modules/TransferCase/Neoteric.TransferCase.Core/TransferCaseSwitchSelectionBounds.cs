using Meadow.Units;

namespace Neoteric.TransferCase;

public struct TransferCaseSwitchSelectionBounds
{
    public TransferCasePosition Position { get; set; }
    public Resistance SwitchResistance { get; set; }
    public Voltage MinVoltage { get; set; }
    public Voltage MaxVoltage { get; set; }

    public readonly Voltage MidRangeVoltage => (MaxVoltage + MinVoltage) / 2;

    public readonly bool IsActive(Voltage testVoltage)
    {
        if (testVoltage < MinVoltage) return false;
        if (testVoltage > MaxVoltage) return false;
        return true;
    }
}
