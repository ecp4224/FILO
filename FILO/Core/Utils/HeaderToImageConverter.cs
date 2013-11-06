/*
    This file is part of FILO.

    FILO is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    FILO is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with FILO.  If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace FILO.Core.Utils
{
    #region HeaderToImageConverter
    //Taken from: http://www.codeproject.com/Articles/21248/A-Simple-WPF-Explorer-Tree modified for use with FILO
    [ValueConversion(typeof (string), typeof (bool))]
    public class HeaderToImageConverter : IValueConverter
    {
        private static Dictionary<string, BitmapImage> cache = new Dictionary<string, BitmapImage>();
        private static readonly BitmapImage DefaultImage = Bitmap2BitmapImage(SystemIcons.WinLogo.ToBitmap());
        private static readonly BitmapImage DriveImage;
        private static readonly BitmapImage FolderImage;
        public static HeaderToImageConverter Instance;
        static HeaderToImageConverter()
        {
            Instance = new HeaderToImageConverter();
            Uri uri;
            try
            {
                uri = new Uri("pack://application:,,,/GUI/Images/diskdrive.png");
                DriveImage = new BitmapImage(uri);
            }
            catch
            {
                DriveImage = DefaultImage;
            }
            try
            {
                uri = new Uri("pack://application:,,,/GUI/Images/folder.png");
                FolderImage = new BitmapImage(uri);
            }
            catch
            {
                FolderImage = DefaultImage;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = value as string;
            if (s == null)
                return DefaultImage;
            Uri uri;
            BitmapImage source;
            if (s.Length == 3)
            {
                return DriveImage;
            }
            if (Path.HasExtension(s))
            {
                if (cache.ContainsKey(s))
                    return cache[s];
                try
                {
                    var iconForFile = Icon.ExtractAssociatedIcon(s);
                    if (iconForFile != null)
                    {
                        BitmapImage img = Bitmap2BitmapImage(iconForFile.ToBitmap());
                        cache.Add(s, img);
                        return img;
                    }
                    return DefaultImage;
                }
                catch
                {
                }
            }
            return FolderImage;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        private static BitmapImage Bitmap2BitmapImage(Bitmap bitmap)
        {
            BitmapImage bitmapImage = null;
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }

            return bitmapImage;
        }
    }

    #endregion // DoubleToIntegerConverter
}
