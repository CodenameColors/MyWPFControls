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
	public class ResizeService
	{
		private readonly ResizeAdorner _resizeAdorner;

		public Adorner Adorner => _resizeAdorner;

		public ResizeService(UIElement adornedElement)
		{
			var adornerLayer = AdornerLayer.GetAdornerLayer(adornedElement);

			_resizeAdorner = new ResizeAdorner(adornedElement);

			Debug.Assert(adornerLayer != null, nameof(adornerLayer) + " != null");


			adornerLayer.Add(_resizeAdorner);
		}
	}
}
