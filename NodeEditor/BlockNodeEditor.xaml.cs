using NodeEditor.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using NodeEditor.Components.Logic;
using NodeEditor.Resources;

namespace NodeEditor
{
	/// <summary>
	/// Interaction logic for UserControl1.xaml
	/// </summary>
	public partial class BlockNodeEditor : UserControl
	{

		#region ItemSource
		public IEnumerable ItemsSource
		{
			get { return (IEnumerable)GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
		}

		public static readonly DependencyProperty ItemsSourceProperty =
				DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(BlockNodeEditor), new PropertyMetadata(new PropertyChangedCallback(OnItemsSourcePropertyChanged)));

		private static void OnItemsSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			var control = sender as BlockNodeEditor;
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
					bn.EntryNode = (new ConnectionNode(bn, "EnterNode", p, ECOnnectionType.Enter));
					bn.OutputNodes.Add(new ConnectionNode(bn, "OutputNode1", p1, ECOnnectionType.Exit));
				}
				else if (e.NewItems[0] is GetConstantNodeBlock)
				{
					GetConstantNodeBlock bn = (GetConstantNodeBlock)e.NewItems[0];
					NodeEditor_Canvas.Children.Add(bn);
				}
				else if (e.NewItems[0] is SetConstantNodeBlock)
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
					DependencyProperty.Register("AllowNodeDeletion", typeof(bool), typeof(BlockNodeEditor),
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
		Point NewBlockLocation = new Point();
		object SelectedBlockNode = null;
		ConnectionNode SelectedNode = null;
		int SelectedNodeRow = 0;

		public ObservableCollection<RuntimeVars> TestingVars_list = new ObservableCollection<RuntimeVars>();
		public ObservableCollection<String> SceneCharacters_list = new ObservableCollection<string>();

		private Dictionary<String, List<BaseNodeBlock>> VarDisplayBlocks_dict = new Dictionary<String, List<BaseNodeBlock>>();

		public ObservableCollection<NodeEditorException> CurrentErrors = new ObservableCollection<NodeEditorException>();

		public BaseNodeBlock StartExecutionBlock { get; set; }
		public BaseNodeBlock CurrentExecutionBlock;

		/// <summary>
		/// constructor
		/// </summary>
		public BlockNodeEditor()
		{
			InitializeComponent();
			TestingVars_list.CollectionChanged += TestingVars_list_CollectionChanged;

			//List
			TestingVars_list.Add(new RuntimeVars() { VarName = ("ChoiceVar"), VarData = 0 });
			VarDisplayBlocks_dict.Add(TestingVars_list.Last().VarName, new List<BaseNodeBlock>());

			StartBlockNode bn = new StartBlockNode();
			Canvas.SetLeft(bn, 50); Canvas.SetTop(bn, 200);
			NodeEditor_Canvas.Children.Add(bn);

			//Set Starting PTRs
			StartExecutionBlock = bn;
			CurrentExecutionBlock = StartExecutionBlock;

			ExitBlockNode bnn = new ExitBlockNode();
			Canvas.SetLeft(bnn, 400); Canvas.SetTop(bnn, 200);
			NodeEditor_Canvas.Children.Add(bnn);

		}

		#region Panning

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
				if (!(this.FindResource("DeleteBaseNode_CM") as ContextMenu).IsOpen)
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

			foreach (UIElement childElement in NodeEditor_BackCanvas.Children)
			{
				if (childElement is Path bPath)
				{
					double startx = ((bPath.Data as PathGeometry).Figures[0] as PathFigure).StartPoint.X;
					double starty = ((bPath.Data as PathGeometry).Figures[0] as PathFigure).StartPoint.Y;
					((bPath.Data as PathGeometry).Figures[0] as PathFigure).StartPoint = new Point(startx+MPos.X, starty+MPos.Y);

					for (int i = 0; i < ((bPath.Data as PathGeometry).Figures[0] as PathFigure).Segments.Count; i++)
					{
						double x = (((bPath.Data as PathGeometry).Figures[0] as PathFigure).Segments[i] as BezierSegment).Point1.X;
						double y = (((bPath.Data as PathGeometry).Figures[0] as PathFigure).Segments[i] as BezierSegment).Point1.Y;
						(((bPath.Data as PathGeometry).Figures[0] as PathFigure).Segments[i] as BezierSegment).Point1 = new Point(x+MPos.X, y+MPos.Y);

						x = (((bPath.Data as PathGeometry).Figures[0] as PathFigure).Segments[i] as BezierSegment).Point2.X;
						y = (((bPath.Data as PathGeometry).Figures[0] as PathFigure).Segments[i] as BezierSegment).Point2.Y;
						(((bPath.Data as PathGeometry).Figures[0] as PathFigure).Segments[i] as BezierSegment).Point2 = new Point(x + MPos.X, y + MPos.Y);

						x = (((bPath.Data as PathGeometry).Figures[0] as PathFigure).Segments[i] as BezierSegment).Point3.X;
						y = (((bPath.Data as PathGeometry).Figures[0] as PathFigure).Segments[i] as BezierSegment).Point3.Y;
						(((bPath.Data as PathGeometry).Figures[0] as PathFigure).Segments[i] as BezierSegment).Point3 = new Point(x + MPos.X, y + MPos.Y);
					}
					//Canvas.SetLeft(childElement, x + MPos.X);
					//Canvas.SetTop(childElement, y + MPos.Y);
				}
			}
			//moves the Grid, and canvas to perform a panning effect/.
			Canvas_grid.Viewport = new Rect(Canvas_grid.Viewport.X + MPos.X, Canvas_grid.Viewport.Y + MPos.Y,
				Canvas_grid.Viewport.Width, Canvas_grid.Viewport.Height);

			GridOffset.X -= MPos.X / 10; //keeps in sync
			GridOffset.Y -= MPos.Y / 10;
		}
		#endregion

		#region Zooming
		public double LEZoomLevel = 1;
		int LEGridHeight = 40;
		int LEGridWidth = 40;

		/// <summary>
		/// handles mouse scroll events
		/// Zooming is handled here.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void LevelEditor_Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			Console.WriteLine("scroollll");


			if (e.Delta > 0) //zoom in!
			{
				LEZoomLevel += .2;
				Canvas_grid.Transform = new ScaleTransform(LEZoomLevel, LEZoomLevel);
				NodeEditor_Canvas.RenderTransform = new ScaleTransform(LEZoomLevel, LEZoomLevel);
				Console.WriteLine(String.Format("W:{0},  H{1}", LEGridWidth, LEGridHeight));
				
				//scale all the paths
				for(int i = 0; i < NodeEditor_BackCanvas.Children.Count; i++)
				{
					if (NodeEditor_BackCanvas.Children[i] is Path bPath)
					{
						bPath.RenderTransform = new ScaleTransform(LEZoomLevel, LEZoomLevel);
					}
				}


			}
			else  //zoom out!
			{
				LEZoomLevel -= .2;
				Canvas_grid.Transform = new ScaleTransform(LEZoomLevel, LEZoomLevel);
				NodeEditor_Canvas.RenderTransform = new ScaleTransform(LEZoomLevel, LEZoomLevel);
				Console.WriteLine(String.Format("W:{0},  H{1}", LEGridWidth, LEGridHeight));
				//TODO: resize selection rectangle
				//scale all the paths
				for (int i = 0; i < NodeEditor_BackCanvas.Children.Count; i++)
				{
					if (NodeEditor_BackCanvas.Children[i] is Path bPath)
					{
						bPath.RenderTransform = new ScaleTransform(LEZoomLevel, LEZoomLevel);
					}
				}
			}

			if (LEZoomLevel < .2)
			{
				LEZoomLevel = .2;
				Canvas_grid.Transform = new ScaleTransform(LEZoomLevel, LEZoomLevel);
				NodeEditor_Canvas.RenderTransform = new ScaleTransform(LEZoomLevel, LEZoomLevel);
				//scale all the paths
				for (int i = 0; i < NodeEditor_BackCanvas.Children.Count; i++)
				{
					if (NodeEditor_BackCanvas.Children[i] is Path bPath)
					{
						bPath.RenderTransform = new ScaleTransform(LEZoomLevel, LEZoomLevel);
					}
				}
				return;
			} //do not allow this be 0 which in turn / by 0;

