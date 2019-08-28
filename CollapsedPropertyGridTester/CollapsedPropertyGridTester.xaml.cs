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

namespace CollapsedPropertyGridTest
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class CollapsedPropertyGridTester : Window
	{
		ObservableCollection<object> bags = new ObservableCollection<object>();

		public CollapsedPropertyGridTester()
		{
			InitializeComponent();

			Bag bb = new Bag() { Name = "Bag1"};
			TextBox tb = new TextBox() { IsEnabled = true }; tb.KeyDown += sendtest;
			bb.Properties.Add(new Tuple<string, object, Control>("Text", "data", tb));
			bb.Properties.Add(new Tuple<string, object, Control>("ComboBox", new List<String>() { "one", "two", "three" }, new ComboBox()));
			bb.Properties.Add(new Tuple<string, object, Control>("CheckBox", true, new CheckBox()));
			bb.Properties.Add(new Tuple<string, object, Control>("Custom", null, new DropDownCustomColorPicker.CustomColorPicker()));

			Bag bb1 = new Bag() { Name="bag2"};
			tb = new TextBox() { IsEnabled = true }; tb.KeyDown += sendtest;
			bb1.Properties.Add(new Tuple<string, object, Control>("test", "behhh", tb));
			bb1.Properties.Add(new Tuple<string, object, Control>("ComboBox", new List<String>() { "1", "2", "3" }, new ComboBox()));
			bb1.Properties.Add(new Tuple<string, object, Control>("CheckBox", false, new CheckBox()));
			bb1.Properties.Add(new Tuple<string, object, Control>("Custom", null, new DropDownCustomColorPicker.CustomColorPicker()));

			bags.Add(bb); bags.Add(bb1);

		}

		public void sendtest(object sender, EventArgs e)
		{
			Console.WriteLine("memes");
		}


		private void SetItemSource_Click(object sender, RoutedEventArgs e)
		{
			Cpgrid.ItemsSource = bags;
		}

		private void AddItem_Click(object sender, RoutedEventArgs e)
		{
			Bag bb1 = new Bag() { Name = "bag2" };
			TextBox tb = new TextBox() { IsEnabled = true }; tb.KeyDown += sendtest;
			bb1.Properties.Add(new Tuple<string, object, Control>("test", "wasd", tb));
			bb1.Properties.Add(new Tuple<string, object, Control>("ComboBox", new List<String>() { "a", "b", "c" }, new ComboBox()));
			bb1.Properties.Add(new Tuple<string, object, Control>("CheckBox", false, new CheckBox()));
			bb1.Properties.Add(new Tuple<string, object, Control>("Custom", null, new DropDownCustomColorPicker.CustomColorPicker()));

			bags.Add(bb1);
		}

		private void ChangeItem_Click(object sender, RoutedEventArgs e)
		{
			Bag bb = ((Bag)bags[0]); bb.Name = "NewName!";
			bags[0] = bb; 
		}
		private void InternalChange_Click(object sender, RoutedEventArgs e)
		{
			((Bag)bags[1]).Properties[0] = new Tuple<string, object, Control>(((Bag)bags[1]).Properties[0].Item1, "REEEEE", ((Bag)bags[1]).Properties[0].Item3);
		}

		private void RemoveItem_Click(object sender, RoutedEventArgs e)
		{
			bags.RemoveAt(1);
		}

		private void Clear_Click(object sender, RoutedEventArgs e)
		{
			bags.Clear();
		}


	}
}
