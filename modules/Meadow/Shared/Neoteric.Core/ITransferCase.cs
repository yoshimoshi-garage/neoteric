using System;

namespace Neoteric;

public interface ITransferCase
{
    event EventHandler<TransferCasePosition>? GearChanging;
    event EventHandler<TransferCasePosition>? GearChanged;

    void StartControlLoop();
    TransferCasePosition CurrentGear { get; }
    bool IsShifting { get; }
    void RequestShiftTo(TransferCasePosition position);
}
