using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace Tinkoff.Acquiring.UI.Model
{
    class UriToBitmapImageConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType, Object parameter, String language)
        {
            if (String.IsNullOrEmpty(value as String))
            {
                return null;
            }
            return new BitmapImage(new Uri((String) value, UriKind.RelativeOrAbsolute));
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, String language)
        {
            throw new NotImplementedException();
        }
    }
}