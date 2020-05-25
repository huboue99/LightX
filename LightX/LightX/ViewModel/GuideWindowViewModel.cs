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

        private TestInstructions _currentTest;
        private Instruction _currentInstruction;
        private ParametersList _currentList;
        private RunList _currentTestsState;
        private BitmapImage _currentImage;
        private int _instructionIndex = 0;

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

        public Instruction CurrentInstruction
        {
            get { return _currentInstruction; }
            set
            {
                if (value != _currentInstruction)
                {
                    _currentInstruction = value;
                    RaisePropertyChanged(() => CurrentInstruction);
                }
            }
        }

        //public int TestIndex
        //{
        //    get { return _testIndex; }
        //    set
        //    {
        //        if (value != _testIndex)
        //        {
        //            _testIndex = value;
        //            RaisePropertyChanged(() => TestIndex);
        //        }
        //    }
        //}

        #endregion Properties

        #region Actions

        public bool NextInstruction()
        {
            if(_instructionIndex < CurrentTest.Instructions.Count - 1)
            {
                ++_instructionIndex;
                FetchAllData();
                return true;
            }
            return false;
        }

        public bool PreviousInstruction()
        {
            if (_instructionIndex > 0)
            {
                --_instructionIndex;
                FetchAllData();
                return true;
            }
            return false;
        }

        #endregion Actions

        #region DataFetching

        private void FetchCurrentImage()
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            image.UriSource = new Uri(_currentInstruction.ImagesPath[0], UriKind.Relative);
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            CurrentImage = image;
        }

        private void FetchCurrentTestList(ObservableCollection<Tests> testList, int testIndex)
        {
            CurrentTestsState = new RunList(testList);
            CurrentTestsState[testIndex].Foreground = Brushes.Black;
            CurrentTestsState[testIndex].FontWeight = FontWeights.Bold;
        }

        private void FetchAllData()
        {
            _currentInstruction = CurrentTest.Instructions[_instructionIndex];
            CurrentList = new ParametersList(_currentInstruction);
            FetchCurrentImage();
        }

        #endregion DataFetching

        public GuideWindowViewModel(TestInstructions test, ObservableCollection<Tests> testList, int i)
        {
            CurrentTest = test;
            FetchCurrentTestList(testList, i);
            FetchAllData();
        }
    }
}
