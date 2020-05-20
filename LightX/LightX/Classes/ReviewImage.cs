
namespace LightX.Classes
{
    public class ReviewImage : BaseClass
    {
        #region Fields

        private string _image;
        private bool _isSelected = false;
        private bool _isActive = false;

        #endregion Fields

        #region Properties

        public string Image
        {
            get { return _image; }
            set
            {
                if (value != _image)
                {
                    _image = value;
                    OnPropertyChanged("Image");
                }
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }

        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (value != _isActive)
                {
                    _isActive = value;
                    OnPropertyChanged("IsActive");
                }
            }
        }

        #endregion Properties
    }
}
