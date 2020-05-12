using GalaSoft.MvvmLight;
using LightX_01.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace LightX_01.ViewModel
{
    public class FinishWindowViewModel : ViewModelBase
    {
        #region Fields

        private Exam _currentExam;
        private ObservableCollection<ReviewImage> _reviewImages;

        #endregion Fields

        #region Properties

        public Exam CurrentExam
        {
            get { return _currentExam; }
            set
            {
                if (value != _currentExam)
                {
                    _currentExam = value;
                    RaisePropertyChanged(() => CurrentExam);
                }
            }
        }

        public ObservableCollection<ReviewImage> ReviewImages
        {
            get { return _reviewImages; }
            set
            {
                if (value != _reviewImages)
                {
                    _reviewImages = value;
                    RaisePropertyChanged(() => ReviewImages);
                }
            }
        }

        #endregion Properties

        #region Actions

        internal void ActiveImageEvent(Image image)
        {
            string path = Path.ChangeExtension((image.DataContext as string), null);

            foreach (ReviewImage reviewImage in ReviewImages)
            {
                if (reviewImage.Image.Contains(path))
                    reviewImage.IsActive = true;
                else if(reviewImage.IsActive)
                {
                    reviewImage.IsActive = false;
                }
            }
            RaisePropertyChanged(() => ReviewImages);
        }

        #endregion Actions

        public FinishWindowViewModel(Exam exam)
        {
            CurrentExam = exam;
            ReviewImages = new ObservableCollection<ReviewImage>();
            foreach (TestResults result in exam.Results)
            {
                foreach (string path in result.ResultsImages)
                {
                    ReviewImages.Add(new ReviewImage() { Image = path });
                }
            }
            ReviewImages[0].IsActive = true;
        }
    }
}
