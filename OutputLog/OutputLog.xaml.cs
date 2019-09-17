using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OutputLog
{
  /// <summary>
  /// Interaction logic for UserControl1.xaml
  /// </summary>
  public partial class OutputLog : UserControl
  {
    public OutputLog()
    {
        InitializeComponent();
    }

		public void AddLogItem(String logitem)
		{
			OutputLog_RTB.AppendText(logitem);
			OutputLog_RTB.AppendText("\n");
		}

		public void AddErrorLogItem(int ErrorCode, String Desc, String ToolName, bool isWarning)
		{
			//add row
			OutputLog_Grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(40) });
			//Add error type error
			Image image = GetErrorImageType(ErrorCode, isWarning);
			Grid.SetRow(image, OutputLog_Grid.RowDefinitions.Count -1); Grid.SetColumn(image, 0);
			OutputLog_Grid.Children.Add(image);
			//Add Error Code
			TextBlock ErrTB = new TextBlock() { Foreground = Brushes.White, Text = ErrorCode.ToString(), Margin = new Thickness(5) };
			Grid.SetRow(ErrTB, OutputLog_Grid.RowDefinitions.Count - 1); Grid.SetColumn(ErrTB, 1);
			OutputLog_Grid.Children.Add(ErrTB);
			//Add Error Desc
			TextBlock DescTB = new TextBlock() { Foreground = Brushes.White, Text = Desc, Margin = new Thickness(5) };
			Grid.SetRow(DescTB, OutputLog_Grid.RowDefinitions.Count - 1); Grid.SetColumn(DescTB, 2);
			OutputLog_Grid.Children.Add(DescTB);
			//Add Tool type/name
			TextBlock TypeTB = new TextBlock() { Foreground = Brushes.White, Text = ToolName, Margin = new Thickness(5) };
			Grid.SetRow(TypeTB, OutputLog_Grid.RowDefinitions.Count - 1); Grid.SetColumn(TypeTB, 3);
			OutputLog_Grid.Children.Add(TypeTB);

		}

		public Image GetErrorImageType(int ErrorCode, bool isWarning)
		{
			Image image = new Image() { Width = 25, Height = 25 };
			var pic = new System.Windows.Media.Imaging.BitmapImage();
			pic.BeginInit();
			//Image for type of error
			if (ErrorCode == 0 && isWarning)
				pic.UriSource = new Uri("Images/Warning.png", UriKind.Relative); // url is from the xml
			else if (ErrorCode != 0)
				pic.UriSource = new Uri("Images/Error.png", UriKind.Relative); // url is from the xml
			else
				pic.UriSource = new Uri("Images/CheckMark.png", UriKind.Relative); // url is from the xml
			pic.EndInit();
			image.Source = pic;
			return image;
		}

		

		public void ClearErrorLog()
		{
			OutputLog_Grid.Children.Clear();
			OutputLog_Grid.RowDefinitions.Clear();
		}



		private void Header_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
		{
			Grid g = (Grid)((GridSplitter)sender).Parent;

			for(int i = 0; i < g.ColumnDefinitions.Count; i++)
			{
				OutputLog_Grid.ColumnDefinitions[i].Width = g.ColumnDefinitions[i].Width;
			}

		}

		private void RowData_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
		{
			Grid g = (Grid)((GridSplitter)sender).Parent;

			for (int i = 0; i < g.ColumnDefinitions.Count; i++)
			{
				header_grid.ColumnDefinitions[i].Width = g.ColumnDefinitions[i].Width;
			}

		}

		private void OutputLog_RTB_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			ContextMenu cm = this.FindResource("ClearLog_CM") as ContextMenu;
			//((MenuItem)cm.Items[0]).IsChecked = ((MenuItem)cm.Items[0]).IsChecked;
			cm.PlacementTarget = sender as ContentControl;
			cm.IsOpen = true;
		}

		private void ClearLog_CM_Click(object sender, RoutedEventArgs e)
		{
			TextRange txt = new TextRange(OutputLog_RTB.Document.ContentStart, OutputLog_RTB.Document.ContentEnd);
			txt.Text = "";
		}

		private void ClearErrorLog_CM_Click(object sender, RoutedEventArgs e)
		{
			ClearErrorLog();
		}

		private void ScrollViewer_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			ContextMenu cm = this.FindResource("ClearErrorLog_CM") as ContextMenu;
			//((MenuItem)cm.Items[0]).IsChecked = ((MenuItem)cm.Items[0]).IsChecked;
			cm.PlacementTarget = sender as ContentControl;
			cm.IsOpen = true;
		}
	}
}
