using NodeEditor.Components;
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

namespace NodeEditor
{
  /// <summary>
  /// Interaction logic for UserControl1.xaml
  /// </summary>
  public partial class NodeEditor : UserControl
  {

		#region ItemSource
		public IEnumerable ItemsSource
		{
			get { return (IEnumerable)GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
		}

		public static readonly DependencyProperty ItemsSourceProperty =
				DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(NodeEditor), new PropertyMetadata(new PropertyChangedCallback(OnItemsSourcePropertyChanged)));

		private static void OnItemsSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			var control = sender as NodeEditor;
			if (control != null)
				control.OnItemsSourceChanged((IEnumerable)e.OldValue, (IEnumerable)e.NewValue);
		}

		private void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
		{
			// Remove handler for oldValue.CollectionChanged
			if (oldValue != null)
			{
				((ObservableCollection<BaseNode>)oldValue).CollectionChanged -= new NotifyCollectionChangedEventHandler(newValueINotifyCollectionChanged_CollectionChanged);
			}
			// Add handler for newValue.CollectionChanged (if possible)
			if (null != newValue)
			{
				((ObservableCollection<BaseNode>)newValue).CollectionChanged += new NotifyCollectionChangedEventHandler(newValueINotifyCollectionChanged_CollectionChanged);
			}
		}
		#endregion

		internal class NodeBlockDragAdorner : Adorner
		{
			private ContentPresenter _adorningContentPresenter;
			internal object Data { get; set; }
			internal DataTemplate Template { get; set; }
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

			public NodeBlockDragAdorner(UIElement uiElement) : base(uiElement)
			{
				_adorningContentPresenter = new ContentPresenter();
				_adorningContentPresenter.Opacity = 0.5;
				_layer = AdornerLayer.GetAdornerLayer(uiElement);

				_layer.Add(this);
				IsHitTestVisible = false;
			}
		}

		static NodeBlockDragAdorner _dragAdorner;

		void newValueINotifyCollectionChanged_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if(e.Action == NotifyCollectionChangedAction.Add)
			{
				NodeEditor_BackCanvas.Children.Add((BaseNode)e.NewItems[0]);
			}
			if(e.Action == NotifyCollectionChangedAction.Remove)
			{

			}


			//ContentControl bor = (ContentControl)this.Resources["TimelineBlock_CC"];
			//Tracks_Grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(75) });
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

			//Canvas c = new Canvas() { Background = Brushes.Gray, Margin = new Thickness(0, 3, 0, 3) };
			//c.MouseEnter += C_MouseEnter;
			//Grid.SetRow(timeline, Tracks_Grid.RowDefinitions.Count - 1);

			////add my custom time block
			//TimeBlock timeBlock = new TimeBlock(timeline, 0) { Trackname = "Memes", Width = 100, Margin = new Thickness(0, 0, 0, 3) };
			//Canvas.SetLeft(timeBlock, 1);
			//timeline.Children.Add(timeBlock);
			//Timelines_Grid.Children.Add(timeline);

