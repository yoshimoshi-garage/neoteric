namespace Neoteric.TransferCase;

public class FordSwitchSettings : ISelectorSwitchVoltageSettings
{
    public double Low4Min { get; set; }
    public double Low4Max { get; set; }
    public double High4Min { get; set; }
    public double High4Max { get; set; }
    public double High2Min { get; set; }
    public double High2Max { get; set; }

    public FordSwitchSettings()
    {
        Low4Min = 1.0;
        Low4Max = 1.23;
        High4Min = 1.23;
        High4Max = 1.52;
        High2Min = 1.52;
        High2Max = 2.25;
    }
}
