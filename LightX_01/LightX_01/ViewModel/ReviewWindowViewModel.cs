using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;

namespace LightX_01.ViewModel
{
    public class ReviewWindowViewModel : ViewModelBase
    {
        #region Fields

        private BitmapImage _currentImage;
        private string _currentComment;
        private ICommand _confirmCommand;
        private ICommand _cancelCommand;

        #endregion Fields

        #region Properties
        
        public BitmapImage CurrentImage
        {
            get { return _currentImage; }
            set
            {
                _currentImage = value;
                RaisePropertyChanged(() => CurrentImage);
            }
        }

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

        #endregion Properties

        public ICommand ConfirmCommand
        {
            get
            {
                if (_confirmCommand == null)
                {
                    _confirmCommand = new RelayCommand<Window>(
                        param => ConfirmImage(param)
                        );
                }
                return _confirmCommand;
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new RelayCommand<Window>(
                        param => CancelImage(param)
                        );
                }
                return _cancelCommand;
            }
        }

        private void ConfirmImage(Window currentWindow)
        {

        }

        private void CancelImage(Window currentWindow)
        {
            //
            CloseWindow(currentWindow);
        }

        private void CloseWindow(Window currentWindow)
        {
            currentWindow.Close();
        }

        public ReviewWindowViewModel(BitmapImage image)
        {
            _currentImage = image;
        }
    }
}
