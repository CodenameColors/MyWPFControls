using System;
using System.Collections.Generic;
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

namespace SMRControl
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
		/// ITS IMPORTANT THAT THIS CONTROL IS ONLY USED FOR CANVASES
    /// </summary>
    public partial class SMRControl : UserControl
    {

		bool isDown, isDragging, isSelected;
		UIElement selectedElement = null;
		double originalLeft, originalTop;
		Point startPoint;

    public SMRControl()
    {
      InitializeComponent();
			this.Loaded += SMRControl_Loaded;
    }

		AdornerLayer adornerLayer;

		private void SMRControl_Loaded(object sender, RoutedEventArgs e)
		{
			Console.WriteLine("loaded");

			//registering mouse events
			this.MouseLeftButtonDown += MainWindow_MouseLeftButtonDown;
			this.MouseLeftButtonUp += MainWindow_MouseLeftButtonUp;
			this.MouseMove += MainWindow_MouseMove;
			this.MouseLeave += MainWindow_MouseLeave;

			((Canvas)this.Parent).PreviewMouseLeftButtonDown += MyCanvas_PreviewMouseLeftButtonDown;
			((Canvas)this.Parent).PreviewMouseLeftButtonUp += MyCanvas_PreviewMouseLeftButtonUp;
		}


		private void StopDragging()
		{
			if (isDown)
			{
				isDown = isDragging = false;
			}
		}


		private void MyCanvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			StopDragging();
			e.Handled = true;
		}

		private void MyCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			//removing selected element
			if (isSelected)
			{
				isSelected = false;
				if (selectedElement != null)
				{
					adornerLayer.Remove(adornerLayer.GetAdorners(selectedElement)[0]);
					selectedElement = null;
				}
			}

			// select element if any element is clicked other then canvas
			if (e.Source != ((Canvas)this.Parent))
			{
				isDown = true;
				startPoint = e.GetPosition(((Canvas)this.Parent));

				selectedElement = e.Source as UIElement;

				originalLeft = Canvas.GetLeft(selectedElement);
				originalTop = Canvas.GetTop(selectedElement);

				//adding adorner on selected element
				adornerLayer = AdornerLayer.GetAdornerLayer(selectedElement);
				adornerLayer.Add(new BorderAdorner(selectedElement));
				isSelected = true;
				e.Handled = true;
			}
		}

		private void MainWindow_MouseMove(object sender, MouseEventArgs e)
		{
			//handling mouse move event and setting canvas top and left value based on mouse movement
			if (isDown)
			{
				if ((!isDragging) &&
						((Math.Abs(e.GetPosition(((Canvas)this.Parent)).X - startPoint.X) > SystemParameters.MinimumHorizontalDragDistance) ||
						(Math.Abs(e.GetPosition(((Canvas)this.Parent)).Y - startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)))
					isDragging = true;

				if (isDragging)
				{
					Point position = Mouse.GetPosition(((Canvas)this.Parent));
					Canvas.SetTop(selectedElement, position.Y - (startPoint.Y - originalTop));
					Canvas.SetLeft(selectedElement, position.X - (startPoint.X - originalLeft));
				}
			}
		}

		private void MainWindow_MouseLeave(object sender, MouseEventArgs e)
		{
			//stop dragging on mouse leave
			StopDragging();
			Console.WriteLine("MoveUp_leave");
			e.Handled = true;
		}

		private void MainWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			//stop dragging on mouse left button up
			StopDragging();
			Console.WriteLine("MoveUp");
			e.Handled = true;
		}

		private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			//remove selected element on mouse down
			if (isSelected)
			{
				isSelected = false;
				if (selectedElement != null)
				{
					adornerLayer.Remove(adornerLayer.GetAdorners(selectedElement)[0]);
					selectedElement = null;
				}
			}
		}

		public void SetVisibility(Visibility visibility)
		{
			this.adornerLayer.Visibility = Visibility;
		}

	}

	public class BorderAdorner : Adorner
	{
		//use thumb for resizing elements
		Thumb topLeft, topRight, bottomLeft, bottomRight;
		//visual child collection for adorner
		VisualCollection visualChilderns;

		public BorderAdorner(UIElement element) : base(element)
		{
			visualChilderns = new VisualCollection(this);

			//adding thumbs for drawing adorner rectangle and setting cursor
			BuildAdornerCorners(ref topLeft, Cursors.SizeNWSE);
			BuildAdornerCorners(ref topRight, Cursors.SizeNESW);
			BuildAdornerCorners(ref bottomLeft, Cursors.SizeNESW);
			BuildAdornerCorners(ref bottomRight, Cursors.SizeNWSE);

			//registering drag delta events for thumb drag movement
			topLeft.DragDelta += TopLeft_DragDelta;
			topRight.DragDelta += TopRight_DragDelta;
			bottomLeft.DragDelta += BottomLeft_DragDelta;
			bottomRight.DragDelta += BottomRight_DragDelta;
		}


		private void BottomRight_DragDelta(object sender, DragDeltaEventArgs e)
		{
			FrameworkElement adornedElement = this.AdornedElement as FrameworkElement;
			Thumb bottomRightCorner = sender as Thumb;
			//setting new height and width after drag
			if (adornedElement != null && bottomRightCorner != null)
			{
				EnforceSize(adornedElement);

				double oldWidth = adornedElement.Width;
				double oldHeight = adornedElement.Height;

				double newWidth = Math.Max(adornedElement.Width + e.HorizontalChange, bottomRightCorner.DesiredSize.Width);
				double newHeight = Math.Max(e.VerticalChange + adornedElement.Height, bottomRightCorner.DesiredSize.Height);

				adornedElement.Width = newWidth;
				adornedElement.Height = newHeight;
			}
		}

		private void TopRight_DragDelta(object sender, DragDeltaEventArgs e)
		{
			FrameworkElement adornedElement = this.AdornedElement as FrameworkElement;
			Thumb topRightCorner = sender as Thumb;
			//setting new height, width and canvas top after drag
			if (adornedElement != null && topRightCorner != null)
			{
				EnforceSize(adornedElement);

				double oldWidth = adornedElement.Width;
				double oldHeight = adornedElement.Height;

				double newWidth = Math.Max(adornedElement.Width + e.HorizontalChange, topRightCorner.DesiredSize.Width);
				double newHeight = Math.Max(adornedElement.Height - e.VerticalChange, topRightCorner.DesiredSize.Height);
				adornedElement.Width = newWidth;

				double oldTop = Canvas.GetTop(adornedElement);
				double newTop = oldTop - (newHeight - oldHeight);
				adornedElement.Height = newHeight;
				Canvas.SetTop(adornedElement, newTop);
			}
		}

		private void TopLeft_DragDelta(object sender, DragDeltaEventArgs e)
		{
			FrameworkElement adornedElement = this.AdornedElement as FrameworkElement;
			Thumb topLeftCorner = sender as Thumb;
			//setting new height, width and canvas top, left after drag
			if (adornedElement != null && topLeftCorner != null)
			{
				EnforceSize(adornedElement);

				double oldWidth = adornedElement.Width;
				double oldHeight = adornedElement.Height;

				double newWidth = Math.Max(adornedElement.Width - e.HorizontalChange, topLeftCorner.DesiredSize.Width);
				double newHeight = Math.Max(adornedElement.Height - e.VerticalChange, topLeftCorner.DesiredSize.Height);

				double oldLeft = Canvas.GetLeft(adornedElement);
				double newLeft = oldLeft - (newWidth - oldWidth);
				adornedElement.Width = newWidth;
				Canvas.SetLeft(adornedElement, newLeft);

				double oldTop = Canvas.GetTop(adornedElement);
				double newTop = oldTop - (newHeight - oldHeight);
				adornedElement.Height = newHeight;
				Canvas.SetTop(adornedElement, newTop);
			}
		}

		private void BottomLeft_DragDelta(object sender, DragDeltaEventArgs e)
		{
			FrameworkElement adornedElement = this.AdornedElement as FrameworkElement;
			Thumb topRightCorner = sender as Thumb;
			//setting new height, width and canvas left after drag
			if (adornedElement != null && topRightCorner != null)
			{
				EnforceSize(adornedElement);

				double oldWidth = adornedElement.Width;
				double oldHeight = adornedElement.Height;

				double newWidth = Math.Max(adornedElement.Width - e.HorizontalChange, topRightCorner.DesiredSize.Width);
				double newHeight = Math.Max(adornedElement.Height + e.VerticalChange, topRightCorner.DesiredSize.Height);

				double oldLeft = Canvas.GetLeft(adornedElement);
				double newLeft = oldLeft - (newWidth - oldWidth);
				adornedElement.Width = newWidth;
				Canvas.SetLeft(adornedElement, newLeft);

				adornedElement.Height = newHeight;
			}
		}

		public void BuildAdornerCorners(ref Thumb cornerThumb, Cursor customizedCursors)
		{
			//adding new thumbs for adorner to visual childern collection
			if (cornerThumb != null) return;
			cornerThumb = new Thumb() { Cursor = customizedCursors, Height = 10, Width = 10, Opacity = 0.5, Background = new SolidColorBrush(Colors.Gray) };
			visualChilderns.Add(cornerThumb);
		}

		public void EnforceSize(FrameworkElement element)
		{
			if (element.Width.Equals(Double.NaN))
				element.Width = element.DesiredSize.Width;
			if (element.Height.Equals(Double.NaN))
				element.Height = element.DesiredSize.Height;

			//enforce size of element not exceeding to it's parent element size
			FrameworkElement parent = element.Parent as FrameworkElement;

			if (parent != null)
			{
				element.MaxHeight = parent.ActualHeight;
				element.MaxWidth = parent.ActualWidth;
			}
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			base.ArrangeOverride(finalSize);

			double desireWidth = AdornedElement.DesiredSize.Width;
			double desireHeight = AdornedElement.DesiredSize.Height;

			double adornerWidth = this.DesiredSize.Width;
			double adornerHeight = this.DesiredSize.Height;

			//arranging thumbs
			topLeft.Arrange(new Rect(-adornerWidth / 2, -adornerHeight / 2, adornerWidth, adornerHeight));
			topRight.Arrange(new Rect(desireWidth - adornerWidth / 2, -adornerHeight / 2, adornerWidth, adornerHeight));
			bottomLeft.Arrange(new Rect(-adornerWidth / 2, desireHeight - adornerHeight / 2, adornerWidth, adornerHeight));
			bottomRight.Arrange(new Rect(desireWidth - adornerWidth / 2, desireHeight - adornerHeight / 2, adornerWidth, adornerHeight));

			return finalSize;
		}
		protected override int VisualChildrenCount { get { return visualChilderns.Count; } }
		protected override Visual GetVisualChild(int index) { return visualChilderns[index]; }
		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);
		}
	}

}
