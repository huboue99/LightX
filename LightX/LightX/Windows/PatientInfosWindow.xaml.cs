using LightX.Classes;
using LightX.ViewModel;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LightX
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    
    public partial class PatientInfosWindow : Window
    {
        public Exam Exam
        {
            get { return _patientInfosWindowViewModel.Exam; }
            set
            {
                _patientInfosWindowViewModel.Exam = value;
            }
        }

        private readonly PatientInfosWindowViewModel _patientInfosWindowViewModel;
        public PatientInfosWindow()
        {
            _patientInfosWindowViewModel = new PatientInfosWindowViewModel();
            
            InitializeComponent();
            FileNumber.Focus();
            DataContext = _patientInfosWindowViewModel;
            this.Title = "LightX - Nouvel Examen";
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void KeywordButton_Click(object sender, RoutedEventArgs e)
        {
            string keyword = (sender as System.Windows.Controls.Button).Content.ToString();
            _patientInfosWindowViewModel.RemoveKeyword(keyword);
        }

        private void Txt_OnTextChange(object sender, TextChangedEventArgs e)
        {
            TextBox txtInput = sender as TextBox;
            var Emps = from emp in _patientInfosWindowViewModel.KeywordsList where emp.ToLowerInvariant().Contains(txtInput.Text.ToLowerInvariant()) select emp;
            txt.AutoCompleteItemSource = Emps;

        }

        private void Txt_OnSelectedItemChange(object sender, EventArgs e)
        {
            if (txt.SelectedItem != null)
            {
                string keyword = txt.SelectedItem.ToString();
                if (!_patientInfosWindowViewModel.KeywordsList.Contains(keyword))
                {
                    var result = System.Windows.MessageBox.Show("The keyword you are trying to add does not exist.\nDo you want to add it to the keyword list?", "", MessageBoxButton.YesNoCancel);
                    if (result == MessageBoxResult.Yes)
                    {
                        _patientInfosWindowViewModel.KeywordsList.Add(keyword);
                        _patientInfosWindowViewModel.SaveKeywordList(_patientInfosWindowViewModel.KeywordsList);
                    }
                    else
                        return;
                }
                if (!_patientInfosWindowViewModel.Keywords.Contains(keyword))
                    _patientInfosWindowViewModel.Keywords.Add(keyword);
                (sender as LightX.Controls.AutoCompleteTextBox).ClearSearchInput();
            }
            
            //if (txt.SelectedItem == null)
            //    tb.Text = "Selcted Employee :";
            //else
            //{
            //    tb.Text = "Selcted Employee : " + txt.SelectedItem.ToString();
            //}
        }
    }
}
