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
		public MainWindow()
		{
			
			InitializeComponent();
			pg = ttt;


			ss.Add("A");
			ss.Add("B");
			ss.Add("C");

			EObjectType e = (EObjectType)Enum.Parse(typeof(EObjectType), "1");
			
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			//ttt.AddRow();
			foreach (String s in ss)
			{
				ttt.AddProperty(s, new TextBox(), "hhhhhhhhnnnnnnggggg");

			}
			ttt.AddProperty("isActive?", new CheckBox(), true, CheckBox_Click);
			List<String> name = Enum.GetNames(typeof(EObjectType)).ToList();
			ttt.AddProperty("Choice Time!", new ComboBox(), name);

			List<Label> lList = new List<Label>();
			
			for(int i = 0; i < 4; i++)
			{
				lList.Add(new Label() { Height = 30, Content = "Test Label " + i, Foreground=Brushes.White });
			}

			List<Control> tboxes = new List<Control>();
			tboxes.Add(new TextBox() { Height = 30, Foreground = Brushes.Green, HorizontalAlignment = HorizontalAlignment.Stretch, IsEnabled = false });
			tboxes.Last().KeyDown += T1Down;
			tboxes.Add(new TextBox() { Height = 30, Foreground = Brushes.Black, HorizontalAlignment = HorizontalAlignment.Stretch });
			tboxes.Last().KeyDown += T2Down;
			tboxes.Add(new CheckBox());
			tboxes.Add(new ComboBox());

			List<String> vs = new List<string>();
			vs.Add("Dank"); vs.Add("Memes");

			ttt.AddProperty(lList, tboxes, vs);
			ttt.AddProperty("Background", new DropDownCustomColorPicker.CustomColorPicker(), null);
			//this.PropDictionary = ttt.PropDictionary; //update...
		}

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

		private void Button1_Click(object sender, RoutedEventArgs e)
		{

		}

		//testing the add CM
		private void Button2_Click(object sender, RoutedEventArgs e)
		{
			MenuItem menuItem = new MenuItem() { Header = "memes" };
			menuItem.Click += AddClickything;
			ttt.AddRightClick_ContextMenuItem(menuItem);
		}

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
