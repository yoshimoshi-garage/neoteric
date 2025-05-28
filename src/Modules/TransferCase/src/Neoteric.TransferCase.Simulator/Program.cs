using Neoteric;

namespace Neaoteric.TransferCase.Simulator;

internal class Program
{
    static void Main(string[] args)
    {
        var controller = new NTCCSim();

        while (true)
        {
            Console.ReadKey();
            Console.WriteLine("---");
            controller.RequestGear(TransferCasePosition.High2);
            Console.ReadKey();
            Console.WriteLine("---");
            controller.RequestGear(TransferCasePosition.High4);
            Console.ReadKey();
            Console.WriteLine("---");
            controller.RequestGear(TransferCasePosition.Low4);
        }
    }
}
