using GalaSoft.MvvmLight;
using LightX.Classes;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LightX.ViewModel
{
    public class GuideWindowViewModel : ViewModelBase
    {
        #region Fields

        private ObservableCollection<Tests> _testList;
        private TestInstructions _currentTest;
        private ParametersList _currentList;
        private RunList _currentTestsState;
        private BitmapImage _currentImage;
        private int _testIndex;

        #endregion Fields

        #region Properties

        public TestInstructions CurrentTest
        {
            get { return _currentTest; }
            set
            {
                _currentTest = value;
                RaisePropertyChanged(() => CurrentTest);
            }
        }

        public ParametersList CurrentList
        {
            get { return _currentList; }
            set
            {
                _currentList = value;
                RaisePropertyChanged(() => CurrentList);
            }
        }

        public RunList CurrentTestsState
        {
            get { return _currentTestsState; }
            set
            {
                if (_currentTestsState != value)
                {
                    _currentTestsState = value;
                    RaisePropertyChanged(() => CurrentTestsState);
                }
            }
        }

        public BitmapImage CurrentImage
        {
            get { return _currentImage; }
            set
            {
                if (value != _currentImage)
                {
                    _currentImage = value;
                    RaisePropertyChanged(() => CurrentImage);
                }
            }
        }

        public int TestIndex
        {
            get { return _testIndex; }
            set
            {
                if (value != _testIndex)
                {
                    _testIndex = value;
                    RaisePropertyChanged(() => TestIndex);
                }
            }
        }

        #endregion Properties

        #region Actions



        #endregion Actions

        #region DataFetching

        private void FetchCurrentImage()
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            image.UriSource = new Uri(CurrentTest.ImagesPath[0], UriKind.Relative);
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            CurrentImage = image;
        }

        private void FetchCurrentTestList()
        {
            CurrentTestsState = new RunList(_testList);
            CurrentTestsState[TestIndex].Foreground = Brushes.Black;
            CurrentTestsState[TestIndex].FontWeight = FontWeights.Bold;
        }

        private void FetchAllData()
        {
            CurrentList = new ParametersList(CurrentTest);
            FetchCurrentTestList();
            FetchCurrentImage();
        }

        #endregion DataFetching

        public GuideWindowViewModel(TestInstructions test, ObservableCollection<Tests> testList, int i)
        {
            _testList = testList;
            CurrentTest = test;
            TestIndex = i;
            FetchAllData();
        }
    }
}
