using GalaSoft.MvvmLight;
using LightX.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LightX.ViewModel
{
    public class GuideWindowViewModel : ViewModelBase
    {
        #region Fields

        private List<GuideData> _currentTest;
        private RunList _currentTestsState;
        private string _testTitle;
        private int _instructionIndex = 0;

        #endregion Fields

        #region Properties

        public List<GuideData> CurrentTest
        {
            get { return _currentTest; }
            set
            {
                _currentTest = value;
                RaisePropertyChanged(() => CurrentTest);
            }
        }

        public string TestTitle
        {
            get { return _testTitle; }
            set
            {
                _testTitle = value;
                RaisePropertyChanged(() => TestTitle);
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

        //public BitmapImage CurrentImage
        //{
        //    get { return _currentImage; }
        //    set
        //    {
        //        if (value != _currentImage)
        //        {
        //            _currentImage = value;
        //            RaisePropertyChanged(() => CurrentImage);
        //        }
        //    }
        //}

        //public Instruction CurrentInstruction
        //{
        //    get { return _currentInstruction; }
        //    set
        //    {
        //        if (value != _currentInstruction)
        //        {
        //            _currentInstruction = value;
        //            RaisePropertyChanged(() => CurrentInstruction);
        //        }
        //    }
        //}

        public int InstructionIndex
        {
            get { return _instructionIndex; }
            set
            {
                if (value != _instructionIndex)
                {
                    _instructionIndex = value;
                    RaisePropertyChanged(() => InstructionIndex);
                }
            }
        }

        #endregion Properties

        #region Actions

        public bool NextInstruction()
        {
            if(_instructionIndex < CurrentTest.Count - 1)
            {
                ++InstructionIndex;
                //SelectTab(_instructionIndex);
                return true;
            }
            return false;
        }

        public bool PreviousInstruction()
        {
            if (_instructionIndex > 0)
            {
                --InstructionIndex;
                //SelectTab(_instructionIndex);
                return true;
            }
            return false;
        }

        #endregion Actions

        #region DataFetching

        private BitmapImage FetchImage(string path)
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            //image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            image.UriSource = new Uri(path, UriKind.Relative);
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            return image;
        }

        private void FetchCurrentTestList(ObservableCollection<Tests> testList, int testIndex)
        {
            CurrentTestsState = new RunList(testList);
            CurrentTestsState[testIndex].Foreground = Brushes.Black;
            CurrentTestsState[testIndex].FontWeight = FontWeights.Bold;
        }

        //private void FetchAllData()
        //{
        //    _currentInstruction = CurrentTest.Instructions[_instructionIndex];
        //    CurrentList = new ParametersList(_currentInstruction);
        //    FetchCurrentImage();
        //}

        #endregion DataFetching

        public GuideWindowViewModel(TestInstructions test, ObservableCollection<Tests> testList, int i)
        {
            TestTitle = test.TestTitle;
            CurrentTest = new List<GuideData>();
            int j = 1;
            foreach (Instruction instruction in test.Instructions)
            {
                CurrentTest.Add(new GuideData() { ParamList = new ParametersList(instruction), InstructionsNotes = instruction.InstructionsNotes, Image = FetchImage(instruction.ImagesPath[0]), Id = j++ });
            }

            FetchCurrentTestList(testList, i);
        }
    }
}
