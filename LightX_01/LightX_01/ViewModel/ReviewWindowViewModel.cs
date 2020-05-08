using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LightX_01.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace LightX_01.ViewModel
{
    public class ReviewWindowViewModel : ViewModelBase
    {
        #region Fields
        // Main data holders
        private string[] _images;
        protected string _currentImage;
        private ObservableCollection<ReviewImage> _reviewImages;
        private string _currentComment;

        // General purpose vars
        public string ImageCount { get; set; }
        public bool ImageIsSelectable { get; set; } = true;
        
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

        internal void ActiveImageEvent(Image image)
        {
            foreach (ReviewImage reviewImage in ReviewImages)
            {
                if (reviewImage.Image.Contains((image.DataContext as ReviewImage).Image))
                    reviewImage.IsActive = true;
                else
                    reviewImage.IsActive = false;
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

        #region Actions
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

        private void CancelImage(Window currentWindow)
        {
            currentWindow.DialogResult = false;
            CloseWindow(currentWindow);
        }

        private void CloseWindow(Window currentWindow)
        {
            currentWindow.Close();
        }

        #endregion Actions

        public ReviewWindowViewModel(List<string> images, string comment)
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

            ReviewImages = new ObservableCollection<ReviewImage>();
            foreach(string image in images)
            {
                ReviewImages.Add(new ReviewImage() { Image = image });
            }

            ReviewImages[0].IsActive = true;
            _currentComment = comment;

            ImageCount = images.Count.ToString();
            ImageIsSelectable = images.Count > 1;

            if (!ImageIsSelectable)
                ReviewImages[0].IsSelected = true;
        }
    }
}
