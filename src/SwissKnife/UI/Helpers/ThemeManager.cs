using System;
using System.IO;
using System.Text.Json;
using WpfApplication = System.Windows.Application;
using System.Windows;

namespace SwissKnife.UI.Helpers;

/// <summary>
/// Manages application theme switching between Light and Dark themes.
/// </summary>
public static class ThemeManager
{
    public enum Theme
    {
        Dark,
        Light
    }

    private const string DarkThemeUri = "UI/Themes/DarkTheme.xaml";
    private const string LightThemeUri = "UI/Themes/LightTheme.xaml";
    private const string ConfigFileName = "swissknife.config.json";

    private static Theme _currentTheme = Theme.Dark;
    private static string ConfigFilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "SwissKnife",
        ConfigFileName
    );

    /// <summary>
    /// Gets the currently applied theme.
    /// </summary>
    public static Theme CurrentTheme => _currentTheme;

    /// <summary>
    /// Event raised when the theme changes.
    /// </summary>
    public static event EventHandler<Theme>? ThemeChanged;

    /// <summary>
    /// Applies the specified theme to the application.
    /// </summary>
    /// <param name="theme">The theme to apply.</param>
    public static void ApplyTheme(Theme theme)
    {
        if (_currentTheme == theme)
        {
            return;
        }

        var themeUri = theme == Theme.Dark ? DarkThemeUri : LightThemeUri;
        var newTheme = new ResourceDictionary
        {
            Source = new Uri(themeUri, UriKind.Relative)
        };

        // Replace the first merged dictionary (the theme dictionary)
        if (WpfApplication.Current.Resources.MergedDictionaries.Count > 0)
        {
            WpfApplication.Current.Resources.MergedDictionaries[0] = newTheme;
        }
        else
        {
            WpfApplication.Current.Resources.MergedDictionaries.Insert(0, newTheme);
        }

        _currentTheme = theme;
        SaveThemePreference(theme);
        ThemeChanged?.Invoke(null, theme);
    }

    /// <summary>
    /// Loads and applies the saved theme preference, or defaults to Dark theme.
    /// </summary>
    public static void LoadSavedTheme()
    {
        var savedTheme = LoadThemePreference();
        ApplyTheme(savedTheme);
    }

    /// <summary>
    /// Toggles between Light and Dark themes.
    /// </summary>
    public static void ToggleTheme()
    {
        var newTheme = _currentTheme == Theme.Dark ? Theme.Light : Theme.Dark;
        ApplyTheme(newTheme);
    }

    private static void SaveThemePreference(Theme theme)
    {
        try
        {
            var configDir = Path.GetDirectoryName(ConfigFilePath);
            if (!string.IsNullOrEmpty(configDir) && !Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir);
            }

            var config = new { Theme = theme.ToString() };
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigFilePath, json);
        }
        catch
        {
            // Fallback silently if save fails
        }
    }

    private static Theme LoadThemePreference()
    {
        try
        {
            if (File.Exists(ConfigFilePath))
            {
                var json = File.ReadAllText(ConfigFilePath);
                var config = JsonSerializer.Deserialize<ThemeConfig>(json);

                if (config?.Theme != null && Enum.TryParse<Theme>(config.Theme, out var theme))
                {
                    return theme;
                }
            }
        }
        catch
        {
            // Fallback to default
        }

        return Theme.Dark; // Default theme
    }

    private class ThemeConfig
    {
        public string? Theme { get; set; }
    }
}
