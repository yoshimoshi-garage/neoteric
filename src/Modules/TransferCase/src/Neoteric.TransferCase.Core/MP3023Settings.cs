namespace Neoteric.TransferCase;

public class MP3023Settings : ITransferCaseVoltageSettings
{
    public double Low4Min { get; set; }
    public double Low4Max { get; set; }
    public double High4Min { get; set; }
    public double High4Max { get; set; }
    public double High2Min { get; set; }
    public double High2Max { get; set; }

    public MP3023Settings()
    {
        Low4Min = 1.4;
        Low4Max = 1.5;
        High4Min = 3.4;
        High4Max = 3.5;
        High2Min = 2.4;
        High2Max = 2.5;
    }
}
