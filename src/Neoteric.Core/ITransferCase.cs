using System;
using System.Threading.Tasks;

namespace Neoteric;

public interface ITransferCase
{
    event EventHandler<TransferCasePosition>? GearChanged;

    TransferCasePosition CurrentGear { get; }
    bool IsShifting { get; }
    Task ShiftTo(TransferCasePosition position);
}
