using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace LightX.Classes
{
    class UriToCachedImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;

            if (!string.IsNullOrEmpty(value.ToString()))
            {
                string path = value.ToString();
                if (Path.GetExtension(path) != "jpeg")
                    path = Path.ChangeExtension(path, ".jpeg");

                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = new Uri(path);
                bi.DecodePixelWidth = 200;
                // must be loaded from cache if we want to be able to delete or move it right away.
                // but not useful if only wants to see them (like in the examReviewWindow)
                bi.CacheOption = parameter == null ? BitmapCacheOption.OnLoad : BitmapCacheOption.None;
                bi.CreateOptions = parameter == null ? BitmapCreateOptions.None : BitmapCreateOptions.IgnoreImageCache;
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
                image = Path.ChangeExtension(image, ".jpeg");

                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = new Uri(image);
                bi.CacheOption = parameter == null ? BitmapCacheOption.OnLoad : BitmapCacheOption.None;
                bi.CreateOptions = parameter == null ? BitmapCreateOptions.None : BitmapCreateOptions.IgnoreImageCache;
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

    class TitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;

            Exam exam = value as Exam;
            string title = "LightX";
            if (exam.Patient != null)
               title = $"LightX - {exam.Patient.FirstName} {exam.Patient.LastName} - {exam.ExamDate.Day:D2}/{exam.ExamDate.Month:D2}/{exam.ExamDate.Year} - {exam.ExamDate.Hour:D2}:{exam.ExamDate.Minute:D2}:{exam.ExamDate.Second:D2}";

            return title;
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

            int numberOfImages = ((LightX.ViewModel.ReviewWindowViewModel)(values[1])).ReviewImages.Count;
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

    class ImageIsActiveConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values == null)
                return null;
            string path = values[0] as string;
            ObservableCollection<ReviewImage> reviewImages = (ObservableCollection<ReviewImage>)values[1];
            foreach (ReviewImage reviewImage in reviewImages)
            {
                if (reviewImage.Image.Contains(path))
                {
                    if (reviewImage.IsActive)
                        return System.Windows.Visibility.Visible;
                    return System.Windows.Visibility.Collapsed;
                }
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("Two way conversion is not supported.");
        }
    }
}
