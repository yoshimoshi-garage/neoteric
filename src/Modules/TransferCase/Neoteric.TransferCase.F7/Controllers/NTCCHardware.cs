﻿using Meadow;
using Meadow.Devices;
using System;

namespace Neoteric.TransferCase.F7;

public static class NTCCHardware
{
    public static ITransferCaseController Create(bool interlockEnabled = true)
    {
        // we currently support only 1 controller NTCC v.1b
        if (Resolver.Device is F7FeatherV2 f7)
        {
            return new NTCCv1b(f7, interlockEnabled);
        }

        throw new Exception("Only F7Feather V2 compute modules are currently supported");
    }

}
