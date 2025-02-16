using Avalonia.Controls;
using Neoteric.Desktop.Models;
using Neoteric.Desktop.ViewModels;

namespace Neoteric.Desktop;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var windowService = new WindowService(this);
        DataContext = new MainWindowViewModel(windowService);
    }
}