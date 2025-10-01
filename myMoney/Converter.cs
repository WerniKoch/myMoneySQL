using System;
using System.Windows.Data;
using System.Windows.Markup;

namespace myMoney
{
    public class MoneyValueToBrushConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            decimal num;
            if (!Decimal.TryParse(value.ToString(), out num))
            {
                return "Black";
            }
            return num >= 0 ? "Black" : "Red";
        }

        public object? ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
