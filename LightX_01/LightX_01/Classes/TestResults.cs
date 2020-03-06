using System.Collections.Generic;

namespace LightX_01.Classes
{
    public class TestResults : BaseClass
    {
        #region Fields

        private string _testTitle;
        private CameraSettings _camSettings;
        private string _comments;
        private List<string> _resultsImages;

        #endregion Fields

        #region Properties

        public string TestTitle
        {
            get { return _testTitle; }
            set
            {
                if (value != _testTitle)
                {
                    _testTitle = value;
                    OnPropertyChanged("TestTitle");
                }
            }
        }

        public CameraSettings CamSettings
        {
            get { return _camSettings; }
            set
            {
                if (value != _camSettings)
                {
                    _camSettings = value;
                    OnPropertyChanged("CamSettings");
                }
            }
        }

        public string Comments
        {
            get { return _comments; }
            set
            {
                if (value != _comments)
                {
                    _comments = value;
                    OnPropertyChanged("Comments");
                }
            }
        }

        public List<string> ResultsImages
        {
            get { return _resultsImages; }
            set
            {
                if (value != _resultsImages)
                {
                    _resultsImages = value;
                    OnPropertyChanged("ResultsImages");
                }
            }
        }

        #endregion Properties
    }
}
