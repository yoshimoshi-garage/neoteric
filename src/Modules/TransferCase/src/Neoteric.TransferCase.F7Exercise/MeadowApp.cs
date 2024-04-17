using Meadow;
using Meadow.Devices;
using System.Threading.Tasks;

namespace Neaoteric.TransferCase.Test;

public class MeadowApp : App<F7FeatherV2>
{
    public override Task Initialize()
    {

        return base.Initialize();
    }

    public override Task Run()
    {
        return RawHardwareExercises.ReadAnalogGearPosition(Device.Pins.A01);
        //return RawHardwareExercises.ReadSelectorSwitch(Device.Pins.A00);
        //return RawHardwareExercises.ReadEnableInterlock(Device.Pins.D05);
        //return RawHardwareExercises.ReadSelectionJumper(Device.Pins.D06);
        //return RawHardwareExercises.SimpleRelayCycles(Device.Pins.D01, Device.Pins.D00);
        //return RawHardwareExercises.ReadInputSwitches(
        //    Device.Pins.D13, Device.Pins.D12, Device.Pins.D11, Device.Pins.D10);
    }
}
