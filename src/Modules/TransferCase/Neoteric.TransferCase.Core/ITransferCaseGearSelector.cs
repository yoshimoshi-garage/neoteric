using System;
using System.Collections.Generic;

namespace Neoteric.TransferCase.Core;

public interface ITransferCaseGearSelector : IEnumerable<TransferCaseSwitchSelectionBounds>
{
    event EventHandler<TransferCasePosition>? RequestedPositionChanged;
}
