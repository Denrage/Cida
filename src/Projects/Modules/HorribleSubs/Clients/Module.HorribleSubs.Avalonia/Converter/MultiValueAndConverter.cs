using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Module.HorribleSubs.Avalonia.Converter
{
    public class MultiValueAndConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (var item in values)
            {
                if (item is BindingNotification notification)
                {
                    if (!notification.HasValue || !(bool)notification.Value)
                    {
                        return false;
                    }
                }
                else if(item is bool booleanValue)
                {
                    if (booleanValue)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
    }
}
