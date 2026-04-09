using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using Tldr.Core;

namespace Tldr.Platform;

public sealed class DetailLevelToDoubleConverter : MarkupExtension, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is DetailLevel dl ? (double)dl : 1.0;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is double d ? (DetailLevel)(int)Math.Round(d) : DetailLevel.Standard;

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}
