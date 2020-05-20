using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LightX.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace LightX.ViewModel
{
    public class FinishWindowViewModel : ViewModelBase
    {
        #region Fields

        private Exam _currentExam;
        private ObservableCollection<ReviewImage> _reviewImages;

        // Command definitions
        //private RelayCommand _newPhotoCommand;

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

        #region RelayCommands

        #endregion RelayCommands

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
            if (CurrentExam.Results.Last().Id != Tests.NewTest)
                CurrentExam.Results.Add(new TestResults() { TestTitle = "New test", Id = Tests.NewTest });
            foreach (TestResults result in exam.Results)
            {
                if (result.ResultsImages != null)
                    foreach (string path in result.ResultsImages)
                    {
                        ReviewImages.Add(new ReviewImage() { Image = path });
                    }
            }
            ReviewImages[0].IsActive = true;
        }
    }
}
