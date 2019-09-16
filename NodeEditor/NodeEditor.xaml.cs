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

namespace NodeEditor
{
  /// <summary>
  /// Interaction logic for UserControl1.xaml
  /// </summary>
  public partial class NodeEditor : UserControl
  {


		Point MPos = new Point();
		Point GridOffset = new Point();
		private ImageBrush imgtilebrush;
		private int newx;
		private int newy;
		Rectangle selectrect = new Rectangle();
		bool bAdd = false;


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


	}
}
