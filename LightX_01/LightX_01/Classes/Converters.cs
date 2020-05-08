using System;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace LightX_01.Classes
{
    class UriToCachedImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;

            if (!string.IsNullOrEmpty(value.ToString()))
            {
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = new Uri(value.ToString() + ".jpeg");
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.EndInit();
                return bi;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("Two way conversion is not supported.");
        }
    }

    class ReviewImagesToCachedImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;

            string image = ((ObservableCollection<ReviewImage>)value)[0].Image;
            foreach (ReviewImage reviewImage in (ObservableCollection<ReviewImage>)value)
            {
                if (reviewImage.IsActive)
                {
                    image = reviewImage.Image;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(image))
            {
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = new Uri(image + ".jpeg");
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.EndInit();
                return bi;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("Two way conversion is not supported.");
        }
    }

    class ReviewImageSizeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values == null)
                return null;
            else if ((double)values[0] <= 0.0)
                return 0;

            int marginSize = 5;
            double containerWidth = (double)values[0];

            int numberOfImages = ((LightX_01.ViewModel.ReviewWindowViewModel)(values[1])).ReviewImages.Count;
            double numPerRow = (double)numberOfImages;

            if (numberOfImages > 2 && numberOfImages < 7)
                numPerRow = 3.0;
            else if (numberOfImages >= 7)
                numPerRow = 4.0;

            double size = (int)(containerWidth / numPerRow) - marginSize * 2;
            
            return size;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("Two way conversion is not supported.");
        }
    }
}
