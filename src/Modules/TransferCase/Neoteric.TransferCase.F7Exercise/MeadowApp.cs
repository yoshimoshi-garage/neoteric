using Meadow;
using Meadow.Devices;
using Meadow.Hardware;
using Neoteric.TransferCase;
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
        return RawHardwareExercises.ReadSelectionJumper(Device.Pins.D06);
        //return RawHardwareExercises.SimpleRelayCycles(Device.Pins.D01, Device.Pins.D00);
    }
}

public static class RawHardwareExercises
{
    public static async Task SimpleRelayCycles(IPin pinA, IPin pinB)
    {
        var motor = new GearSelectionMotor(pinA, pinB);

        while (true)
        {
            Resolver.Log.Info("Shift up...");
            motor.BeginShiftUp();
            await Task.Delay(1000);
            Resolver.Log.Info("Stop...");
            motor.StopShift();
            await Task.Delay(1000);
            Resolver.Log.Info("Shift down...");
            motor.BeginShiftDown();
            await Task.Delay(1000);
            Resolver.Log.Info("Stop...");
            motor.StopShift();
            await Task.Delay(1000);
        }
    }

    public static async Task ReadSelectionJumper(IPin jumperPin)
    {
        while (true)
        {
            using var selectionPort = jumperPin.CreateDigitalInputPort(Meadow.Hardware.ResistorMode.Disabled);
            if (selectionPort.State)
            {
                Resolver.Log.Info("SELECTION JUMPER IS HIGH");
            }
            else
            {
                Resolver.Log.Info("SELECTION JUMPER IS LOW");
            }

            await Task.Delay(3000);
        }
    }
}
