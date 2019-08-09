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
using System.Windows.Threading;
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
		DispatcherTimer PlayerTimer = new DispatcherTimer(DispatcherPriority.Normal);
		List<Timeline> timelines = new List<Timeline>();
		double ScaleWidth = 1.0;

		static TimeBlockDragAdorner _dragAdorner;

		public int TimeWidth
		{
			get { return (int)GetValue(TimeWidthProperty); }
			set { SetValue(TimeWidthProperty, value); }
		}

		public static readonly DependencyProperty TimeWidthProperty =
			DependencyProperty.Register("TimeWidth", typeof(int), typeof(TimelinePlayer),
				new PropertyMetadata(40, new PropertyChangedCallback(OnTimeWidthPropertyChanged)));

		private static void OnTimeWidthPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			Console.WriteLine("TimeWidth Changed");
			TimelinePlayer tlp = (TimelinePlayer)sender;
			int position = tlp.TimeWidth; int count = 1;

			//create the timeline. keep going until the end is reached
			while (position < tlp.TimelineScrubber_SV.ActualWidth)
			{


				Line l = new Line() { X1 = position, X2 = position, Y1 = 0, Y2 = 20, Fill = Brushes.White, StrokeThickness = 2, Stroke = Brushes.White };
				tlp.TimelineScrubber_Canvas.Children.Add(l);


				TextBlock time = new TextBlock() { Foreground = Brushes.White, FontSize = 10, Text = count.ToString() };
				Canvas.SetLeft(time, position +2);
				tlp.TimelineScrubber_Canvas.Children.Add(time);

				position += tlp.TimeWidth; count++;
			}



			//TimelineScrubber_Canvas.Children.Add(l);

		}

		private void RedrawTimeScrubber(Canvas timescrubber, int timewidth)
		{
			timescrubber.Children.Clear();
			int position = timewidth; int count = 1;
			//create the timeline. keep going until the end is reached
			while (position < (timescrubber).ActualWidth)
			{
				Line l = new Line() { X1 = position, X2 = position, Y1 = 0, Y2 = 20, Fill = Brushes.White, StrokeThickness = 2, Stroke = Brushes.White };
				timescrubber.Children.Add(l);


				TextBlock time = new TextBlock() { Foreground = Brushes.White, FontSize = 10, Text = count.ToString() };
				Canvas.SetLeft(time, position + 2);
				timescrubber.Children.Add(time);

				position += timewidth; count++;
			}


		}


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

			Timeline timeline = new Timeline(TimeWidth)
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
			TimeBlock timeBlock = new TimeBlock(timeline, 0) { Trackname = "Memes", Width = 100, Margin = new Thickness(0, 0, 0, 3) };
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
			PlayerTimer.Tick += PlayTimer_Tick;
		}


		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			RedrawTimeScrubber(TimelineScrubber_Canvas, TimeWidth);
		}


		private void AddTrack_BTN_Click(object sender, RoutedEventArgs e)
		{
			Console.WriteLine("AddTrack");

			ContentControl bor = (ContentControl)this.Resources["TimelineBlock_CC"];
			Tracks_Grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(75) });
			Grid.SetRow(bor, Tracks_Grid.RowDefinitions.Count - 1);
			Tracks_Grid.Children.Add(bor);
			Timelines_Grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(75) });

			Timeline timeline = new Timeline(TimeWidth)
			{
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				Background = Brushes.Gray,
				Margin = new Thickness(0, 3, 0, 3)
			};
			Grid.SetRow(timeline, Tracks_Grid.RowDefinitions.Count - 1);
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

		
		private void TimeBlock_Resize_Left(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
		{

			TimeBlock tb = ((TimeBlock)((Thumb)sender).TemplatedParent);

			if (tb.Width - e.HorizontalChange > 5)
			{
				tb.Width -= e.HorizontalChange;
				Canvas.SetLeft(tb, VisualTreeHelper.GetOffset(tb).X + e.HorizontalChange);
				tb.StartTime = Canvas.GetLeft(tb) * tb.TimelineParent.TimePerPixel;
			}
		}
		private void TimeBlock_Resize_Right(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
		{

			TimeBlock tb = ((TimeBlock)((Thumb)sender).TemplatedParent);
			if(tb.Width + e.HorizontalChange > 5)
				tb.Width += e.HorizontalChange;
			tb.StartTime = Canvas.GetLeft(tb) * tb.TimelineParent.TimePerPixel;

			double left = Canvas.GetLeft(tb);
			if (left + tb.Width > tb.TimelineParent.ActualWidth)
			{ //expands the time line if the block goes off screen
				tb.TimelineParent.Width = left + tb.Width;
				TimelineScrubber_Canvas.Width = tb.TimelineParent.Width;
				RedrawTimeScrubber(TimelineScrubber_Canvas, TimeWidth);
			}
		}

		private void MoveThumb_Middle_DragDelta(object sender, DragDeltaEventArgs e)
		{
			TimeBlock tb = ((TimeBlock)((Thumb)sender).TemplatedParent);
			if (VisualTreeHelper.GetOffset(tb).X + e.HorizontalChange > 0) {
				if (CanMoveTimeBlock(timelines[Grid.GetRow(tb.TimelineParent)], tb, (int)(Canvas.GetLeft(tb) + e.HorizontalChange), (int)(Canvas.GetLeft(tb) + tb.ActualWidth + e.HorizontalChange)))
				{
					Canvas.SetLeft(tb, VisualTreeHelper.GetOffset(tb).X + e.HorizontalChange);
					tb.StartTime = Canvas.GetLeft(tb) * tb.TimelineParent.TimePerPixel;
					tb.BringIntoView();
				}
			}

			Console.WriteLine(Grid.GetRow(tb.TimelineParent));

			//don't let the users put timeblock in side eaachother

			//only move the timeblock when the mouse is not over another time block


			double left = Canvas.GetLeft(tb);
			if (left + tb.Width > tb.TimelineParent.ActualWidth)
			{ //expands the time line if the block goes off screen
				tb.TimelineParent.Width = left + tb.Width;
				TimelineScrubber_Canvas.Width = tb.TimelineParent.Width;
				RedrawTimeScrubber(TimelineScrubber_Canvas, TimeWidth);
			}

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

		private void SetSnapLocation(List<Timeline> timelines, TimeBlock desiredsnapper, int timeline)
		{

		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			ContentControl cc = (ContentControl)((Border)((Grid)((StackPanel)((Button)sender).Parent).Parent).Parent).Parent;
			Console.WriteLine(Grid.GetRow(cc));

			timelines[Grid.GetRow(cc)].AddTimeBlock(new TimeBlock(timelines[Grid.GetRow(cc)], 0) { Trackname = "Memes", Width = 100, Margin = new Thickness(0, 0, 0, 3) });
			
			
		}

		private void ScrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			Console.WriteLine("Mouse wheel over tracks");
			if (e.Delta < 0)
			{
				if (ScaleWidth - .2 >= 0.2)
				{
					TimelineScrubber_Canvas.RenderTransform = new ScaleTransform(ScaleWidth -= .2, 1.0);
					foreach(Timeline tline in timelines)
					{
						tline.RenderTransform = new ScaleTransform(ScaleWidth, 1.0);
					}
				}
			}
			else
			{
				TimelineScrubber_Canvas.RenderTransform = new ScaleTransform(ScaleWidth += .2, 1.0);
				foreach (Timeline tline in timelines)
				{
					tline.RenderTransform = new ScaleTransform(ScaleWidth, 1.0);
				}
			}
			RedrawTimeScrubber(TimelineScrubber_Canvas, TimeWidth);

		}

		private void PlayTimeline_BTN_Click(object sender, RoutedEventArgs e)
		{
			PlayerTimer.Interval = TimeSpan.FromMilliseconds(16);
			PlayerTimer.Start();
		}

		void PlayTimer_Tick(object sender, EventArgs e)
		{
			if (Timelines_Grid.ActualWidth -5 > PlayLine.X1) { 
				PlayLine.X1 += 1; PlayLine.X2 += 1; PlayLine.BringIntoView(new Rect(PlayLine.X1, PlayLine.Y1, 2, 100));
			}
			else PlayerTimer.Stop();
			Console.WriteLine("tick");
		}

		private void Timelines_Grid_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			PlayLine.Y2 = Tracks_Grid.ActualHeight;
			TempStartLine.Y2 = Tracks_Grid.ActualHeight;
		}

		private void PlayerStop_BTN_Click(object sender, RoutedEventArgs e)
		{
			PlayerTimer.Stop();
			PlayLine.X1 = TempStartLine.X1; PlayLine.X2 = TempStartLine.X2;
		}

		private void PlayerPause_BTN_Click(object sender, RoutedEventArgs e)
		{
			PlayerTimer.Stop();
		}

		private void ScrollViewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			Point Mpos = Mouse.GetPosition((ScrollViewer)sender);
			if (Mpos.X < Timelines_Grid.ActualWidth - 1)
				TempStartLine.X1 = Mpos.X; TempStartLine.X2 = Mpos.X + 2;
		}

		private void ScrollViewer_PreviewMouseMove(object sender, MouseEventArgs e)
		{
			if(e.LeftButton == MouseButtonState.Pressed)
			{
				Point Mpos = Mouse.GetPosition((ScrollViewer)sender);
				if(Mpos.X < Timelines_Grid.ActualWidth-1)
					TempStartLine.X1 = Mpos.X; TempStartLine.X2 = Mpos.X + 2;
			}
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
