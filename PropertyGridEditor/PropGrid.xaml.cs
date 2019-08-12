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
using DrWPF.Windows.Data;
using DropDownCustomColorPicker;

namespace PropertyGridEditor
{
	/// <summary>
	/// Interaction logic for UserControl1.xaml
	/// </summary>
	public partial class PropGrid : UserControl
	{
		//they key is the property name
		//value.Item1 is here to denote the control we wil be displaying to the screen
		//Value.Item2 is the actual data.
		//Dictionary<String, Tuple<Control, object>> properties = new Dictionary<string, Tuple<Control, object>>();
		public ObservableDictionary<String, object> PropDictionary = new ObservableDictionary<string, object>();
		private ObservableDictionary<String, object> FilteredPropDictionary = new ObservableDictionary<string, object>();
		public String CurrentProp = "";

		Grid NonFilteredGrid;

		public PropGrid()
		{
			InitializeComponent();
		}

		//THIS IS HERE TO ALLOW DESIGNER TO RENDER
		//static PropGrid()
		//{
		//	DefaultStyleKeyProperty.OverrideMetadata(typeof(PropGrid), new FrameworkPropertyMetadata(typeof(PropGrid)));
		//}

		public PropGrid(ref ObservableDictionary<String, object> d)
		{
			InitializeComponent();
			PropDictionary = d;
		}


#region AddProperty

