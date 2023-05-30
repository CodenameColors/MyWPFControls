using System.Windows.Controls;
using System.Windows.Shapes;

namespace CroppingImageLibrary.Services.Tools
{
	internal class CropShape
	{
		public Shape Shape { get; }
		public Shape DashedShape { get; }
		public bool bIsCords { get; }

		public CropShape(Shape shape, Shape dashedShape, bool bCords)
		{
			Shape = shape;
			DashedShape = dashedShape;
			bIsCords = bCords;
		}

		public void Redraw(double newX, double newY, double newWidth, double newHeight)
		{
			//dont use negative value
			if (newHeight >= 0 && newWidth >= 0)
			{
				RedrawSolidShape(newX, newY, newWidth, newHeight);
				RedrawDashedShape();
			}
		}

		private void RedrawSolidShape(double newX, double newY, double newWidth, double newHeight)
		{
			Canvas.SetLeft(Shape, newX);
			Canvas.SetTop(Shape, newY);
			if (bIsCords)
			{
				Shape.Width = 1;
				Shape.Height = 1;
			}
			else
			{
				Shape.Width = newWidth;
				Shape.Height = newHeight;
			}
		}

		private void RedrawDashedShape()
		{
			if (bIsCords)
			{
				DashedShape.Height = 1;
				DashedShape.Width = 1;
			}
			else
			{
				DashedShape.Width = Shape.Width;
				DashedShape.Height = Shape.Height;
			}
			Canvas.SetLeft(DashedShape, Canvas.GetLeft(Shape));
			Canvas.SetTop(DashedShape, Canvas.GetTop(Shape));
		}
	}
}
