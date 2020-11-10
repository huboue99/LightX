using LightX.Classes;
using LightX.Controls;
using LightX.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace ExamViewer
{
    
    public partial class ExamBrowser : Window
    {
        private readonly ExamBrowserViewModel _examBrowsweViewModel;

        private GridViewColumnHeader listViewSortCol = null;

        private SortAdorner listViewSortAdorner = null;


        public ExamBrowser()
        {
            _examBrowsweViewModel = new ExamBrowserViewModel();
            InitializeComponent();
            DataContext = _examBrowsweViewModel;
            this.Title = "LightX - Exam Viewer";
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.Show();
        }

        private void HeaderSort_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();
            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                examListObject.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            examListObject.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }

        public class SortAdorner : Adorner
        {
            private static Geometry ascGeometry =
                Geometry.Parse("M 0 4 L 3.5 0 L 7 4 Z");

            private static Geometry descGeometry =
                Geometry.Parse("M 0 0 L 3.5 4 L 7 0 Z");

            public ListSortDirection Direction { get; private set; }

            public SortAdorner(UIElement element, ListSortDirection dir)
                : base(element)
            {
                this.Direction = dir;
            }

            protected override void OnRender(DrawingContext drawingContext)
            {
                base.OnRender(drawingContext);

                if (AdornedElement.RenderSize.Width < 20)
                    return;

                TranslateTransform transform = new TranslateTransform
                    (
                        AdornedElement.RenderSize.Width - 15,
                        (AdornedElement.RenderSize.Height - 5) / 2
                    );
                drawingContext.PushTransform(transform);

                Geometry geometry = ascGeometry;
                if (this.Direction == ListSortDirection.Descending)
                    geometry = descGeometry;
                drawingContext.DrawGeometry(Brushes.Black, null, geometry);

                drawingContext.Pop();
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _examBrowsweViewModel.GetFilteredList();
        }

        private void Exam_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Exam selectedExam = ((ListView)sender).SelectedItem as Exam;
            _examBrowsweViewModel.ExamSelected(selectedExam);
        }
    }
}