		public void AddLabelProp(String PropName, ref int num)
		{
			if (PropDictionary.ContainsKey(PropName)) return; //avoid dict crash
																												//add a row.
			InnerPropGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30) });

			//add the label
			num = InnerPropGrid.RowDefinitions.Count - 1;

			Border bor = new Border() //create label then add it.
			{
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				Background = Brushes.Transparent,
				Tag = PropName
			};
			Grid.SetColumnSpan(bor, 2);
			Grid.SetRow(bor, num);
			Grid.SetZIndex(bor, -1);
			bor.MouseRightButtonDown += Ctype_MouseRightButtonDown;
			

			Label l = new Label() //create label then add it.
			{
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				Content = PropName,
				Background = Brushes.Transparent,
				Foreground = Brushes.White,
				Tag = PropName
			};

			Grid.SetRow(l, num);
			Grid.SetColumn(l, 0);
			InnerPropGrid.Children.Add(l); //add label to grid and display
			InnerPropGrid.Children.Add(bor); //add label to grid and display
		}

		/// <summary>
		/// Add a property to the grid, And set a custom event callback.
		/// </summary>
		/// <param name="PropName">Name of the property to add</param>
		/// <param name="ctype">The type of control that will be added to the grid</param>
		/// <param name="data">The data that will be inserted/set to the control</param>
		/// <param name="handler">the custom event handler</param>
		public void AddProperty(String PropName, TextBox ctype, String data, KeyEventHandler handler)
		{
			if (PropDictionary.ContainsKey(PropName)) return; //avoid dict crash
																												//add a row.

			int num = 0;
			//add label
			AddLabelProp(PropName, ref num);
			PropDictionary.Add(PropName, data);

			if (data is String)
			{
				ctype.HorizontalAlignment = HorizontalAlignment.Stretch;
				ctype.Margin = new Thickness(10, 2, 10, 2);
				((TextBox)ctype).Text = (String)data;
				ctype.Height = 30;

				Grid.SetRow(ctype, num);
				Grid.SetColumn(ctype, 1);
				ctype.BringIntoView();
				ctype.Tag = PropName; //used for EZ dictionary access later
				ctype.KeyDown += handler;

				InnerPropGrid.Children.Add(ctype); //add the desired control type.
			}
		}

		/// <summary>
		/// Add a property to the grid, And set a custom event callback.
		/// </summary>
		/// <param name="PropName">Name of the property to add</param>
		/// <param name="ctype">The type of control that will be added to the grid</param>
		/// <param name="data">The data that will be inserted/set to the control</param>
		/// <param name="handler">the custom event handler</param>
		public void AddProperty(String PropName, CheckBox ctype, bool data, RoutedEventHandler handler)
		{
			if (PropDictionary.ContainsKey(PropName)) return; //avoid dict crash

			int num = 0;
			//add label
			AddLabelProp(PropName, ref num);
			PropDictionary.Add(PropName, data);

			ctype.HorizontalAlignment = HorizontalAlignment.Left;
			ctype.VerticalAlignment = VerticalAlignment.Center;
			ctype.Margin = new Thickness(10, 2, 10, 2);
			ctype.Height = 30;

			Grid.SetRow(ctype, num);
			Grid.SetColumn(ctype, 1);
			ctype.BringIntoView();
			ctype.Tag = PropName; //used for EZ dictionary access later
			((CheckBox)ctype).Click += handler;
			((CheckBox)ctype).IsChecked = data;
			InnerPropGrid.Children.Add(ctype); //add the desired control type.
		}

		/// <summary>
		/// Add a property to the grid, And set a custom event callback.
		/// </summary>
		/// <param name="PropName">Name of the property to add</param>
		/// <param name="ctype">The type of control that will be added to the grid</param>
		/// <param name="data">The data that will be inserted/set to the control</param>
		/// <param name="handler">the custom event handler</param>
		public void AddProperty(String PropName, ComboBox ctype, List<String> data, SelectionChangedEventHandler handler)
		{
			if (PropDictionary.ContainsKey(PropName)) return; //avoid dict crash

			int num = 0;
			//add label
			AddLabelProp(PropName, ref num);
			PropDictionary.Add(PropName, data);

			ctype.HorizontalAlignment = HorizontalAlignment.Left;
			ctype.VerticalAlignment = VerticalAlignment.Center;
			ctype.Margin = new Thickness(10, 2, 10, 2);
			((ComboBox)ctype).ItemsSource = (List<String>)data;
			ctype.Height = 30;

			Grid.SetRow(ctype, num);
			Grid.SetColumn(ctype, 1);
			ctype.BringIntoView();
			ctype.Tag = PropName; //used for EZ dictionary access later
			((ComboBox)ctype).SelectionChanged += handler;
			((ComboBox)ctype).SelectedIndex = 0;
			InnerPropGrid.Children.Add(ctype); //add the desired control type.
			
		}

		/// <summary>
		/// Add a property to the grid with DEFAULT event call back.
		/// </summary>
		/// <param name="PropName">Name of the property to add</param>
		/// <param name="ctype">The type of control that will be added to the grid</param>
		/// <param name="data">The data that will be inserted/set to the control</param>
		public void AddProperty(String PropName, Control ctype, object data)
		{
			if (PropDictionary.ContainsKey(PropName)) return; //avoid dict crash
																												//add a row.

			int num = 0;
			//add label
			AddLabelProp(PropName, ref num);
			PropDictionary.Add(PropName, data);

			if (ctype is TextBox)
			{
				if (data is String)
				{
					ctype.HorizontalAlignment = HorizontalAlignment.Stretch;
					ctype.Margin = new Thickness(10, 2, 10, 2);
					((TextBox)ctype).Text = (String)data;
					ctype.Height = 30;

					Grid.SetRow(ctype, num);
					Grid.SetColumn(ctype, 1);
					ctype.BringIntoView();
					ctype.Tag = PropName + "_W"; //used for EZ dictionary access later
					ctype.KeyDown += Ctype_KeyDown;

					InnerPropGrid.Children.Add(ctype); //add the desired control type.
				}
			}
			else if (ctype is ComboBox)
			{
				if (data is List<String>)
				{
					ctype.HorizontalAlignment = HorizontalAlignment.Stretch;
					ctype.VerticalAlignment = VerticalAlignment.Center;
					ctype.Margin = new Thickness(10, 2, 10, 2);
					if(((ComboBox)ctype).ItemsSource == null)
						((ComboBox)ctype).ItemsSource = (List<String>)data;
					if(ctype.Height == 0)
						ctype.Height = 30;
					if (ctype.Height > 30)
						InnerPropGrid.RowDefinitions[num].Height = new GridLength(ctype.Height);

					Grid.SetRow(ctype, num);
					Grid.SetColumn(ctype, 1);
					ctype.BringIntoView();
					ctype.Tag = PropName; //used for EZ dictionary access later
					//((ComboBox)ctype).SelectionChanged += PropGrid_SelectionChanged;

					//((ComboBox)ctype).SelectedIndex = 0;
					InnerPropGrid.Children.Add(ctype); //add the desired control type.
				}
			}
			else if (ctype is CheckBox)
			{
				if (data is Boolean)
				{
					ctype.HorizontalAlignment = HorizontalAlignment.Left;
					ctype.VerticalAlignment = VerticalAlignment.Center;
					ctype.Margin = new Thickness(10, 2, 10, 2);
					ctype.Height = 30;

					Grid.SetRow(ctype, num);
					Grid.SetColumn(ctype, 1);
					ctype.BringIntoView();
					ctype.Tag = PropName; //used for EZ dictionary access later
					((CheckBox)ctype).Click += Ctype_Click;

					if (data is Boolean)
						((CheckBox)ctype).IsChecked = (bool)data;
					InnerPropGrid.Children.Add(ctype); //add the desired control type.
				}
			}
			else if (ctype is CustomColorPicker)
			{
				ctype.HorizontalAlignment = HorizontalAlignment.Left;
				ctype.VerticalAlignment = VerticalAlignment.Center;
				ctype.Margin = new Thickness(10, 2, 10, 2);
				ctype.Height = 30;

				Grid.SetRow(ctype, num);
				Grid.SetColumn(ctype, 1);
				ctype.BringIntoView();
				ctype.Tag = PropName; //used for EZ dictionary access later
				//((CustomColorPicker)ctype).SelectedColorChanged += ;

				InnerPropGrid.Children.Add(ctype); //add the desired control type.
			}

		}

		/// <summary>
		/// Adds property to the grid. Allows multiple labels, and input based controls!
		/// </summary>
		/// <param name="LabelsList">List of Labels that will be displayed.</param>
		/// <param name="InputControls">List of controls that allow for user input.</param>
		/// <param name="data">The data that will be placed in inputcontrols on init</param>
		/// <param name="eventHandlers">Delegates that will be activated on eventhandlers. (keydown, mousedown, etc)</param>
		public void AddProperty(List<Label> LabelsList, List<Control> InputControls, List<String> data)
		{
			//is the current number of columns enough?
			while (InnerPropGrid.ColumnDefinitions.Count < LabelsList.Count + InputControls.Count)
			{
				InnerPropGrid.ColumnDefinitions.Add(new ColumnDefinition() { });

				//add a grid splitter!
				GridSplitter gridSplitter = new GridSplitter() { Width = 2, VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Left, Background = Brushes.Gray };
				Grid.SetColumn(gridSplitter, InnerPropGrid.ColumnDefinitions.Count - 1);
				Grid.SetRowSpan(gridSplitter, int.MaxValue);
				Grid.SetZIndex(gridSplitter, int.MaxValue);
				InnerPropGrid.Children.Add(gridSplitter);
			}
			//resize the columns
			int newwidth = (int)(500 / InnerPropGrid.ColumnDefinitions.Count);
			foreach (ColumnDefinition cd in InnerPropGrid.ColumnDefinitions)
			{
				cd.Width = new GridLength(newwidth);
			}

			

			//add the input controls!
			AddInputControls(LabelsList[1].Content.ToString(), InputControls, data, LabelsList.Count);
			NonFilteredGrid = Property_Grid;
			//add the labels to the grid
			AddLabels(LabelsList); ;
		}

		#endregion

