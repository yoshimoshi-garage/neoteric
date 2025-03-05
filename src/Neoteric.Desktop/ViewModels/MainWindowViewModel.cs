using Meadow.CLI;
using Meadow.CLI.Commands.DeviceManagement;
using Meadow.Hcom;
using Neoteric.Desktop.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

namespace Neoteric.Desktop.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private MeadowConnectionManager _connectionManager;
        private ObservableCollection<DeviceSetting> _originalSystemItems;
        private ObservableCollection<DeviceSetting> _originalSwitchItems;
        private ObservableCollection<DeviceSetting> _originalTCaseItems;
        private string _selectedPort;
        private Dictionary<string, string> _changedSettings = new Dictionary<string, string>();
        private Dictionary<string, object> _otherSystemSettings = new Dictionary<string, object>();
        private readonly IWindowService _windowService;

        public bool HasChanges => _changedSettings.Count > 0;

        public ObservableCollection<DeviceSetting> SystemItems { get; } = new ObservableCollection<DeviceSetting>();
        public ObservableCollection<DeviceSetting> SwitchItems { get; } = new ObservableCollection<DeviceSetting>();
        public ObservableCollection<DeviceSetting> TCaseItems { get; } = new ObservableCollection<DeviceSetting>();
        public ObservableCollection<string> AvailablePorts { get; } = new ObservableCollection<string>();

        public string SelectedPort
        {
            get => _selectedPort;
            set => this.RaiseAndSetIfChanged(ref _selectedPort, value);
        }

        private string _statusMessage;
        private bool _hasError;

        public string StatusMessage
        {
            get => _statusMessage;
            set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
        }

        public bool HasError
        {
            get => _hasError;
            set => this.RaiseAndSetIfChanged(ref _hasError, value);
        }

        public ReactiveCommand<Unit, Unit> ApplyCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }
        public ReactiveCommand<Unit, Unit> RefreshPortsCommand { get; }
        public ReactiveCommand<Unit, Unit> GetDataCommand { get; }

        public MainWindowViewModel(IWindowService windowService)
        {
            _windowService = windowService;
            _connectionManager = new MeadowConnectionManager(new SettingsManager());
            RefreshPorts();

            ApplyCommand = ReactiveCommand.Create(ExecuteApplyCommand);
            CancelCommand = ReactiveCommand.Create(ExecuteCancelCommand);
            RefreshPortsCommand = ReactiveCommand.Create(RefreshPorts);
            GetDataCommand = ReactiveCommand.CreateFromTask(GetDataFromDevice);
        }

        private async Task GetDataFromDevice()
        {
            if (string.IsNullOrEmpty(SelectedPort))
                return;

            try
            {
                StatusMessage = "Reading device settings...";
                HasError = false;

                IMeadowConnection? connection = null;

                connection = _connectionManager.GetConnectionForRoute(SelectedPort);

                if (connection == null)
                {
                    StatusMessage = "Unable to find device on port";
                    HasError = true;
                    return;
                }

                await connection.Attach();
                var settingsManager = new MeadowSettingsManager(connection.Device);
                var settings = await settingsManager.ReadAppSettings();

                // Clear all collections
                SystemItems.Clear();
                SwitchItems.Clear();
                TCaseItems.Clear();
                _otherSystemSettings.Clear();

                // Load Interlock settings
                if (settings.TryGetValue("Interlock", out var interlockSettings))
                {
                    var subSettings = (Dictionary<object, object>)interlockSettings;
                    foreach (var subSetting in subSettings)
                    {
                        var newSetting = new DeviceSetting();
                        newSetting.Initialize("Interlock", subSetting.Key.ToString(), subSetting.Value.ToString());
                        newSetting.ValueChanged += Setting_ValueChanged;
                        SystemItems.Add(newSetting);
                    }
                }

                // Load GearLock settings
                if (settings.TryGetValue("GearLock", out var gearLockSettings))
                {
                    var subSettings = (Dictionary<object, object>)gearLockSettings;
                    foreach (var subSetting in subSettings)
                    {
                        var newSetting = new DeviceSetting();
                        newSetting.Initialize("GearLock", subSetting.Key.ToString(), subSetting.Value.ToString());
                        newSetting.ValueChanged += Setting_ValueChanged;
                        SystemItems.Add(newSetting);
                    }
                }

                // Load Switch settings
                if (settings.TryGetValue("Switch", out var switchSettings))
                {
                    foreach (var setting in (Dictionary<object, object>)switchSettings)
                    {
                        var subSettings = (Dictionary<object, object>)setting.Value;
                        foreach (var subSetting in subSettings)
                        {
                            var newSetting = new DeviceSetting();
                            var settingKey = $"{setting.Key}:{subSetting.Key}";
                            newSetting.Initialize("Switch", settingKey, subSetting.Value.ToString());
                            newSetting.ValueChanged += Setting_ValueChanged;
                            SwitchItems.Add(newSetting);
                        }
                    }
                }

                // Load TCase settings
                if (settings.TryGetValue("TCase", out var tcaseSettings))
                {
                    foreach (var setting in (Dictionary<object, object>)tcaseSettings)
                    {
                        var subSettings = (Dictionary<object, object>)setting.Value;
                        foreach (var subSetting in subSettings)
                        {
                            var newSetting = new DeviceSetting();
                            var settingKey = $"{setting.Key}:{subSetting.Key}";
                            newSetting.Initialize("TCase", settingKey, subSetting.Value.ToString());
                            newSetting.ValueChanged += Setting_ValueChanged;
                            TCaseItems.Add(newSetting);
                        }
                    }
                }

                // Store other system settings
                foreach (var kvp in settings)
                {
                    if (kvp.Key != "Switch" && kvp.Key != "TCase" &&
                        kvp.Key != "Interlock" && kvp.Key != "GearLock")
                    {
                        _otherSystemSettings[kvp.Key] = kvp.Value;
                    }
                }

                _changedSettings.Clear();
                SaveOriginalData();
                _windowService.InvalidateWindow();
                StatusMessage = "Settings loaded successfully";
            }
            catch (System.Exception ex)
            {
                HasError = true;
                StatusMessage = $"Error reading settings: {ex.Message}";
            }
        }

        private async void RefreshPorts()
        {
            try
            {
                StatusMessage = "Scanning for devices...";
                HasError = false;

                AvailablePorts.Clear();
                var ports = await MeadowConnectionManager.GetSerialPorts();
                foreach (var port in ports)
                {
                    AvailablePorts.Add(port);
                }

                if (AvailablePorts.Any())
                {
                    SelectedPort = AvailablePorts.First();
                    StatusMessage = "Device scan complete";
                }
                else
                {
                    StatusMessage = "No devices found";
                }
            }
            catch (System.Exception ex)
            {
                HasError = true;
                StatusMessage = $"Error scanning for devices: {ex.Message}";
            }
        }

        private void SaveOriginalData()
        {
            _originalSystemItems = new ObservableCollection<DeviceSetting>(
                SystemItems.Select(item => new DeviceSetting { Key = item.Key, Value = item.Value }));
            _originalSwitchItems = new ObservableCollection<DeviceSetting>(
                SwitchItems.Select(item => new DeviceSetting { Key = item.Key, Value = item.Value }));
            _originalTCaseItems = new ObservableCollection<DeviceSetting>(
                TCaseItems.Select(item => new DeviceSetting { Key = item.Key, Value = item.Value }));
        }

        private void Setting_ValueChanged(object sender, EventArgs e)
        {
            var setting = (DeviceSetting)sender;
            if (setting.IsModified)
            {
                _changedSettings[setting.FullKey] = setting.Value;
            }
            else
            {
                _changedSettings.Remove(setting.FullKey);
            }
        }

        private async void ExecuteApplyCommand()
        {
            if (string.IsNullOrEmpty(SelectedPort))
                return;

            try
            {
                StatusMessage = "Sending settings to device...";
                HasError = false;

                var connection = _connectionManager.GetConnectionForRoute(SelectedPort);

                if (connection == null)
                {
                    StatusMessage = "Unable to find device on port";
                    HasError = true;
                    return;
                }

                await connection.Attach();
                var settingsManager = new MeadowSettingsManager(connection.Device);

                if (await connection.Device.IsRuntimeEnabled())
                {
                    await connection.Device.RuntimeDisable();
                }

                var allSettings = new Dictionary<string, string>();

                // Add other system settings
                foreach (var setting in _otherSystemSettings)
                {
                    if (setting.Value is Dictionary<object, object> subDict)
                    {
                        FlattenDictionary(subDict, setting.Key, allSettings);
                    }
                    else
                    {
                        allSettings[setting.Key] = setting.Value.ToString();
                    }
                }

                // Add displayed system settings
                foreach (var item in SystemItems)
                {
                    allSettings[$"{item.Category}:{item.Key}"] = item.Value;
                }

                // Add Switch settings
                foreach (var item in SwitchItems)
                {
                    allSettings[$"{item.Category}:{item.Key}"] = item.Value;
                }

                // Add TCase settings
                foreach (var item in TCaseItems)
                {
                    allSettings[$"{item.Category}:{item.Key}"] = item.Value;
                }

                await settingsManager.WriteAppSetting(allSettings);
                _changedSettings.Clear();
                SaveOriginalData();
                StatusMessage = "Settings saved successfully";
            }
            catch (System.Exception ex)
            {
                HasError = true;
                StatusMessage = $"Error saving settings: {ex.Message}";
            }
        }

        private void ExecuteCancelCommand()
        {
            StatusMessage = "Changes cancelled";
            HasError = false;

            SystemItems.Clear();
            SwitchItems.Clear();
            TCaseItems.Clear();

            foreach (var item in _originalSystemItems)
            {
                SystemItems.Add(new DeviceSetting { Key = item.Key, Value = item.Value });
            }

            foreach (var item in _originalSwitchItems)
            {
                SwitchItems.Add(new DeviceSetting { Key = item.Key, Value = item.Value });
            }

            foreach (var item in _originalTCaseItems)
            {
                TCaseItems.Add(new DeviceSetting { Key = item.Key, Value = item.Value });
            }

            _changedSettings.Clear();
        }

        private void FlattenDictionary(Dictionary<object, object> dict, string prefix, Dictionary<string, string> result)
        {
            foreach (var kvp in dict)
            {
                var newKey = string.IsNullOrEmpty(prefix) ? kvp.Key.ToString() : $"{prefix}:{kvp.Key}";

                if (kvp.Value is Dictionary<object, object> subDict)
                {
                    FlattenDictionary(subDict, newKey, result);
                }
                else
                {
                    result[newKey] = kvp.Value.ToString();
                }
            }
        }
    }
}