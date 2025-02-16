using Meadow;
using Meadow.Devices;
using System.Threading.Tasks;

namespace Neoteric.TransferCase.F7;

public class MeadowApp : App<F7FeatherV2>
{
    private ITransferCaseController _controller;

    public override Task Initialize()
    {
        Resolver.Log.Info("Settings:");
        foreach (var kvp in Resolver.App.Settings)
        {
            Resolver.Log.Info($"  {kvp.Key}: {kvp.Value}");
        }

        // TODO: if it's a BW4419, we don't handle that.  Need to refactor for that 
        var settings = new TransferCaseSettings<FordSwitchSettings, MP3023Settings>(Resolver.App.Settings);

        _controller = NTCCHardware.Create(settings);

        return base.Initialize();
    }

    public override Task Run()
    {
        return base.Run();
    }

}