using Meadow.Hardware;
using Meadow.Units;

namespace Neoteric.TransferCase;

public class ThreePositionFordTransferCaseSwitch : AnalogTransferCaseGearSelectorSwitch
{
    private static TransferCaseSwitchSelectionBounds[] GenerateRequesters()
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
                MinVoltage = 1.00.Volts(),
                MaxVoltage = 1.23.Volts()
            },
            new TransferCaseSwitchSelectionBounds
            {
                Position = TransferCasePosition.High4,
                SwitchResistance = 270.Ohms(),
                MinVoltage = 1.23.Volts(),
                MaxVoltage = 1.52.Volts()
            },
            new TransferCaseSwitchSelectionBounds
            {
                Position = TransferCasePosition.High2,
                SwitchResistance = 620.Ohms(),
                MinVoltage = 1.52.Volts(),
                MaxVoltage = 2.25.Volts()
            }
        };
    }

    public ThreePositionFordTransferCaseSwitch(IAnalogInputPort inputPort)
        : base(inputPort, GenerateRequesters())

    {
    }
}