#region DefaultEvents
		private void Ctype_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			CurrentProp = ((Border)sender).Tag.ToString();
			ContextMenu cm = this.FindResource("RemoveProp_CM") as ContextMenu;
			cm.PlacementTarget = sender as UIElement;
			cm.IsOpen = true;
		}

		private void OpenClearCM(object sender, MouseButtonEventArgs e)
		{
			ContextMenu cm = this.FindResource("ClearProps_CM") as ContextMenu;
			cm.PlacementTarget = sender as UIElement;
			cm.IsOpen = true;
		}

		private void ClearProps_Click(object sender, RoutedEventArgs e)
		{
			foreach (String s in PropDictionary.Keys.ToList())
				RemoveProperty(s);
		}

		public void ClearProperties()
		{
			foreach (String s in PropDictionary.Keys.ToList())
				RemoveProperty(s);
		}

		private void RemoveProperty_Click(object sender, RoutedEventArgs e)
		{
			RemoveProperty(CurrentProp);
		}

		public void RemoveProperty(String PropName)
		{
			if (!PropDictionary.ContainsKey(PropName)) return; //don't remove what doesn't exist
			int num = 0;
			while (PropDictionary.Keys.ToList()[num] != PropName)
				num++; //inc until key is found.

			//remove from display
			InnerPropGrid.Children.Remove(GetChildren(InnerPropGrid, num, 0));
			InnerPropGrid.Children.Remove(GetChildren(InnerPropGrid, num, 1));


			//move all the other rows UP
			for (int numr = num + 1; numr < InnerPropGrid.RowDefinitions.Count; numr++)
			{
				Grid.SetRow(GetChildren(InnerPropGrid, numr, 0), numr - 1);
				Grid.SetRow(GetChildren(InnerPropGrid, numr, 1), numr - 1);
			}
			InnerPropGrid.RowDefinitions.RemoveAt(num);
			PropDictionary.Remove(PropName); //remove from dictionary
		}

		private void PropGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Console.WriteLine("Changed Combobox selection");
			PropDictionary[((ComboBox)sender).Tag.ToString()] = ((ComboBox)sender).SelectedIndex;
		}

		private void Ctype_Click(object sender, RoutedEventArgs e)
		{
			Console.WriteLine("Clicked Checkbox");
			PropDictionary[((CheckBox)sender).Tag.ToString()] = ((CheckBox)sender).IsChecked;
		}

		private void Ctype_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				Console.WriteLine("KeyDown on TextBox");
				PropDictionary[((TextBox)sender).Tag.ToString()] = ((TextBox)sender).Text;
			}
		}
