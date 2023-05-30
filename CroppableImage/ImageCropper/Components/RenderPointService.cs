using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace ImageCropper.Components
{

	public class RenderPointService
	{
		public delegate void SetRenderPoint(int x, int y);
		public SetRenderPoint SetRenderPoint_hook;


		private readonly RenderPointAdorner _renderPointAdorner;
		private readonly Tools.RenderPointTool _renderPointTool;
		private readonly Canvas _canvas;
		private bool _bIsActive = false;

		public RenderPointService(FrameworkElement adornedElement)
		{
			var adornerLayer = AdornerLayer.GetAdornerLayer(adornedElement);

			_canvas = new Canvas
			{
				Height = adornedElement.ActualHeight,
				Width = adornedElement.ActualWidth
			};
			_renderPointAdorner = new RenderPointAdorner(adornedElement, _canvas);

			Debug.Assert(adornerLayer != null, nameof(adornerLayer) + " != null");
			adornerLayer.Add(_renderPointAdorner);

			_renderPointTool = new Tools.RenderPointTool(_canvas);

			// Mouse events
			_canvas.MouseMove += _renderPointAdorner_MouseMove;
			_canvas.MouseLeftButtonDown += _canvas_MouseLeftButtonDown;

			_renderPointTool.Redraw(0, 0, 0, 0);

		}

		// We have decided on the new renderpoint position. So let's kill the service
		private void _canvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if(SetRenderPoint_hook != null)
			{
				var point = e.GetPosition(_canvas);
				SetRenderPoint_hook((int)point.X, (int)point.Y);
			}
		}

		// Redraw the mouse position to the screen with the adorners
		private void _renderPointAdorner_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
		{
			var point = e.GetPosition(_canvas);
			_renderPointTool.Redraw(point.X, point.Y, point.X, point.Y);
		}



		public void ClearAdorners(FrameworkElement adornedElements)
		{

			var adornerLayer = AdornerLayer.GetAdornerLayer(adornedElements);
			var adornersOfStackPanel = adornerLayer.GetAdorners(adornedElements);
			if (adornersOfStackPanel == null) return;
			foreach (var adorner in adornersOfStackPanel)
				adornerLayer.Remove(adorner);
		}
	}
}
