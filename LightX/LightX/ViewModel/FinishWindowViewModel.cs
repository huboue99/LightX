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

        internal bool ActiveImageEvent(Image image)
        {
            string path = Path.ChangeExtension((image.DataContext as string), null);
            bool imageHasChanged = false;
            foreach (ReviewImage reviewImage in ReviewImages)
            {
                if (reviewImage.Image.Contains(path))
                {
                    if (!reviewImage.IsActive)
                    {
                        reviewImage.IsActive = true;
                        imageHasChanged = true;
                    }
                    else
                    {
                        GC.Collect();
                        return imageHasChanged;
                    }
                }
                else if (reviewImage.IsActive)
                {
                    reviewImage.IsActive = false;
                }
            }

            if (imageHasChanged)
                RaisePropertyChanged(() => ReviewImages);
            GC.Collect();
            return imageHasChanged;
        }

        #endregion Actions

        public FinishWindowViewModel(Exam exam)
        {
            CurrentExam = exam;
            ReviewImages = new ObservableCollection<ReviewImage>();


            if (CurrentExam.Results.Last().Id != Tests.NewTest)
                CurrentExam.Results.Add(new TestResults()
                    {
                        TestTitle = "New test",
                        Id = Tests.NewTest,
                        PathToImages = string.Format("{0}\\{1}", CurrentExam.ResultsPath, "New test")
                    });

            foreach (TestResults result in exam.Results)
            {
                if (result.ResultsImages != null)
                    foreach (string path in result.ResultsImages)
                    {
                        ReviewImages.Add(new ReviewImage() { Image = path });
                    }
            }
            // if the opperator has only skipped all the tests.
            if (ReviewImages.Count > 0)
                ReviewImages[0].IsActive = true;
        }
    }
}
