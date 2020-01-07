using GalaSoft.MvvmLight.Command;
using LightX_01.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LightX_01.ViewModel
{
    public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
    {
        private PatientData patientData;

        //private ICommand exitMainWindow;

        public string FirstName
        {
            get
            {
                return patientData.FirstName;
            }
            set
            {
                if(patientData.FirstName != value)
                {
                    patientData.FirstName = value;
                    OnPropertyChange("FirstName");
                }
            }
        }

        public string LastName
        {
            get
            {
                return patientData.LastName;
            }
            set
            {
                if (patientData.LastName != value)
                {
                    patientData.LastName = value;
                    OnPropertyChange("LastName");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChange(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public RelayCommand<Window> CloseWindowCommand { get; private set; }
        public RelayCommand<Window> CreateNewExamCommand { get; private set; }

        public MainWindowViewModel()
        {
            patientData = new PatientData();
            this.CloseWindowCommand = new RelayCommand<Window>(this.CloseWindow);
            this.CreateNewExamCommand = new RelayCommand<Window>(this.CreateNewExam);
        }

        private void CloseWindow(Window window)
        {
            if (window != null)
            {
                window.Close();
            }
        }

        private void CreateNewExam(Window window)
        {
            if (string.IsNullOrEmpty(patientData.FirstName) || string.IsNullOrWhiteSpace(patientData.FirstName) || string.IsNullOrWhiteSpace(patientData.LastName) || string.IsNullOrEmpty(patientData.LastName))
            {
                MessageBox.Show("Veuillez préciser le prénom et le nom.");
            }
            else
            {
                patientData.FirstName = patientData.FirstName.Trim();
                patientData.LastName = patientData.LastName.Trim();

                GuideWindow objGuideWindow = new GuideWindow(patientData);
                //objGuideWindow.DataContext = this;
                //this.Visibility = Visibility.Hidden; // Hidding the current window
                this.CloseWindow(window);
                objGuideWindow.Show();
            }
        }

        //public ICommand ExitMainWindow
        //{
        //    get
        //    {
        //        return exitMainWindow
        //            ?? (exitMainWindow = new ActionCommand(() => { this.Close(); }));
        //    }
        //}

        //public ICommand CreateNewExam_key
        //{
        //    get
        //    {
        //        return createNewExam_key
        //            ?? (createNewExam_key = new ActionCommand(() => { CreateNewExam(); }));
        //    }
        //}

    }
}
