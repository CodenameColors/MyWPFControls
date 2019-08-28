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
using PropertyGridEditor;
using DrWPF.Windows.Data;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Threading;

namespace PropGridTester
{
	public enum EObjectType
	{
		None,
		Folder,
		File,
		Sprite,
		GameEvent 
	};
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		PropGrid pg;
		List<String> ss = new List<String>();
		public ObservableDictionary<String, object> PropDictionary = new ObservableDictionary<string, object>();
		private Bag bb = new Bag();
		DispatcherTimer timer = new DispatcherTimer();

		public MainWindow()
		{
			
			InitializeComponent();
			pg = ttt;


			ss.Add("A");
			ss.Add("B");
			ss.Add("C");

			EObjectType e = (EObjectType)Enum.Parse(typeof(EObjectType), "1");

			timer.Interval = TimeSpan.FromMilliseconds(10);
			timer.Tick += TickTest;
			TextBox tb = new TextBox() { IsEnabled = true }; tb.KeyDown += sendtest;
			bb.DictionaryValues.Add(new Tuple<string, object, Control>("Text", "data", tb));
			bb.DictionaryValues.Add(new Tuple<string, object, Control>("ComboBox", new List<String>() { "one", "two", "three" }, new ComboBox()));
			bb.DictionaryValues.Add(new Tuple<string, object, Control>("CheckBox", true, new CheckBox()));
			bb.DictionaryValues.Add(new Tuple<string, object, Control>("Custom", null, new DropDownCustomColorPicker.CustomColorPicker()));

		}


		public void sendtest(object sender, EventArgs e)
		{
			Console.WriteLine("memes");
		}

		

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			pg.ItemsSource = bb.DictionaryValues;
}
		private void Button1_Click(object sender, RoutedEventArgs e)	
		{
			//bb.Setdictval("Text Box", "Test");
			timer.Start();
		}

		private void Button2_Click(object sender, RoutedEventArgs e)
		{
			bb.DictionaryValues.Add(new Tuple<string, object, Control>("ComboBox-1", new List<String>() { "memes", "big", "chungus" }, new ComboBox()));
		}

		public void TickTest(object sender, EventArgs e)
		{
			bb.DictionaryValues[0] = new Tuple<string, object, Control>(bb.DictionaryValues[0].Item1, "data2", bb.DictionaryValues[0].Item3);
		}

		//private void Button_Click(object sender, RoutedEventArgs e)
		//{
		//	//ttt.AddRow();
		//	foreach (String s in ss)
		//	{
		//		ttt.AddProperty(s, new TextBox(), "hhhhhhhhnnnnnnggggg");

		//	}
		//	ttt.AddProperty("isActive?", new CheckBox(), true, CheckBox_Click);
		//	List<String> name = Enum.GetNames(typeof(EObjectType)).ToList();
		//	ttt.AddProperty("Choice Time!", new ComboBox(), name);

		//	List<Label> lList = new List<Label>();

		//	for(int i = 0; i < 4; i++)
		//	{
		//		lList.Add(new Label() { Height = 30, Content = "Test Label " + i, Foreground=Brushes.White });
		//	}

		//	List<Control> tboxes = new List<Control>();
		//	tboxes.Add(new TextBox() { Height = 30, Foreground = Brushes.Green, HorizontalAlignment = HorizontalAlignment.Stretch, IsEnabled = false });
		//	tboxes.Last().KeyDown += T1Down;
		//	tboxes.Add(new TextBox() { Height = 30, Foreground = Brushes.Black, HorizontalAlignment = HorizontalAlignment.Stretch });
		//	tboxes.Last().KeyDown += T2Down;
		//	tboxes.Add(new CheckBox());
		//	tboxes.Add(new ComboBox());

		//	List<String> vs = new List<string>();
		//	vs.Add("Dank"); vs.Add("Memes");

		//	ttt.AddProperty(lList, tboxes, vs);
		//	ttt.AddProperty("Background", new DropDownCustomColorPicker.CustomColorPicker(), null);
		//	//this.PropDictionary = ttt.PropDictionary; //update...
		//}

		public void CheckBox_Click(object sender, RoutedEventArgs e)
		{
			Console.WriteLine("Custom Event Fires!");
		}

		public void T1Down(object sender, KeyEventArgs e)
		{
			Console.WriteLine("T1 down");
		}

		public void T2Down(object sender, KeyEventArgs e)
		{
			Console.WriteLine("T2 down");
		}

		

		//testing the add CM
		

		public void AddClickything(object sender, RoutedEventArgs e)
		{
			Console.WriteLine("WIP right now sorry...");

		}

		private void TextBox_KeyDown(object sender, KeyEventArgs e)
		{
			String s = ((TextBox)sender).Text + e.Key;
			ttt.ResetFilter();
			//ttt.FilterList(s);
			ttt.FilterList(new int[] { 0,1,3});
		}
	}
}
