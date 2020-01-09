﻿using LightX_01.Classes;
using LightX_01.ViewModel;
using System.Windows;

namespace LightX_01
{
    /// <summary>
    /// Interaction logic for GuideWindow.xaml
    /// </summary>
    /// 
    
    public partial class GuideWindow : Window
    {
        private readonly GuideWindowViewModel _guideWindowViewModel;

        public GuideWindow(Exam exam)
        {
            _guideWindowViewModel = new GuideWindowViewModel(exam);
            InitializeComponent();
            DataContext = _guideWindowViewModel;

            Patient patientData = exam.Patient;
            this.Title = $"LightX - {patientData.FirstName} {patientData.LastName} - {exam.ExamDate.Day:D2}/{exam.ExamDate.Month:D2}/{exam.ExamDate.Year} - {exam.ExamDate.Hour:D2}:{exam.ExamDate.Minute:D2}:{exam.ExamDate.Second:D2}";
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            
        }
    }
}
