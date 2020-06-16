using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace LightX.Controls
{
    public class AutoCompleteTextBox : Control
    {

        private TextBox TXT_SEARCHINPUT;//For user input  
        private Popup PUP_AC;// For AutoCompleteList Popup  
        private DataGrid DG_AC;// DataGrid To Display List Of Filtered Items  

        public static readonly DependencyProperty AutoCompleteColumnsProperty = DependencyProperty.Register("AutoCompleteColumns", typeof(ObservableCollection<DataGridColumn>), typeof(AutoCompleteTextBox));
        public static readonly DependencyProperty AutoCompleteItemSourceProperty = DependencyProperty.Register("AutoCompleteItemSource", typeof(IEnumerable<object>), typeof(AutoCompleteTextBox));
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(AutoCompleteTextBox));
        public static readonly DependencyProperty AutoCompletePlacementDependencyProperty = DependencyProperty.Register("AutoCompletePlacement", typeof(PlacementMode), typeof(AutoCompleteTextBox));
        public static readonly DependencyProperty AutoCompletePlacementTargetDependencyProperty = DependencyProperty.Register("AutoCompletePlacementTarget", typeof(UIElement), typeof(AutoCompleteTextBox));
        public static readonly DependencyProperty AutoCompleteHorizontalOffsetDependencyProperty = DependencyProperty.Register("AutoCompleteHorizontalOffset", typeof(double), typeof(AutoCompleteTextBox));
        public static readonly DependencyProperty AutoCompleteVerticalOffsetDependencyProperty = DependencyProperty.Register("AutoCompleteVerticalOffset", typeof(double), typeof(AutoCompleteTextBox));
        public static readonly DependencyProperty AutoCompleteWidthDependencyProperty = DependencyProperty.Register("AutoCompleteWidth", typeof(double), typeof(AutoCompleteTextBox));
        public static readonly DependencyProperty AutoCompleteHeightDependencyProperty = DependencyProperty.Register("AutoCompleteHeight", typeof(double), typeof(AutoCompleteTextBox));

        public void ClearSearchInput()
        {
            this.TXT_SEARCHINPUT.Text = string.Empty;
            PUP_AC.IsOpen = false;
            this.SelectedItem = null;
        }

        public ObservableCollection<DataGridColumn> AutoCompleteColumns
        {
            get
            {
                return (ObservableCollection<DataGridColumn>)GetValue(AutoCompleteColumnsProperty);
            }
            set
            {
                SetValue(AutoCompleteColumnsProperty, value);
            }
        }
        public IEnumerable<object> AutoCompleteItemSource
        {
            get
            {
                return (IEnumerable<object>)GetValue(AutoCompleteItemSourceProperty);
            }
            set
            {
                SetValue(AutoCompleteItemSourceProperty, value);
            }
        }

        public object SelectedItem
        {
            get
            {
                return GetValue(SelectedItemProperty);
            }
            set
            {
                SetValue(SelectedItemProperty, value);

                if (value != null)
                {
                    TXT_SEARCHINPUT.Text = value.ToString();
                    TXT_SEARCHINPUT.Select(TXT_SEARCHINPUT.Text.Length, 0);
                }

                if (this.OnSelectedItemChange != null)
                    this.OnSelectedItemChange.Invoke(this, new EventArgs());
            }
        }
        public PlacementMode AutoCompletePlacement
        {
            get
            {
                return (PlacementMode)GetValue(AutoCompletePlacementDependencyProperty);
            }
            set
            {
                SetValue(AutoCompletePlacementDependencyProperty, value);
            }
        }
        public UIElement AutoCompletePlacementTarget
        {
            get
            {
                return (UIElement)GetValue(AutoCompletePlacementTargetDependencyProperty);
            }
            set
            {
                SetValue(AutoCompletePlacementTargetDependencyProperty, value);
            }
        }
        public double AutoCompleteHorizontalOffset
        {
            get { return (double)GetValue(AutoCompleteHorizontalOffsetDependencyProperty); }
            set { SetValue(AutoCompleteHorizontalOffsetDependencyProperty, value); }
        }
        public double AutoCompleteVerticalOffset
        {
            get { return (double)GetValue(AutoCompleteVerticalOffsetDependencyProperty); }
            set { SetValue(AutoCompleteVerticalOffsetDependencyProperty, value); }
        }
        public double AutoCompleteWidth
        {
            get { return (double)GetValue(AutoCompleteWidthDependencyProperty); }
            set
            {
                SetValue(AutoCompleteWidthDependencyProperty, value);
            }
        }
        public double AutoCompleteHeight
        {
            get { return (double)GetValue(AutoCompleteHeightDependencyProperty); }
            set
            {
                SetValue(AutoCompleteHeightDependencyProperty, value);
            }
        }

        // allows programmer to filter records as per input  
        public event TextChangedEventHandler OnTextChange;
        // event to perform operation on ItemSelection Changed  
        public event EventHandler OnSelectedItemChange;

        public AutoCompleteTextBox()
        {
            AutoCompleteColumns = new ObservableCollection<DataGridColumn>();
        }


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            TXT_SEARCHINPUT = this.Template.FindName("TXT_SEARCHINPUT", this) as TextBox;
            TXT_SEARCHINPUT.TextChanged += TXT_SEARCHINPUT_TextChanged;
            TXT_SEARCHINPUT.PreviewKeyDown += TXT_SEARCHINPUT_PreviewKeyDown;

            PUP_AC = this.Template.FindName("PUP_AC", this) as Popup;

            DG_AC = this.Template.FindName("DG_AC", this) as DataGrid;
            DG_AC.MouseLeftButtonUp += DG_AC_MouseLeftButtonUp;

            DG_AC.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            foreach (DataGridColumn column in AutoCompleteColumns)
            {
                column.Width = 5000;
                DG_AC.Columns.Add(column);
            }
        }


        //Select Item From List On Mouse Click  
        private void DG_AC_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var item = DG_AC.SelectedItem;
            if (item == null)
            {
                this.SelectedItem = null;
                return;
            }

            if (PUP_AC.IsOpen)
            {
                this.SelectedItem = item;
                PUP_AC.IsOpen = false;
            }
        }

        // Navigates through Items using Up/Down keyboard keys and select item on Enter key  
        private void TXT_SEARCHINPUT_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Down)
            {
                if (DG_AC.Items.Count > 0)
                {
                    int SelectedIndex = DG_AC.SelectedIndex;
                    if (SelectedIndex < DG_AC.Items.Count)
                        DG_AC.SelectedIndex++;
                }
            }
            else if (e.Key == System.Windows.Input.Key.Up)
            {
                if (DG_AC.Items.Count > 0)
                {
                    int SelectedIndex = DG_AC.SelectedIndex;
                    if (SelectedIndex > 0)
                        DG_AC.SelectedIndex--;
                }
            }
            else if (e.Key == System.Windows.Input.Key.Enter)
            {
                var item = DG_AC.SelectedItem;
                if (item == null)
                {
                    if (DG_AC.Items.Count > 0)
                        this.SelectedItem = DG_AC.Items[0];
                    else
                        this.SelectedItem = TXT_SEARCHINPUT.Text;
                    PUP_AC.IsOpen = false;
                    return;
                }

                if (PUP_AC.IsOpen)
                {
                    this.SelectedItem = item;
                    PUP_AC.IsOpen = false;
                }

            }
        }

        //Displays AutoComplete Pupup On User Input And Fire Event  'OnTextChange' to filter records  
        private void TXT_SEARCHINPUT_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SelectedItem != null && SelectedItem.ToString() != TXT_SEARCHINPUT.Text)
                this.SelectedItem = null;

            if (string.IsNullOrEmpty(TXT_SEARCHINPUT.Text))
            {
                PUP_AC.IsOpen = false;
            }
            else
            {
                PUP_AC.IsOpen = true;
            }

            if (this.OnTextChange != null)
            {
                this.OnTextChange.Invoke(sender, e);
            }
        }

    }
}
