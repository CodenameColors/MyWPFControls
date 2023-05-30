using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace ImageCropper.Components
{
	internal class RenderPointAdorner : Adorner
	{
		private readonly Canvas _overlayCanvas;
		private readonly VisualCollection _visualCollection;

		public RenderPointAdorner(UIElement adornedElement, Canvas overlayCanvas) : base(adornedElement)
		{
			_overlayCanvas = overlayCanvas;
			_visualCollection = new VisualCollection(this);
			_visualCollection.Add(_overlayCanvas);
		}

		protected override int VisualChildrenCount => _visualCollection.Count;

		protected override Visual GetVisualChild(int index) => _visualCollection[index];

		protected override Size ArrangeOverride(Size size)
		{
			Size finalSize = base.ArrangeOverride(size);
			_overlayCanvas.Arrange(new Rect(0, 0, AdornedElement.RenderSize.Width, AdornedElement.RenderSize.Height));
			return finalSize;
		}
	}
}
