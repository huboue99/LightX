
namespace LightX_01.Classes
{
    public class ReviewImage : BaseClass
    {
        #region Fields

        private string _image;
        private bool _isSelected;

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

        #endregion Properties
    }
}
