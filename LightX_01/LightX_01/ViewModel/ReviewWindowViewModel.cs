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
        //private readonly ObservableCollection<BitmapImage> _images;
        //private readonly BitmapImage[] _images;
        //private readonly ObservableCollection<string> _images;
        private string[] _images;
        protected string _currentImage;
        private ObservableCollection<bool> _selectedImages;
        private ObservableCollection<ReviewImage> _reviewImages;
        private string _currentComment;

        // General purpose vars
        private int _currentImageIndex = 0;
        private bool _notLastImage;
        private bool _notFirstImage;
        public string ImageCount { get; set; }
        public bool ImageIsSelectable { get; set; } = true;
        
        // Command definitions
        private RelayCommand _nextImageCommand;
        private RelayCommand _previousImageCommand;
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
            //int index = Images.ToList().FindIndex(a => a.Contains(image.DataContext as string));
            //SelectedImages[index] = !SelectedImages[index];
            //RaisePropertyChanged(() => SelectedImages);

            foreach (ReviewImage reviewImage in ReviewImages)
            {
                if (reviewImage.Image.Contains((image.DataContext as ReviewImage).Image))
                    reviewImage.IsSelected = !reviewImage.IsSelected;
            }
            RaisePropertyChanged(() => CurrentImageIsSelected);
            RaisePropertyChanged(() => ReviewImages);
        }

        //public BitmapImage CurrentImage
        //{
        //    get { return _images[_currentImageIndex]; }
        //}

        public string CurrentImage
        {
            //get { return _images[_currentImageIndex] + ".jpeg"; }
            get { return _currentImage; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                _currentImage = string.Empty;
                _currentImage = value;
                RaisePropertyChanged(() => CurrentImage);
                }
                else
                {
                    MessageBox.Show("CurrentImage is trying to read an empty path.");
                }
            }
        }

        public string[] Images
        {
            get { return _images; }
            set
            {
                _images = value;
                RaisePropertyChanged(() => Images);
            }
        }

        public ObservableCollection<bool> SelectedImages
        {
            get { return _selectedImages; }
            set
            {
                if (value != _selectedImages)
                {
                    _selectedImages = value;
                    RaisePropertyChanged(() => SelectedImages);
                    RaisePropertyChanged(() => CurrentImageIsSelected);
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

        public int CurrentImageIndex
        {
            get { return _currentImageIndex; }
            set
            {
                if (value != _currentImageIndex)
                {
                    _currentImageIndex = value;
                    RaisePropertyChanged(() => CurrentImageIndex);
                    //CurrentImage = _images[_currentImageIndex] + ".jpeg";
                    RaisePropertyChanged(() => CurrentImageIndexString);
                    RaisePropertyChanged(() => CurrentImageIsSelected);
                    RaisePropertyChanged(() => ReviewImages);
                    RaisePropertyChanged(() => NotLastImage);
                    RaisePropertyChanged(() => NotFirstImage);
                }
            }
        }

        public string CurrentImageIndexString
        {
            get { return (_currentImageIndex + 1).ToString(); }
        }

        public bool CurrentImageIsSelected
        {
            get { return ReviewImages[_currentImageIndex].IsSelected; }
            set
            {
                if (value != ReviewImages[_currentImageIndex].IsSelected)
                {
                    ReviewImages[_currentImageIndex].IsSelected = value;
                    //RaisePropertyChanged(() => SelectedImages);
                    RaisePropertyChanged(() => CurrentImageIsSelected);
                    RaisePropertyChanged(() => ReviewImages);
                }
            }
        }

        public bool NotLastImage
        {
            get { return (_currentImageIndex < ReviewImages.Count - 1); }
            set
            {
                if (value != _notLastImage)
                {
                    _notLastImage = value;
                    RaisePropertyChanged(() => NotLastImage);
                }
            }
        }

        public bool NotFirstImage
        {
            get { return (_currentImageIndex > 0); }
            set
            {
                if (value != _notFirstImage)
                {
                    _notFirstImage = value;
                    RaisePropertyChanged(() => NotFirstImage);
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

        public RelayCommand NextImageCommand
        {
            get
            {
                if(_nextImageCommand == null)
                    _nextImageCommand = new RelayCommand(NextImage, true);
                return _nextImageCommand;
            }
        }

        public RelayCommand PreviousImageCommand
        {
            get
            {
                if (_previousImageCommand == null)
                    _previousImageCommand = new RelayCommand(PreviousImage, true);
                return _previousImageCommand;
            }
        }

        #endregion RelayCommands

        #region Actions
        public void NextImage()
        {
            if(NotLastImage)
                if (!string.IsNullOrEmpty(ReviewImages[++CurrentImageIndex].Image))
                    CurrentImage = ReviewImages[CurrentImageIndex].Image + ".jpeg";
                else
                    MessageBox.Show("CurrentImage is trying to read an empty path.");
            //CurrentImageIndex++;
            GC.Collect();
        }

        //public BitmapImage NImage()
        //{
        //    if (NotLastImage)
        //        return _images[++CurrentImageIndex];
        //    else
        //        return CurrentImage;
        //}

        public void PreviousImage()
        {
            if(NotFirstImage)
                if (!string.IsNullOrEmpty(ReviewImages[--CurrentImageIndex].Image))
                    CurrentImage = ReviewImages[CurrentImageIndex].Image + ".jpeg";
                else
                    MessageBox.Show("CurrentImage is trying to read an empty path.");
            //CurrentImageIndex--;
            GC.Collect();
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


        //public ReviewWindowViewModel(ObservableCollection<BitmapImage> images, string comment)
        public ReviewWindowViewModel(List<string> images, string comment)
        {
            //_images = new BitmapImage[images.Count];
            //images.CopyTo(_images, 0);

            // remove all the empty string or without path (sometimes happen)
            for (int i = 0; i < images.Count; ++i)
            {
                if (images[i] == ".jpeg" || string.IsNullOrEmpty(images[i]))
                {
                    images.RemoveAt(i);
                    Console.WriteLine("A bad path has been removed from the review images list.");
                }
            }

            //_images = images.ToArray();
            ReviewImages = new ObservableCollection<ReviewImage>();
            foreach(string image in images)
            {
                ReviewImages.Add(new ReviewImage() { Image = image, IsSelected = false });
            }

            CurrentImageIndex = 0;
            if(!string.IsNullOrEmpty(ReviewImages[_currentImageIndex].Image))
                CurrentImage = ReviewImages[_currentImageIndex].Image + ".jpeg";
            else
            {
                MessageBox.Show("CurrentImage is trying to read an empty path.");
            }
            _currentComment = comment;

            ImageCount = images.Count.ToString();
            ImageIsSelectable = images.Count > 1;
            //SelectedImages = new ObservableCollection<bool>(Enumerable.Repeat(false, images.Count).ToArray());

            if (!ImageIsSelectable)
                ReviewImages[0].IsSelected = true;

            // weird caching issue for first image, so do a "refresh" by using previous command
            //PreviousImage();
        }
    }
}
