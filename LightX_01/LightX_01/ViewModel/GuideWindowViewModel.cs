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
    public class GuideWindowViewModel : ViewModelBase//, INotifyPropertyChanged
    {
        private PatientData patientData;

        public GuideWindowViewModel(PatientData passed_patientData)
        {
            patientData = passed_patientData;
        }
    }
}
