﻿using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.MicroLayout;
using Meadow.Hardware;
using Meadow.Peripherals.Displays;
using System;
using static Meadow.Foundation.Motors.BidirectionalDcMotor;

namespace Neoteric.TransferCase.F7;

internal class DisplayService
{
    private IPixelDisplay _display;
    private DisplayScreen _screen;

    private Label[] _labels;
    private int _currentRow = 0;
    private int _rows = 4;

    public DisplayService(IPixelDisplay display)
    {
        _display = display;
        _screen = new DisplayScreen(display);

        var font = new Font8x12();
        var y = 0;

        var rowHeight = 20;

        _labels = new Label[_rows];
        for (var r = 0; r < _rows; r++)
        {
            _labels[r] = new Label(y, 0, _screen.Width, rowHeight)
            {
                Font = font
            };
            y += rowHeight;
        }
    }

    public void Clear()
    {
        for (var r = 0; r < _rows; r++)
        {
            _labels[r].Text = string.Empty;
            _currentRow = 0;
        }
    }

    public void Report(string message)
    {
        while (_currentRow >= _rows)
        {
            for (var r = 0; r < _rows - 2; r++)
            {
                _labels[r].Text = _labels[r + 1].Text;
            }
            _currentRow--;
        }

        _labels[_currentRow].Text = message;
        _currentRow++;
    }
}

/// <summary>
/// Controller for either the BW4419 or MP3023NQH Transfer cases using the Ford 3-position selector switch
/// </summary>
public class NTCCv1b
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

    private readonly ITransferCase _transferCase;
    private readonly ITransferCaseGearSelector _gearSelector;
    private readonly DisplayService? _displayService;

    public NTCCv1b(F7FeatherV2 device)
    {
        // do we have a display attached?
        var i2c = device.CreateI2cBus(I2cBusSpeed.High);
        try
        {
            var display = new Ssd1306(i2c);
            _displayService = new DisplayService(display);
        }
        catch (Exception ex)
        {
            // TODO: blink a warning
            Resolver.Log.Info("No Debug Display found");
        }


        // what transfer case are we selecting (J1)

        using var selectionPort = device.Pins.D06.CreateDigitalInputPort(Meadow.Hardware.ResistorMode.Disabled);

        Resolver.Log.Info($" Motor");
        var motor = new GearSelectionMotor(device.Pins.D01, device.Pins.D00);
        motor.StateChanged += OnMotorStateChanged;

        var interlock = new SafetyInterlockSwitch(device.Pins.D05);

        if (selectionPort.State)
        {
            Resolver.Log.Info("SELECTION JUMPER IS HIGH - BW4419");

            _displayService?.Report("BW4419");

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
            _displayService?.Report("MP3023NQH");

            _transferCase = new MP3023NQH(motor, device.Pins.A01.CreateAnalogInputPort(), interlock);
        }

        _gearSelector = new ThreePositionFordTransferCaseSwitch(device.Pins.A00.CreateAnalogInputPort());
    }

    private void OnMotorStateChanged(object sender, MotorState e)
    {
    }
}