using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
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

		List<Timeline> timelines = new List<Timeline>();

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
			Tracks_Grid.RowDefinitions.Add(new RowDefinition(){ Height=new GridLength(75)});
			Grid.SetRow(bor, Tracks_Grid.RowDefinitions.Count - 1);
			Tracks_Grid.Children.Add(bor);
			Timelines_Grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(75) });

			Timeline timeline = new Timeline()
			{
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				Background = Brushes.Gray,
				Margin = new Thickness(0, 3, 0, 3)
			};

			//Canvas c = new Canvas() { Background = Brushes.Gray, Margin=new Thickness(0,3,0,3)};
			//c.MouseEnter += C_MouseEnter;
			Grid.SetRow(timeline, Tracks_Grid.RowDefinitions.Count - 1);

			////add my custom time block
			TimeBlock timeBlock = new TimeBlock(timeline) { Trackname = "Memes", Width = 100, Margin = new Thickness(0, 0, 0, 3) };
			Canvas.SetLeft(timeBlock, 1);
			//timeline.Children.Add(timeBlock);
			Timelines_Grid.Children.Add(timeline);

			timelines.Add(timeline);

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
			//timelines.Add(new Timeline());


			ContentControl bor = (ContentControl)this.Resources["TimelineBlock_CC"];
			Tracks_Grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(75) });
			Grid.SetRow(bor, Tracks_Grid.RowDefinitions.Count - 1);
			Tracks_Grid.Children.Add(bor);
			Timelines_Grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(75) });

			Timeline timeline = new Timeline()
			{
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				Background = Brushes.Gray,
				Margin = new Thickness(0, 3, 0, 3)
			};

			//Canvas c = new Canvas() { Background = Brushes.Gray, Margin=new Thickness(0,3,0,3)};
			//c.MouseEnter += C_MouseEnter;
			Grid.SetRow(timeline, Tracks_Grid.RowDefinitions.Count - 1);

			////add my custom time block
			TimeBlock timeBlock = new TimeBlock(timeline) { Trackname = "Memes", Width = 100, Margin = new Thickness(0, 0, 0, 3) };
			Canvas.SetLeft(timeBlock, 1);
			//timeline.Children.Add(timeBlock);
			Timelines_Grid.Children.Add(timeline);

			timelines.Add(timeline);


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

		private void TimeBlock_Resize_Right(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
		{

			TimeBlock tb = ((TimeBlock)((Thumb)sender).TemplatedParent);
			if(tb.Width + e.HorizontalChange > 5)
				tb.Width += e.HorizontalChange;
		}

		private void TimeBlock_Resize_Left(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
		{

			TimeBlock tb = ((TimeBlock)((Thumb)sender).TemplatedParent);

			if (tb.Width - e.HorizontalChange > 5)
			{
				tb.Width -= e.HorizontalChange;
				Canvas.SetLeft(tb, VisualTreeHelper.GetOffset(tb).X + e.HorizontalChange);
			}
		}

		private void MoveThumb_Middle_DragDelta(object sender, DragDeltaEventArgs e)
		{
			TimeBlock tb = ((TimeBlock)((Thumb)sender).TemplatedParent);
			if (VisualTreeHelper.GetOffset(tb).X + e.HorizontalChange > 0) {
				if (CanMoveTimeBlock(timelines[Grid.GetRow(tb.TimelineParent)], tb,(int)(Canvas.GetLeft(tb) + e.HorizontalChange), (int)(Canvas.GetLeft(tb) + tb.ActualWidth + e.HorizontalChange)))
					Canvas.SetLeft(tb, VisualTreeHelper.GetOffset(tb).X + e.HorizontalChange);
			}

			Console.WriteLine(Grid.GetRow(tb.TimelineParent));

			//don't let the users put timeblock in side eaachother

			//only move the timeblock when the mouse is not over another time block

		}

		private bool CanMoveTimeBlock(Timeline tline, TimeBlock desiredmove, int left, int right)
		{
			bool retb = true;
			//there are multiple Timeblocks in a timeline
			foreach(TimeBlock TBlock in tline.timeBlocksLL)
			{
				if (desiredmove == TBlock) continue;
				int block_l = (int)Canvas.GetLeft(TBlock);
				int block_r = (int)(block_l + TBlock.Width);

				//if (left > block_l && right < block_r)
				//	retb &= false;

				if (right > block_l && right < block_r)
					retb &= false;
				if (left > block_l && left < block_r)
					retb &= false;
			}
			return retb;
		}

		private void SetSnapLocation(List<Timeline> timelines, TimeBlock desiredsnapper)
		{

		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			ContentControl cc = (ContentControl)((Border)((Grid)((StackPanel)((Button)sender).Parent).Parent).Parent).Parent;
			Console.WriteLine(Grid.GetRow(cc));

			timelines[Grid.GetRow(cc)].AddTimeBlock(new TimeBlock(timelines[Grid.GetRow(cc)]) { Trackname = "Memes", Width = 100, Margin = new Thickness(0, 0, 0, 3) });
			
			
		}
	}

	public class NumberedTickBar : TickBar
	{
		protected override void OnRender(DrawingContext dc)
		{

			Size size = new Size(base.ActualWidth, base.ActualHeight);
			int tickCount = (int)((this.Maximum - this.Minimum) / this.TickFrequency) + 1;
			if ((this.Maximum - this.Minimum) % this.TickFrequency == 0)
				tickCount -= 1;
			Double tickFrequencySize;
			// Calculate tick's setting
			tickFrequencySize = (size.Width * this.TickFrequency / (this.Maximum - this.Minimum));
			string text = "";
			FormattedText formattedText = null;
			double num = this.Maximum - this.Minimum;
			int i = 0;
			// Draw each tick text
			for (i = 0; i <= tickCount; i++)
			{
				text = Convert.ToString(Convert.ToInt32(this.Minimum + this.TickFrequency * i), 10);

				formattedText = new FormattedText(text, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdana"), 8, Brushes.Black);
				dc.DrawText(formattedText, new Point((tickFrequencySize * i), 30));

			}
		}
	}


}
