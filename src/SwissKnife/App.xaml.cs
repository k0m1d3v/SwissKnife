using System.Windows;
using WpfApplication = System.Windows.Application;
using SwissKnife.UI.Helpers;

namespace SwissKnife;

public partial class App : WpfApplication
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Load saved theme preference
        ThemeManager.LoadSavedTheme();
    }
}
