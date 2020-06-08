using System.Collections.Generic;

namespace LightX.Classes
{
    public enum Tests
    {
        Conjonctive             = 0x00, 
        CobaltFilter            = 0X01,
        VanHerick               = 0X02, 
        Cornea                  = 0X03, 
        AnteriorChamber         = 0X04, 
        Lens                    = 0X05, 
        IrisTransillumination   = 0X06, 
        PupillaryMargin         = 0X07, 
        NewTest                 = 0x10
    }

    public class TestResults : BaseClass
    {
        #region Fields

        private string _testTitle;
        private Tests _id;
        private CameraSettings _camSettings;
        private string _comments;
        private string _pathToImages;
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

        public Tests Id
        {
            get { return _id; }
            set
            {
                if (value != _id)
                {
                    _id = value;
                    OnPropertyChanged("Id");
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

        public string PathToImages
        {
            get { return _pathToImages; }
            set
            {
                if (value != _pathToImages)
                {
                    _pathToImages = value;
                    OnPropertyChanged("PathToImages");
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
