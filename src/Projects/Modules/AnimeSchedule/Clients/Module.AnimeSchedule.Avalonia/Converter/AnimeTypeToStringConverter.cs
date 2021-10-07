using Avalonia.Data.Converters;
using Module.AnimeSchedule.Avalonia.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Module.AnimeSchedule.Avalonia.Converter
{
    public class AnimeTypeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AnimeType type)
            {
                return type switch
                {
                    AnimeType.Crunchyroll => "Crunchyroll",
                    AnimeType.Nibl => "Nibl",
                    _ => "Unknown Type",
                };
            }

            throw new InvalidOperationException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