			//ZoomFactor_TB.Text = String.Format("({0})%  ({1}x{1})", 100 * LEZoomLevel, LEGridWidth * LEZoomLevel);
			//ScaleFullMapEditor();
		}

		#endregion

		#region Drawing
		private void SetCurveEndPoint(Path p, Point end)
		{
			PathFigure pf = ((PathGeometry)p.Data).Figures[0];
			BezierSegment bsOld = (BezierSegment)pf.Segments[0];
			bsOld.Point3 = end;
			bsOld.Point2 = new Point((end.X + bsOld.Point1.X) / 2, end.Y);
		}
		private void SetCurveStartPoint(Path p, Point start)
		{
			PathFigure pf = ((PathGeometry)p.Data).Figures[0];
			pf.StartPoint = start;
			BezierSegment bsOld = (BezierSegment)pf.Segments[0];
			bsOld.Point1 = start;
			bsOld.Point2 = new Point((start.X + bsOld.Point3.X) / 2, start.Y);
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


		#region Constants

		private void StartDrawingDialogueNodeCurve(object sender)
		{
			try
			{
				SelectedBlockNode = (DialogueNodeBlock)((Grid)sender).TemplatedParent;
				SelectedNodeRow = Grid.GetRow((Grid)sender);
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
			SelectedBlockNode = (BaseNodeBlock)((Grid)sender).TemplatedParent;
			SelectedNodeRow = Grid.GetRow((Grid)sender);
			if (!((Grid)sender).Name.Contains("Exit"))
				SelectedNode = ((BaseNodeBlock)SelectedBlockNode).OutputNodes[SelectedNodeRow];
			else
				SelectedNode = ((BaseNodeBlock)SelectedBlockNode).ExitNode;
		}

		#endregion

		#region StartBlock
		public void StartDrawingStartBlock(object sender)
		{
			if(((BaseNodeBlock)((Grid)sender).TemplatedParent).ExitNode.ConnectedNodes.Count == 1) return; //ONLY ONE OUTPUT
			SelectedBlockNode = (BaseNodeBlock)((Grid)sender).TemplatedParent;
			SelectedNodeRow = Grid.GetRow((Grid)sender);
			SelectedNode = ((BaseNodeBlock)SelectedBlockNode).ExitNode;

		}
		#endregion

		#region Arithmetic
		private void StartDrawingArithmeticBlock(object sender)
		{
			SelectedBlockNode = (BaseNodeBlock)((Grid)sender).TemplatedParent;
			SelectedNodeRow = Grid.GetRow((Grid)sender);
			SelectedNode = ((BaseArithmeticBlock)SelectedBlockNode).OutValue;
		}
		#endregion

		private void StartDrawingLogicBlock(object sender)
		{
			SelectedBlockNode = (BaseNodeBlock)((Grid)sender).TemplatedParent;
			SelectedNodeRow = Grid.GetRow((Grid)sender);
			SelectedNode = ((BaseLogicNodeBlock)SelectedBlockNode).Output;
		}

		#region Conditional
		private void StartDrawingConditionalBlock(object sender)
		{
			SelectedBlockNode = (BaseNodeBlock)((Grid)sender).TemplatedParent;
			SelectedNodeRow = (((Grid)sender).Name.Contains("False") ? 1 : 0);
			SelectedNode = ((ConditionalNodeBlock)SelectedBlockNode).OutputNodes[SelectedNodeRow];
		}
		#endregion

		#endregion

		#region MoveBlockNodes
		private void MoveThumb_Middle_DragDelta(object sender, DragDeltaEventArgs e)
		{
			object obj = ((Thumb)sender).TemplatedParent;
			if (obj is DialogueNodeBlock dia)
			{
				MoveDialogueBlock(sender, e);
			}
			else if (obj is GetConstantNodeBlock)
			{
				GetConstantNodeBlock BN = ((GetConstantNodeBlock)((Thumb)sender).TemplatedParent);
				Canvas.SetLeft(BN, VisualTreeHelper.GetOffset(BN).X + e.HorizontalChange);
				Canvas.SetTop(BN, VisualTreeHelper.GetOffset(BN).Y + e.VerticalChange);

				Point Start = new Point(Canvas.GetLeft(BN) + 95, Canvas.GetTop(BN) + 40);
				for (int j = 0; j < BN.output.Curves.Count; j++)
					SetCurveStartPoint(BN.output.Curves[j], Start);
			}
			else if (obj is SetConstantNodeBlock)
			{
				MoveConstantBlock(sender, e);
			}
			else if (obj is StartBlockNode)
			{
				StartBlockNode BN = ((StartBlockNode)((Thumb)sender).TemplatedParent);
				Canvas.SetLeft(BN, VisualTreeHelper.GetOffset(BN).X + e.HorizontalChange);
				Canvas.SetTop(BN, VisualTreeHelper.GetOffset(BN).Y + e.VerticalChange);

				Point Start = new Point(Canvas.GetLeft(BN) + 75, Canvas.GetTop(BN) + 30);
				for (int j = 0; j < BN.ExitNode.Curves.Count; j++)
					SetCurveStartPoint(BN.ExitNode.Curves[j], Start);
			}
			else if(obj is ExitBlockNode)
			{
				ExitBlockNode BN = ((ExitBlockNode)((Thumb)sender).TemplatedParent);
				Canvas.SetLeft(BN, VisualTreeHelper.GetOffset(BN).X + e.HorizontalChange);
				Canvas.SetTop(BN, VisualTreeHelper.GetOffset(BN).Y + e.VerticalChange);

				Point Start = new Point(Canvas.GetLeft(BN), Canvas.GetTop(BN) + 30);
				for (int j = 0; j < BN.EntryNode.Curves.Count; j++)
					SetCurveEndPoint(BN.EntryNode.Curves[j], Start);
			}
			else if (obj is ConditionalNodeBlock)
			{
				MoveConditionalBlock(sender, e);
			}
			else if(obj is BaseArithmeticBlock)
			{
				MoveArithmeticBlock(sender, e);
			}
			else if(obj is BaseLogicNodeBlock)
			{
				MoveLogicBlock(sender, e);
			}
		}

		private void MoveDialogueBlock(object sender, DragDeltaEventArgs e)
		{
			DialogueNodeBlock BN = ((DialogueNodeBlock)((Thumb)sender).TemplatedParent);
			Canvas.SetLeft(BN, VisualTreeHelper.GetOffset(BN).X + e.HorizontalChange);
			Canvas.SetTop(BN, VisualTreeHelper.GetOffset(BN).Y + e.VerticalChange);

			if (NodeEditor_BackCanvas.Children.Count > 1)
			{
				//move all inputs
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
							Point end1 = end; end1.Y += 20 + (40 * (i)) + 20;
							SetCurveEndPoint(BN.InputNodes[i].Curves[j], end1);
						}
					}
				}

				//move all curves to the entry node
				for (int j = 0; j < BN.EntryNode.Curves.Count; j++)
				{
					Point end = new Point(Canvas.GetLeft(BN), Canvas.GetTop(BN)); //end.Y +=(10); //the 10 is middle of circle
					Point end1 = end; end1.Y += 10;
					SetCurveEndPoint(BN.EntryNode.Curves[j], end1);
				}


				if (BN.OutputNodes.Count > 0) //move the "left most" node
				{
					for (int i = 0; i < BN.OutputNodes.Count; i++)
					{
						Point start = new Point(Canvas.GetLeft(BN) + 150, Canvas.GetTop(BN)); start.Y += 20 + (40 *
							(BN.InputNodes.Count - 1 < 0 ? 0 : (BN.InputNodes.Count))) + (40 * i) + 20; //the 20 is middle of circle
						for (int j = 0; j < BN.OutputNodes[i].Curves.Count; j++)
							SetCurveStartPoint(BN.OutputNodes[i].Curves[j], start);
						//Point end = new Point(Canvas.GetLeft(BN), Canvas.GetTop(BN)); end.Y += 40 + (10); //the 10 is middle of circle
						//SetCurveEndPoint((Path)NodeEditor_BackCanvas.Children[1], end);
					}
				}
			}
		}

		private void MoveConstantBlock(object sender, DragDeltaEventArgs e)
		{
			SetConstantNodeBlock BN = ((SetConstantNodeBlock)((Thumb)sender).TemplatedParent);
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
							Point end1 = end; end1.Y += 20 + (30 * (i)) + 15;
							SetCurveEndPoint(BN.InputNodes[i].Curves[j], end1);
						}
					}
				}

				//move all curves to the entry node
				for (int j = 0; j < BN.EntryNode.Curves.Count; j++)
				{
					Point end = new Point(Canvas.GetLeft(BN), Canvas.GetTop(BN)); //end.Y +=(10); //the 10 is middle of circle
					Point end1 = end; end1.Y += 10;
					SetCurveEndPoint(BN.EntryNode.Curves[j], end1);
				}

				//move all curves to the exit node
				for (int j = 0; j < BN.ExitNode.Curves.Count; j++)
				{
					Point end = new Point(Canvas.GetLeft(BN), Canvas.GetTop(BN)); //end.Y +=(10); //the 10 is middle of circle
					Point end1 = end; end1.X += 150; end1.Y += 10;
					SetCurveStartPoint(BN.ExitNode.Curves[j], end1);
				}


				if (BN.OutputNodes.Count > 0) //move the "left most" node
				{
					for (int i = 0; i < BN.OutputNodes.Count; i++)
					{
						Point start = new Point(Canvas.GetLeft(BN) + 150, Canvas.GetTop(BN)); start.Y += 20 + 15;
						for (int j = 0; j < BN.OutputNodes[i].Curves.Count; j++)
							SetCurveStartPoint(BN.OutputNodes[i].Curves[j], start);
						//Point end = new Point(Canvas.GetLeft(BN), Canvas.GetTop(BN)); end.Y += 40 + (10); //the 10 is middle of circle
						//SetCurveEndPoint((Path)NodeEditor_BackCanvas.Children[1], end);
					}
				}
			}
		}

		private void MoveArithmeticBlock(object sender, DragDeltaEventArgs e)
		{
			BaseArithmeticBlock BN = ((BaseArithmeticBlock)((Thumb)sender).TemplatedParent);
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
							Point end1 = end; end1.Y += 20 + (30 * (i)) + 15;
							SetCurveEndPoint(BN.InputNodes[i].Curves[j], end1);
						}
					}
				}

				if (BN.OutputNodes.Count > 0) //move the "left most" node
				{
					for (int i = 0; i < BN.OutputNodes.Count; i++)
					{
						Point start = new Point(Canvas.GetLeft(BN) + 150, Canvas.GetTop(BN)); start.Y += 20 + 15;
						for (int j = 0; j < BN.OutputNodes[i].Curves.Count; j++)
							SetCurveStartPoint(BN.OutputNodes[i].Curves[j], start);
						//Point end = new Point(Canvas.GetLeft(BN), Canvas.GetTop(BN)); end.Y += 40 + (10); //the 10 is middle of circle
						//SetCurveEndPoint((Path)NodeEditor_BackCanvas.Children[1], end);
					}
				}
			}
		}

		private void MoveLogicBlock(object sender, DragDeltaEventArgs e)
		{
			BaseLogicNodeBlock BN = ((BaseLogicNodeBlock)((Thumb)sender).TemplatedParent);
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
							Point end1 = end; end1.Y += 20 + (30 * (i)) + 15;
							SetCurveEndPoint(BN.InputNodes[i].Curves[j], end1);
						}
					}
				}

				if (BN.OutputNodes.Count > 0) //move the "left most" node
				{
					for (int i = 0; i < BN.OutputNodes.Count; i++)
					{
						Point start = new Point(Canvas.GetLeft(BN) + 150, Canvas.GetTop(BN)); start.Y += 20 + 15;
						for (int j = 0; j < BN.OutputNodes[i].Curves.Count; j++)
							SetCurveStartPoint(BN.OutputNodes[i].Curves[j], start);
						//Point end = new Point(Canvas.GetLeft(BN), Canvas.GetTop(BN)); end.Y += 40 + (10); //the 10 is middle of circle
						//SetCurveEndPoint((Path)NodeEditor_BackCanvas.Children[1], end);
					}
				}
			}
		}

		private void MoveConditionalBlock(object sender, DragDeltaEventArgs e)
		{
			ConditionalNodeBlock BN = ((ConditionalNodeBlock)((Thumb)sender).TemplatedParent);
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
							Point end1 = end; end1.Y += 20 + (30 * (i)) + 15;
							SetCurveEndPoint(BN.InputNodes[i].Curves[j], end1);
						}
					}
				}

				//move all curves to the entry node
				for (int j = 0; j < BN.EntryNode.Curves.Count; j++)
				{
					Point end = new Point(Canvas.GetLeft(BN), Canvas.GetTop(BN)); //end.Y +=(10); //the 10 is middle of circle
					Point end1 = end; end1.Y += 10;
					SetCurveEndPoint(BN.EntryNode.Curves[j], end1);
				}

				if (BN.OutputNodes.Count > 0) //move the "left most" node
				{
					for (int i = 0; i < BN.OutputNodes.Count; i++)
					{
						Point start = new Point(Canvas.GetLeft(BN) + 150, Canvas.GetTop(BN)); start.Y += 20 + (30 * (i)) + 15;
						for (int j = 0; j < BN.OutputNodes[i].Curves.Count; j++)
							SetCurveStartPoint(BN.OutputNodes[i].Curves[j], start);
						//Point end = new Point(Canvas.GetLeft(BN), Canvas.GetTop(BN)); end.Y += 40 + (10); //the 10 is middle of circle
						//SetCurveEndPoint((Path)NodeEditor_BackCanvas.Children[1], end);
					}
				}
			}
		}


		#endregion

		#region Connecting

		/// <summary>
		/// This method is here for drawing AND connecting the selected to node to the current node. (Input) So output -> input
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MoveThumb_Left_PreviewMouseMove(object sender, MouseEventArgs e)
		{
			Console.WriteLine("MouseMoving in Input");
			if (isMDown)
			{
				object obj = null;
				obj = ((Grid)sender).TemplatedParent;
				if (obj is null)
					obj = ((Grid)((Grid)sender).Parent).TemplatedParent; //the input nodes are added to style templated ON RUNTIME

				if (obj is DialogueNodeBlock)
					EndDrawingCurveDialogueBlock(sender);
				else if (obj is GetConstantNodeBlock)
				{
					EndDrawingCurveConstantBlock(sender);
				}
				else if (obj is SetConstantNodeBlock)
				{
					EndDrawingCurveConstantBlock(sender);
				}
				else if(obj is ExitBlockNode)
				{
					EndDrawingCurveExitBlock(sender);
				}
				else if(obj is BaseArithmeticBlock)
				{
					EndDrawingCurveArithmeticBlock(sender);
				}
				else if(obj is BaseLogicNodeBlock)
				{
					EndDrawingCurveLogicBlock(sender);
				}
				else if(obj is ConditionalNodeBlock)
				{
					EndDrawingCurveConditionalBlock(sender);
				}
			}
		}

		#region Dialogue

		private void EndDrawingCurveDialogueBlock(object sender)
		{
			if (SelectedBlockNode == null) return;
			BaseNodeBlock BN = null; int inrow = 0;

			BN = (BaseNodeBlock)((Grid)sender).TemplatedParent;
			inrow = Grid.GetRow(((Grid)sender)); //the one offset is for the enter node.
			if (BN is null)
			{
				BN = (BaseNodeBlock)((Grid)((Grid)sender).Parent).TemplatedParent;
				inrow = Grid.GetRow(((Grid)sender)); //the one offset is for the enter node.
			}
			if (BN == null) return;
			//get paths
			Point end = Mouse.GetPosition(NodeEditor_BackCanvas);
			Path p = CreateBezierCurve(CurveStart, end);
			//do the types match?
			if (SelectedBlockNode is DialogueNodeBlock)
			{
				//types match check
				if (SelectedNode.NodeType != ECOnnectionType.Exit)
				{
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
				else
				{
					//Draw the curve
					NodeEditor_BackCanvas.Children.Add(p);
					isMDown = false;

					BN.EntryNode.ConnectedNodes.Add(SelectedNode); BN.EntryNode.Curves.Add(p);
					((DialogueNodeBlock)SelectedBlockNode).OutputNodes[SelectedNodeRow].ConnectedNodes.Add(BN.EntryNode);
					((DialogueNodeBlock)SelectedBlockNode).OutputNodes[SelectedNodeRow].Curves.Add(p);
				}
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
			else if (SelectedBlockNode is SetConstantNodeBlock)
			{
				if (SelectedNode.NodeType == ECOnnectionType.Exit && ((Grid)sender).Name.Contains("Entry"))
				{
					//the types are matching
					NodeEditor_BackCanvas.Children.Add(p);
					isMDown = false;

					BN.EntryNode.ConnectedNodes.Add(SelectedNode); BN.EntryNode.Curves.Add(p);
					SelectedNode.ConnectedNodes.Add(BN.EntryNode); SelectedNode.Curves.Add(p);
				}
				else if (SelectedNode.NodeType != ECOnnectionType.Exit)
				{
					if (SelectedNode.NodeType != BN.InputNodes[inrow].NodeType) return;
					//the types are matching
					NodeEditor_BackCanvas.Children.Add(p);
					isMDown = false;

					BN.InputNodes[inrow].ConnectedNodes.Add(SelectedNode); BN.InputNodes[inrow].Curves.Add(p);
					SelectedNode.ConnectedNodes.Add(BN.InputNodes[inrow]); SelectedNode.Curves.Add(p);
				}
			}
			else if (SelectedBlockNode is StartBlockNode)
			{
				if (SelectedNode.NodeType == ECOnnectionType.Exit && ((Grid)sender).Name.Contains("Entry"))
				{
					NodeEditor_BackCanvas.Children.Add(p);
					isMDown = false;

					BN.EntryNode.ConnectedNodes.Add(SelectedNode); BN.EntryNode.Curves.Add(p);
					((StartBlockNode)SelectedBlockNode).ExitNode.ConnectedNodes.Add(BN.EntryNode);
					((StartBlockNode)SelectedBlockNode).ExitNode.Curves.Add(p);
				}
			}
			else if (SelectedBlockNode is BaseArithmeticBlock)
			{
				//CAN ONLY BE A DATA TYPE
				if (SelectedNode.NodeType != BN.InputNodes[inrow].NodeType) return; //it doesn't match

				NodeEditor_BackCanvas.Children.Add(p);
				isMDown = false;

				BN.InputNodes[inrow].ConnectedNodes.Add(SelectedNode); BN.InputNodes[inrow].Curves.Add(p);
				((BaseArithmeticBlock)SelectedBlockNode).OutValue.ConnectedNodes.Add(BN.InputNodes[inrow]);
				((BaseArithmeticBlock)SelectedBlockNode).OutValue.Curves.Add(p);
			}
			else if (SelectedBlockNode is BaseLogicNodeBlock)
			{
				//CAN ONLY BE A DATA TYPE
				if (SelectedNode.NodeType != BN.InputNodes[inrow].NodeType) return; //it doesn't match

				NodeEditor_BackCanvas.Children.Add(p);
				isMDown = false;

				BN.InputNodes[inrow].ConnectedNodes.Add(SelectedNode); BN.InputNodes[inrow].Curves.Add(p);
				((BaseLogicNodeBlock)SelectedBlockNode).Output.ConnectedNodes.Add(BN.InputNodes[inrow]);
				((BaseLogicNodeBlock)SelectedBlockNode).Output.Curves.Add(p);
			}
			else if (SelectedBlockNode is ConditionalNodeBlock)
			{
				if (SelectedNode.NodeType == ECOnnectionType.Exit && ((Grid)sender).Name.Contains("Entry"))//WE KNOW IT MUST AN BE AN EXIT
				{
					NodeEditor_BackCanvas.Children.Add(p);
					isMDown = false;

					BN.EntryNode.ConnectedNodes.Add(SelectedNode); BN.EntryNode.Curves.Add(p);
					((ConditionalNodeBlock)SelectedBlockNode).OutputNodes[SelectedNodeRow].ConnectedNodes.Add(BN.EntryNode);
					((ConditionalNodeBlock)SelectedBlockNode).OutputNodes[SelectedNodeRow].Curves.Add(p);
				}
			}
		}
		#endregion

		#region ExitBlock
		private void EndDrawingCurveExitBlock(object sender)
		{
			
			if (SelectedBlockNode == null || SelectedNode == null) return;
			if (SelectedNode.NodeType != ECOnnectionType.Exit) return; //ONLY ALLOW EXIT NODES

			Point end = Mouse.GetPosition(NodeEditor_BackCanvas);
			Path p = CreateBezierCurve(CurveStart, end);

			ExitBlockNode BN = null; BN = (ExitBlockNode)((Grid)sender).TemplatedParent;
			SelectedNode.ConnectedNodes.Add(BN.EntryNode); SelectedNode.Curves.Add(p);
			BN.EntryNode.ConnectedNodes.Add(SelectedNode); BN.EntryNode.Curves.Add(p);
			NodeEditor_BackCanvas.Children.Add(p);
			isMDown = false;
		}
		#endregion

		#region Constants
		private void EndDrawingCurveConstantBlock(object sender)
		{
			Console.WriteLine("entered the set node inputs");
			if (SelectedBlockNode == null || SelectedNode == null) return;
			BaseNodeBlock BN = null; int inrow = 0;

			BN = (BaseNodeBlock)((Grid)sender).TemplatedParent;
			inrow = Grid.GetRow(((Grid)sender)); //the one offset is for the enter node.
			if (BN is null)
			{
				BN = (BaseNodeBlock)((Grid)((Grid)sender).Parent).TemplatedParent;
				inrow = Grid.GetRow(((Grid)sender)); //the one offset is for the enter node.
			}
			if (BN == null) return;
			Point end = Mouse.GetPosition(NodeEditor_BackCanvas);
			Path p = CreateBezierCurve(CurveStart, end);
			//what is the previous selected node
			if (SelectedBlockNode is DialogueNodeBlock)
			{//IT CAN ONLY BE AN EXIT NODE
				if (SelectedNode.NodeType != ECOnnectionType.Exit) return;
				//Draw the curve
				NodeEditor_BackCanvas.Children.Add(p);
				isMDown = false;

				BN.EntryNode.ConnectedNodes.Add(SelectedNode); BN.EntryNode.Curves.Add(p);
				((DialogueNodeBlock)SelectedBlockNode).OutputNodes[SelectedNodeRow].ConnectedNodes.Add(BN.EntryNode);
				((DialogueNodeBlock)SelectedBlockNode).OutputNodes[SelectedNodeRow].Curves.Add(p);
			}
			else if (SelectedBlockNode is SetConstantNodeBlock)
			{
				//types match check
				if (SelectedNode.NodeType != ECOnnectionType.Exit)
				{
					if (((SetConstantNodeBlock)SelectedBlockNode).OutputNodes[SelectedNodeRow].NodeType != BN.InputNodes[inrow].NodeType)
					{
						if (((SetConstantNodeBlock)SelectedBlockNode).OutputNodes[SelectedNodeRow].NodeType == ECOnnectionType.Exit && BN.InputNodes[inrow].NodeType == ECOnnectionType.Enter) { } //this is just an ignore.
						else if (((SetConstantNodeBlock)SelectedBlockNode).OutputNodes[SelectedNodeRow].NodeType != BN.InputNodes[inrow].NodeType) return;
					}
					//Draw the curve
					NodeEditor_BackCanvas.Children.Add(p);
					isMDown = false;

					BN.InputNodes[inrow].ConnectedNodes.Add(SelectedNode); BN.InputNodes[inrow].Curves.Add(p);
					((SetConstantNodeBlock)SelectedBlockNode).OutputNodes[SelectedNodeRow].ConnectedNodes.Add(BN.InputNodes[inrow]);
					((SetConstantNodeBlock)SelectedBlockNode).OutputNodes[SelectedNodeRow].Curves.Add(p);
				}
				else
				{
					//Draw the curve
					NodeEditor_BackCanvas.Children.Add(p);
					isMDown = false;

					BN.EntryNode.ConnectedNodes.Add(SelectedNode); BN.EntryNode.Curves.Add(p);
					((SetConstantNodeBlock)SelectedBlockNode).OutputNodes[SelectedNodeRow].ConnectedNodes.Add(BN.EntryNode);
					((SetConstantNodeBlock)SelectedBlockNode).OutputNodes[SelectedNodeRow].Curves.Add(p);
				}
			}
			else if (SelectedBlockNode is GetConstantNodeBlock)
			{
				//CAN ONLY BE A DATA TYPE
				if (SelectedNode.NodeType != BN.InputNodes[inrow].NodeType) return; //it doesn't match
																																						//Draw the curve
				NodeEditor_BackCanvas.Children.Add(p);
				isMDown = false;

				BN.InputNodes[inrow].ConnectedNodes.Add(SelectedNode); BN.InputNodes[inrow].Curves.Add(p);
				((GetConstantNodeBlock)SelectedBlockNode).output.ConnectedNodes.Add(BN.InputNodes[inrow]);
				((GetConstantNodeBlock)SelectedBlockNode).output.Curves.Add(p);

			}
			else if(SelectedBlockNode is StartBlockNode)
			{
				if(SelectedNode.NodeType == ECOnnectionType.Exit && ((Grid)sender).Name.Contains("Entry"))
				{
					NodeEditor_BackCanvas.Children.Add(p);
					isMDown = false;

					BN.EntryNode.ConnectedNodes.Add(SelectedNode); BN.EntryNode.Curves.Add(p);
					((StartBlockNode)SelectedBlockNode).ExitNode.ConnectedNodes.Add(BN.EntryNode);
					((StartBlockNode)SelectedBlockNode).ExitNode.Curves.Add(p);
				}
			}
			else if (SelectedBlockNode is BaseArithmeticBlock)
			{
				//CAN ONLY BE A DATA TYPE
				if (SelectedNode.NodeType != BN.InputNodes[inrow].NodeType) return; //it doesn't match

				NodeEditor_BackCanvas.Children.Add(p);
				isMDown = false;

				BN.InputNodes[inrow].ConnectedNodes.Add(SelectedNode); BN.InputNodes[inrow].Curves.Add(p);
				((BaseArithmeticBlock)SelectedBlockNode).OutValue.ConnectedNodes.Add(BN.InputNodes[inrow]);
				((BaseArithmeticBlock)SelectedBlockNode).OutValue.Curves.Add(p);
			}
			else if (SelectedBlockNode is BaseLogicNodeBlock) //We can also chain together the same node type. Again only outputval.
			{
				//the data types MUST match!
				if (SelectedNode.NodeType == ECOnnectionType.Bool)
				{
					NodeEditor_BackCanvas.Children.Add(p);
					isMDown = false;

					SelectedNode.ConnectedNodes.Add(BN.InputNodes[inrow]); SelectedNode.Curves.Add(p);
					BN.InputNodes[inrow].ConnectedNodes.Add(SelectedNode); BN.InputNodes[inrow].Curves.Add(p);
				}
			}
			else if (SelectedBlockNode is ConditionalNodeBlock)
			{
				if (SelectedNode.NodeType == ECOnnectionType.Exit && ((Grid)sender).Name.Contains("Entry"))//WE KNOW IT MUST AN BE AN EXIT
				{
					NodeEditor_BackCanvas.Children.Add(p);
					isMDown = false;

					BN.EntryNode.ConnectedNodes.Add(SelectedNode); BN.EntryNode.Curves.Add(p);
					((ConditionalNodeBlock)SelectedBlockNode).OutputNodes[SelectedNodeRow].ConnectedNodes.Add(BN.EntryNode);
					((ConditionalNodeBlock)SelectedBlockNode).OutputNodes[SelectedNodeRow].Curves.Add(p);
				}
			}
			if (BN.InputNodes[1].ConnectedNodes.Count > 0 && BN is SetConstantNodeBlock)
				(BN as SetConstantNodeBlock).NewValConnected = true;

		}
		#endregion

		#region Arithmetic
		private void EndDrawingCurveArithmeticBlock(object sender)
		{
			Console.WriteLine("entered the set node inputs");
			if (SelectedBlockNode == null || SelectedNode == null) return;
			BaseNodeBlock BN = null; int inrow = 0;

			BN = (BaseNodeBlock)((Grid)sender).TemplatedParent;
			inrow = Grid.GetRow(((Grid)sender)); //the one offset is for the enter node.
			if (BN is null)
			{
				BN = (BaseNodeBlock)((Grid)((Grid)sender).Parent).TemplatedParent;
				inrow = Grid.GetRow(((Grid)sender)); //the one offset is for the enter node.
			}
			if (BN == null) return;
			Point end = Mouse.GetPosition(NodeEditor_BackCanvas);
			Path p = CreateBezierCurve(CurveStart, end);
			//what is the previous selected node
			if (SelectedBlockNode is GetConstantNodeBlock) //Get constant can be a int value out.
			{
				//the data types MUST match!
				if(SelectedNode.NodeType == ECOnnectionType.Int)
				{
					NodeEditor_BackCanvas.Children.Add(p);
					isMDown = false;

					SelectedNode.ConnectedNodes.Add(BN.InputNodes[inrow]); SelectedNode.Curves.Add(p);
					BN.InputNodes[inrow].ConnectedNodes.Add(SelectedNode); BN.InputNodes[inrow].Curves.Add(p);
				}
			}
			else if(SelectedBlockNode is SetConstantNodeBlock) //same with set constant BUT ONLY the outputVal.
			{
				//the data types MUST match!
				if (SelectedNode.NodeType == ECOnnectionType.Int)
				{
					NodeEditor_BackCanvas.Children.Add(p);
					isMDown = false;

					SelectedNode.ConnectedNodes.Add(BN.InputNodes[inrow]); SelectedNode.Curves.Add(p);
					BN.InputNodes[inrow].ConnectedNodes.Add(SelectedNode); BN.InputNodes[inrow].Curves.Add(p);
				}
			}
			else if(SelectedBlockNode is BaseArithmeticBlock) //We can also chain together the same node type. Again only outputval.
			{
				//the data types MUST match!
				if (SelectedNode.NodeType == ECOnnectionType.Int)
				{
					NodeEditor_BackCanvas.Children.Add(p);
					isMDown = false;

					SelectedNode.ConnectedNodes.Add(BN.InputNodes[inrow]); SelectedNode.Curves.Add(p);
					BN.InputNodes[inrow].ConnectedNodes.Add(SelectedNode); BN.InputNodes[inrow].Curves.Add(p);
				}
			}
			if (BN.InputNodes[1].ConnectedNodes.Count > 0 && BN is BaseArithmeticBlock)
				(BN as BaseArithmeticBlock).NewValConnected = true;
		}
		#endregion

		#region Logic
		private void EndDrawingCurveLogicBlock(object sender)
		{
			Console.WriteLine("entered the set node inputs");
			if (SelectedBlockNode == null || SelectedNode == null) return;
			BaseNodeBlock BN = null; int inrow = 0;

			BN = (BaseNodeBlock)((Grid)sender).TemplatedParent;
			inrow = Grid.GetRow(((Grid)sender)); //the one offset is for the enter node.
			if (BN is null)
			{
				BN = (BaseNodeBlock)((Grid)((Grid)sender).Parent).TemplatedParent;
				inrow = Grid.GetRow(((Grid)sender)); //the one offset is for the enter node.
			}
			if (BN == null) return;
			Point end = Mouse.GetPosition(NodeEditor_BackCanvas);
			Path p = CreateBezierCurve(CurveStart, end);
			//what is the previous selected node
			if (SelectedBlockNode is GetConstantNodeBlock) //Get constant can be a int value out.
			{
				//the data types MUST match!
				if (SelectedNode.NodeType == ECOnnectionType.Bool)
				{
					NodeEditor_BackCanvas.Children.Add(p);
					isMDown = false;

					SelectedNode.ConnectedNodes.Add(BN.InputNodes[inrow]); SelectedNode.Curves.Add(p);
					BN.InputNodes[inrow].ConnectedNodes.Add(SelectedNode); BN.InputNodes[inrow].Curves.Add(p);
				}
			}
			else if (SelectedBlockNode is SetConstantNodeBlock) //same with set constant BUT ONLY the outputVal.
			{
				//the data types MUST match!
				if (SelectedNode.NodeType == ECOnnectionType.Bool)
				{
					NodeEditor_BackCanvas.Children.Add(p);
					isMDown = false;

					SelectedNode.ConnectedNodes.Add(BN.InputNodes[inrow]); SelectedNode.Curves.Add(p);
					BN.InputNodes[inrow].ConnectedNodes.Add(SelectedNode); BN.InputNodes[inrow].Curves.Add(p);
				}
			}
			else if (SelectedBlockNode is BaseLogicNodeBlock) //We can also chain together the same node type. Again only outputval.
			{
				//the data types MUST match!
				if (SelectedNode.NodeType == ECOnnectionType.Bool)
				{
					NodeEditor_BackCanvas.Children.Add(p);
					isMDown = false;

					SelectedNode.ConnectedNodes.Add(BN.InputNodes[inrow]); SelectedNode.Curves.Add(p);
					BN.InputNodes[inrow].ConnectedNodes.Add(SelectedNode); BN.InputNodes[inrow].Curves.Add(p);
				}
			}
			if (BN.InputNodes[1].ConnectedNodes.Count > 0 && BN is BaseLogicNodeBlock)
				(BN as BaseLogicNodeBlock).NewValConnected = true;
		}
		#endregion

		#region Conditionals
		private void EndDrawingCurveConditionalBlock(object sender)
		{
			Console.WriteLine("entered the set node inputs");
			if (SelectedBlockNode == null || SelectedNode == null) return;
			BaseNodeBlock BN = null; int inrow = 0;

			BN = (BaseNodeBlock)((Grid)sender).TemplatedParent;
			inrow = Grid.GetRow(((Grid)sender)); //the one offset is for the enter node.
			if (BN is null)
			{
				BN = (BaseNodeBlock)((Grid)((Grid)sender).Parent).TemplatedParent;
				inrow = Grid.GetRow(((Grid)sender)); //the one offset is for the enter node.
			}
			if (BN == null) return;
			Point end = Mouse.GetPosition(NodeEditor_BackCanvas);
			Path p = CreateBezierCurve(CurveStart, end);
			//what is the previous selected node
			if (SelectedBlockNode is DialogueNodeBlock)
			{
				//types match check
				if (SelectedNode.NodeType != ECOnnectionType.Exit)
				{
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
				else
				{
					//Draw the curve
					NodeEditor_BackCanvas.Children.Add(p);
					isMDown = false;

					BN.EntryNode.ConnectedNodes.Add(SelectedNode); BN.EntryNode.Curves.Add(p);
					((DialogueNodeBlock)SelectedBlockNode).OutputNodes[SelectedNodeRow].ConnectedNodes.Add(BN.EntryNode);
					((DialogueNodeBlock)SelectedBlockNode).OutputNodes[SelectedNodeRow].Curves.Add(p);
				}
			}
			else if (SelectedBlockNode is GetConstantNodeBlock)
			{
				//CAN ONLY BE A DATA TYPE
				if (SelectedNode.NodeType != BN.InputNodes[inrow].NodeType) return; //it doesn't match
																																						//Draw the curve
				NodeEditor_BackCanvas.Children.Add(p);
				isMDown = false;

				BN.InputNodes[inrow].ConnectedNodes.Add(SelectedNode); BN.InputNodes[inrow].Curves.Add(p);
				((GetConstantNodeBlock)SelectedBlockNode).output.ConnectedNodes.Add(BN.InputNodes[inrow]);
				((GetConstantNodeBlock)SelectedBlockNode).output.Curves.Add(p);

			}
			else if (SelectedBlockNode is SetConstantNodeBlock)
			{
				if(SelectedNode.NodeType == ECOnnectionType.Exit)
				{
					NodeEditor_BackCanvas.Children.Add(p);
					isMDown = false;

					BN.EntryNode.ConnectedNodes.Add(SelectedNode); BN.EntryNode.Curves.Add(p);
					((SetConstantNodeBlock)SelectedBlockNode).ExitNode.ConnectedNodes.Add(BN.EntryNode);
					((SetConstantNodeBlock)SelectedBlockNode).ExitNode.Curves.Add(p);
				}
				else
				{
					if (SelectedNode.NodeType != BN.InputNodes[inrow].NodeType) return; //it doesn't match
					NodeEditor_BackCanvas.Children.Add(p);
					isMDown = false;

					BN.InputNodes[inrow].ConnectedNodes.Add(SelectedNode); BN.InputNodes[inrow].Curves.Add(p);
					((SetConstantNodeBlock)SelectedBlockNode).OutValue.ConnectedNodes.Add(BN.InputNodes[inrow]);
					((SetConstantNodeBlock)SelectedBlockNode).OutValue.Curves.Add(p);
				}
			}
			else if (SelectedBlockNode is StartBlockNode)
			{
				if (SelectedNode.NodeType == ECOnnectionType.Exit && ((Grid)sender).Name.Contains("Entry"))
				{
					NodeEditor_BackCanvas.Children.Add(p);
					isMDown = false;

					BN.EntryNode.ConnectedNodes.Add(SelectedNode); BN.EntryNode.Curves.Add(p);
					((StartBlockNode)SelectedBlockNode).ExitNode.ConnectedNodes.Add(BN.EntryNode);
					((StartBlockNode)SelectedBlockNode).ExitNode.Curves.Add(p);
				}
			}
			else if (SelectedBlockNode is BaseArithmeticBlock)
			{
				//CAN ONLY BE A DATA TYPE
				if (SelectedNode.NodeType != BN.InputNodes[inrow].NodeType) return; //it doesn't match

				NodeEditor_BackCanvas.Children.Add(p);
				isMDown = false;

				BN.InputNodes[inrow].ConnectedNodes.Add(SelectedNode); BN.InputNodes[inrow].Curves.Add(p);
				((BaseArithmeticBlock)SelectedBlockNode).OutValue.ConnectedNodes.Add(BN.InputNodes[inrow]);
				((BaseArithmeticBlock)SelectedBlockNode).OutValue.Curves.Add(p);
			}
			else if (SelectedBlockNode is BaseLogicNodeBlock) //We can also chain together the same node type. Again only outputval.
			{
				//the data types MUST match!
				if (SelectedNode.NodeType == ECOnnectionType.Bool)
				{
					NodeEditor_BackCanvas.Children.Add(p);
					isMDown = false;

					SelectedNode.ConnectedNodes.Add(BN.InputNodes[inrow]); SelectedNode.Curves.Add(p);
					BN.InputNodes[inrow].ConnectedNodes.Add(SelectedNode); BN.InputNodes[inrow].Curves.Add(p);
				}
			}
			else if (SelectedBlockNode is ConditionalNodeBlock)
			{
				if (SelectedNode.NodeType == ECOnnectionType.Exit && ((Grid)sender).Name.Contains("Entry"))//WE KNOW IT MUST AN BE AN EXIT
				{
					NodeEditor_BackCanvas.Children.Add(p);
					isMDown = false;

					BN.EntryNode.ConnectedNodes.Add(SelectedNode); BN.EntryNode.Curves.Add(p);
					((ConditionalNodeBlock)SelectedBlockNode).OutputNodes[SelectedNodeRow].ConnectedNodes.Add(BN.EntryNode);
					((ConditionalNodeBlock)SelectedBlockNode).OutputNodes[SelectedNodeRow].Curves.Add(p);
				}
			}
			if (BN.InputNodes[1].ConnectedNodes.Count > 0 && BN is SetConstantNodeBlock)
				(BN as SetConstantNodeBlock).NewValConnected = true;

		}
		#endregion

		#endregion

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

		private void AddDialogueRow_BTN_Click(object sender, RoutedEventArgs e)
		{

		}

		/// <summary>
		/// This method sets the reference, and drawing position of the FIRST NODE. or the "Output" of a Output->Input relationship
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MoveThumb_Right_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			Console.WriteLine("Mouse Down on Output");
			isMDown = true;
			CurveStart = Mouse.GetPosition(NodeEditor_BackCanvas);

			object obj = null;
			obj = ((Grid)sender).TemplatedParent;
			if (obj is null)
				obj = ((Grid)((Grid)sender).Parent).TemplatedParent; //the input nodes are added to style templated ON RUNTIME

			if (obj is DialogueNodeBlock)
				StartDrawingDialogueNodeCurve(sender);
			else if (obj is GetConstantNodeBlock)
				StartDrawingConstantNode(sender);
			else if (obj is SetConstantNodeBlock)
				StartDrawingConstantNode(sender);
			else if (obj is StartBlockNode)
				StartDrawingStartBlock(sender);
			else if (obj is ConditionalNodeBlock)
				StartDrawingConditionalBlock(sender);
			else if (obj is BaseArithmeticBlock)
				StartDrawingArithmeticBlock(sender);
			else if (obj is BaseLogicNodeBlock)
				StartDrawingLogicBlock(sender);

		}

		private void MoveThumb_Left_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			Console.WriteLine("Mouse Up on Input");
		}

		private void MoveThumb_Left_PreviewDragEnter(object sender, DragEventArgs e)
		{
			Console.WriteLine("Mouse Dragged into Input");
		}

		private void AddDialogueOutput_BTN_Click(object sender, RoutedEventArgs e)
		{
			Grid basegrid = (Grid)((Button)sender).Parent;
			Grid OutputGrid = null;

			DialogueNodeBlock BN = (DialogueNodeBlock)((Button)sender).TemplatedParent;
			BN.DialogueData.Add("Memes_");

			foreach (UIElement item in basegrid.Children)
			{
				if (item is Grid && ((Grid)item).Name.Contains("Output"))
				{ OutputGrid = item as Grid; break; }
			}
			//add the dialouge textblock
			OutputGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(40) });
			TextBlock tb = new TextBlock()
			{
				Margin = new Thickness(5),
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top,
				Foreground = Brushes.White
			};
			Binding myBinding = new Binding();
			myBinding.Source = BN;
			myBinding.Path = new PropertyPath(String.Format("DialogueData[{0}]", OutputGrid.RowDefinitions.Count - 1));
			//myBinding.RelativeSource= RelativeSource.TemplatedParent;
			myBinding.Mode = BindingMode.TwoWay;
			myBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

			tb.SetBinding(TextBlock.TextProperty, myBinding);


			Grid.SetRow(tb, OutputGrid.RowDefinitions.Count - 1); Grid.SetColumn(tb, 1);
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
			g.PreviewMouseRightButtonDown += COnnectionNode_RightClick;
			g.Name = "OutputNode" + OutputGrid.RowDefinitions.Count;
			OutputGrid.Children.Add(g);

			//add the output node data wise
			Point p = new Point(Canvas.GetLeft(BN) + 150, Canvas.GetTop(BN) + 20 + (20 * OutputGrid.RowDefinitions.Count));
			BN.OutputNodes.Add(new ConnectionNode(BN, "OutputNode" + OutputGrid.RowDefinitions.Count, p, ECOnnectionType.Exit));


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
			gg.PreviewMouseRightButtonDown += COnnectionNode_RightClick;
			gg.Name = "InputNode" + inputGrid.RowDefinitions.Count;
			inputGrid.Children.Add(gg);

			//Add the combo box to tell/change what data type is allow on this node
			ComboBox cb = new ComboBox() { IsEnabled = false, HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center };
			cb.Items.Add("Bool"); cb.Items.Add("Int");
			cb.SelectedIndex = 1;
			Grid.SetRow(cb, inputGrid.RowDefinitions.Count - 1); Grid.SetColumn(cb, 2);
			inputGrid.Children.Add(cb);

			//add the Input node data wise
			Point pp = new Point(Canvas.GetLeft(BN) + 150, Canvas.GetTop(BN) + 20 + (20 * inputGrid.RowDefinitions.Count));
			BN.InputNodes.Add(new ConnectionNode(BN, "InputNode" + inputGrid.RowDefinitions.Count, pp, ECOnnectionType.Int));
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
			g.PreviewMouseRightButtonDown += COnnectionNode_RightClick;
			g.Name = "InputNode" + inputGrid.RowDefinitions.Count;
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
			BN.InputNodes.Add(new ConnectionNode(BN, "InputNode" + inputGrid.RowDefinitions.Count, p, ECOnnectionType.Bool));

		}

		/// <summary>
		/// THis method is activated when the user changed the input type for a dialogue block node.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void DialogueInput_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			//change the circle color display wise

			//first find the circle
			Grid nodegrid = ((Grid)((ComboBox)sender).Parent);
			for (int i = 0; i < nodegrid.Children.Count; i++)
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
						(nodegrid.TemplatedParent as DialogueNodeBlock).InputNodes[(Grid.GetRow(sender as ComboBox))].NodeType = ECOnnectionType.Bool;
					}
					else if ((sender as ComboBox).SelectedIndex == 1)
					{
						ell.Fill = Brushes.BlueViolet;
						(nodegrid.TemplatedParent as DialogueNodeBlock).InputNodes[(Grid.GetRow(sender as ComboBox))].NodeType = ECOnnectionType.Int;
					}
				}
			}
		}

		/// <summary>
		/// Right click on a base node. Opens up the context menu for node deletion
		/// ONLY IS ALLOWED IF SET IN XAML
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BaseNodeRight_BTN_Click(object sender, MouseButtonEventArgs e)
		{
			if (AllowNodeDeletion)
			{
				Point p = Mouse.GetPosition(NodeEditor_BackCanvas);
				SelectedBlockNode = (BaseNodeBlock)(sender as Grid).TemplatedParent;
				ContextMenu cm = this.FindResource("DeleteBaseNode_CM") as ContextMenu;
				cm.PlacementTarget = sender as ContentControl;
				cm.IsOpen = true;
			}
		}

		/// <summary>
		/// Deletes the currently selected Node.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void DeleteBaseNode_Click(object sender, RoutedEventArgs e)
		{
			Console.WriteLine("right clicked on node.");
			if (SelectedBlockNode is BaseNodeBlock)
				NodeEditor_Canvas.Children.Remove(((BaseNodeBlock)SelectedBlockNode));
		
		}

		/// <summary>
		/// This method occurs when the user RIGHT clicks on a connection node (Ellipse).
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void COnnectionNode_RightClick(object sender, MouseButtonEventArgs e)
		{
			Point p = Mouse.GetPosition(NodeEditor_BackCanvas);
			SelectedBlockNode = (BaseNodeBlock)(sender as Grid).TemplatedParent;
			if (SelectedBlockNode == null)
				SelectedBlockNode = (BaseNodeBlock)((Grid)((Grid)sender).Parent).TemplatedParent;
			String TempStr = "";

			if ((sender as Grid).Name.Contains("Entry"))
			{
				SelectedNodeRow = 0;//Grid.GetRow((Grid)(sender as Grid));
				SelectedNode = ((BaseNodeBlock)SelectedBlockNode).EntryNode;
				TempStr = "In";
			}
			else if ((sender as Grid).Name.Contains("Exit"))
			{
				SelectedNodeRow = 0;//Grid.GetRow((Grid)(sender as Grid));
				SelectedNode = ((BaseNodeBlock)SelectedBlockNode).ExitNode;
				TempStr = "Out";
			}
			else if ((sender as Grid).Name.Contains("Out"))
			{
				SelectedNodeRow = Grid.GetRow((Grid)(sender as Grid));
				SelectedNode = ((BaseNodeBlock)SelectedBlockNode).OutputNodes[SelectedNodeRow];
				TempStr = "Out";
			}
			else
			{
				SelectedNodeRow = Grid.GetRow((Grid)(sender as Grid));
				SelectedNode = ((BaseNodeBlock)SelectedBlockNode).InputNodes[SelectedNodeRow];
				TempStr = "In";
			}

			ContextMenu cm = this.FindResource("DeleteConnections_CM") as ContextMenu;

			for (int i = cm.Items.Count - 1; i >= 2; i--)
				cm.Items.RemoveAt(i);


			for (int i = 0; i < SelectedNode.ConnectedNodes.Count; i++)
			{
				MenuItem mi = new MenuItem() { Header = String.Format("{0}:{1}", TempStr, i + 1) };
				mi.PreviewMouseLeftButtonDown += DeleteConnections_Click;
				cm.Items.Add(mi);
			}

			cm.PlacementTarget = sender as ContentControl;
			cm.IsOpen = true;
		}

		/// <summary>
		/// This method occurs when the user RIGHT clicks on a connection node (Ellipse).
		/// and the user has clicked on delete node in the context menu
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void DeleteConnections_Click(object sender, MouseButtonEventArgs e)
		{
			Console.WriteLine("Clicked Deletion of Node");
			int ival = ((ContextMenu)(sender as MenuItem).Parent).Items.IndexOf(sender); ival -= 2;
			if (ival < 0) return;
			NodeEditor_BackCanvas.Children.Remove(SelectedNode.Curves[ival]);
			SelectedNode.ConnectedNodes[ival].Curves.Remove(SelectedNode.Curves[ival]);
			SelectedNode.Curves.RemoveAt(ival);

			if (SelectedNode.ConnectedNodes[ival].ConnectedNodes.IndexOf(SelectedNode) >= 0)
				SelectedNode.ConnectedNodes[ival].ConnectedNodes.Remove(SelectedNode);

			//TODO: Implement the polymorphism 
			//this is here for the removal of the CONSTANT controls on the blocks display wise
			if (SelectedBlockNode is SetConstantNodeBlock && (SelectedBlockNode as SetConstantNodeBlock).NewValue.ConnectedNodes.Count == 0)
				(SelectedBlockNode as SetConstantNodeBlock).NewValConnected = false;
			if(SelectedNode.ConnectedNodes[ival].ParentBlock is SetConstantNodeBlock && (SelectedNode.ConnectedNodes[ival].ParentBlock as SetConstantNodeBlock).NewValue.ConnectedNodes.Count == 0)
				(SelectedNode.ConnectedNodes[ival].ParentBlock as SetConstantNodeBlock).NewValConnected = false;

			SelectedNode.ConnectedNodes.RemoveAt(ival);

		}

		//Add a testing Variable to the UI and list 
		private void AddRuntimeVar_BTN_Click(object sender, RoutedEventArgs e)
		{
			TestingVar_Grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(20) });
			//List
			TestingVars_list.Add(new RuntimeVars() { VarName = ("Var_" + (TestingVar_Grid.RowDefinitions.Count - 1)), VarData = 0 , OrginalVarData = 0});
			VarDisplayBlocks_dict.Add(TestingVars_list.Last().VarName, new List<BaseNodeBlock>());
			//textbox for naming the runtime variables
			TextBox TBlock = new TextBox()
			{
				Background = Brushes.Transparent,
				Foreground = Brushes.White,
				Text = "Var_" + (TestingVar_Grid.RowDefinitions.Count - 1)
			};
			TBlock.TextChanged += RuntimeVar_Name_Changed;
			Grid.SetColumn(TBlock, 0); Grid.SetRow(TBlock, TestingVar_Grid.RowDefinitions.Count - 1);
			TestingVar_Grid.Children.Add(TBlock);

			//textbox for updating the data of the runtime variables
			TextBox tb = new TextBox()
			{
				Text = "0"
			};
			tb.TextChanged += RuntimeVar_Data_Changed;
			Grid.SetColumn(tb, 1); Grid.SetRow(tb, TestingVar_Grid.RowDefinitions.Count - 1);
			TestingVar_Grid.Children.Add(tb);

			//textbox for updating the data of the runtime variables
			// ReSharper disable once IdentifierTypo
			TextBox tblive = new TextBox()
			{
				IsReadOnly = true
			};
			
			Binding binding = new Binding();
			binding.Source = TestingVars_list.Last().VarData;
			tblive.DataContext = binding;
			Grid.SetColumn(tblive, 2); Grid.SetRow(tblive, TestingVar_Grid.RowDefinitions.Count - 1);
			TestingVar_Grid.Children.Add(tblive);

			//combobox
			ComboBox cb = new ComboBox() { HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center };
			cb.SelectionChanged += RuntimeVar_Type_Changed;
			cb.Items.Add("Bool"); cb.Items.Add("Int");
			cb.SelectedIndex = 1;
			Grid.SetColumn(cb, 3); Grid.SetRow(cb, TestingVar_Grid.RowDefinitions.Count - 1);
			TestingVar_Grid.Children.Add(cb);

		}

		private void RuntimeVar_Type_Changed(object sender, SelectionChangedEventArgs e)
		{
			if (!(TestingVars_list.Count > 0)) return;
			int dtype = 0;
			//get the grid row position
			int currow = Grid.GetRow((ComboBox)sender) + 1; //+1 for choice var
			if (((ComboBox)sender).SelectedIndex == 0)
			{
				TestingVars_list[currow] = new RuntimeVars() { VarName = TestingVars_list[currow].VarName, Type = typeof(bool), VarData = false, OrginalVarData = false};
				dtype = 4;
			}
			else if (((ComboBox)sender).SelectedIndex == 1)
			{
				TestingVars_list[currow] = new RuntimeVars() { VarName = TestingVars_list[currow].VarName, Type = typeof(int), VarData = 0, OrginalVarData = 0};
				dtype = 3;
			}
			//if (!VarDisplayBlocks_dict.ContainsKey(TestingVars_list[currow])) { Console.WriteLine("DNE"); return; }
			foreach (BaseNodeBlock bnb in VarDisplayBlocks_dict[TestingVars_list[currow].VarName])
			{
				if (bnb is GetConstantNodeBlock getConstant)
				{
					getConstant.DType = (ECOnnectionType)dtype;
					getConstant.output.NodeType = (ECOnnectionType)dtype;
					getConstant.InternalData = TestingVars_list[currow];

					DeleteAllNodeConnection((bnb as GetConstantNodeBlock).output);
				}
				else if (bnb is SetConstantNodeBlock)
				{
					(bnb as SetConstantNodeBlock).DType = (ECOnnectionType)dtype;
					(bnb as SetConstantNodeBlock).OldValue.NodeType = (ECOnnectionType)dtype;
					(bnb as SetConstantNodeBlock).NewValue.NodeType = (ECOnnectionType)dtype;
					(bnb as SetConstantNodeBlock).OutValue.NodeType = (ECOnnectionType)dtype;

					DeleteAllNodeConnection((bnb as SetConstantNodeBlock).OldValue);
					DeleteAllNodeConnection((bnb as SetConstantNodeBlock).NewValue);
					DeleteAllNodeConnection((bnb as SetConstantNodeBlock).OutValue);
				}
			}
		}

		private void RuntimeVar_Data_Changed(object sender, TextChangedEventArgs e)
		{
			if (!(TestingVars_list.Count > 0)) return;
			//get the grid row position
			int currow = Grid.GetRow((TextBox)sender) + 1; //+1 for the choice var

			if (TestingVars_list[currow].Type == typeof(bool))
			{
				if (((TextBox)sender).Text.ToLower() != "t" && ((TextBox)sender).Text.ToLower() != "f") { ((TextBox)sender).Text = ""; return; }

				if (((TextBox) sender).Text.ToLower() == "t")
				{
					TestingVars_list[currow].VarData = true;
					TestingVars_list[currow].OrginalVarData = true;
				}
				else
				{
					TestingVars_list[currow].VarData = false;
					TestingVars_list[currow].OrginalVarData = false;

				}
			}
			else if (TestingVars_list[currow].Type == typeof(int))
			{
				if (((TextBox)sender).Text.All(x => char.IsDigit(x)) && ((TextBox)sender).Text != "") { }
				else { ((TextBox)sender).Text = ""; return; }
				TestingVars_list[currow].VarData = Int32.Parse(((TextBox)sender).Text);
				TestingVars_list[currow].OrginalVarData = Int32.Parse(((TextBox)sender).Text);
			}
		}

		private void RuntimeVar_Name_Changed(object sender, TextChangedEventArgs e)
		{
			if (!(TestingVars_list.Count > 0)) return;
			//get the grid row position
			int currow = Grid.GetRow((TextBox)sender) + 1; //+1 for choice var

			//Var Names are specific
			if (((TextBox)sender).Text.All(x => char.IsLetterOrDigit(x) || x == '_'))
			{
				//just letters and digits.
			}
			else
			{
				//e.Handled = true;
				((TextBox)sender).Text = ((TextBox)sender).Text.Substring(0, ((TextBox)sender).Text.Length - 1);
				((TextBox)sender).SelectionStart = ((TextBox)sender).Text.Length;
				((TextBox)sender).SelectionLength = 0;
			}

			List<BaseNodeBlock> tempBaseNodeBlocks = VarDisplayBlocks_dict[TestingVars_list[currow].VarName];
			int index = Array.IndexOf(VarDisplayBlocks_dict.Keys.ToArray(), TestingVars_list[currow].VarName);
			VarDisplayBlocks_dict.Remove(TestingVars_list[currow].VarName);
			VarDisplayBlocks_dict.Add(((TextBox)sender).Text, tempBaseNodeBlocks);


			//set it new so we have a property change event go off
			TestingVars_list[currow] = new RuntimeVars() { VarName = ((TextBox)sender).Text, Type = TestingVars_list[currow].Type, VarData = TestingVars_list[currow].VarData, OrginalVarData = TestingVars_list[currow].VarData}; //;
			foreach (BaseNodeBlock bnb in VarDisplayBlocks_dict[TestingVars_list[currow].VarName])
			{
				if (bnb is GetConstantNodeBlock getConstant)
				{
					getConstant.InternalData = TestingVars_list[currow];
					getConstant.VarHeader = getConstant.InternalData.VarName;

					DeleteAllNodeConnection((bnb as GetConstantNodeBlock).output);
				}
			}


		}

		private void NodeEditor_BackCanvas_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			//Window w = new AddBlockMenu();
			//w.Show();

			ContextMenu cm = (ContextMenu)this.Resources["AddBlock_CM"];
			MenuItem mi_get = (MenuItem)cm.Items.GetItemAt(6);
			mi_get.Items.Clear();
			MenuItem mi_set = (MenuItem)cm.Items.GetItemAt(7);
			mi_set.Items.Clear();
			MenuItem mi_dia = (MenuItem)cm.Items.GetItemAt(4);
			mi_dia.Items.Clear();


			foreach (RuntimeVars item in TestingVars_list)
			{
				MenuItem mi_getvar = new MenuItem() { Header = item.VarName };
				mi_getvar.Click += RuntimeVar_Get_Click;
				mi_get.Items.Add(mi_getvar);

				MenuItem mi_setvar = new MenuItem() { Header = item.VarName };
				if (item.VarName == "ChoiceVar") mi_setvar.IsEnabled = false; //DO NOT ALLOW SET CHOCIEVAR
				mi_setvar.Click += RuntimeVar_Set_Click;
				mi_set.Items.Add(mi_setvar);
			}
			foreach (String Name in SceneCharacters_list)
			{
				mi_dia.Items.Add(new MenuItem() { Header = Name });
			}

			cm.MaxHeight = 400;
			cm.IsOpen = true;
			NewBlockLocation = Mouse.GetPosition(NodeEditor_BackCanvas);
		}

		private void RuntimeVar_Set_Click(object sender, RoutedEventArgs e)
		{
			//add the set
			RuntimeVars selectedRV = RuntimeVars.GetRuntimeVar(TestingVars_list.ToList(), ((MenuItem)sender).Header.ToString());
			SetConstantNodeBlock bn = null;
			if (selectedRV.Type == typeof(bool))
				bn = new SetConstantNodeBlock(ECOnnectionType.Bool);
			else if (selectedRV.Type == typeof(int))
				bn = new SetConstantNodeBlock(ECOnnectionType.Int);
			Canvas.SetLeft(bn, NewBlockLocation.X); Canvas.SetTop(bn, NewBlockLocation.Y);
			VarDisplayBlocks_dict[selectedRV.VarName].Add(bn);
			NodeEditor_Canvas.Children.Add(bn);

			//add the get
			GetConstantNodeBlock bnget = null;
			if (selectedRV.Type == typeof(bool))
				bnget = new GetConstantNodeBlock(ECOnnectionType.Bool, ref selectedRV);
			else if (selectedRV.Type == typeof(int))
				bnget = new GetConstantNodeBlock(ECOnnectionType.Int, ref selectedRV);
			Canvas.SetLeft(bnget, NewBlockLocation.X - 175); Canvas.SetTop(bnget, NewBlockLocation.Y - 10);
			NodeEditor_Canvas.Children.Add(bnget);
			VarDisplayBlocks_dict[selectedRV.VarName].Add(bnget);
			bnget.Loaded += RuntimeVarSetBlock_Loaded;
		}

		/// <summary>
		/// Handles the drawing, and setting of connections. AFTER both block (get and set) are loaded to the screen.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void RuntimeVarSetBlock_Loaded(object sender, RoutedEventArgs e)
		{
			GetConstantNodeBlock get = (GetConstantNodeBlock)(NodeEditor_Canvas.Children[NodeEditor_Canvas.Children.Count - 1]);
			SetConstantNodeBlock set = (SetConstantNodeBlock)(NodeEditor_Canvas.Children[NodeEditor_Canvas.Children.Count - 2]);
			if (get != null && set != null)
			{
				//data reference connection
				get.output.ConnectedNodes.Add(set.OldValue);
				set.OldValue.ConnectedNodes.Add(get.output);

				//drawing start
				Point get_mos = new Point(Canvas.GetLeft(get) + 100, Canvas.GetTop(get) + 40);
				Point set_mos = new Point(Canvas.GetLeft(set) + 150, Canvas.GetTop(set) + 45);

				//curve references
				Path p = CreateBezierCurve(get_mos, set_mos);
				get.output.Curves.Add(p);
				set.OldValue.Curves.Add(p);

				//actual drawing of the curve
				NodeEditor_BackCanvas.Children.Add(p);
			}
		}

		private void RuntimeVar_Get_Click(object sender, RoutedEventArgs e)
		{
			RuntimeVars selectedRV = RuntimeVars.GetRuntimeVar(TestingVars_list.ToList(), ((MenuItem)sender).Header.ToString());
			GetConstantNodeBlock bn = null;
			if (selectedRV.Type == typeof(bool))
				bn = new GetConstantNodeBlock(ECOnnectionType.Bool, ref selectedRV);
			else if (selectedRV.Type == typeof(int))
				bn = new GetConstantNodeBlock(ECOnnectionType.Int, ref selectedRV);
			Canvas.SetLeft(bn, NewBlockLocation.X); Canvas.SetTop(bn, NewBlockLocation.Y);
			NodeEditor_Canvas.Children.Add(bn);
			VarDisplayBlocks_dict[selectedRV.VarName].Add(bn);
		}

		private void AddDialogueBlock_BTN_Click(object sender, RoutedEventArgs e)
		{
			AddDialogueBlockToGraph();
		}

		public void AddDialogueBlockToGraph(DialogueNodeBlock dialogueNode, String CharacterName = "", object Timeblock = null)
		{
			DialogueNodeBlock bn = dialogueNode; //TODO: Change this from "" -> MenuItem.Header
			if (Timeblock != null)
				bn.LinkedTimeBlock = Timeblock;
			NodeEditor_Canvas.Children.Add(bn);
			Point p = new Point(0, 10); Point p1 = new Point(150, 20 + 20);
			bn.EntryNode = (new ConnectionNode(bn, "EnterNode", p, ECOnnectionType.Enter));
			bn.OutputNodes.Add(new ConnectionNode(bn, "OutputNode1", p1, ECOnnectionType.Exit));
		}

		public void AddDialogueBlockToGraph(String CharacterName="", object Timeblock=null)
		{
			DialogueNodeBlock bn = new DialogueNodeBlock(CharacterName); //TODO: Change this from "" -> MenuItem.Header
			if (Timeblock != null)
				bn.LinkedTimeBlock = Timeblock;
			NodeEditor_Canvas.Children.Add(bn);
			Point p = new Point(0, 10); Point p1 = new Point(150, 20 + 20);
			bn.EntryNode = (new ConnectionNode(bn, "EnterNode", p, ECOnnectionType.Enter));
			bn.OutputNodes.Add(new ConnectionNode(bn, "OutputNode1", p1, ECOnnectionType.Exit));
		}

		private void TestingVars_list_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.OldItems == null) return;
			//if ((e.OldItems[0] as RuntimeVars).VarName == (e.NewItems[0] as RuntimeVars).VarName) return;
			if (e.Action == NotifyCollectionChangedAction.Replace)
			{
				int index = Array.FindIndex<RuntimeVars>(TestingVars_list.ToArray(), x => x.VarName == (e.NewItems[0] as RuntimeVars).VarName);
				for (int i = 0; i < TestingVar_Grid.Children.Count; i++)
				{
					if (Grid.GetRow(TestingVar_Grid.Children[i]) == index-1 && Grid.GetColumn(TestingVar_Grid.Children[i]) == 2)
					{
						(TestingVar_Grid.Children[i] as TextBox).Text = (e.NewItems[0] as RuntimeVars).VarData.ToString();
					}
				}
			}
		}

		private void DeleteAllNodeConnection(ConnectionNode Node)
		{
			for (int i = 0; i < Node.ConnectedNodes.Count; i++)
			{
				DeleteConnection(Node, Node.ConnectedNodes[i].ParentBlock);
			}
		}

		public void DeleteConnection(ConnectionNode Node, BaseNodeBlock CBlock)
		{
			//what connection are we looking for?
			int index = -1; int count = 0;
			foreach (ConnectionNode item in Node.ConnectedNodes)
			{
				if (item.ParentBlock == CBlock)
					index = count;
				count++;
			}
			if (index >= 0)
			{
				Node.ConnectedNodes.RemoveAt(index);
				NodeEditor_BackCanvas.Children.Remove(Node.Curves[index]);
				Node.Curves.RemoveAt(index);
			}



		}

		private void ConnectNodes(ConnectionNode From, ConnectionNode To)
		{

		}


		public partial class RuntimeVars
		{
			public string VarName { get; set; }
			public object VarData { get; set; }
			public object OrginalVarData { get; set; }
			public Type Type { get; set; }

			public RuntimeVars()
			{
				Type = typeof(int);
			}

			public static RuntimeVars GetRuntimeVar(List<RuntimeVars> list, String Name)
			{
				foreach (RuntimeVars item in list)
				{
					if (item.VarName == Name) return item;
				}
				return null;
			}

			public bool Equals(RuntimeVars rv)
			{
				return rv.Type.Equals(Type) && rv.VarData.Equals(VarData) && rv.VarName.Equals(VarName);
			}

			public override bool Equals(object obj)
			{
				return this.Equals(obj as RuntimeVars);
			}

			public override int GetHashCode()
			{
				return 1;//VarName.GetHashCode() ^ VarData.GetHashCode() ^ Type.GetHashCode();
			}

		}

		private void ConditonalsLT_MI_Click(object sender, RoutedEventArgs e)
		{
			if ((sender as MenuItem).Header.ToString() == "Bool")
			{
				ConditionalNodeBlock bn = new ConditionalNodeBlock(ECOnnectionType.Bool);
				NodeEditor_Canvas.Children.Add(bn);
			}
			else if ((sender as MenuItem).Header.ToString() == "Int")
			{
				ConditionalNodeBlock bn = new ConditionalNodeBlock(ECOnnectionType.Int);
				NodeEditor_Canvas.Children.Add(bn);
			}
			else
			{
				ConditionalNodeBlock bn = new ConditionalNodeBlock(ECOnnectionType.Int);
				NodeEditor_Canvas.Children.Add(bn);
			}
		}

		private void Math_MI_Click(object sender, RoutedEventArgs e)
		{
			MenuItem mi = sender as MenuItem;
			if (mi.Header.ToString().Contains("Add"))
			{
				BaseArithmeticBlock bnn = new AddBlock();
				NodeEditor_Canvas.Children.Add(bnn);
			}
			else if (mi.Header.ToString().Contains("Sub"))
			{
				BaseArithmeticBlock bnn = new SubtractBlock();
				NodeEditor_Canvas.Children.Add(bnn);
			}
			else if (mi.Header.ToString().Contains("Mul"))
			{
				BaseArithmeticBlock bnn = new MultiplyBlock();
				NodeEditor_Canvas.Children.Add(bnn);
			}
			else if (mi.Header.ToString().Contains("Div"))
			{
				BaseArithmeticBlock bnn = new DivisionBlock();
				NodeEditor_Canvas.Children.Add(bnn);
			}
			else if (mi.Header.ToString().Contains("Mod"))
			{
				BaseArithmeticBlock bnn = new ModuloBlock();
				NodeEditor_Canvas.Children.Add(bnn);
			}
		}

		private void LogicAND_MI_Click(object sender, RoutedEventArgs e)
		{
			MenuItem mi = sender as MenuItem;
			if (mi.Header.ToString().Contains("AND"))
			{
				BaseLogicNodeBlock bn = new ANDBlock();
				NodeEditor_Canvas.Children.Add(bn);
			}
			else if (mi.Header.ToString().Contains("OR"))
			{
				BaseLogicNodeBlock bn = new ORBlock();
				NodeEditor_Canvas.Children.Add(bn);
			}
			else if (mi.Header.ToString().Contains("NOT"))
			{
				BaseLogicNodeBlock bn = new NOTBlock();
				NodeEditor_Canvas.Children.Add(bn);
			}

		}

		public void ResetExecution()
		{
			CurrentExecutionBlock.ActiveStatus = EActiveStatus.Disabled;
			CurrentExecutionBlock = StartExecutionBlock;
			CurrentExecutionBlock.ActiveStatus = EActiveStatus.Active;
			for (int i = 0; i < TestingVars_list.Count; i++)
			{
				TestingVars_list[i].VarData = TestingVars_list[i].OrginalVarData;
			}
		}

		private void StartBlockExecution_MI_Click(object sender, RoutedEventArgs e)
		{
			ResetExecution();
		}

		public bool StartBlockExecution()
		{
			bool b = true;
			if (CurrentExecutionBlock.OnStartNodeBlockExecution(ref CurrentExecutionBlock)) b = true;
			while (CurrentExecutionBlock.ErrorStack.Count > 0)
			{
				CurrentErrors.Add(CurrentExecutionBlock.ErrorStack.Pop());
				b = false;
			}
			return b;
		}

		private void RunOnStart_MI_Click(object sender, RoutedEventArgs e)
		{
			StartBlockExecution();
		}

		public bool ExecuteBlock()
		{
			bool b = true;
			if (CurrentExecutionBlock.NodeBlockExecution(ref CurrentExecutionBlock) && CurrentExecutionBlock.ErrorStack.Count == 0) b = true;
			while (CurrentExecutionBlock.ErrorStack.Count > 0)
			{
				if (CurrentExecutionBlock.ErrorStack.Peek() is ChangeVarFlag flag)
				{
					//TestingVars_list[TestingVars_list.(flag.VarToChange)] = flag.NewVar;
					//TestingVars_list.Single(x => x.VarName == flag.VarToChange.VarName);
					int i = Array.FindIndex<RuntimeVars>(TestingVars_list.ToArray(), x => x.VarName == flag.VarToChange.VarName);
					TestingVars_list[i] = flag.NewVar;
					CurrentErrors.Add(CurrentExecutionBlock.ErrorStack.Pop());
					b = false;
				}
				else
				{
					CurrentErrors.Add(CurrentExecutionBlock.ErrorStack.Pop());
					b = false;
				}
			}

			return b;
		}

		private void ExecuteBlock_MI_Click(object sender, RoutedEventArgs e)
		{
			ExecuteBlock();
		}

		public bool EndblockExecution()
		{
			bool b = true;
			if (CurrentExecutionBlock is ExitBlockNode)
			{
				CurrentErrors.Add(new NodeEditorException("Dialogue Scene Completed!"));
			}
			CurrentExecutionBlock.OnEndNodeBlockExecution(ref CurrentExecutionBlock);
			return b;
		}

		private void RunOnExit_MI_Click(object sender, RoutedEventArgs e)
		{
			EndblockExecution();
		}

		private void EvalOnStart_MI_Click(object sender, RoutedEventArgs e)
		{
			//CurrentExecutionBlock.OnStartEvaluateInternalData(null);
		}
		private void EvalBlock_MI_Click(object sender, RoutedEventArgs e)
		{
			//CurrentExecutionBlock.EvaluateInternalData(TODO);
		}

		private void EvalOnExit_MI_Click(object sender, RoutedEventArgs e)
		{
			CurrentExecutionBlock.OnEndEvaluateInternalData();
		}

	}
	/// <summary>
	/// This is the starting pointer for a given graph
	/// CONTAINS ONE OUTPUT (EXIT NODE)
	/// </summary>
	public partial class StartBlockNode : BaseNodeBlock
	{
		public StartBlockNode()
		{
			this.ExitNode = new ConnectionNode(this, "ExitNode", new Point(0, 0), ECOnnectionType.Exit);
		}

		public override bool OnStartEvaluateInternalData()
		{
			throw new NotImplementedException();
		}
		public override bool EvaluateInternalData(BaseNodeBlock connectedBlock)
		{
			throw new NotImplementedException();
		}
		public override bool OnEndEvaluateInternalData()
		{
			throw new NotImplementedException();
		}

		public override void DeleteConnection(EConditionalTypes contype, int row)
		{
			throw new NotImplementedException();
		}

		public override bool OnStartNodeBlockExecution(ref BaseNodeBlock currentNB)
		{
		//check to make sure the exit node is connected.
		if (this.ExitNode.ConnectedNodes.Count == 0)
		{
			this.ActiveStatus = EActiveStatus.Error;
			return false;
		}

		this.NodeBlockExecution(ref currentNB);
		this.ActiveStatus = EActiveStatus.Active;
		return true;
		}
		public override bool NodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			return true;
		}

		public override void OnEndNodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			if (this.ExitNode.ConnectedNodes.Count > 0)
			{
				currentNB = this.ExitNode.ConnectedNodes[0].ParentBlock;
				this.ActiveStatus = EActiveStatus.Disabled;
				//currentNB.OnStartNodeBlockExecution(ref currentNB);
			}
		}
	}

	/// <summary>
	/// THis is the Stopping pointer for a given graph
	/// CONTAINS ONE INPUT (ENTRY NODE)
	/// </summary>
	public partial class ExitBlockNode : BaseNodeBlock
	{
		public ExitBlockNode()
		{
			this.EntryNode = new ConnectionNode(this, "EntryNode", new Point(0, 0), ECOnnectionType.Enter);
		}

		public override void DeleteConnection(EConditionalTypes contype, int row)
		{
			throw new NotImplementedException();
		}

		public override bool EvaluateInternalData(BaseNodeBlock connectedBlock)
		{
			throw new NotImplementedException();
		}

		public override bool NodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			return true;
		}

		public override bool OnEndEvaluateInternalData()
		{
			return true;
		}

		public override void OnEndNodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			this.ActiveStatus = EActiveStatus.Done;
			return;
		}

		public override bool OnStartEvaluateInternalData()
		{
			throw new NotImplementedException();
		}

		public override bool OnStartNodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			//check to make sure the exit node is connected.
			if (this.EntryNode.ConnectedNodes.Count == 0)
			{
				this.ActiveStatus = EActiveStatus.Error;
				return false;
			}

			//this.NodeBlockExecution(ref currentNB);
			this.ActiveStatus = EActiveStatus.Active;
			return true;
		}
	}



}
