using System.Windows;
using System.Windows.Controls;
using SwissKnife.UI.Models;

namespace SwissKnife.UI.Helpers;

public sealed class CategoryTemplateSelector : DataTemplateSelector
{
    public DataTemplate? CryptoTemplate { get; set; }
    public DataTemplate? PdfTemplate { get; set; }
    public DataTemplate? ConvertTemplate { get; set; }
    public DataTemplate? UtilityTemplate { get; set; }
    public DataTemplate? SettingsTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        if (item is not CategoryItem category)
        {
            return base.SelectTemplate(item, container);
        }

        return category.Key.ToLowerInvariant() switch
        {
            "crypto" => CryptoTemplate,
            "pdf" => PdfTemplate,
            "convert" => ConvertTemplate,
            "utility" => UtilityTemplate,
            "settings" => SettingsTemplate,
            _ => base.SelectTemplate(item, container)
        };
    }
}
