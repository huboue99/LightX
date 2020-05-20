
namespace LightX.Classes
{
    public class CameraSettings : BaseClass
    {
        #region Fields

        private string _flash;
        private string _shutterSpeed;
        private string _fNumber;
        private string _iso;
        private string _burstNumber;

        #endregion Fields

        #region Properties

        public string Flash
        {
            get { return _flash; }
            set
            {
                if (value != _flash)
                {
                    _flash = value;
                    OnPropertyChanged("Flash");
                }
            }
        }

        public string ShutterSpeed
        {
            get { return _shutterSpeed; }
            set
            {
                if (value != _shutterSpeed)
                {
                    _shutterSpeed = value;
                    OnPropertyChanged("ShutterSpeed");
                }
            }
        }

        public string FNumber
        {
            get { return _fNumber; }
            set
            {
                if (value != _fNumber)
                {
                    _fNumber = value;
                    OnPropertyChanged("FNumber");
                }
            }
        }

        public string Iso
        {
            get { return _iso; }
            set
            {
                if (value != _iso)
                {
                    _iso = value;
                    OnPropertyChanged("Iso");
                }
            }
        }

        public string BurstNumber
        {
            get { return _burstNumber; }
            set
            {
                if (value != _burstNumber)
                {
                    _burstNumber = value;
                    OnPropertyChanged("BurstNumber");
                }
            }
        }

        #endregion Properties
    }
}
