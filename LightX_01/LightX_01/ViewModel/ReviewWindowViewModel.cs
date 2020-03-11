using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
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
        private ObservableCollection<BitmapImage> _images;
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

        public BitmapImage CurrentImage
        {
            get { return _images[_currentImageIndex]; }
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
                    RaisePropertyChanged(() => CurrentImage);
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
            get { return (_currentImageIndex < _images.Count - 1); }
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
                CurrentImageIndex++;
        }

        public void PreviousImage()
        {
            if(NotFirstImage)
                CurrentImageIndex--;
        }

        private void ConfirmImage(Window currentWindow)
        {
            currentWindow.DialogResult = true;
            CloseWindow(currentWindow);
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


        public ReviewWindowViewModel(ObservableCollection<BitmapImage> images, string comment)
        {
            _images = images;
            _currentComment = comment;

            ImageCount = images.Count.ToString();
            ImageIsSelectable = images.Count > 1;
            SelectedImages = new ObservableCollection<bool>(Enumerable.Repeat(false, images.Count).ToArray());
            if (!ImageIsSelectable)
                SelectedImages[0] = true;
        }
    }
}
