using Meadow;
using Meadow.Devices;
using Meadow.Hardware;
using System.Threading.Tasks;
using static Meadow.Foundation.Motors.BidirectionalDcMotor;

namespace Neoteric.TransferCase.F7;

/// <summary>
/// Controller for either the BW4419 or MP3023NQH Transfer cases using the Ford 3-position selector switch
/// </summary>
public class NTCv1b
{
    /*
    F7Featherv2 pin map for the NTCv1b

    A00 TC Gear Selector Switch
    A01 MP3023 Analog Position Sensor

    D00 TC Selector Motor Relay CW
    D01 TC Selector Motor Relay CCW

    D05 Safety Interlock Enable (enable == low)
    D06 Transfer Case Selection (BOTTOM POS (high)==BW4419, TOP POS(low)==MP3023)

    D07 I2C CLK for Optional debug display
    D08 I2C DAT for Optional debug display

    D10 4419 POS SW 4
    D11 4419 POS SW 3
    D12 4419 POS SW 2
    D13 4419 POS SW 1


    */

    private ITransferCase _transferCase;

    public NTCv1b(F7FeatherV2 device)
    {
        // what transfer case are we selecting (J1)

        using var selectionPort = device.Pins.D06.CreateDigitalInputPort(Meadow.Hardware.ResistorMode.Disabled);

        Resolver.Log.Info($" Motor");
        var motor = new GearSelectionMotor(device.Pins.D01, device.Pins.D00);
        motor.StateChanged += OnMotorStateChanged;

        var interlock = new SafetyInterlockSwitch(device.Pins.D05);

        if (selectionPort.State)
        {
            Resolver.Log.Info("SELECTION JUMPER IS HIGH - BW4419");



            Resolver.Log.Info($" Transfer case");
            var sw1 = device.Pins.D13.CreateDigitalInputPort(ResistorMode.InternalPullDown);
            var sw2 = device.Pins.D12.CreateDigitalInputPort(ResistorMode.InternalPullDown);
            var sw3 = device.Pins.D11.CreateDigitalInputPort(ResistorMode.InternalPullDown);
            var sw4 = device.Pins.D10.CreateDigitalInputPort(ResistorMode.InternalPullDown);

            _transferCase = new BW4419(motor, sw1, sw2, sw3, sw4, interlock);

        }
        else
        {
            Resolver.Log.Info("SELECTION JUMPER IS LOW - MP3023NQH");
        }
    }

    private void OnMotorStateChanged(object sender, MotorState e)
    {
    }
}

public class MeadowApp : App<F7FeatherV2>
{
    public override Task Initialize()
    {
        return base.Initialize();
    }

    public override Task Run()
    {
        return base.Run();
    }

}