using Avalonia.Controls;
using System;

namespace Neoteric.Desktop.Models;

public class WindowService : IWindowService
{
    private readonly Window _window;

    public WindowService(Window window)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window));
    }

    public void InvalidateWindow()
    {
        _window.InvalidateMeasure();
        _window.SizeToContent = SizeToContent.WidthAndHeight;
    }
}
