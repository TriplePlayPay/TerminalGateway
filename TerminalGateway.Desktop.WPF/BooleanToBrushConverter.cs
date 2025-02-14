using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TerminalGateway.Desktop.WPF
{
    public class BooleanToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isConnected = (value is bool b && b);

            if (isConnected)
            {
                // Example green gradient
                return new LinearGradientBrush(
                    Color.FromRgb(0x00, 0x80, 0x00),  // start color (darkish green)
                    Color.FromRgb(0x00, 0xFF, 0x00),  // end color (bright green)
                    90.0                              // angle in degrees
                );
            }
            else
            {
                // Example red gradient
                return new LinearGradientBrush(
                    Color.FromRgb(0x80, 0x00, 0x00),  // dark red
                    Color.FromRgb(0xFF, 0x00, 0x00),  // bright red
                    90.0
                );
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("ConvertBack not supported");
        }
    }
}
