using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using NLog;

namespace CVCapturePanel.Converters
{
    public class BitmapSourceConvert : IValueConverter
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            using (var memoryStream = new MemoryStream())
            {
                try
                {
                    ((Bitmap)value).Save(memoryStream, ImageFormat.Bmp);
                    var image = new BitmapImage();
                    image.BeginInit();
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    image.StreamSource = new MemoryStream(memoryStream.ToArray());
                    image.EndInit();
                    return image;
                }
                catch (Exception e)
                {
                    logger.Error("Some error occurred in frame conversion {e}", e);
                }

                return new BitmapImage();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}