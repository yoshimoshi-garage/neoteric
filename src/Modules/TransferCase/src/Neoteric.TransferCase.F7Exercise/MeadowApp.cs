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

    public override async Task Run()
    {
        //var t1 = RawHardwareExercises.ReadAnalogGearPosition(Device.Pins.A01);
        //var t1 = RawHardwareExercises.ReadSelectorSwitch(Device.Pins.A00);
        //return RawHardwareExercises.ReadEnableInterlock(Device.Pins.D05);
        //return RawHardwareExercises.ReadSelectionJumper(Device.Pins.D06);
        var t1 = RawHardwareExercises.SimpleRelayCycles(Device.Pins.D01, Device.Pins.D00, Device.Pins.D03);
        //return RawHardwareExercises.ReadInputSwitches(
        //    Device.Pins.D13, Device.Pins.D12, Device.Pins.D11, Device.Pins.D10);
        //_ = RawHardwareExercises.ToggleHubLock(Device.Pins.D04);
        //_ = RawHardwareExercises.ToggleMotorLock(Device.Pins.D03);
        await t1;
    }
}
