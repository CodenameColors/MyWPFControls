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
				((ObservableCollection<DialogueNodeBlock>)oldValue).CollectionChanged -= new NotifyCollectionChangedEventHandler(newValueINotifyCollectionChanged_CollectionChanged);
			}
			// Add handler for newValue.CollectionChanged (if possible)
			if (null != newValue)
			{
				((ObservableCollection<object>)newValue).CollectionChanged += new NotifyCollectionChangedEventHandler(newValueINotifyCollectionChanged_CollectionChanged);
			}
		}

		void newValueINotifyCollectionChanged_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Add)
			{
				if (e.NewItems[0] is DialogueNodeBlock)
				{
					DialogueNodeBlock bn = (DialogueNodeBlock)e.NewItems[0];
					NodeEditor_Canvas.Children.Add(bn);
					Point p = new Point(0, 10); Point p1 = new Point(150, 20 + 20);
					bn.InputNodes.Add(new ConnectionNode("EnterNode", p, ECOnnectionType.Enter));
					bn.OutputNodes.Add(new ConnectionNode("OutputNode1", p1, ECOnnectionType.Exit));
				}
				else if (e.NewItems[0] is GetConstantNodeBlock)
				{
					GetConstantNodeBlock bn = (GetConstantNodeBlock)e.NewItems[0];
					NodeEditor_Canvas.Children.Add(bn);
				}
				else if(e.NewItems[0] is SetConstantNodeBlock)
				{
					SetConstantNodeBlock bn = (SetConstantNodeBlock)e.NewItems[0];
					NodeEditor_Canvas.Children.Add(bn);
				}
			}
			if (e.Action == NotifyCollectionChangedAction.Remove)
			{

			}
		}


		#endregion

		/// <summary>
		/// This Dependency property is here to either allow base node right click deletion
		/// </summary>
		#region AllowNodeDeletion
		public bool AllowNodeDeletion
		{
			get { return (bool)GetValue(AllowNodeDeletionProperty); }
			set { SetValue(AllowNodeDeletionProperty, value); }
		}

		public static readonly DependencyProperty AllowNodeDeletionProperty =
					DependencyProperty.Register("AllowNodeDeletion", typeof(bool), typeof(NodeEditor),
						new PropertyMetadata(false, new PropertyChangedCallback(OnAllowNodeDeletionChange)));
		private static void OnAllowNodeDeletionChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{

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

		
		Point MPos = new Point();
		Point GridOffset = new Point();
		private ImageBrush imgtilebrush;
		private int newx;
		private int newy;
		Rectangle selectrect = new Rectangle();

		bool isMDown = false;
		bool isMInNode = false;
		Point CurveStart = new Point();
		object SelectedBlockNode = null;
		ConnectionNode SelectedNode = null;
		int SelectedNodeRow = 0;


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
			{
				isMDown = false;
				if(!(this.FindResource("DeleteBaseNode_CM") as ContextMenu).IsOpen)
					SelectedBlockNode = null;
				SelectedNodeRow = -1;
			}

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
			object obj = ((Thumb)sender).TemplatedParent;
			if (obj is DialogueNodeBlock)
				MoveDialogueBlock(sender, e);
			else if(obj is GetConstantNodeBlock)
			{
				GetConstantNodeBlock BN = ((GetConstantNodeBlock)((Thumb)sender).TemplatedParent);
				Canvas.SetLeft(BN, VisualTreeHelper.GetOffset(BN).X + e.HorizontalChange);
				Canvas.SetTop(BN, VisualTreeHelper.GetOffset(BN).Y + e.VerticalChange);

				Point Start = new Point (Canvas.GetLeft(BN) + 100, Canvas.GetTop(BN) + 40);
				for (int j = 0; j < BN.output.Curves.Count; j++)
					SetCurveStartPoint(BN.output.Curves[j], Start);
			}
			else if (obj is SetConstantNodeBlock)
			{
				SetConstantNodeBlock BN = ((SetConstantNodeBlock)((Thumb)sender).TemplatedParent);
				Canvas.SetLeft(BN, VisualTreeHelper.GetOffset(BN).X + e.HorizontalChange);
				Canvas.SetTop(BN, VisualTreeHelper.GetOffset(BN).Y + e.VerticalChange);
			}
		}
		

		private void MoveDialogueBlock(object sender, DragDeltaEventArgs e)
		{
			DialogueNodeBlock BN = ((DialogueNodeBlock)((Thumb)sender).TemplatedParent);
			Canvas.SetLeft(BN, VisualTreeHelper.GetOffset(BN).X + e.HorizontalChange);
			Canvas.SetTop(BN, VisualTreeHelper.GetOffset(BN).Y + e.VerticalChange);

			if (NodeEditor_BackCanvas.Children.Count > 1)
			{
				for (int i = 0; i < BN.InputNodes.Count; i++)
				{
					Point end = new Point(Canvas.GetLeft(BN), Canvas.GetTop(BN)); //end.Y +=(10); //the 10 is middle of circle
					for (int j = 0; j < BN.InputNodes[i].Curves.Count; j++)
					{
						if (BN.InputNodes[i].Name.Contains("Enter"))
						{
							Point end1 = end; end1.Y += 10;
							SetCurveEndPoint(BN.InputNodes[i].Curves[j], end1);
						}
						else
						{
							Point end1 = end; end1.Y += 40 + (20 * i);
							SetCurveEndPoint(BN.InputNodes[i].Curves[j], end1);
						}
					}
				}

				if (BN.OutputNodes.Count > 0) //move the "left most" node
				{
					for (int i = 0; i < BN.OutputNodes.Count; i++)
					{
						Point start = new Point(Canvas.GetLeft(BN) + 150, Canvas.GetTop(BN)); start.Y += 20 + (40 * 
							( BN.InputNodes.Count - 2 < 0 ? 0 : (BN.InputNodes.Count - 1))) + (40 * i) + 20; //the 20 is middle of circle
						for (int j = 0; j < BN.OutputNodes[i].Curves.Count; j++)
							SetCurveStartPoint(BN.OutputNodes[i].Curves[j], start);
						//Point end = new Point(Canvas.GetLeft(BN), Canvas.GetTop(BN)); end.Y += 40 + (10); //the 10 is middle of circle
						//SetCurveEndPoint((Path)NodeEditor_BackCanvas.Children[1], end);
					}
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

			object obj = null;
			if (sender is Thumb)
				obj = ((Thumb)sender).TemplatedParent;
			else if (sender is Grid)
				obj = ((Grid)((Grid)sender).Parent).TemplatedParent; //the input nodes are added to style templated ON RUNTIME

			if (obj is DialogueNodeBlock)
				StartDrawingDialogueNodeCurve(sender);
			else if(obj is GetConstantNodeBlock)
				StartDrawingConstantNode(sender);
		}

		private void StartDrawingDialogueNodeCurve(object sender)
		{
			try
			{
				SelectedBlockNode = (DialogueNodeBlock)((Thumb)sender).TemplatedParent;
				SelectedNodeRow = Grid.GetRow((Thumb)sender);
				SelectedNode = ((DialogueNodeBlock)SelectedBlockNode).OutputNodes[SelectedNodeRow];
			}
			catch
			{
				//im a garbo coder at times. So this is making sure my REF is set
				if (SelectedBlockNode == null)
				{
					SelectedBlockNode = (DialogueNodeBlock)((Grid)((Grid)sender).Parent).TemplatedParent;
					SelectedNodeRow = Grid.GetRow((Grid)sender);
					SelectedNode = ((DialogueNodeBlock)SelectedBlockNode).OutputNodes[SelectedNodeRow];
				}
			}
		}

		private void StartDrawingConstantNode(object sender)
		{
			SelectedBlockNode = (GetConstantNodeBlock)((Thumb)sender).TemplatedParent;
			SelectedNodeRow = Grid.GetRow((Thumb)sender); 
			SelectedNode = ((GetConstantNodeBlock)SelectedBlockNode).output;
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
				object obj = null;
				if (sender is Thumb)
					obj = ((Thumb)sender).TemplatedParent;
				else if (sender is Grid)
					obj = ((Grid)((Grid)sender).Parent).TemplatedParent; //the input nodes are added to style templated ON RUNTIME

				if (obj is DialogueNodeBlock)
					EndDrawingCurveDialogueBlock(sender);
				else if (obj is GetConstantNodeBlock)
				{
					EndDrawingCurveConstantBlock(sender);
				}
				else if (obj is SetConstantNodeBlock)
				{

				}

			}
		}

		private void EndDrawingCurveDialogueBlock(object sender)
		{
			if (SelectedBlockNode == null) return;
			DialogueNodeBlock BN = null; int inrow = 0;
			try
			{
				BN = (DialogueNodeBlock)((Thumb)sender).TemplatedParent;
			}
			catch
			{
				BN = (DialogueNodeBlock)((Grid)((Grid)sender).Parent).TemplatedParent;
				inrow = Grid.GetRow(((Grid)sender)) + 1; //the one offset is for the enter node.
			}
			if (BN == null) return;
			//get paths
			Point end = Mouse.GetPosition(NodeEditor_BackCanvas);
			Path p = CreateBezierCurve(CurveStart, end);
			//do the types match?
			if (SelectedBlockNode is DialogueNodeBlock)
			{
				//types match check
				if (((DialogueNodeBlock)SelectedBlockNode).OutputNodes[SelectedNodeRow].NodeType != BN.InputNodes[inrow].NodeType)
				{
					if (((DialogueNodeBlock)SelectedBlockNode).OutputNodes[SelectedNodeRow].NodeType == ECOnnectionType.Exit && BN.InputNodes[inrow].NodeType == ECOnnectionType.Enter) { } //this is just an ignore.
					else if (((DialogueNodeBlock)SelectedBlockNode).OutputNodes[SelectedNodeRow].NodeType != BN.InputNodes[inrow].NodeType) return;
				}
				//Draw the curve
				NodeEditor_BackCanvas.Children.Add(p);
				isMDown = false;

				BN.InputNodes[inrow].ConnectedNodes.Add(SelectedNode); BN.InputNodes[inrow].Curves.Add(p);
				((DialogueNodeBlock)SelectedBlockNode).OutputNodes[SelectedNodeRow].ConnectedNodes.Add(BN.InputNodes[inrow]);
				((DialogueNodeBlock)SelectedBlockNode).OutputNodes[SelectedNodeRow].Curves.Add(p);
			}
			else if (SelectedBlockNode is GetConstantNodeBlock) //the previous selected node is a CONSTANT node block
			{
				//the constant node can ONLY output data types. SO they must match
				if (((GetConstantNodeBlock)SelectedBlockNode).output.NodeType != BN.InputNodes[inrow].NodeType) return; //doesn't match
				NodeEditor_BackCanvas.Children.Add(p);
				isMDown = false;
				BN.InputNodes[inrow].ConnectedNodes.Add(SelectedNode); BN.InputNodes[inrow].Curves.Add(p);
				((GetConstantNodeBlock)SelectedBlockNode).output.ConnectedNodes.Add(BN.InputNodes[inrow]);
				((GetConstantNodeBlock)SelectedBlockNode).output.Curves.Add(p);
			}
		}

		private void EndDrawingCurveConstantBlock(object sender)
		{
			Console.WriteLine("this SHOULD NEVER be called... ");
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
			Canvas.SetZIndex(p, -1);
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

			//add the output node Display wise
			Grid g = new Grid()
			{
				ShowGridLines = true,
				Width = 20,
				Height = 20
			};
			Ellipse ee = new Ellipse()
			{
				Height = 20,
				Width = 20,
				Fill = Brushes.White,
				Cursor = Cursors.Hand,
				Margin = new Thickness(-10, -10, -10, -10),
				HorizontalAlignment = HorizontalAlignment.Right,
				VerticalAlignment = VerticalAlignment.Center
			};
			g.Children.Add(ee);
			Grid.SetRow(g, OutputGrid.RowDefinitions.Count - 1); Grid.SetColumn(g, 2);
			g.PreviewMouseLeftButtonDown += MoveThumb_Right_MouseLeftButtonDown;
			OutputGrid.Children.Add(g);

			//add the output node data wise
			DialogueNodeBlock BN = (DialogueNodeBlock)((Button)sender).TemplatedParent;
			Point p = new Point(Canvas.GetLeft(BN)+ 150, Canvas.GetTop(BN) + 20 + (20 * OutputGrid.RowDefinitions.Count));
			BN.OutputNodes.Add(new ConnectionNode("OutputNode" + OutputGrid.RowDefinitions.Count, p, ECOnnectionType.Exit));


			//add an input node IF row definition count is 2. This is the dialogue choice val. ONLY CAN HAVE ONE
			if (OutputGrid.RowDefinitions.Count != 2) return;
			Grid inputGrid = null;
			foreach (UIElement item in basegrid.Children)
			{
				if (item is Grid && ((Grid)item).Name.Contains("Input"))
				{ inputGrid = item as Grid; break; }
			}

			if (inputGrid.RowDefinitions.Count >= 2) return; //imit out dialogue nodes to TWO inputs only

			//add the dialouge textblock
			inputGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(40) });
			TextBlock tbb = new TextBlock()
			{
				Text = "Choice Val",
				Margin = new Thickness(5),
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top,
				Foreground = Brushes.White
			};
			Grid.SetRow(tbb, inputGrid.RowDefinitions.Count - 1); Grid.SetColumn(tbb, 1);
			inputGrid.Children.Add(tbb);

			//add the output node Display wise
			Grid gg = new Grid() { ShowGridLines = true, Width = 20, Height = 20 };
			Ellipse eee = new Ellipse()
			{
				Height = 20,
				Width = 20,
				Fill = Brushes.BlueViolet,
				Cursor = Cursors.Hand,
				Margin = new Thickness(-10, -10, -10, -10),
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Center
			};
			gg.Children.Add(eee);
			Grid.SetRow(gg, inputGrid.RowDefinitions.Count - 1); Grid.SetColumn(gg, 0);
			gg.PreviewMouseMove += MoveThumb_Left_PreviewMouseMove;
			inputGrid.Children.Add(gg);

			//Add the combo box to tell/change what data type is allow on this node
			ComboBox cb = new ComboBox() { IsEnabled = false, HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center};
			cb.Items.Add("Bool"); cb.Items.Add("Int");
			cb.SelectedIndex = 1;
			Grid.SetRow(cb, inputGrid.RowDefinitions.Count - 1); Grid.SetColumn(cb, 2);
			inputGrid.Children.Add(cb);

			//add the Input node data wise
			Point pp = new Point(Canvas.GetLeft(BN) + 150, Canvas.GetTop(BN) + 20 + (20 * inputGrid.RowDefinitions.Count));
			BN.InputNodes.Add(new ConnectionNode("InputNode" + inputGrid.RowDefinitions.Count, pp, ECOnnectionType.Int));





		}

		private void AddDialogueInput_BTN_Click(object sender, RoutedEventArgs e)
		{
			Grid basegrid = (Grid)((Button)sender).Parent;
			Grid inputGrid = null;
			foreach (UIElement item in basegrid.Children)
			{
				if (item is Grid && ((Grid)item).Name.Contains("Input"))
				{ inputGrid = item as Grid; break; }
			}

			if (inputGrid.RowDefinitions.Count >= 2) return; //imit out dialogue nodes to TWO inputs only

			//add the dialouge textblock
			inputGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(40) });
			TextBlock tb = new TextBlock()
			{
				Text = "Unlocking Var",
				Margin = new Thickness(5),
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top,
				Foreground = Brushes.White
			};
			Grid.SetRow(tb, inputGrid.RowDefinitions.Count - 1); Grid.SetColumn(tb, 1);
			inputGrid.Children.Add(tb);

			//add the output node Display wise
			Grid g = new Grid() { ShowGridLines = true, Width = 20, Height = 20 };
			Ellipse ee = new Ellipse()
			{
				Height = 20,
				Width = 20,
				Fill = Brushes.Red,
				Cursor = Cursors.Hand,
				Margin = new Thickness(-10, -10, -10, -10),
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Center
			};
			g.Children.Add(ee);
			Grid.SetRow(g, inputGrid.RowDefinitions.Count - 1); Grid.SetColumn(g, 0);
			g.PreviewMouseMove += MoveThumb_Left_PreviewMouseMove;
			inputGrid.Children.Add(g);

			//Add the combo box to tell/change what data type is allow on this node
			ComboBox cb = new ComboBox() { HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center };
			cb.Items.Add("Bool"); cb.Items.Add("Int");
			cb.SelectedIndex = 0;
			cb.SelectionChanged += DialogueInput_SelectionChanged;

			Grid.SetRow(cb, inputGrid.RowDefinitions.Count - 1); Grid.SetColumn(cb, 2);
			inputGrid.Children.Add(cb);

			//add the Input node data wise
			DialogueNodeBlock BN = (DialogueNodeBlock)((Button)sender).TemplatedParent;
			Point p = new Point(Canvas.GetLeft(BN) + 150, Canvas.GetTop(BN) + 20 + (20 * inputGrid.RowDefinitions.Count));
			BN.InputNodes.Add(new ConnectionNode("InputNode" + inputGrid.RowDefinitions.Count, p, ECOnnectionType.Bool));

		}

		private void DialogueInput_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			//change the circle color display wise

			//first find the circle
			Grid nodegrid = ((Grid)((ComboBox)sender).Parent);
			for (int i =0; i < nodegrid.Children.Count; i++)
			{
				if ((nodegrid.Children[i] is Grid) && Grid.GetRow(nodegrid.Children[i] as Grid) == Grid.GetRow(sender as ComboBox))
				{
					Ellipse ell = null;
					if (((Grid)nodegrid.Children[i]).Children[0] is Ellipse)
						ell = (Ellipse)((Grid)nodegrid.Children[i]).Children[0];
					//change the color
					if ((sender as ComboBox).SelectedIndex == 0)
					{
						ell.Fill = Brushes.Red;
						(nodegrid.TemplatedParent as DialogueNodeBlock).InputNodes[(Grid.GetRow(sender as ComboBox)) + 1].NodeType = ECOnnectionType.Bool;
					}
					else if((sender as ComboBox).SelectedIndex == 1)
					{
						ell.Fill = Brushes.BlueViolet;
						(nodegrid.TemplatedParent as DialogueNodeBlock).InputNodes[(Grid.GetRow(sender as ComboBox)) + 1].NodeType = ECOnnectionType.Int;
					}
				}
			}
		}

		/// <summary>
		/// Right click on a base node. Opens up the context menu
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BaseNodeRight_BTN_Click(object sender, MouseButtonEventArgs e)
		{
			if (AllowNodeDeletion)
			{
				Point p = Mouse.GetPosition(NodeEditor_BackCanvas);
				SelectedBlockNode = (DialogueNodeBlock)(sender as Grid).TemplatedParent;
				ContextMenu cm = this.FindResource("DeleteBaseNode_CM") as ContextMenu;
				cm.PlacementTarget = sender as ContentControl;
				cm.IsOpen = true;
			}
		}

		private void DeleteBaseNode_Click(object sender, RoutedEventArgs e)
		{
			Console.WriteLine("right clicked on node.");
			if (SelectedBlockNode is DialogueNodeBlock)
				NodeEditor_Canvas.Children.Remove(((DialogueNodeBlock)SelectedBlockNode));
			else if (SelectedBlockNode is GetConstantNodeBlock)
			{

			}

		}

		private void COnnectionNode_RightClick(object sender, MouseButtonEventArgs e)
		{
			Point p = Mouse.GetPosition(NodeEditor_BackCanvas);
			SelectedBlockNode = (DialogueNodeBlock)(sender as Grid).TemplatedParent;
			String TempStr = "";

			if ((sender as Grid).Name.Contains("Entry"))
			{
				SelectedNodeRow = 0;//Grid.GetRow((Grid)(sender as Grid));
				SelectedNode = ((DialogueNodeBlock)SelectedBlockNode).InputNodes[0];
				TempStr = "In";
			}
			else if ((sender as Grid).Name.Contains("Out"))
			{
				SelectedNodeRow = Grid.GetRow((Grid)(sender as Grid));
				SelectedNode = ((DialogueNodeBlock)SelectedBlockNode).OutputNodes[SelectedNodeRow];
				TempStr = "Out";
			}
			else
			{
				SelectedNodeRow = Grid.GetRow((Grid)(sender as Grid)) + 1;
				SelectedNode = ((DialogueNodeBlock)SelectedBlockNode).InputNodes[SelectedNodeRow];
				TempStr = "In";
			}

			ContextMenu cm = this.FindResource("DeleteConnections_CM") as ContextMenu;

			for (int i = cm.Items.Count-1; i >= 2; i--)
				cm.Items.RemoveAt(i);

			
			for(int i = 0; i < SelectedNode.ConnectedNodes.Count; i++)
			{
				MenuItem mi = new MenuItem() { Header = String.Format("{0}:{1}", TempStr, i+1)};
				mi.PreviewMouseLeftButtonDown += DeleteConnections_Click;
				cm.Items.Add(mi);
			}

			cm.PlacementTarget = sender as ContentControl;
			cm.IsOpen = true;
		}

		private void DeleteConnections_Click(object sender, MouseButtonEventArgs e)
		{
			Console.WriteLine("Clicked Deletion of Node");
			int ival = ((ContextMenu)(sender as MenuItem).Parent).Items.IndexOf(sender); ival -= 2;
			NodeEditor_BackCanvas.Children.Remove(SelectedNode.Curves[ival]);
			SelectedNode.ConnectedNodes[ival].Curves.Remove(SelectedNode.Curves[ival]);
			SelectedNode.Curves.RemoveAt(ival);

			if(SelectedNode.ConnectedNodes[ival].ConnectedNodes.IndexOf(SelectedNode) >= 0)
				SelectedNode.ConnectedNodes[ival].ConnectedNodes.Remove(SelectedNode);

			
			SelectedNode.ConnectedNodes.RemoveAt(ival);

		}


	}
}
