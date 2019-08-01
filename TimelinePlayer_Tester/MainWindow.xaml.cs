using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace TimelinePlayer_Tester
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{

		public ObservableCollection<object> Titles { get; set; }

		public MainWindow()
		{
			Titles = new ObservableCollection<object>();
			
			InitializeComponent();
		}
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			Timeline.ItemsSource = Titles;
		}

		private void AddItem_BTN_Click(object sender, RoutedEventArgs e)
		{
			Timeline.ItemsSource = Titles;
			Titles.Add("");
		}

		
	}
}
