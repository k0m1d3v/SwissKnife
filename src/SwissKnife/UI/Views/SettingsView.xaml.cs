using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WpfUserControl = System.Windows.Controls.UserControl;
using WpfApplication = System.Windows.Application;
using SwissKnife.UI.Helpers;

namespace SwissKnife.UI.Views;

public partial class SettingsView : WpfUserControl
{
    public SettingsView()
    {
        InitializeComponent();
        Loaded += SettingsView_Loaded;
        ThemeManager.ThemeChanged += OnThemeChanged;
    }

    private void SettingsView_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateThemeSelection(ThemeManager.CurrentTheme);
    }

    private void OnThemeChanged(object? sender, ThemeManager.Theme theme)
    {
        UpdateThemeSelection(theme);
    }

    private void DarkThemeCard_Click(object sender, MouseButtonEventArgs e)
    {
        ThemeManager.ApplyTheme(ThemeManager.Theme.Dark);
    }

    private void LightThemeCard_Click(object sender, MouseButtonEventArgs e)
    {
        ThemeManager.ApplyTheme(ThemeManager.Theme.Light);
    }

    private void UpdateThemeSelection(ThemeManager.Theme currentTheme)
    {
        var accentBrush = (SolidColorBrush)WpfApplication.Current.Resources["AccentBrush"];
        var borderBrush = (SolidColorBrush)WpfApplication.Current.Resources["BorderBrushLight"];

        if (currentTheme == ThemeManager.Theme.Dark)
        {
            DarkThemeCard.BorderBrush = accentBrush;
            DarkThemeCard.BorderThickness = new Thickness(2);
            LightThemeCard.BorderBrush = borderBrush;
            LightThemeCard.BorderThickness = new Thickness(2);
        }
        else
        {
            DarkThemeCard.BorderBrush = borderBrush;
            DarkThemeCard.BorderThickness = new Thickness(2);
            LightThemeCard.BorderBrush = accentBrush;
            LightThemeCard.BorderThickness = new Thickness(2);
        }
    }
}
