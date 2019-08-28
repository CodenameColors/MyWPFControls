using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PropertyGridEditor;

namespace CollapsedPropertyGrid
{
  /// <summary>
  /// Interaction logic for UserControl1.xaml
  /// </summary>
  public partial class CollapsedPropertyGrid : UserControl
  {

		ObservableCollection<object> PropertyGrids = new ObservableCollection<object>();

		public IEnumerable ItemsSource
		{
			get { return (IEnumerable)GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
		}

		public static readonly DependencyProperty ItemsSourceProperty =
				DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(CollapsedPropertyGrid), new PropertyMetadata(new PropertyChangedCallback(OnItemsSourcePropertyChanged)));

		private static void OnItemsSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			var control = sender as CollapsedPropertyGrid;
			if (control != null)
				control.OnItemsSourceChanged((IEnumerable)e.OldValue, (IEnumerable)e.NewValue);
		}

		private void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
		{
			// Remove handler for oldValue.CollectionChanged
			if (oldValue != null)
			{
				((ObservableCollection<object>)oldValue).CollectionChanged -= new NotifyCollectionChangedEventHandler(newValueINotifyCollectionChanged_CollectionChanged);
			}
			// Add handler for newValue.CollectionChanged (if possible)
			if (null != newValue)
			{
				((ObservableCollection<object>)newValue).CollectionChanged += new NotifyCollectionChangedEventHandler(newValueINotifyCollectionChanged_CollectionChanged);

				foreach (object o in newValue)
				{
					if(PropertyHasValue(o, "Properties") != null)
					{
						ObservableCollection<Tuple<String, object, Control>> obs = (ObservableCollection < Tuple<String, object, Control> > )PropertyHasValue(o, "Properties");
						PropertyGridEditor.PropGrid pgrid = new PropGrid() { HorizontalAlignment = HorizontalAlignment.Stretch, Width = PGrid_TreeView.ActualWidth - 50};
						foreach (Tuple<String, object, Control> t in obs)
						{
							pgrid.AddProperty(t.Item1, t.Item3, t.Item2);
						}
						pgrid.ItemsSource = obs;
						String s = "NameNotSet";
						if (PropertyHasValue(o, "Name") != null)
							s = PropertyHasValue(o, "Name").ToString();

						TreeViewItem tvi = new TreeViewItem() { Header = s, Foreground = Brushes.White };
						tvi.Items.Add(pgrid);
						PGrid_TreeView.Items.Add(tvi);
					}
				}
			}
		}

		void newValueINotifyCollectionChanged_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if(e.Action == NotifyCollectionChangedAction.Add)
			{
				foreach (object o in e.NewItems)
				{
					if (PropertyHasValue(o, "Properties") != null)
					{
						ObservableCollection<Tuple<String, object, Control>> obs = (ObservableCollection<Tuple<String, object, Control>>)PropertyHasValue(o, "Properties");
						PropertyGridEditor.PropGrid pgrid = new PropGrid() { HorizontalAlignment = HorizontalAlignment.Stretch, Width = PGrid_TreeView.ActualWidth - 50 };
						foreach (Tuple<String, object, Control> t in obs)
						{
							pgrid.AddProperty(t.Item1, t.Item3, t.Item2);
						}
						pgrid.ItemsSource = obs;
						String s = "NameNotSet";
						if (PropertyHasValue(o, "Name") != null)
							s = PropertyHasValue(o, "Name").ToString();

						TreeViewItem tvi = new TreeViewItem() { Header = s, Foreground = Brushes.White };
						tvi.Items.Add(pgrid);
						PGrid_TreeView.Items.Add(tvi);
					}
				}
			}
			else if (e.Action == NotifyCollectionChangedAction.Remove)
			{
				PGrid_TreeView.Items.RemoveAt(e.OldStartingIndex);
			}
			else if (e.Action == NotifyCollectionChangedAction.Replace)
			{
				(((PropertyGridEditor.PropGrid)((TreeViewItem)PGrid_TreeView.Items[e.OldStartingIndex]).Items[0])).ItemsSource = null;
				(((PropertyGridEditor.PropGrid)((TreeViewItem)PGrid_TreeView.Items[e.OldStartingIndex]).Items[0])).ClearProperties();
				PGrid_TreeView.Items.RemoveAt(e.OldStartingIndex);
				foreach (object o in e.NewItems)
				{
					if (PropertyHasValue(o, "Properties") != null)
					{
						ObservableCollection<Tuple<String, object, Control>> obs = (ObservableCollection<Tuple<String, object, Control>>)PropertyHasValue(o, "Properties");
						PropertyGridEditor.PropGrid pgrid = new PropGrid() { HorizontalAlignment = HorizontalAlignment.Stretch, Width = PGrid_TreeView.ActualWidth - 50 };
						foreach (Tuple<String, object, Control> t in obs)
						{
							pgrid.AddProperty(t.Item1, t.Item3, t.Item2);
						}
						pgrid.ItemsSource = obs;
						String s = "NameNotSet";
						if (PropertyHasValue(o, "Name") != null)
							s = PropertyHasValue(o, "Name").ToString();

						TreeViewItem tvi = new TreeViewItem() { Header = s, Foreground = Brushes.White };
						tvi.Items.Add(pgrid);
						
						PGrid_TreeView.Items.Insert(e.OldStartingIndex, tvi);
					}
				}
			}
			else if(e.Action == NotifyCollectionChangedAction.Reset)
			{
				PGrid_TreeView.Items.Clear();
			}
		}

		public CollapsedPropertyGrid()
    {
      InitializeComponent();
    }




		public static object PropertyHasValue(object obj, string propertyName)
		{
			try
			{
				if (obj != null)
				{
					Type t = obj.GetType();
					PropertyInfo prop = t.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
					if (prop != null)
					{
						object val = prop.GetValue(obj, null);
						string sVal = Convert.ToString(val);
						if (sVal != String.Empty)
						{
							Console.WriteLine("property exists, and has data.");
							return val;
						}
					}
				}

				Console.WriteLine("Given Obj doesn't exist. it's null");
				return null;
			}
			catch
			{
				Console.WriteLine("An error occurred.");
				return null;
			}
		}

	}
}
