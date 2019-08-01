using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TimelinePlayer.Components;

namespace TimelinePlayer
{


	internal class TimeBlockDragAdorner : Adorner
	{
		private ContentPresenter _adorningContentPresenter;
		internal object Data { get; set; }
		internal DataTemplate Template {get;set;}
		Point _mousePosition;
		public Point MousePosition
		{
			get
			{
				return _mousePosition;
			}
			set
			{
				if (_mousePosition != value)
				{
					_mousePosition = value;
					_layer.Update(AdornedElement);
				}

			}
		}

		AdornerLayer _layer;

		public TimeBlockDragAdorner(UIElement uiElement) : base(uiElement)
		{
			_adorningContentPresenter = new ContentPresenter();
			//_adorningContentPresenter.Content = uiElement.DataContext;
			//_adorningContentPresenter.ContentTemplate = template;
			_adorningContentPresenter.Opacity = 0.5;
			_layer = AdornerLayer.GetAdornerLayer(uiElement);

			_layer.Add(this);
			IsHitTestVisible = false;
		}


	}




	/// <summary>
	/// Interaction logic for UserControl1.xaml
	/// </summary>
	public partial class TimelinePlayer : UserControl
  {

		static TimeBlockDragAdorner _dragAdorner;
		public IEnumerable ItemsSource
		{
			get { return (IEnumerable)GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
		}

		public static readonly DependencyProperty ItemsSourceProperty =
				DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(TimelinePlayer), new PropertyMetadata(new PropertyChangedCallback(OnItemsSourcePropertyChanged)));

		private static void OnItemsSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			var control = sender as TimelinePlayer;
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
			}
		}

		void newValueINotifyCollectionChanged_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			//Do your stuff here.
			Console.WriteLine("Added item to source");
			//Tracks_SV.Children.Add(new Button());
			//TrackItems_LB.Items.Add(sender);

			//Button bor = new Button() { Background = Brushes.Aqua };
			ContentControl bor = (ContentControl)this.Resources["TimelineBlock_CC"];
			Tracks_Grid.RowDefinitions.Add(new RowDefinition(){ Height=new GridLength(50)});
			Grid.SetRow(bor, Tracks_Grid.RowDefinitions.Count - 1);
			Tracks_Grid.Children.Add(bor);
			Timelines_Grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(50) });
			Canvas c = new Canvas() { Background = Brushes.Purple, Margin=new Thickness(0,3,0,3)};
			c.MouseEnter += C_MouseEnter;
			Grid.SetRow(c, Tracks_Grid.RowDefinitions.Count - 1);

			//add my custom time block
			TimeBlock timeBlock = new TimeBlock() { Width = 100, Margin = new Thickness(0,3,0,3)};
			c.Children.Add(timeBlock);
			Timelines_Grid.Children.Add(c);
		}

		private void C_MouseEnter(object sender, MouseEventArgs e)
		{
			_dragAdorner = new TimeBlockDragAdorner((Canvas)sender);
		}

		public TimelinePlayer()
    {
			InitializeComponent();
			Console.WriteLine("init");

			
		}

		private void AddTrack_BTN_Click(object sender, RoutedEventArgs e)
		{
			Console.WriteLine("AddTrack");
			
		}

		private void Timelines_VertScroll(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			Tracks_SV.ScrollToVerticalOffset(e.NewValue);
		}


		private void ScrollViewer_Scroll(object sender, ScrollEventArgs e)
		{
			//if(e. == SelectiveScrollingOrientation.Horizontal)
			//Tracks_SV.ScrollToVerticalOffset(e.NewValue);
			//TimelineScrubber_SV.ScrollToHorizontalOffset(e.NewValue);
		}

		private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			Tracks_SV.ScrollToVerticalOffset(e.VerticalOffset);
			TimelineScrubber_SV.ScrollToHorizontalOffset(e.HorizontalOffset);
		}

		void TimeLineControl_DragOver(object sender, DragEventArgs e)
		{
			
		}


	}
}