			//timelines.Add(timeline);

		}





		Point MPos = new Point();
		Point GridOffset = new Point();
		private ImageBrush imgtilebrush;
		private int newx;
		private int newy;
		Rectangle selectrect = new Rectangle();


		bool isMDown = false;
		bool isMInNode = false;
		Point CurveStart = new Point();


		public NodeEditor()
    {
        InitializeComponent();
    }






		/// <summary>
		/// This method takes care of mouse movement events on the main level editor canvas.
		/// Panning is handled in here.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void LevelEditor_BackCanvas_MouseMove(object sender, MouseEventArgs e)
		{
			//we need to display the cords.
			Point p = Mouse.GetPosition(NodeEditor_BackCanvas);
			String point = String.Format("({0}, {1}) OFF:({2}, {3})", (int)p.X, (int)p.Y, (int)Canvas_grid.Viewport.X, (int)Canvas_grid.Viewport.Y);

			//which way is mouse moving?
			MPos -= (Vector)e.GetPosition(LevelEditor_Canvas);
			//is the middle mouse button down?
			if (e.MiddleButton == MouseButtonState.Pressed)
			{
				LevelEditorPan();
			}

			MPos = e.GetPosition(LevelEditor_Canvas); //set this for the iteration
		}

		private Point RelativeGridSnap(Point p, bool abs = true)
		{

			//find the Relative grid size in pixels
			int relgridsize = (int)(40 * Math.Round(LevelEditor_Canvas.RenderTransform.Value.M11, 1));
			//find the offset amount.
			int Xoff = (int)(Math.Abs(Canvas_grid.Viewport.X));
			int YOff = (int)(Math.Abs(Canvas_grid.Viewport.Y));

			//what is the left over amount?
			Xoff %= 40;
			YOff %= 40;

			//relative snap offset
			Xoff = 40 - Xoff;
			YOff = 40 - YOff;

			Xoff = (int)(Xoff * LevelEditor_Canvas.RenderTransform.Value.M11);
			YOff = (int)(YOff * LevelEditor_Canvas.RenderTransform.Value.M11);

			if (Xoff == 40) Xoff = 0;
			if (YOff == 40) YOff = 0;

			//divide the sumation by the relative grid size
			Point relpoint = new Point((int)((p.X - Xoff) / relgridsize), (int)((p.Y - YOff) / relgridsize));
			relpoint.X *= (relgridsize);
			relpoint.Y *= (relgridsize);

			if (abs)
			{ //return the abs size. Base 40x40 grid.
				return new Point(relpoint.X + Xoff, relpoint.Y + YOff);//this gives us the cell number. Use this and multiply by the base value.
			}
			else //rel grid size
			{
				return new Point();
			}


		}

		private Point GetGridSnapCords(Point p)
		{
			int Xoff = (int)(Math.Abs(Canvas_grid.Viewport.X)) % 40; Xoff = 40 - Xoff; //offset
			int YOff = (int)(Math.Abs(Canvas_grid.Viewport.Y)) % 40; YOff = 40 - YOff;

			p.X -= Math.Floor(p.X - Xoff) % 40;  //TODO: Add the offset so we can fill the grid AFTER PAnNNG
			p.Y -= Math.Floor(p.Y - YOff) % 40;
			return p;
		}

		/// <summary>
		/// Performs the panning effect on the main level editor canvas.
		/// </summary>
		private void LevelEditorPan()
		{
			//this is here so when we pan the tiles work with the relative cords we are moving to. Its allows the tiles to maintain position data.
			foreach (UIElement child in LevelEditor_Canvas.Children)
			{

				double x = Canvas.GetLeft(child);
				double y = Canvas.GetTop(child);
				Canvas.SetLeft(child, x + MPos.X);
				Canvas.SetTop(child, y + MPos.Y);
			}
			//moves the Grid, and canvas to perform a panning effect/.
			Canvas_grid.Viewport = new Rect(Canvas_grid.Viewport.X + MPos.X, Canvas_grid.Viewport.Y + MPos.Y,
				Canvas_grid.Viewport.Width, Canvas_grid.Viewport.Height);

			GridOffset.X -= MPos.X / 10; //keeps in sync
			GridOffset.Y -= MPos.Y / 10;
		}


		private void LevelEditor_BackCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			Point pos = Mouse.GetPosition(NodeEditor_BackCanvas);
			//Point p = GetGridSnapCords(Mouse.GetPosition(LevelEditor_Canvas));
			Point p = GetGridSnapCords(pos);

			newx = (int)p.X; newy = (int)p.Y;
			//NewPos_TB.Text = p.ToString();

			if (LevelEditor_Canvas.Children.Contains(selectrect))
				LevelEditor_Canvas.Children.Remove(selectrect);
			Canvas.SetLeft(selectrect, newx); Canvas.SetTop(selectrect, newy); Canvas.SetZIndex(selectrect, 100);
			LevelEditor_Canvas.Children.Add(selectrect);

		}

		private void MoveThumb_Middle_DragDelta(object sender, DragDeltaEventArgs e)
		{
			BaseNode BN = ((BaseNode)((Thumb)sender).TemplatedParent);
			Canvas.SetLeft(BN, VisualTreeHelper.GetOffset(BN).X + e.HorizontalChange);
			Canvas.SetTop(BN, VisualTreeHelper.GetOffset(BN).Y + e.VerticalChange);
		}

		private void AddDialogueRow_BTN_Click(object sender, RoutedEventArgs e)
		{
			
		}

		private void MoveThumb_Right_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			Console.WriteLine("Mouse Down on Output");
			isMDown = true;
			CurveStart = Mouse.GetPosition(NodeEditor_BackCanvas);

		}

		private void MoveThumb_Left_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			Console.WriteLine("Mouse Up on Input");
		}

		private void MoveThumb_Left_PreviewMouseMove(object sender, MouseEventArgs e)
		{
			Console.WriteLine("MouseMoving in Input");
			if(isMDown)
			{
				Point end = Mouse.GetPosition(NodeEditor_BackCanvas);
				BezierSegment bs = new BezierSegment()
				{
					Point1 = CurveStart,
					Point2 = new Point((CurveStart.X + end.X) / 2, end.Y),
					Point3 = end,
				};

				PathGeometry pathGeometry = new PathGeometry();

				PathFigure pathFigure = new PathFigure();
				pathFigure.StartPoint = bs.Point1;
				pathFigure.IsClosed = false;

				pathGeometry.Figures.Add(pathFigure);
				pathFigure.Segments.Add(bs);

				Path p = new Path();
				p.Stroke = Brushes.White;
				p.StrokeThickness = 5;
				p.Data = pathGeometry;

				NodeEditor_BackCanvas.Children.Add(p);
				isMDown = false;
			}
			
		}


		private void MoveThumb_Left_PreviewDragEnter(object sender, DragEventArgs e)
		{
			Console.WriteLine("Mouse Dragged into Input");
		}
	}
}
