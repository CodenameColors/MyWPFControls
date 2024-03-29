﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
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
			_adorningContentPresenter.Opacity = 0.5;
			_layer = AdornerLayer.GetAdornerLayer(uiElement);

			_layer.Add(this);
			IsHitTestVisible = false;
		}
	}


	/// <summary>
	/// Interaction logic for UserControl1.xaml
	/// </summary>
	public partial class TimelinePlayer : UserControl, INotifyPropertyChanged
  {
	  public event PropertyChangedEventHandler PropertyChanged;

	  public void OnPropertyChanged(string name)
	  {
		  PropertyChangedEventHandler handler = PropertyChanged;
		  if (handler != null)
		  {
			  handler(this, new PropertyChangedEventArgs(name));
		  }
	  }


		#region delegates
		public delegate void SyncFunction_Hook();
		public SyncFunction_Hook TimeBlockSync = null;

		public delegate void OnSlectionChange_Hook(object Selected);
		public OnSlectionChange_Hook SelectionChanged_Hook;

		public delegate Object OnCreateChoiceTimeblock_Hook(TimeBlock tbref);
		/// <summary>
		/// when creating a timeblock for my application i need to have a dialogue choice blocks
		/// for linking purposes. so this hooking delegate will allow the user to give us one.
		/// </summary>
		/// <param name="Selected">ALWAYS put </param>
		/// <returns>Dialogue block</returns>
		public OnCreateTimeblock_Hook OnCreateChoiceTimeblockHook;

		public delegate Object OnCreateTimeblock_Hook(TimeBlock tbref, bool bchoices);
		/// <summary>
		/// when creating a timeblock for my application i need to have a dialogue
		/// for linking purposes. so this hooking delegate will allow the user to give us one.
		/// </summary>
		/// <param name="Selected">ALWAYS put </param>
		/// <returns>Dialogue block</returns>
		public OnCreateTimeblock_Hook OnCreateTimeblockHook;

		public delegate void OnReset_Hook();
		public OnReset_Hook Reset_Hook;

		public delegate void OnUIPositionChange(HorizontalAlignment hori, VerticalAlignment vert, int timelineInd);
		public OnUIPositionChange ChangeUIPosition_Hook;
		#endregion

		double TimeScrubber_BaseSize = 0.0;
		public ObservableCollection<TimeBlock> ActiveTBblocks = new ObservableCollection<TimeBlock>();
		DispatcherTimer PlayerTimer = new DispatcherTimer(DispatcherPriority.Normal);
		List<Timeline> timelines = new List<Timeline>();
		double ScaleWidth = 1.0;

		private TimeBlock SelectedTB = null;
		public Timeline selectedTimeline = null;
		private Point pointToAdd = new Point();

		//public object SelectedControl = new object();


		private Stopwatch tStopwatch = null;
		private float _elapsedTime = 0.0f;
		public float ElapsedTime
		{
			get => _elapsedTime;
			set
			{
				_elapsedTime = value;
				OnPropertyChanged("ElapsedTime");
			}
		}

		public object SelectedControl
		{
			get { return selectedcontrol; }
			set
			{
				selectedcontrol = value;
				OnSelectedControl_Changed(value);
			}
		}
		private object selectedcontrol = null;


		public void OnSelectedControl_Changed(object sender)
		{
			if (SelectionChanged_Hook != null)
				SelectionChanged_Hook(sender);
		}

		private void MoveThumb_Middle_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (sender is TimeBlock timeBlock)
			{
				SelectedControl = timeBlock;
			}
		}

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
			TimelineScrubber_Canvas.Width = (TimeScrubber_BaseSize * ScaleWidth);
			//create the timeline. keep going until the end is reached
			while (position < (timescrubber).Width)
			{
				Line l = new Line() { X1 = position, X2 = position, Y1 = 0, Y2 = 20, Fill = Brushes.White, StrokeThickness = 2, Stroke = Brushes.White };
				timescrubber.Children.Add(l);


				TextBlock time = new TextBlock() { Foreground = Brushes.White, FontSize = 10, Text = count.ToString() };
				Canvas.SetLeft(time, position + 2);
				timescrubber.Children.Add(time);

				position += timewidth; count++;
			}

			foreach (Timeline tline in timelines)
			{
				tline.Width = (timescrubber).Width;
				tline.TimePerPixel = 1.0 / timewidth;
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
				((ObservableCollection<Timeline>)oldValue).CollectionChanged -= new NotifyCollectionChangedEventHandler(newValueINotifyCollectionChanged_CollectionChanged);
			}
			// Add handler for newValue.CollectionChanged (if possible)
			if (null != newValue)
			{
				((ObservableCollection<Timeline>)newValue).CollectionChanged += new NotifyCollectionChangedEventHandler(newValueINotifyCollectionChanged_CollectionChanged);
			}
		}

		/// <summary>
		/// A time block has been added to the be timeline linked list. So display it.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void newValueINotifyCollectionChanged_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{

			if (e.Action == NotifyCollectionChangedAction.Add)
			{
				foreach (Timeline tl in e.NewItems)
				{
					ContentControl bor = (ContentControl)this.Resources["TimelineBlock_CC"];
					Tracks_Grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(75) });
					Grid.SetRow(bor, Tracks_Grid.RowDefinitions.Count - 1);
					Tracks_Grid.Children.Add(bor);
					Timelines_Grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(75) });

					tl.HorizontalAlignment = HorizontalAlignment.Stretch;
					tl.VerticalAlignment = VerticalAlignment.Stretch;
					tl.Background = Brushes.Gray;
					tl.Margin = new Thickness(0, 3, 0, 3);
					tl.MouseRightButtonDown += ShowAddTimeBlockCM;

					//Canvas c = new Canvas() { Background = Brushes.Gray, Margin=new Thickness(0,3,0,3)};
					//c.MouseEnter += C_MouseEnter;
					Grid.SetRow(tl, Tracks_Grid.RowDefinitions.Count - 1);

					Timelines_Grid.Children.Add(tl);
					timelines.Add(tl);

				}
			}

			//ContentControl bor = (ContentControl)this.Resources["TimelineBlock_CC"];
			//Tracks_Grid.RowDefinitions.Add(new RowDefinition(){ Height=new GridLength(75)});
			//Grid.SetRow(bor, Tracks_Grid.RowDefinitions.Count - 1);
			//Tracks_Grid.Children.Add(bor);
			//Timelines_Grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(75) });

			//Timeline timeline = new Timeline(TimeWidth, TimelineScrubber_Canvas.ActualWidth)
			//{
			//	HorizontalAlignment = HorizontalAlignment.Stretch,
			//	VerticalAlignment = VerticalAlignment.Stretch,
			//	Background = Brushes.Gray,
			//	Margin = new Thickness(0, 3, 0, 3),
			//	TrackName = "Emma"
			//};
			//timeline.MouseRightButtonDown += ShowAddTimeBlockCM;

			////Canvas c = new Canvas() { Background = Brushes.Gray, Margin=new Thickness(0,3,0,3)};
			////c.MouseEnter += C_MouseEnter;
			//Grid.SetRow(timeline, Tracks_Grid.RowDefinitions.Count - 1);

			//////add my custom time block
			//TimeBlock timeBlock = new TimeBlock(timeline, 0) { Trackname = "Memes", Width = 100, Margin = new Thickness(0, 0, 0, 3) };
			//Canvas.SetLeft(timeBlock, 1);
			////timeline.Children.Add(timeBlock);
			//Timelines_Grid.Children.Add(timeline);

			//timelines.Add(timeline);

		}

		private void ShowAddTimeBlockCM_CC(object sender, MouseButtonEventArgs e)
		{
			SelectedTB = (sender as ContentPresenter).TemplatedParent as TimeBlock;
			ContextMenu cm = this.FindResource("DeleteTimeblock_CM") as ContextMenu;
			cm.PlacementTarget = sender as ContentControl;
			cm.IsOpen = true;
		}

		private void ShowAddTimeBlockCM(object sender, MouseButtonEventArgs e)
		{
			selectedTimeline = (sender as Timeline);
			pointToAdd = Mouse.GetPosition(Timelines_Grid);
			ContextMenu cm = this.FindResource("AddTimeblock_CM") as ContextMenu;
			//((MenuItem)cm.Items[0]).IsChecked = ((MenuItem)cm.Items[0]).IsChecked;
			cm.PlacementTarget = sender as ContentControl;
			//SelectedCC = sender as ContentControl;
			cm.IsOpen = true;
		}

		private void C_MouseEnter(object sender, MouseEventArgs e)
		{
			_dragAdorner = new TimeBlockDragAdorner((Canvas)sender);
		}

		public TimelinePlayer()
    {
			InitializeComponent();

			this.DataContext = this;

			SelectedControl = new object();
			Console.WriteLine("init");
			PlayerTimer.Tick += PlayTimer_Tick;
			KeyUp += TimelinePlayer_KeyUp;
			ActiveTBblocks.CollectionChanged += ActiveTBblocks_CollectionChanged;
		}



		private void TimelinePlayer_KeyUp(object sender, KeyEventArgs e)
		{
			if(e.Key == Key.LeftCtrl)
			{
				SnapLine.Visibility = Visibility.Hidden; 
			}
		}

		private void Scrubber_Loaded(object sender, RoutedEventArgs e)
		{
			TimelineScrubber_Canvas.Width = TimelineScrubber_SV.ActualWidth;
			TimeScrubber_BaseSize = TimelineScrubber_Canvas.Width;
			RedrawTimeScrubber(TimelineScrubber_Canvas, TimeWidth);
		}


		public void AddTimeline(Timeline tl )
		{
			ContentControl bor = (ContentControl)this.Resources["TimelineBlock_CC"];
			((TextBlock)((StackPanel)((Grid)((Border)(bor.Content)).Child).Children[1]).Children[1]).Text = tl.TrackName;
			if(tl.TrackImagePath != null)
				((Image)((StackPanel)((Grid)((Border)(bor.Content)).Child).Children[1]).Children[0]).Source = 
					new BitmapImage(new Uri(tl.TrackImagePath));


			Tracks_Grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(75) });
			Grid.SetRow(bor, Tracks_Grid.RowDefinitions.Count - 1);
			Tracks_Grid.Children.Add(bor);
			Timelines_Grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(75) });

			tl.TimePerPixel = 1.0 / (double)TimeWidth;
			tl.Width = TimelineScrubber_Canvas.ActualWidth;
			tl.HorizontalAlignment = HorizontalAlignment.Stretch;
			tl.VerticalAlignment = VerticalAlignment.Stretch;
			tl.Background = Brushes.Gray;
			tl.Margin = new Thickness(0, 3, 0, 3);
			tl.MouseRightButtonDown += ShowAddTimeBlockCM;

			//Canvas c = new Canvas() { Background = Brushes.Gray, Margin=new Thickness(0,3,0,3)};
			//c.MouseEnter += C_MouseEnter;
			Grid.SetRow(tl, Tracks_Grid.RowDefinitions.Count - 1);

			Timelines_Grid.Children.Add(tl);
			timelines.Add(tl);
		}

		public void AddTimeline(String TrackName = "")
		{
			Tracks_Grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(75) });
			Timelines_Grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(75) });

			Timeline timeline = new Timeline(TimeWidth, TimelineScrubber_Canvas.ActualWidth)
			{
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Stretch,
				Background = Brushes.Gray,
				Margin = new Thickness(0, 3, 0, 3),
				TrackName = TrackName
			};
			timeline.MouseRightButtonDown += ShowAddTimeBlockCM;
			Grid.SetRow(timeline, Tracks_Grid.RowDefinitions.Count - 1);
			Timelines_Grid.Children.Add(timeline);

			ContentControl bor = (ContentControl)this.Resources["TimelineBlock_CC"];
			//((TextBlock)bor.("Timeline_Text")).Text = timeline.TrackName;
			((TextBlock)((StackPanel)((Grid)((Border)(bor.Content)).Child).Children[1]).Children[1]).Text = timeline.TrackName;

			Grid.SetRow(bor, Tracks_Grid.RowDefinitions.Count - 1);
			Tracks_Grid.Children.Add(bor);
			timeline.MouseDown += Timeline_Click;

			timelines.Add(timeline);
		}

		public void Timeline_Click(object sender, RoutedEventArgs e)
		{
			SelectedControl = (Timeline)sender;
		}

		private void AddTrack_BTN_Click(object sender, RoutedEventArgs e)
		{
			AddTimeline();
		}

		private void AddTimeblock(Timeline Destimeline, double LeftPosition)
		{
			Tracks_Grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(75) });
			Timelines_Grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(75) });

			Timeline timeline = new Timeline(TimeWidth, TimelineScrubber_Canvas.ActualWidth)
			{
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Stretch,
				Background = Brushes.Gray,
				Margin = new Thickness(0, 3, 0, 3),
				TrackName = "Emma"
			};
			Grid.SetRow(timeline, Tracks_Grid.RowDefinitions.Count - 1);
			Timelines_Grid.Children.Add(timeline);

			ContentControl bor = (ContentControl)this.Resources["TimelineBlock_CC"];
			//((TextBlock)bor.("Timeline_Text")).Text = timeline.TrackName;
			((TextBlock)((StackPanel)((Grid)((Border)(bor.Content)).Child).Children[1]).Children[1]).Text = timeline.TrackName;

			Grid.SetRow(bor, Tracks_Grid.RowDefinitions.Count - 1);
			Tracks_Grid.Children.Add(bor);


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

			//snapping
			if (Keyboard.IsKeyDown(Key.LeftCtrl))
			{
				TimeBlock snap_tb = null;
				foreach (Timeline tline in timelines)
					if ((snap_tb = GetSnapStatus(tline, tb)) != null) break;
				if (snap_tb != null)
				{
					SetSnap(snap_tb, tb, false);
				}
			}
			tb.StartTime = Canvas.GetLeft(tb) * tb.TimelineParent.TimePerPixel;
			SelectedControl = tb;
		}
		private void TimeBlock_Resize_Right(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
		{

			TimeBlock tb = ((TimeBlock)((Thumb)sender).TemplatedParent);
			if (tb.Width + e.HorizontalChange > 5)
				tb.Width += e.HorizontalChange;
			

			double left = Canvas.GetLeft(tb);
			if (left + tb.Width > tb.TimelineParent.ActualWidth)
			{ //expands the time line if the block goes off screen
				tb.Width = left + tb.Width;
				foreach (Timeline tline in timelines)
					tline.Width = tb.Width + e.HorizontalChange;
				TimeScrubber_BaseSize = tb.TimelineParent.Width / ScaleWidth;
				TimelineScrubber_Canvas.Width = tb.TimelineParent.Width;
				RedrawTimeScrubber(TimelineScrubber_Canvas, TimeWidth);
			}

			//snapping
			if (Keyboard.IsKeyDown(Key.LeftCtrl))
			{
				TimeBlock snap_tb = null;
				foreach (Timeline tline in timelines)
					if ((snap_tb = GetSnapStatus(tline, tb)) != null) break;
				if (snap_tb != null && Math.Abs(Canvas.GetLeft(snap_tb) - Mouse.GetPosition(Timelines_Grid).X) <= 5)
				{
					SetSnap(snap_tb, tb, false);
				}
			}
			tb.EndTime = (Canvas.GetLeft(tb) + tb.ActualWidth) * tb.TimelineParent.TimePerPixel;
			SelectedControl = tb;
		}

		private void MoveThumb_Middle_DragDelta(object sender, DragDeltaEventArgs e)
		{
			if (((TimeBlock)((Thumb)sender).TemplatedParent) is TimeBlock tb)
			{
				if (VisualTreeHelper.GetOffset(tb).X + e.HorizontalChange > 0)
				{
					if (CanMoveTimeBlock(timelines[Grid.GetRow(tb.TimelineParent)], tb, (int)(Canvas.GetLeft(tb) + e.HorizontalChange), (int)(Canvas.GetLeft(tb) + tb.ActualWidth + e.HorizontalChange)))
					{
						Canvas.SetLeft(tb, VisualTreeHelper.GetOffset(tb).X + e.HorizontalChange);
						tb.StartTime = Canvas.GetLeft(tb) * tb.TimelineParent.TimePerPixel;
						tb.BringIntoView();
					}
				}


				double left = Canvas.GetLeft(tb);
				if (left + tb.Width > tb.TimelineParent.Width)
				{ //expands the time line if the block goes off screen
					foreach (Timeline tline in timelines)
						tline.Width = left + tb.Width;
					TimeScrubber_BaseSize = tb.TimelineParent.Width / ScaleWidth;
					TimelineScrubber_Canvas.Width = tb.TimelineParent.Width;
					RedrawTimeScrubber(TimelineScrubber_Canvas, TimeWidth);
				}
				//snapping
				if (Keyboard.IsKeyDown(Key.LeftCtrl))
				{
					TimeBlock snap_tb = null;
					foreach (Timeline tline in timelines)
						if ((snap_tb = GetSnapStatus(tline, tb)) != null) break;
					if (snap_tb != null)
					{
						SetSnap(snap_tb, tb);
					}
				}
				tb.StartTime = Canvas.GetLeft(tb) * tb.TimelineParent.TimePerPixel;
				SelectedControl = tb;
			}
		}

		private void SetSnap(TimeBlock ToSnapto_TB, TimeBlock Snap_TB, bool bMove = true)
		{
			SnapLine.Visibility = Visibility.Visible;
			double left = Canvas.GetLeft(Snap_TB);
			double right = left + Snap_TB.ActualWidth;

			//check to see if the left is within range
			if ((Math.Abs(Canvas.GetLeft(ToSnapto_TB) + ToSnapto_TB.ActualWidth - left)) <= 5 && bMove)
			{
				Canvas.SetLeft(Snap_TB, (Canvas.GetLeft(ToSnapto_TB) + ToSnapto_TB.ActualWidth));
				SnapLine.X1 = (Canvas.GetLeft(Snap_TB));
				SnapLine.X2 = (Canvas.GetLeft(Snap_TB));

			}
			else if ((Math.Abs(Canvas.GetLeft(ToSnapto_TB) + ToSnapto_TB.ActualWidth - left)) <= 5 && !bMove)
			{
				Canvas.SetLeft(Snap_TB, (Canvas.GetLeft(ToSnapto_TB) + ToSnapto_TB.ActualWidth));
				Snap_TB.Width += -1 * (Canvas.GetLeft(ToSnapto_TB) + ToSnapto_TB.ActualWidth - left);
				SnapLine.X1 =  (Canvas.GetLeft(Snap_TB));
				SnapLine.X2 =  (Canvas.GetLeft(Snap_TB));
			}

			//check to see if the right is within range
			else if ((Math.Abs(right - Canvas.GetLeft(ToSnapto_TB)) <= 5) && bMove)
			{
				Canvas.SetLeft(Snap_TB, Canvas.GetLeft(ToSnapto_TB) - Snap_TB.ActualWidth);
				SnapLine.X1 = (Canvas.GetLeft(Snap_TB) + Snap_TB.ActualWidth);
				SnapLine.X2 = (Canvas.GetLeft(Snap_TB) + Snap_TB.ActualWidth);
			}
			else if ((Math.Abs(right - Canvas.GetLeft(ToSnapto_TB)) <= 5) && !bMove)
			{
				Snap_TB.Width = Canvas.GetLeft(ToSnapto_TB) - Canvas.GetLeft(Snap_TB); //+= -1 * (right - Canvas.GetLeft(ToSnapto_TB));
				SnapLine.X1 = (Canvas.GetLeft(ToSnapto_TB));
				SnapLine.X2 = (Canvas.GetLeft(ToSnapto_TB));
			}
			//return block;

		}

		private TimeBlock GetSnapStatus(Timeline timeline, TimeBlock tblock)
		{
			foreach (TimeBlock block in timeline.timeBlocksLL)
			{

				if (block == tblock) continue;
				double left = Canvas.GetLeft(tblock);
				double right = left + tblock.ActualWidth;

				//check to see if the left is within range //left on right
				if ((Math.Abs(Canvas.GetLeft(block) + block.ActualWidth -left)) <= 5)
					return block;

				//left on left
				//if ((Math.Abs(Canvas.GetLeft(block) - left)) <= 5)
				//	return block;

				//check to see if the right is within range //right on left
				if ((Math.Abs(right - Canvas.GetLeft(block)) <= 5))
					return block;

				////right on right
				//if ((Math.Abs(right - Canvas.GetLeft(block) + tblock.ActualWidth) <= 5))
				//	return block;



			}
			SnapLine.Visibility = Visibility.Hidden;
			return null;
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

		/// <summary>
		/// Add a time New timeblock to the timeline
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			ContentControl cc = (ContentControl)((Border)((Grid)((StackPanel)((Button)sender).Parent).Parent).Parent).Parent;
			Console.WriteLine(Grid.GetRow(cc));

			TimeBlock timeBlock = new TimeBlock(timelines[Grid.GetRow(cc)], 2)
			{
				Trackname = "Memes",
				Width = 100,
				Margin = new Thickness(0, 0, 0, 3),
				EndTime = 5,
				StartTime = 2,
			};


			timelines[Grid.GetRow(cc)].AddTimeBlock(timeBlock, TimeWidth);
		}

		private void ScrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (e.Delta < 0)
			{
				if (ScaleWidth - .2 >= 0.2)
				{
					ScaleWidth -= .2;
					//TimelineScrubber_Canvas.RenderTransform = new ScaleTransform(ScaleWidth -= .2, 1.0);
					foreach (Timeline tline in timelines)
					{
						//tline.RenderTransform = new ScaleTransform(ScaleWidth, 1.0);
						
						tline.SetRenderTransform(TimelineScrubber_Canvas.ActualWidth, ScaleWidth, TimeWidth);
					}
				}
			}
			else
			{
				ScaleWidth += .2;
				//TimelineScrubber_Canvas.RenderTransform = new ScaleTransform(ScaleWidth += .2, 1.0);
				foreach (Timeline tline in timelines)
				{
					//tline.RenderTransform = new ScaleTransform(ScaleWidth, 1.0);
					tline.SetRenderTransform(TimelineScrubber_Canvas.ActualWidth, ScaleWidth, TimeWidth);
				}
			}
			TimeWidth = (int)(60 * ScaleWidth);
			RedrawTimeScrubber(TimelineScrubber_Canvas, (int)(TimeWidth));
			//PlayerTimer.Interval = TimeSpan.FromMilliseconds(1.0/TimeWidth);
			//PlayLine.X1 *= ScaleWidth; PlayLine.X2 *= ScaleWidth;
		}

		void PlayTimer_Tick(object sender, EventArgs e)
		{
			if (Timelines_Grid.ActualWidth - 5 > PlayLine.X1)
			{
				PlayLine.X1 += 1; PlayLine.X2 += 1; PlayLine.BringIntoView(new Rect(PlayLine.X1, PlayLine.Y1, 2, 100));
				ElapsedTime = tStopwatch.ElapsedMilliseconds / 1000.0f;
			}
			else { PlayerTimer.Stop(); ActiveTBblocks.Clear(); }
			
			//scan EVERY timeline for active blocks. 
			foreach (Timeline tline in timelines)
			{
				if (tline.ActiveBlock == null)
					continue;

				double time = (PlayLine.X1) * (1.0 / TimeWidth);
				if (time >= tline.ActiveBlock.Value.StartTime && time <= tline.ActiveBlock.Value.EndTime && !ActiveTBblocks.Contains(tline.ActiveBlock.Value))	//add
				{
					ActiveTBblocks.Add(tline.ActiveBlock.Value); tline.SetActiveBlock(time);
				}
				else if (time > tline.ActiveBlock.Value.EndTime && ActiveTBblocks.Contains(tline.ActiveBlock.Value))//remove
				{
					ActiveTBblocks.Remove(tline.ActiveBlock.Value); tline.SetActiveBlock(time);
				}

			}
		}

		private void ActiveTBblocks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{

		}


		private void Timelines_Grid_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			PlayLine.Y2 = Tracks_Grid.ActualHeight;
			TempStartLine.Y2 = Tracks_Grid.ActualHeight;
			SnapLine.Y2 = Tracks_Grid.ActualHeight;
		}

		public void ResetTimeline()
		{
			Reset_Hook?.Invoke();
			PlayerTimer.Stop();
			ActiveTBblocks.Clear();
			PlayLine.X1 = 0; PlayLine.X2 = 0;
		}

		private void PlayerReset_BTN_Click(object sender, RoutedEventArgs e)
		{
			tStopwatch.Reset();
			ResetTimeline();
		}

		public void StopTimeline()
		{
			PlayerTimer.Stop();
			ActiveTBblocks.Clear();
			PlayLine.X1 = TempStartLine.X1; PlayLine.X2 = TempStartLine.X2;
		}

		private void PlayerStop_BTN_Click(object sender, RoutedEventArgs e)
		{
			tStopwatch.Reset();
			StopTimeline();
		}

		public void PauseTimeline()
		{
			PlayerTimer.Stop();
		}

		private void PlayerPause_BTN_Click(object sender, RoutedEventArgs e)
		{
			tStopwatch.Stop();
			PauseTimeline();
		}

		public void ResumeTimeline()
		{
			PlayerTimer.Start();

			if (TimeBlockSync != null)
			{
				TimeBlockSync();
			}
		}

		public void PlayTimeline()
		{
			ActiveTBblocks.Clear();
			PlayerTimer.Interval = TimeSpan.FromMilliseconds((1.0 / TimeWidth) * 1000);
			PlayerTimer.Start();

			if (TimeBlockSync != null)
			{
				TimeBlockSync();
			}

			foreach (Timeline tline in timelines)
				tline.InitActiveBlock(PlayLine.X1 * (1.0 / TimeWidth / ScaleWidth));
		}

		private void PlayTimeline_BTN_Click(object sender, RoutedEventArgs e)
		{
			//Set up the stop watch
			if(tStopwatch == null) tStopwatch = Stopwatch.StartNew();
			tStopwatch.Start();
			ElapsedTime = tStopwatch.ElapsedMilliseconds / 1000.0f;

			PlayTimeline();
		}

		private void ScrollViewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			//Point Mpos = Mouse.GetPosition((ScrollViewer)sender);
			//if (Mpos.X < Timelines_Grid.ActualWidth - 1)
			//	TempStartLine.X1 = Mpos.X; TempStartLine.X2 = Mpos.X + 2;
		}

		private void ScrollViewer_PreviewMouseMove(object sender, MouseEventArgs e)
		{
			//	if(e.LeftButton == MouseButtonState.Pressed)
			//	{
			//		Point Mpos = Mouse.GetPosition((ScrollViewer)sender);
			//		if(Mpos.X < Timelines_Grid.ActualWidth-1)
			//			TempStartLine.X1 = Mpos.X; TempStartLine.X2 = Mpos.X + 2;
			//	}
		}

		private void TimelineScrubber_SV_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			Point Mpos = Mouse.GetPosition((ScrollViewer)sender);
			if (Mpos.X < Timelines_Grid.ActualWidth - 1)
				TempStartLine.X1 = Mpos.X; TempStartLine.X2 = Mpos.X;
		}

		private void TimelineScrubber_SV_PreviewMouseMove(object sender, MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed)
			{
				Point Mpos = Mouse.GetPosition((ScrollViewer)sender);
				if (Mpos.X < Timelines_Grid.ActualWidth - 1)
					TempStartLine.X1 = Mpos.X; TempStartLine.X2 = Mpos.X;
			}
		}

		public List<Timeline> GetTimelines()
		{
			return timelines;
		}

		public int GetTimelinePosition(Timeline tl = null, TimeBlock tb = null)
		{
			if (tl != null)
			{
				return timelines.IndexOf(tl);
			}
			else if(tb != null)
			{
				return timelines.IndexOf(tb.TimelineParent);
			}
			return -1;
		}

		/// <summary>
		/// This method is here to add and position existing time blocks.
		/// Assume this method is called by the engine, and links to the 
		/// NodeEditor.DialogueBlockNode.LinkedTimeblock  
		/// </summary>
		/// <param name="timeBlock"></param>
		public void AddExistingTimeblockToTimeline(TimeBlock timeBlock, int timelineIndex)
		{
			timelines[timelineIndex].AddTimeBlock(timeBlock,TimeWidth);
		}

		/// <summary>
		/// This method will called when the user requests to add a time block to the timeline.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void AddDialogueBlockToTimeline(object sender, RoutedEventArgs e)
		{
			//ContentControl cc = (ContentControl)((Border)((Grid)((StackPanel)((Button)sender).Parent).Parent).Parent).Parent;
			//Console.WriteLine(Grid.GetRow(cc));
			if (OnCreateTimeblockHook != null)
			{
				TimeBlock tbb = new TimeBlock(timelines[Grid.GetRow(selectedTimeline)], 0)
				{
					Trackname = "Memes",
					Width = 100,
					Margin = new Thickness(0, 0, 0, 3),
					StartTime = pointToAdd.X/TimeWidth,
					//EndTime = 2,
				};
				object dialogueblock = OnCreateTimeblockHook(tbb, false);
				if (dialogueblock != null) tbb.LinkedDialogueBlock = dialogueblock;
				else return;
				timelines[Grid.GetRow(selectedTimeline)].AddTimeBlock(tbb, TimeWidth);
			}
		}

		private void AddChoiceDialogueBlockToTimeline(object sender, RoutedEventArgs e)
		{
			if (OnCreateTimeblockHook != null)
			{
				ChoiceTimeBlock tbb = new ChoiceTimeBlock(timelines[Grid.GetRow(selectedTimeline)], 0)
				{
					Trackname = "Memes",
					Width = 100,
					Margin = new Thickness(0, 0, 0, 3),
					StartTime = pointToAdd.X / TimeWidth
				};
				object dialogueblock = OnCreateTimeblockHook(tbb, true);
				if (dialogueblock != null) tbb.LinkedDialogueBlock = dialogueblock;
				else return;
				timelines[Grid.GetRow(selectedTimeline)].AddTimeBlock(tbb, TimeWidth);
			}
		}

		private void Test_BTN_Click(object sender, RoutedEventArgs e)
		{
			timelines[0].timeBlocksLL.First.Value.Duration = 3;
		}

		/// <summary>
		/// Delete one timeblock. used on right click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void DeleteTimeBlock(object sender, RoutedEventArgs e)
		{
			timelines[GetTimelinePosition(null, SelectedTB)].timeBlocksLL?.Remove(SelectedTB);
			timelines[GetTimelinePosition(null, SelectedTB)]?.Children.Remove(SelectedTB);
		}

		/// <summary>
		/// Clear all the timelines
		/// </summary>
		public void DeleteAllTimeBlocks()
		{
			foreach (Timeline tl in timelines)
			{
				int i = 0;
				while (tl.Children.Count > 0)
				{
					if (tl.timeBlocksLL.First.Value is TimeBlock timeBlock)
					{
						tl.timeBlocksLL.Remove(timeBlock);
						tl.Children.Remove(timeBlock);
						timeBlock.UpdateLayout();
					}
				}
				//for (int i = 0; i < tl.Children.Count; i++)
				//{
				//	if (tl.Children[i] is TimeBlock timeBlock)
				//	{
				//		tl.timeBlocksLL.Remove(timeBlock);
				//		tl.Children.Remove(timeBlock);
				//		timeBlock.UpdateLayout();
				//	}
				//}
				//tl.timeBlocksLL.Clear();
				tl.ClearLLTimeBlocks();
				tl.UpdateLayout();
			}
		}

		/// <summary>
		/// Delete all the time blocks in each timeline AFTER the current active time block
		/// </summary>
		public void DeleteTimeBlocksAfter()
		{
			foreach (Timeline tl in timelines)
			{
				if(tl.ActiveBlock is null)
				{
					//if (tl.timeBlocksLL.First != null)
					//	tl.ActiveBlock = tl.timeBlocksLL.First;
					Console.WriteLine("No ActiveBlock in this timeline: {0}", tl.TrackName);
					continue;
				}
				while (tl.ActiveBlock.List != null)
				{
					tl.Children.Remove(tl.timeBlocksLL.Last());
					tl.timeBlocksLL.RemoveLast();
				}
				tl.ActiveBlock = tl.timeBlocksLL.Last;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="newLL">This LL is a mixed bag. Meaning all different character/timeline timeblock are contained in here.</param>
		public void AddTimeblock_LL(LinkedList<TimeBlock> newLL, List<String> CharacterNames, bool resetptr = true)
		{
			foreach (TimeBlock timeblock in newLL)
			{
				int? i = Array.FindIndex(CharacterNames.ToArray(), (x => x == timeblock.Trackname)); //get index
				if (i == -1 && timeblock.Trackname.ToLower() == "choice") i = 0;
				if (i == -1)
				{
					Console.WriteLine("Timeblock doesn't belong");
					continue;
				} //this shouldn't happen...

				timelines[(int) i].AddTimeBlock(timeblock, TimeWidth, resetptr);
				timeblock.TimelineParent = timelines[(int)i];
				if (timelines[(int) i].ActiveBlock == null)
					timelines[(int)i].TimelineisNull_flag = true;
				//	timelines[(int)i].ActiveBlock = timelines[(int) i ].timeBlocksLL.First;
			}

			if (resetptr)
			{
				foreach (Timeline tl in timelines)
				{
					try
					{
						tl.ActiveBlock = tl.ActiveBlock.Next;
					}
					catch (Exception e)
					{
						System.Console.WriteLine("No Blocks Found In this timeline");
					}
					
				}
			}
		}

		

		private void ChangeGameUIAnchorPos_BTN_Click(object sender, RoutedEventArgs e)
		{
			if (ChangeUIPosition_Hook == null) return;
			Button b = sender as Button;
			int rowind = Grid.GetRow(((((sender as Button).Parent as Grid).Parent as Grid).Parent as Border).Parent as ContentControl);
			if (b.Name.ToLower().Contains("topleft"))
			{
				ChangeUIPosition_Hook(HorizontalAlignment.Left, VerticalAlignment.Top, rowind);
			}
			else if (b.Name.ToLower().Contains("topright")) 
			{
				ChangeUIPosition_Hook(HorizontalAlignment.Right, VerticalAlignment.Top, rowind);
			}
			else if (b.Name.ToLower().Contains("bottomleft")) 
			{
				ChangeUIPosition_Hook(HorizontalAlignment.Left, VerticalAlignment.Bottom, rowind);
			}
			else if (b.Name.ToLower().Contains("bottomright")) 
			{
				ChangeUIPosition_Hook(HorizontalAlignment.Right, VerticalAlignment.Bottom, rowind);
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
