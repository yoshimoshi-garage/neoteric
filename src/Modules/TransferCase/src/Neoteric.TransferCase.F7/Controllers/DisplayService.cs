using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.MicroLayout;
using Meadow.Peripherals.Displays;

namespace Neoteric.TransferCase.F7;

internal class DisplayService
{
    private IPixelDisplay _display;
    private DisplayScreen _screen;

    private Label[] _labels;
    private int _currentRow = 0;
    private int _rows = 5;

    public DisplayService(IPixelDisplay display)
    {
        _display = display;
        _screen = new DisplayScreen(display, RotationType._180Degrees);

        var font = new Font8x12();
        var y = 0;

        var rowHeight = 13;

        _labels = new Label[_rows];
        for (var r = 0; r < _rows; r++)
        {
            _labels[r] = new Label(0, y, _screen.Width, rowHeight)
            {
                Font = font,
                VerticalAlignment = VerticalAlignment.Top,
            };
            y += rowHeight;
        }

        _screen.Controls.Add(_labels);
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
        _screen.BeginUpdate();

        while (_currentRow >= _rows)
        {
            for (var r = 0; r < _rows - 1; r++)
            {
                _labels[r].Text = _labels[r + 1].Text;
            }
            _currentRow--;
        }

        _labels[_currentRow].Text = message;
        _currentRow++;

        _screen.EndUpdate();
    }
}
