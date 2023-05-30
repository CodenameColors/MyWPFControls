using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using CropShape = CroppingImageLibrary.Services.Tools.CropShape;

namespace ImageCropper.Components.Tools
{
	internal class RenderPointTool
	{
		private readonly Canvas _canvas;
		private readonly CropShape _cropShape;
		private readonly ShadeTool _shadeService;
		//private readonly ThumbTool _thumbService;
		private readonly TextTool _textService;

		public double TopLeftX => Canvas.GetLeft(_cropShape.Shape);
		public double TopLeftY => Canvas.GetTop(_cropShape.Shape);
		public double BottomRightX => Canvas.GetLeft(_cropShape.Shape) + _cropShape.Shape.Width;
		public double BottomRightY => Canvas.GetTop(_cropShape.Shape) + _cropShape.Shape.Height;
		public double Height => _cropShape.Shape.Height;
		public double Width => _cropShape.Shape.Width;

		public RenderPointTool(Canvas canvas)
		{
			_canvas = canvas;
			_cropShape = new CropShape(new Rectangle
			{
				Height = 0,
				Width = 0,
				Stroke = Brushes.Black,
				StrokeThickness = 1.5
			},
			new Rectangle
			{
				Stroke = Brushes.White,
				StrokeDashArray = new DoubleCollection(new double[] { 4, 4 })
			}, true);

			_shadeService = new ShadeTool(canvas, this);
			//_thumbService = new ThumbTool(canvas, this);
			_textService = new TextTool(this);

			_canvas.Children.Add(_cropShape.Shape);
			_canvas.Children.Add(_cropShape.DashedShape);


			_canvas.Children.Add(_shadeService.ShadeOverlay);

			//_canvas.Children.Add(_thumbService.BottomMiddle);
			//_canvas.Children.Add(_thumbService.LeftMiddle);
			//_canvas.Children.Add(_thumbService.TopMiddle);
			//_canvas.Children.Add(_thumbService.RightMiddle);
			//_canvas.Children.Add(_thumbService.TopLeft);
			//_canvas.Children.Add(_thumbService.TopRight);
			//_canvas.Children.Add(_thumbService.BottomLeft);
			//_canvas.Children.Add(_thumbService.BottomRight);

			_canvas.Children.Add(_textService.TextBlock);
		}

		public void Redraw(double newX, double newY, double newWidth, double newHeight)
		{
			_cropShape.Redraw(newX, newY, newWidth, newHeight);
			_shadeService.Redraw();
			//_thumbService.Redraw();
			_textService.Redraw();
		}
	}

	internal class ShadeTool
	{
		private readonly RenderPointTool _renderPointTool;
		private readonly RectangleGeometry _rectangleGeo;

		public Path ShadeOverlay { get; set; }

		public ShadeTool(Canvas canvas, RenderPointTool cropTool)
		{
			_renderPointTool = cropTool;

			ShadeOverlay = new Path
			{
				Fill = Brushes.Black,
				Opacity = 0.5
			};

			var geometryGroup = new GeometryGroup();
			RectangleGeometry geometry1 =
					new RectangleGeometry(new Rect(new Size(canvas.Width, canvas.Height)));
			_rectangleGeo = new RectangleGeometry(
					new Rect(
							_renderPointTool.TopLeftX,
							_renderPointTool.TopLeftY,
							_renderPointTool.Width,
							_renderPointTool.Height
					)
			);
			geometryGroup.Children.Add(geometry1);
			geometryGroup.Children.Add(_rectangleGeo);
			ShadeOverlay.Data = geometryGroup;
		}

		public void Redraw()
		{
			_rectangleGeo.Rect = new Rect(
					_renderPointTool.TopLeftX,
					_renderPointTool.TopLeftY,
					_renderPointTool.Width,
					_renderPointTool.Height
			);
		}
	}

	internal class TextTool
	{
		const double OffsetTop = 2;
		const double OffsetLeft = 5;

		public TextBlock TextBlock { get; }
		private readonly RenderPointTool _renderPointTool;

		public TextTool(RenderPointTool renderPointTool)
		{
			_renderPointTool = renderPointTool;
			TextBlock = new TextBlock
			{
				Text = "Size counter",
				FontSize = 14,
				Foreground = Brushes.White,
				Background = Brushes.Black,
				Visibility = Visibility.Hidden
			};
		}


		/// <summary>
		///     Manage visibility of text
		/// </summary>
		/// <param name="isVisible">Set current visibility</param>
		public void ShowText(bool isVisible)
		{
			TextBlock.Visibility = isVisible ? Visibility.Visible : Visibility.Hidden;
		}

		/// <summary>
		///     Update (redraw) text label
		/// </summary>
		public void Redraw()
		{
			if (_renderPointTool.Height <= 0 && _renderPointTool.Width <= 0)
				ShowText(false);
			else
				ShowText(true);

			double calculateTop = _renderPointTool.TopLeftY - TextBlock.ActualHeight - OffsetTop;
			if (calculateTop < 0)
				calculateTop = OffsetTop;

			Canvas.SetLeft(TextBlock, _renderPointTool.TopLeftX + OffsetLeft);
			Canvas.SetTop(TextBlock, calculateTop);
			TextBlock.Text = $"X: {_renderPointTool.TopLeftX}, Y: {_renderPointTool.TopLeftY}";
		}
	}
}
