using LightX.Classes;
using LightX.Controls;
using LightX.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            Disease keyword = (sender as System.Windows.Controls.Button).DataContext as Disease;
            _patientInfosWindowViewModel.RemoveKeyword(keyword);
        }

        private void Txt_OnTextChange(object sender, TextChangedEventArgs e)
        {
            TextBox txtInput = sender as TextBox;
            //var Emps = from emp in _patientInfosWindowViewModel.KeywordsList where emp.ToLowerInvariant().Contains(txtInput.Text.ToLowerInvariant()) select emp;
            var Emps = from emp in _patientInfosWindowViewModel.KeywordsList where (((from keywrds in emp.Keywords where keywrds.ToLowerInvariant().Contains(txtInput.Text.ToLowerInvariant()) select keywrds)).Count<string>() != 0) select emp;
            txt.AutoCompleteItemSource = Emps;
        }

        private void Txt_OnSelectedItemChange(object sender, EventArgs e)
        {
            if (txt.SelectedItem != null)
            {
                Disease keyword = txt.SelectedItem as Disease;
                if (!_patientInfosWindowViewModel.KeywordsList.Contains(keyword))
                {
                    var result = System.Windows.MessageBox.Show("The keyword you are trying to add does not exist.\nDo you want to add it to the keyword list?", "", MessageBoxButton.YesNoCancel);
                    if (result == MessageBoxResult.Yes)
                    {
                        string word = (sender as AutoCompleteTextBox).SelectedItem.ToString();
                        keyword = new Disease() { DisplayName = word, Keywords = new List<string>() { word } };
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
        }
    }
}
