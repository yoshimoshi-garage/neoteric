using Meadow.Hardware;
using Meadow.Units;

namespace Neoteric.TransferCase;

public class ThreePositionFordTransferCaseSwitch : AnalogTransferCaseGearSelectorSwitch
{
    private static TransferCaseSwitchSelectionBounds[] GenerateRequesters(ISelectorSwitchVoltageSettings settings)
    {
        return new TransferCaseSwitchSelectionBounds[]
        {
            // measures resistances with TCC v1b
            // OPEN: 2.72V
            //       ----- 2.25
            //  612: 1.74V
            //       ----- 1.52
            //  272: 1.31V
            //       ----- 1.23
            //  126: 1.15V
            new TransferCaseSwitchSelectionBounds
            {
                Position = TransferCasePosition.Low4,
                SwitchResistance = 130.Ohms(),
                MinVoltage = settings.Low4Min.Volts(), // 1.00.Volts(),
                MaxVoltage = settings.Low4Max.Volts(), // 1.23.Volts()
            },
            new TransferCaseSwitchSelectionBounds
            {
                Position = TransferCasePosition.High4,
                SwitchResistance = 270.Ohms(),
                MinVoltage = settings.High4Min.Volts(), // 1.23.Volts(),
                MaxVoltage = settings.High4Max.Volts(), // 1.52.Volts()
            },
            new TransferCaseSwitchSelectionBounds
            {
                Position = TransferCasePosition.High2,
                SwitchResistance = 620.Ohms(),
                MinVoltage = settings.High2Min.Volts(), // 1.52.Volts(),
                MaxVoltage = settings.High2Max.Volts(), // 2.25.Volts()
            }
        };
    }

    public ThreePositionFordTransferCaseSwitch(IAnalogInputPort inputPort, ISelectorSwitchVoltageSettings settings)
        : base(inputPort, GenerateRequesters(settings))

    {
        ReportSettings();
    }
}