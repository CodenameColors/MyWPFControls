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
				BaseNode bn = (BaseNode)e.NewItems[0];
				NodeEditor_Canvas.Children.Add(bn);
				Point p = new Point(0, 10); Point p1 = new Point(150, 20 + 20);
				bn.InputNodes.Add(new ConnectionNode("EnterNode", p));
				bn.OutputNodes.Add(new ConnectionNode("OutputNode1", p1));

			}
			if(e.Action == NotifyCollectionChangedAction.Remove)
			{

			}
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
		BaseNode SelectedNode = null;


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
			MPos -= (Vector)e.GetPosition(NodeEditor_Canvas);
			//is the middle mouse button down?
			if (e.MiddleButton == MouseButtonState.Pressed)
			{
				LevelEditorPan();
			}

			//reset selection vars
			if (e.LeftButton == MouseButtonState.Released)
			{ isMDown = false; SelectedNode = null; }

			MPos = e.GetPosition(NodeEditor_Canvas); //set this for the iteration
		}

		private Point RelativeGridSnap(Point p, bool abs = true)
		{

			//find the Relative grid size in pixels
			int relgridsize = (int)(40 * Math.Round(NodeEditor_Canvas.RenderTransform.Value.M11, 1));
			//find the offset amount.
			int Xoff = (int)(Math.Abs(Canvas_grid.Viewport.X));
			int YOff = (int)(Math.Abs(Canvas_grid.Viewport.Y));

			//what is the left over amount?
			Xoff %= 40;
			YOff %= 40;

			//relative snap offset
			Xoff = 40 - Xoff;
			YOff = 40 - YOff;

			Xoff = (int)(Xoff * NodeEditor_Canvas.RenderTransform.Value.M11);
			YOff = (int)(YOff * NodeEditor_Canvas.RenderTransform.Value.M11);

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
			foreach (UIElement child in NodeEditor_Canvas.Children)
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

			if (NodeEditor_Canvas.Children.Contains(selectrect))
				NodeEditor_Canvas.Children.Remove(selectrect);
			Canvas.SetLeft(selectrect, newx); Canvas.SetTop(selectrect, newy); Canvas.SetZIndex(selectrect, 100);
			NodeEditor_Canvas.Children.Add(selectrect);

		}

		private void MoveThumb_Middle_DragDelta(object sender, DragDeltaEventArgs e)
		{
			BaseNode BN = ((BaseNode)((Thumb)sender).TemplatedParent);
			Canvas.SetLeft(BN, VisualTreeHelper.GetOffset(BN).X + e.HorizontalChange);
			Canvas.SetTop(BN, VisualTreeHelper.GetOffset(BN).Y + e.VerticalChange);
			
			for(int i = 0; i < BN.InputNodes.Count;i++)
			{
				BN.InputNodes[i].NodeLocation.X += e.HorizontalChange;
				BN.InputNodes[i].NodeLocation.Y += e.VerticalChange;
			}
			for (int i = 0; i < BN.OutputNodes.Count; i++)
			{
				BN.OutputNodes[i].NodeLocation.X += e.HorizontalChange;
				BN.OutputNodes[i].NodeLocation.Y += e.VerticalChange;
			}

			if (NodeEditor_BackCanvas.Children.Count > 1)
			{
				if (BN.InputNodes[0].ConnectedNodes.Count != 0) //Move the "right side" node
				{
					Point end = new Point(Canvas.GetLeft(BN), Canvas.GetTop(BN)); //end.Y +=(10); //the 10 is middle of circle
					for (int i = 0; i < BN.InputNodes[0].Curves.Count; i++)
					{
						if (BN.InputNodes[0].Name.Contains("Enter"))
						{
							Point end1 = end; end1.Y += 10;
							SetCurveEndPoint(BN.InputNodes[0].Curves[i], end1);
						}
					}
				}
				else if (BN.OutputNodes[0].ConnectedNodes.Count != 0) //move the "left side" node
				{
					Point start = new Point(Canvas.GetLeft(BN)+150, Canvas.GetTop(BN)); start.Y += 40+ (10); //the 10 is middle of circle
					for(int i = 0;i< BN.OutputNodes[0].Curves.Count; i++)
						SetCurveStartPoint(BN.OutputNodes[0].Curves[i], start);
					//Point end = new Point(Canvas.GetLeft(BN), Canvas.GetTop(BN)); end.Y += 40 + (10); //the 10 is middle of circle
					//SetCurveEndPoint((Path)NodeEditor_BackCanvas.Children[1], end);
				}
			}

		}

		private void SetCurveEndPoint(Path p, Point end)
		{
			PathFigure pf = ((PathGeometry)p.Data).Figures[0];
			BezierSegment bsOld = (BezierSegment)pf.Segments[0];
			bsOld.Point3 = end;
			bsOld.Point2 = new Point((end.X + bsOld.Point1.X)/2, end.Y);
		}
		private void SetCurveStartPoint(Path p, Point start)
		{
			PathFigure pf = ((PathGeometry)p.Data).Figures[0];
			pf.StartPoint = start;
			BezierSegment bsOld = (BezierSegment)pf.Segments[0];
			bsOld.Point1 = start;
			bsOld.Point2 = new Point((start.X + bsOld.Point3.X) / 2, start.Y);
		}

		private void AddDialogueRow_BTN_Click(object sender, RoutedEventArgs e)
		{
			
		}

		private void MoveThumb_Right_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			Console.WriteLine("Mouse Down on Output");
			isMDown = true;
			CurveStart = Mouse.GetPosition(NodeEditor_BackCanvas);
			try
			{
				SelectedNode = (BaseNode)((Thumb)sender).TemplatedParent;
			}
			catch
			{
				//im a garbo coder at times. So this is making sure my REF is set
				if (SelectedNode == null)
					SelectedNode = (BaseNode)((Grid)((Grid)sender).Parent).TemplatedParent;
			}
			

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
				BaseNode BN = (BaseNode)((Thumb)sender).TemplatedParent;
				Point end = Mouse.GetPosition(NodeEditor_BackCanvas);

				Path p = CreateBezierCurve(CurveStart, end);
				NodeEditor_BackCanvas.Children.Add(p);
				isMDown = false;

				BN.InputNodes[0].ConnectedNodes.Add(SelectedNode); BN.InputNodes[0].Curves.Add(p);
				SelectedNode.OutputNodes[0].ConnectedNodes.Add(BN); SelectedNode.OutputNodes[0].Curves.Add(p);

			}
			
		}

		private Path CreateBezierCurve(Point start, Point end)
		{
			BezierSegment bs = new BezierSegment()
			{
				Point1 = start,
				Point2 = new Point((start.X + end.X) / 2, end.Y),
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
			return p;
		}


		private void MoveThumb_Left_PreviewDragEnter(object sender, DragEventArgs e)
		{
			Console.WriteLine("Mouse Dragged into Input");
		}

		private void AddDialogueOutput_BTN_Click(object sender, RoutedEventArgs e)
		{
			Grid basegrid = (Grid)((Button)sender).Parent;
			Grid OutputGrid = null;
			foreach (UIElement item in basegrid.Children)
			{
				if (item is Grid && ((Grid)item).Name.Contains("Output"))
				{ OutputGrid = item as Grid; break; }
			}
			//add the dialouge textblock
			OutputGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(40) });
			TextBlock tb = new TextBlock() { Text = "Memes" , Margin = new Thickness(5),
				HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment=VerticalAlignment.Top};
			Grid.SetRow(tb, OutputGrid.RowDefinitions.Count-1); Grid.SetColumn(tb, 1);
			OutputGrid.Children.Add(tb);

			//add the output node
			ContentControl cc = (ContentControl)this.Resources["OutputNode"];
			Grid.SetRow(cc, OutputGrid.RowDefinitions.Count - 1); Grid.SetColumn(cc, 2);

			Grid g = new Grid() { ShowGridLines = true, Width = 20, Height = 20 };
			Ellipse ee = new Ellipse() { Height = 20, Width = 20, Fill = Brushes.Red,
				Cursor=Cursors.Hand, Margin = new Thickness(-10,-10,-10,-10), HorizontalAlignment = HorizontalAlignment.Right,
				VerticalAlignment = VerticalAlignment.Center
			};
			g.Children.Add(ee);
			Grid.SetRow(g, OutputGrid.RowDefinitions.Count - 1); Grid.SetColumn(g, 2);
			g.PreviewMouseLeftButtonDown += MoveThumb_Right_MouseLeftButtonDown;


			OutputGrid.Children.Add(g);
		}

		private void AddDialogueInput_BTN_Click(object sender, RoutedEventArgs e)
		{

		}
	}
}
