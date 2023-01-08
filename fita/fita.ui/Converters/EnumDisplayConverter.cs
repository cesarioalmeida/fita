using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace fita.ui.Converters;

public class EnumDisplayConverter : MarkupExtension, IValueConverter
{
    public override object ProvideValue(IServiceProvider serviceProvider) => this;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var fieldInfo = value?.GetType().GetField(value.ToString() ?? string.Empty);

        if (fieldInfo is null)
        {
            return DependencyProperty.UnsetValue;
        }
            
        var attributes = (DisplayAttribute[])fieldInfo.GetCustomAttributes(typeof(DisplayAttribute), false);
        return attributes.ElementAt(0).Name ?? value.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}