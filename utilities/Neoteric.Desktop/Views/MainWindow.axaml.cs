using Avalonia.Controls;
using Neoteric.Desktop.Models;
using Neoteric.Desktop.ViewModels;
using System.Reflection;

namespace Neoteric.Desktop;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var windowService = new WindowService(this);
        DataContext = new MainWindowViewModel(windowService);

        Title = $"Transfer Case Settings Adjustment - {GetAppVersion()}";
    }

    public static string GetAppVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        return version != null ? $"v{version.ToString(3)}" : "v1.0.0";
    }
}