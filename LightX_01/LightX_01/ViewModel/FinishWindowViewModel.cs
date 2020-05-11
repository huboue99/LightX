using GalaSoft.MvvmLight;
using LightX_01.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightX_01.ViewModel
{
    public class FinishWindowViewModel : ViewModelBase
    {
        #region Fields

        private Exam _currentExam;

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

        #endregion Properties

        public FinishWindowViewModel(Exam exam)
        {
            CurrentExam = exam;
        }
    }
}
