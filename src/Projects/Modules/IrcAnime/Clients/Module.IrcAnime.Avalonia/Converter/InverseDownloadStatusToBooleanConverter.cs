using Avalonia.Data.Converters;
using Module.IrcAnime.Avalonia.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Module.IrcAnime.Avalonia.Converter
{
    public class InverseDownloadStatusToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PackageStatus packageStatus)
            {
                if (parameter is PackageStatus statusToCheck)
                {
                    return !packageStatus.HasFlag(statusToCheck);
                }
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
