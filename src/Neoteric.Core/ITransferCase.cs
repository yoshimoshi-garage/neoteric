using System;
using System.Threading.Tasks;

namespace Neoteric;

public interface ITransferCaseController
{
}

public interface ITransferCase
{
    event EventHandler<TransferCasePosition>? GearChanging;
    event EventHandler<TransferCasePosition>? GearChanged;

    void StartControlLoop();
    TransferCasePosition CurrentGear { get; }
    bool IsShifting { get; }
    Task ShiftTo(TransferCasePosition position);
}