#endregion

#region Find/Set Property
		public void UpdateProperty(String key, object data)
		{
			if (PropDictionary.ContainsKey(key))
			{
				PropDictionary[key] = data;
			}
		}

		public void SetPropertyData(String PName, object data)
		{
			Control CurControl = (Control)FindProperty(null, PName);
			if (CurControl is TextBox) {
				if (((TextBox)CurControl).Text != data.ToString())
					((TextBox)CurControl).Text = data.ToString();
			}
			else if (CurControl is CheckBox)
				((CheckBox)CurControl).IsEnabled = (bool)data;
			else if (CurControl is ComboBox)
				((ComboBox)CurControl).SelectedIndex = (int)data;
			PropDictionary[PName] = data;
		}

		/// <summary>
		/// Search and find the property control with the given tag or name!
		/// </summary>
		/// <param name="Name"></param>
		/// <param name="Tag"></param>
		/// <returns></returns>
		private UIElement FindProperty(String Name = null, String Tag = null)
		{
			if (Name != null)
			{
				foreach (UIElement uie in InnerPropGrid.Children)
				{
					if (uie is Control)
					{
						if (((Control)uie).Tag == null) continue;
						if (((Control)uie).Name == Name)
						{
							return uie;
						}
					}
				}
			}
			else if (Tag != null)
			{
				foreach (UIElement uie in InnerPropGrid.Children)
				{
					if (uie is Control)
					{
						if (((Control)uie).Tag == null) continue;
						if (((Control)uie).Tag.ToString() == Tag)
						{
							return uie;
						}
					}
				}
			}
			return null;
		}
#endregion

