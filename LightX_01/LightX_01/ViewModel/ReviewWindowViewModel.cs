using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
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
        ObservableCollection<bool> _selectedImages;
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
                if (value != null && value != string.Empty)
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

        public ObservableCollection<bool> SelectedImages
        {
            get { return _selectedImages; }
            set
            {
                if (value != _selectedImages)
                {
                    _selectedImages = value;
                    RaisePropertyChanged(() => SelectedImages);
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
            get { return _selectedImages[_currentImageIndex]; }
            set
            {
                if (value != _selectedImages[_currentImageIndex])
                {
                    _selectedImages[_currentImageIndex] = value;
                    RaisePropertyChanged(() => CurrentImageIsSelected);
                }
            }
        }

        public bool NotLastImage
        {
            get { return (_currentImageIndex < _images.Length - 1); }
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
                CurrentImage = _images[++CurrentImageIndex] + ".jpeg";
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
                CurrentImage = _images[--CurrentImageIndex] + ".jpeg";
            //CurrentImageIndex--;
            GC.Collect();
        }

        private void ConfirmImage(Window currentWindow)
        {
            bool oneIsSelected = false;
            foreach(bool b in SelectedImages)
            {
                oneIsSelected |= b;
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
        public ReviewWindowViewModel(string[] images, string comment)
        {
            //_images = new BitmapImage[images.Count];
            //images.CopyTo(_images, 0);
            _images = images;
            CurrentImageIndex = 0;
            CurrentImage = _images[_currentImageIndex] + ".jpeg";
            _currentComment = comment;

            ImageCount = images.Length.ToString();
            ImageIsSelectable = images.Length > 1;
            SelectedImages = new ObservableCollection<bool>(Enumerable.Repeat(false, images.Length).ToArray());
            //GC.Collect();
            if (!ImageIsSelectable)
                SelectedImages[0] = true;

            // weird caching for first image, so do a "refresh" by using previous command
            PreviousImage();
        }
    }
}
