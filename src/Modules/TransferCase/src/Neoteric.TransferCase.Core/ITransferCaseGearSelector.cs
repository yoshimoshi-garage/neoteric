﻿using System;
using System.Collections.Generic;

namespace Neoteric.TransferCase;

public interface ITransferCaseGearSelector : IEnumerable<TransferCaseSwitchSelectionBounds>
{
    event EventHandler<TransferCasePosition>? RequestedPositionChanged;

    TransferCasePosition RequestedPosition { get; }
    TransferCasePosition CurrentSwitchPosition { get; }
}
