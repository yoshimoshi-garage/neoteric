using Meadow;
using Meadow.Devices;
using System.Threading.Tasks;

namespace Neoteric.TransferCase.F7;

public class MeadowApp : App<F7FeatherV2>
{
    private ITransferCaseController _controller;

    public override Task Initialize()
    {
        _controller = NTCCHardware.Create(true);

        return base.Initialize();
    }

    public override Task Run()
    {
        return base.Run();
    }

}