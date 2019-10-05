using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace CameraCaptureWPF.Converters
{
    public class BitmapSourceConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            using (var ms = new MemoryStream())
            {
                try
                {
                    ((Bitmap)value).Save(ms, ImageFormat.Bmp);
                    var image = new BitmapImage();
                    image.BeginInit();
                    ms.Seek(0, SeekOrigin.Begin);
                    image.StreamSource = new MemoryStream(ms.ToArray());
                    image.EndInit();
                    return image;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
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