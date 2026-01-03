using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SwissKnife.UI.Models;

public sealed class CategoryItem : INotifyPropertyChanged
{
    private bool _isSelected;

    public required string Key { get; init; }
    public required string Name { get; init; }
    public required string IconGlyph { get; init; }
    public string? Subtitle { get; init; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
