using System;
using System.Globalization;
using System.Windows.Data;

namespace myMoney.Controler
{
    public class TypToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int typ)
            {
                return typ switch
                {
                    0 => "Zahlung",
                    1 => "Gutschrift",
                    2 => "Transfer",
                    _ => "Unbekannt"
                };
            }
            return "Ungültig";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}