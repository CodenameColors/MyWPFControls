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
using TimelinePlayer.Components;

namespace TimelinePlayer_Tester
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{

		public ObservableCollection<Timeline> Titles { get; set; }

		public MainWindow()
		{
			Titles = new ObservableCollection<Timeline>();
			
			InitializeComponent();
		}
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			Timeline.ItemsSource = Titles;
			Timeline.ActiveTBblocks.CollectionChanged += ActiveTBblocks_CollectionChanged;
		}

		private void ActiveTBblocks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
			{
				Console.WriteLine("Added Active Time Block");
			}
			else
			{
				Console.WriteLine("Removed Active Time block");
			}
		}

		private void AddItem_BTN_Click(object sender, RoutedEventArgs e)
		{
			Timeline.ItemsSource = Titles;
			Titles.Add(new Timeline(60, 200)
			{
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				Background = Brushes.Gray,
				Margin = new Thickness(0, 3, 0, 3),
				TrackName = "Emma"
			});
			//Titles[0].ItemsSource = Titles[0].Children;
		}

		/// <summary>
		/// Adds a new timeblock to the list. and displays it. It's hooked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Addtblock_BTN_Click(object sender, RoutedEventArgs e)
		{
			Titles[0].AddTimeBlock(new TimeBlock(Titles[0], 0)
			{
				Trackname = "Memes",
				Width = 100,
				Margin = new Thickness(0, 0, 0, 3)
			},0);
		}
	}
}
