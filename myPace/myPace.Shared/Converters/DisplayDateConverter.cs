using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;

namespace myPace.Shared.Converters
{
    public class DisplayDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var dateTimeOffset = (DateTimeOffset)value;
            return dateTimeOffset.DateTime.ToShortDateString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
