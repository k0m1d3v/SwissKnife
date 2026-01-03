using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Windows.Input;
using SwissKnife.UI.Helpers;
using SwissKnife.UI.Models;

namespace SwissKnife.UI.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private CategoryItem? _selectedCategory;
    private string _statusText = "Pronto";

    public MainViewModel()
    {
        Categories = new ObservableCollection<CategoryItem>
        {
            new() { Key = "crypto", Name = "Crypto", IconGlyph = "\uE8C5", Subtitle = "Strumenti crittografici" },
            new() { Key = "pdf", Name = "PDF", IconGlyph = "\uE8A7", Subtitle = "Toolkit PDF" },
            new() { Key = "convert", Name = "Convert", IconGlyph = "\uE8CB", Subtitle = "Conversioni file" },
            new() { Key = "utility", Name = "Utility", IconGlyph = "\uE770", Subtitle = "Strumenti rapidi" },
            new() { Key = "settings", Name = "Settings", IconGlyph = "\uE713", Subtitle = "Preferenze app" },
        };

        _selectedCategory = Categories[0];
        _selectedCategory.IsSelected = true;

        SelectCategoryCommand = new RelayCommand<string>(OnSelectCategory);
    }

    public ObservableCollection<CategoryItem> Categories { get; }

    public CategoryItem? SelectedCategory
    {
        get => _selectedCategory;
        private set
        {
            if (_selectedCategory != value)
            {
                if (_selectedCategory is not null)
                {
                    _selectedCategory.IsSelected = false;
                }

                _selectedCategory = value;

                if (_selectedCategory is not null)
                {
                    _selectedCategory.IsSelected = true;
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedCategoryDisplayName));
                OnPropertyChanged(nameof(SelectedCategorySubtitle));
            }
        }
    }

    public string SelectedCategoryDisplayName => SelectedCategory?.Name ?? "SwissKnife";

    public string SelectedCategorySubtitle => SelectedCategory?.Subtitle ?? "Dashboard";

    public string StatusText
    {
        get => _statusText;
        set
        {
            if (_statusText != value)
            {
                _statusText = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand SelectCategoryCommand { get; }

    private void OnSelectCategory(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        var target = Categories.FirstOrDefault(c => c.Key == key);
        if (target != null && target != SelectedCategory)
        {
            SelectedCategory = target;
            StatusText = $"{target.Name} pronti";
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