#region Filtering

		/// <summary>
		/// Filters out all the dictionary keys that don't contain the given keyword
		/// </summary>
		/// <param name="FilterText"></param>
		public void FilterList(String FilterText)
		{
			int i = 0;

			if (FilterText == "" || FilterText == "Return")
			{
				ResetFilter();
				return;
			}

			foreach (String s in PropDictionary.Keys.ToList())
			{
				if (!s.Contains(FilterText))
				{
					InnerPropGrid.RowDefinitions[i].Height = new GridLength(0);
				}
				i++;
			}
		}

		/// <summary>
		/// These are the indexes that will continue to be displayed
		/// </summary>
		/// <param name="filterindexes"></param>
		public void FilterList(int[] filterindexes)
		{

			foreach (RowDefinition rd in InnerPropGrid.RowDefinitions)
			{
				rd.Height = new GridLength(0);
			}

			foreach (int i in filterindexes)
			{
				InnerPropGrid.RowDefinitions[i].Height = new GridLength(30);
			}
		}

		/// <summary>
		/// Resets the filter so every row is show again!
		/// Sets the row height to non zero
		/// </summary>
		public void ResetFilter()
		{
			foreach (RowDefinition rd in InnerPropGrid.RowDefinitions)
				rd.Height = new GridLength(30);
		}


		#endregion

		private void AddInputControls(String name, List<Control> controls, List<String> data, int offset)
		{
			for (int i = 0; i < controls.Count; i++)
			{
				if (controls[i] is TextBox)
				{

					((TextBox)controls[i]).HorizontalAlignment = HorizontalAlignment.Stretch;
					((TextBox)controls[i]).VerticalContentAlignment = VerticalAlignment.Center;
					((TextBox)controls[i]).Text = data[i];

					Grid.SetRow((controls[i]), InnerPropGrid.RowDefinitions.Count );
					Grid.SetColumn(controls[i], i + offset);
					InnerPropGrid.Children.Add(controls[i]);
					Console.WriteLine("Adding a new TextBox");



				}
				else if (controls[i] is ComboBox)
				{
					((ComboBox)controls[i]).HorizontalAlignment = HorizontalAlignment.Stretch;

					Grid.SetRow((controls[i]), InnerPropGrid.RowDefinitions.Count );
					Grid.SetColumn(controls[i], i + offset);
					InnerPropGrid.Children.Add(controls[i]);
				}
				else if (controls[i] is CheckBox)
				{
					((CheckBox)controls[i]).HorizontalAlignment = HorizontalAlignment.Left;
					((CheckBox)controls[i]).VerticalAlignment = VerticalAlignment.Center;
					Grid.SetRow((controls[i]), InnerPropGrid.RowDefinitions.Count );
					Grid.SetColumn(controls[i], i + offset);
					InnerPropGrid.Children.Add(controls[i]);
				}
			}
			PropDictionary.Add(name, data[0]);
		}

		private void AddLabels(List<Label> labels)
		{
			//add a row!
			InnerPropGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30) });
			int num = InnerPropGrid.RowDefinitions.Count - 1;

			//create the border. allows the right click to work on the WHOLE row
			Border bor = new Border()
			{
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				Background = Brushes.Transparent,
				Tag = PropDictionary.Keys.ToList().Last()
			};
			Grid.SetColumnSpan(bor, 2);
			Grid.SetRow(bor, num);
			Grid.SetZIndex(bor, int.MaxValue - 1);
			bor.MouseRightButtonDown += Ctype_MouseRightButtonDown;
			

			//add the labels to the columns!
			for (int i = 0; i < labels.Count; i++)
			{
				Grid.SetRow(labels[i], num);
				Grid.SetColumn(labels[i], i);
				InnerPropGrid.Children.Add(labels[i]); //add label to grid and display
			}
			InnerPropGrid.Children.Add(bor); //add label to grid and display
		}


		#region WIP

		public void AddRightClick_ContextMenuItem(MenuItem ItemToAdd)
		{
			//get the context menu item
			ContextMenu cm = (ContextMenu)this.Resources["RemoveProp_CM"];
			if(cm != null)
			{
				cm.Items.Add(ItemToAdd);
			}
		}



		#endregion


		private static UIElement GetChildren(Grid grid, int row, int column)
		{
			foreach (UIElement child in grid.Children)
			{
				if (child is GridSplitter ||  child is Border) continue;
				if (Grid.GetRow(child) == row
							&&
					 Grid.GetColumn(child) == column)
				{
					return child;
				}
			}
			return null;
		}


	}
}
