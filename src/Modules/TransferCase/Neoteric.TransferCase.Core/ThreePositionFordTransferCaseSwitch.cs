using Meadow.Hardware;
using Meadow.Units;

namespace Neoteric.TransferCase;

public class ThreePositionFordTransferCaseSwitch : AnalogTransferCaseGearSelectorSwitch
{
    private static TransferCaseSwitchSelectionBounds[] GenerateRequesters()
    {
        return new TransferCaseSwitchSelectionBounds[]
        {
            new TransferCaseSwitchSelectionBounds
            {
                Position = TransferCasePosition.Low4,
                SwitchResistance = 130.Ohms(),
                MinVoltage = 0.40.Volts(),
                MaxVoltage = 0.80.Volts()
            },
            new TransferCaseSwitchSelectionBounds
            {
                Position = TransferCasePosition.High4,
                SwitchResistance = 270.Ohms(),
                MinVoltage = 0.80.Volts(),
                MaxVoltage = 1.20.Volts()
            },
            new TransferCaseSwitchSelectionBounds
            {
                Position = TransferCasePosition.High2,
                SwitchResistance = 620.Ohms(),
                MinVoltage = 1.20.Volts(),
                MaxVoltage = 1.80.Volts()
            }
        };
    }

    public ThreePositionFordTransferCaseSwitch(IAnalogInputPort inputPort)
        : base(inputPort, GenerateRequesters())

    {
    }
}