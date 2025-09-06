using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Neoteric.Desktop.Models;

public class DeviceSetting : INotifyPropertyChanged
{
    private string _key;
    private string _value;
    private string _category;
    private string _originalValue;

    public string Category
    {
        get => _category;
        set
        {
            _category = value;
            OnPropertyChanged();
        }
    }

    public string Key
    {
        get => _key;
        set
        {
            _key = value;
            OnPropertyChanged();
        }
    }

    public string Value
    {
        get => _value;
        set
        {
            if (_value != value)
            {
                _value = value;
                OnPropertyChanged();
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public string FullKey => $"{Category}:{Key}";
    public bool IsModified => _originalValue != Value;

    public event EventHandler ValueChanged;
    public event PropertyChangedEventHandler PropertyChanged;

    public void Initialize(string category, string key, string value)
    {
        Category = category;
        Key = key;
        _value = value;
        _originalValue = value;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
