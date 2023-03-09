using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace ImageCropper.Components
{
	public class ResizeService
	{
		public delegate void MoveParent(int x, int y);
		public MoveParent MoveParent_hook;

		private bool _bIsDraging = false;

		private readonly ResizeAdorner _resizeAdorner;
		public Adorner Adorner => _resizeAdorner;

		public ResizeService(UIElement adornedElement)
		{
			var adornerLayer = AdornerLayer.GetAdornerLayer(adornedElement);

			_resizeAdorner = new ResizeAdorner(adornedElement);

			Debug.Assert(adornerLayer != null, nameof(adornerLayer) + " != null");
			adornerLayer.Add(_resizeAdorner);
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
