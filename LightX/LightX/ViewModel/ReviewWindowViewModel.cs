using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LightX.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LightX.ViewModel
{
    public class ReviewWindowViewModel : ViewModelBase
    {
        #region Fields
        // Main data holders
        private ObservableCollection<ReviewImage> _reviewImages;
        private string _currentComment;

        // Command definitions
        private ICommand _confirmCommand;
        private ICommand _cancelCommand;

        #endregion Fields

        #region Properties

        public string CurrentComment
        {
            get { return _currentComment; }
            set
            {
                if (value != _currentComment)
                {
                    _currentComment = value;
                    RaisePropertyChanged(() => CurrentComment);
                }
            }
        }

        internal void SelectImageEvent(Image image)
        {
            foreach (ReviewImage reviewImage in ReviewImages)
            {
                if (reviewImage.Image.Contains((image.DataContext as ReviewImage).Image))
                {
                    reviewImage.IsSelected = !reviewImage.IsSelected;
                    break;
                }
            }
            RaisePropertyChanged(() => ReviewImages);
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

        private bool AreAllImagesSelected()
        {
            bool b = true;
            foreach (ReviewImage reviewImage in ReviewImages)
            {
                b &= reviewImage.IsSelected;
            }
            return b;
        }

        #endregion Properties

        #region RelayCommands
        public ICommand ConfirmCommand
        {
            get
            {
                if (_confirmCommand == null)
                    _confirmCommand = new RelayCommand<Window>(
                        param => ConfirmImage(param)
                        );
                return _confirmCommand;
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                    _cancelCommand = new RelayCommand<Window>(
                        param => CancelImage(param)
                        );
                return _cancelCommand;
            }
        }

        #endregion RelayCommands

        #region Events

        internal bool ActiveImageEvent(Image image)
        {
            string path = Path.ChangeExtension((image.DataContext as ReviewImage).Image, null);
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

        private void CancelImage(Window currentWindow)
        {
            currentWindow.DialogResult = false;
            CloseWindow(currentWindow);
        }

        private void CloseWindow(Window currentWindow)
        {
            currentWindow.Close();
        }

        #endregion Events

        #region Actions

        internal void SelectAllImages()
        {
            bool b = true;
            if (AreAllImagesSelected())
                b = false;

            foreach (ReviewImage reviewImage in ReviewImages)
            {
                reviewImage.IsSelected = b;
            }
            RaisePropertyChanged(() => ReviewImages);
        }

        private void ConfirmImage(Window currentWindow)
        {
            bool oneIsSelected = false;
            foreach(ReviewImage reviewImage in ReviewImages)
            {
                oneIsSelected |= reviewImage.IsSelected;
            }

            if(!oneIsSelected)
                MessageBox.Show("Veuillez selectionner au moins une photo.");
            else
            {
                currentWindow.DialogResult = true;
                CloseWindow(currentWindow);
            }
        }

        internal void RefreshReviewImages(List<string> images, int activeIndex)
        {
            images = SanitizeImagePathList(images);

            ObservableCollection<ReviewImage> reviewImg = new ObservableCollection<ReviewImage>();
            foreach (string image in images)
                reviewImg.Add(new ReviewImage() { Image = image });

            if (activeIndex < 0 || activeIndex >= reviewImg.Count)
            {
                Console.WriteLine("The specified activeIndex value is out of bound.");
                activeIndex = 0;
            }

            if (ReviewImages != null)
            {
                for (int i = 0; i < ReviewImages.Count; ++i)
                {
                    reviewImg[i].IsSelected = ReviewImages[i].IsSelected;
                }
            }

            reviewImg[activeIndex].IsActive = true;
            ReviewImages = reviewImg;
        }

        internal void RefreshReviewImages(List<string> images)
        {
            // keeps the same active image after the refresh
            int activeIndex = 0;

            while (!ReviewImages[activeIndex].IsActive && activeIndex < ReviewImages.Count - 1)
                ++activeIndex;

            RefreshReviewImages(images, activeIndex);
        }

        private List<string> SanitizeImagePathList(List<string> images)
        {
            // remove all the empty string or without path (sometimes happen)
            for (int i = 0; i < images.Count; ++i)
            {
                if (images[i] == ".jpeg" || string.IsNullOrEmpty(images[i]))
                {
                    images.RemoveAt(i);
                    Console.WriteLine("A bad path has been removed from the review images list.");
                }
            }
            return images;
        }

        #endregion Actions

        internal ReviewWindowViewModel(List<string> images, string comment)
        {
            RefreshReviewImages(images, 0);
            _currentComment = comment;
        }
    }
}
