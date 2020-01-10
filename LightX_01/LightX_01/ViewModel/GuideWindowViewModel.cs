using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LightX_01.Classes;
using Newtonsoft.Json;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System;
using System.Windows.Controls;

namespace LightX_01.ViewModel
{
    public class GuideWindowViewModel : ViewModelBase
    {
        #region Fields

        private Exam _currentExam;
        private GuideData _currentTest;
        private ParametersList _currentList;
        private RunList _currentTestsState;
        private BitmapImage _currentImage;
        private int TestIndex = 0;
        private RelayCommand _nextTestCommand;
        private RelayCommand _previousTestCommand;
        
        
        #endregion Fields

        #region Properties

        public Exam CurrentExam
        {
            get { return _currentExam; }
            set
            {
                if (value != _currentExam)
                {
                    _currentExam = value;
                    RaisePropertyChanged(() => CurrentExam);
                }
            }
        }

        public GuideData CurrentTest
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
        
        #endregion Properties

        #region Commands

        public RelayCommand NextTestCommand
        {
            get
            {
                if (_nextTestCommand == null)
                {
                    _nextTestCommand = new RelayCommand(NextTest, true);
                }
                return _nextTestCommand;
            }
        }

        public RelayCommand PreviousTestCommand
        {
            get
            {
                if (_previousTestCommand == null)
                {
                    _previousTestCommand = new RelayCommand(PreviousTest, true);
                }
                return _previousTestCommand;
            }
        }

        #endregion Commands

        private void UpdateCurrentTest()
        {
            string path = $@".\Resources\{CurrentExam.TestList[TestIndex]}.json";
            using (StreamReader file = File.OpenText(path))
            {
                JsonSerializer serializer = new JsonSerializer();
                GuideData data = (GuideData)serializer.Deserialize(file, typeof(GuideData));
                CurrentTest = data;
            }

        }

        private void UpdateCurrentList()
        {
            CurrentList = new ParametersList(CurrentTest);
        }

        private void UpdateCurrentImage()
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            image.UriSource = new Uri(CurrentTest.ImagesPath[0], UriKind.Relative);
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            CurrentImage = image;
        }

        private void UpdateCurrentTestList()
        {
            CurrentTestsState = new RunList(new List<string>() { "Conjonctivite", "Van Herick", "Cornée", "Chambre Antérieur", "Cristalin", "Marges Pupillaires", "Transillumination de l'Iris", "Filtre Cobalt" });
            CurrentTestsState[TestIndex].Foreground = Brushes.Black;
            CurrentTestsState[TestIndex].FontWeight = FontWeights.Bold;
        }

        private void UpdateAllInfos()
        {
            UpdateCurrentTest();
            UpdateCurrentList(); // make sure to call UpdateCurrentList after UpdateCurrentTest
            UpdateCurrentTestList(); // same
            UpdateCurrentImage();
        }

        private void NextTest()
        {
            if (TestIndex != CurrentExam.TestList.Count - 1)
            {
                TestIndex++;
                UpdateAllInfos();
            }
            else
            {
                MessageBox.Show("Tous les tests ont été effectué.");
            }
        }

        private void PreviousTest()
        {
            if (TestIndex != 0)
            {
                TestIndex--;
                UpdateAllInfos();
            }
        }

        public GuideWindowViewModel(Exam exam)
        {
            CurrentExam = exam;
            UpdateAllInfos();
        }
    }
}
