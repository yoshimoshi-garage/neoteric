using Meadow;
using Meadow.Hardware;
using Neoteric.TransferCase;
using System.Threading.Tasks;

namespace Neaoteric.TransferCase.Test;

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

    public static async Task ReadInputSwitches(
        IPin sw1, IPin sw2, IPin sw3, IPin sw4)
    {
        var in1 = sw1.CreateDigitalInputPort(ResistorMode.ExternalPullDown);
        var in2 = sw2.CreateDigitalInputPort(ResistorMode.ExternalPullDown);
        var in3 = sw3.CreateDigitalInputPort(ResistorMode.ExternalPullDown);
        var in4 = sw4.CreateDigitalInputPort(ResistorMode.ExternalPullDown);

        while (true)
        {
            Resolver.Log.Info($"SWITCHES: {(in1.State ? 1 : 0)} {(in2.State ? 1 : 0)} {(in3.State ? 1 : 0)} {(in4.State ? 1 : 0)}");

            await Task.Delay(1000);
        }
    }

    public static async Task ReadEnableInterlock(IPin jumperPin)
    {
        using var enablePort = jumperPin.CreateDigitalInputPort(Meadow.Hardware.ResistorMode.Disabled);

        while (true)
        {
            if (enablePort.State)
            {
                Resolver.Log.Info("ENABLE IS HIGH");
            }
            else
            {
                Resolver.Log.Info("ENABLE IS LOW");
            }

            await Task.Delay(3000);
        }
    }

    public static async Task ReadSelectorSwitch(IPin analogPin)
    {
        var input = analogPin.CreateAnalogInputPort();

        while (true)
        {
            var result = await input.Read();

            Resolver.Log.Info($"Input: {result.Volts:N2} V");

            await Task.Delay(1000);
        }
    }

    public static async Task ReadAnalogGearPosition(IPin analogPin)
    {
        var input = analogPin.CreateAnalogInputPort();

        while (true)
        {
            var result = await input.Read();

            Resolver.Log.Info($"Position Input: {result.Volts:N2} V");

            await Task.Delay(1000);
        }
    }
}
