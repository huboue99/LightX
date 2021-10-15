using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LightX.Classes;
using System;
using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.IO;
using Newtonsoft.Json;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace LightX.ViewModel
{
    public class GuideWindowViewModel : ViewModelBase
    {
        #region Fields

        private List<GuideData> _currentTest;
        private RunList _currentTestsState;
        private int _instructionIndex = 0;



        public string TestTitle { get; }

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
                return true;
            }
            return false;
        }

        public bool PreviousInstruction()
        {
            if (_instructionIndex > 0)
            {
                --InstructionIndex;
                return true;
            }
            return false;
        }

        
        #endregion Actions

        #region DataFetching

        private BitmapImage FetchImage(List<string> paths)
        {
            if (paths.Count != 0)
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                //image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                image.UriSource = new Uri(paths[0], UriKind.Relative);
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                return image;
            }

            return new BitmapImage();
        }

        private void FetchCurrentTestList(ObservableCollection<Tests> testList, int testIndex)
        {
            CurrentTestsState = new RunList(testList);
            CurrentTestsState[testIndex].Foreground = Brushes.Black;
            CurrentTestsState[testIndex].FontWeight = FontWeights.Bold;
        }

        #endregion DataFetching

        public GuideWindowViewModel(TestInstructions test, ObservableCollection<Tests> testList, int i)
        {
            TestTitle = test.TestTitle;
            CurrentTest = new List<GuideData>();
            int j = 0;
            foreach (Instruction instruction in test.Instructions)
            {
                CurrentTest.Add(new GuideData() { ParamList = new ParametersList(instruction), InstructionsNotes = instruction.InstructionsNotes, Image = FetchImage(instruction.ImagesPath), Id = ++j });
            }

            FetchCurrentTestList(testList, i);
        }
    }
}
