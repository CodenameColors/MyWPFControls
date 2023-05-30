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
using CroppingImageLibrary.Services;
using ImageCropper.Components;

namespace ImageCropper
{
	/// <summary>
	/// Interaction logic for CroppableImage.xaml
	/// </summary>
	public partial class CroppableImage : UserControl
	{
		public delegate void SetRenderPoint_Hook(int x, int y);
		public SetRenderPoint_Hook setRenderPoint_Hook;

		public delegate void UpdateSizeLocation_Hook(double x, double y, double w, double h);
		public UpdateSizeLocation_Hook updateSizeLocation_Hook;

		public delegate void UpdateCropLocation_Hook(double cx, double cy);
		public UpdateCropLocation_Hook updateCropLocation_Hook;

		public CropService CropService { get; private set; }
		public ResizeService ResizeService { get; private set; }
		public RenderPointService RenderPointService { get; private set; }

		private BitmapImage _baseImage = new BitmapImage();
		private CroppedBitmap _croppedImage = null;
		private  FrameworkElement _parentFrameworkElement = null;
		bool _bCanDrag = false;
		bool _bIsDragging = false;
		int _xMouseOffset = 0;
		int _yMouseOffset = 0;

		double XPos = 0.0f;
		double YPos = 0.0f;
		double xscale = 1.0f;
		double yscale = 1.0f;

		private bool _bHasFocus = false;

		public bool bHasFocus
		{
			get => _bHasFocus;
			set
			{
				_bHasFocus = value;
				if (!_bHasFocus && !_bCanDrag)
				{
					if (_parentFrameworkElement != null)
					{
						CropService?.ClearAdorners(_parentFrameworkElement);
						ResizeService?.ClearAdorners(_parentFrameworkElement);
						RenderPointService?.ClearAdorners(_parentFrameworkElement);
					}
					else
					{
						CropService?.ClearAdorners(this);
						ResizeService?.ClearAdorners(this);
						RenderPointService?.ClearAdorners(this);
					}
					if (CropService != null)
						CropService = null;
					if (ResizeService != null)
						ResizeService = null;
					if (RenderPointService != null)
						RenderPointService = null;
				}
			}
		}

		public CroppableImage()
		{
			InitializeComponent();
			CropService = null;
			ResizeService = null;
			RenderPointService = null;
			IsHitTestVisible = true;
		}

		public CroppableImage(FrameworkElement frameworkElement)
		{
			InitializeComponent();
			_parentFrameworkElement = frameworkElement;
			CropService = null;
			ResizeService = null;
			RenderPointService = null;
			IsHitTestVisible = true;
		}

		public CroppedBitmap GetCroppedBitmap()
		{
			return _croppedImage;
		}

		public void SetParentControl(FrameworkElement frameworkElement)
		{
			_parentFrameworkElement = frameworkElement;
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{

			var image = new BitmapImage(new Uri("Resources/defaultimage.jpg", UriKind.Relative));

			if (!IsLoaded)
			{
				// The control has not been added to the visual tree yet, wait until it is loaded
				Loaded += UserControl_Loaded;
				return;
			}

			if (_parentFrameworkElement != null)
			{
				CropService = new CropService(_parentFrameworkElement);
				ResizeService = new ResizeService(_parentFrameworkElement);
				RenderPointService = new RenderPointService(_parentFrameworkElement);
			}
			_baseImage = image;
			SourceImage.Source = image;

		}

		private void RootGrid_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			CropService?.Adorner.RaiseEvent(e);
		}

		private void RootGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			CropService?.Adorner.RaiseEvent(e);

			
		}

		private void RootGrid_Loaded(object sender, RoutedEventArgs e)
		{
			// CropService = new CropService(this);

		}

		public String GetImagePath()
		{
			if(_baseImage.UriSource.IsAbsoluteUri)
				return (_baseImage?.UriSource.AbsolutePath);
			return null;
		}

		public void SetImage(String bitmapImagePath, bool bChangeSize, Int32Rect croppedRect = new Int32Rect(), FrameworkElement parent = null)
		{
			_baseImage = new BitmapImage();
			_baseImage.BeginInit();
			_baseImage.UriSource = new Uri(bitmapImagePath, UriKind.Absolute);
			_baseImage.EndInit();


			SourceImage.Source = _baseImage;
			SourceImage.Stretch = Stretch.Fill;
			if (bChangeSize)
			{
				if (ResizeService == null)
				{
					if (_parentFrameworkElement == null)
						ResizeService = new ResizeService(this);
					else
						ResizeService = new ResizeService(_parentFrameworkElement);
				}

				if (RenderPointService == null)
				{
					if (_parentFrameworkElement == null)
						RenderPointService = new RenderPointService(this);
					else
						RenderPointService = new RenderPointService(_parentFrameworkElement);

				}


				this.UpdateLayout();

				xscale = SourceImage.DesiredSize.Width / _baseImage.PixelWidth;
				yscale = SourceImage.DesiredSize.Height / _baseImage.PixelHeight;

				this.Width = _baseImage.PixelWidth;
				this.Height = _baseImage.PixelHeight;

				if (_parentFrameworkElement == null)
				{
					ResizeService?.ClearAdorners(this);
					CropService = new CropService(this);
					CropService?.ClearAdorners(this);
					ResizeService = new ResizeService(this);
				}
				else
				{
					ResizeService?.ClearAdorners(_parentFrameworkElement);
					CropService = new CropService(_parentFrameworkElement);
					CropService?.ClearAdorners(_parentFrameworkElement);
					ResizeService = new ResizeService(_parentFrameworkElement);
				}


				(ResizeService.Adorner as ResizeAdorner).Resize_Hook = ResizeImage;
				_croppedImage = null;

				// Reset the cropped data
				if (croppedRect == Int32Rect.Empty)
					_croppedImage = new CroppedBitmap(_baseImage, new Int32Rect(0, 0, (int)_baseImage.PixelWidth, (int)_baseImage.PixelHeight));
				else
				{
					_croppedImage = new CroppedBitmap(_baseImage, croppedRect);
					SourceImage.Source = _croppedImage;
					SourceImage.Stretch = Stretch.Fill;
				}
			}
		}

		private void CropImage_OnClick(object sender, RoutedEventArgs e)
		{
			CropService?.ClearAdorners(this);
			if (CropService != null)
				CropService = null;
			CropService = new CropService(this);
		}

		private void ResetCropSize_OnClick(object sender, RoutedEventArgs e)
		{
			SourceImage.Source = _baseImage;
			SourceImage.Stretch = Stretch.Fill;

			if (ResizeService == null)
				ResizeService = new ResizeService(this);

			this.UpdateLayout();

			xscale = SourceImage.Source.Width / _baseImage.PixelWidth;
			yscale = SourceImage.Source.Height / _baseImage.PixelHeight;

			this.Width = _baseImage.PixelWidth;
			this.Height = _baseImage.PixelHeight;

			ResizeService?.ClearAdorners(this);
			CropService = new CropService(this);
			CropService?.ClearAdorners(this);
			ResizeService = new ResizeService(this);
			(ResizeService.Adorner as ResizeAdorner).Resize_Hook = ResizeImage;

			_croppedImage = null;

			// Reset the cropped data
			_croppedImage = new CroppedBitmap(_baseImage, new Int32Rect(0, 0, (int)_baseImage.PixelWidth, (int)_baseImage.PixelHeight));

			
		}

		private void ImageBorder_OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
		{
			ContextMenu cm = this.FindResource("CroppableImage_CM") as ContextMenu;
			cm.PlacementTarget = sender as Grid;
			cm.IsOpen = true;
		}

		private void RootGrid_OnLostFocus(object sender, RoutedEventArgs e)
		{
			CropService?.ClearAdorners(this);
			CropService = null;
		}

		private void SourceImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			//var adornerLayer = AdornerLayer.GetAdornerLayer(this);
			//if (adornerLayer.GetAdorners(adornerLayer) == null || adornerLayer.GetAdorners(adornerLayer)?.Contains(_dragAdorner) == false)
			//{ 
			//	_dragAdorner = new DragAdorner(this);
			//	adornerLayer?.Add(_dragAdorner);
			//}
			if (ResizeService == null)
			{
				ResizeService?.ClearAdorners(this);
				ResizeService = new ResizeService(this);
				(ResizeService.Adorner as ResizeAdorner).Resize_Hook = ResizeImage;
				(ResizeService).MoveParent_hook = MoveControl;
			}

		}

		private void ResizeImage(double width, double height)
		{
			xscale = (double)width / _baseImage.PixelWidth;
			yscale = (double)height / _baseImage.PixelHeight;
			if (_croppedImage != null)
			{
				var bitmap = new TransformedBitmap(_croppedImage,
					new ScaleTransform(xscale, yscale));

				SourceImage.Source = bitmap;
				this.MaxWidth = width;
				this.MaxHeight = height;

				this.Width = this.MaxWidth;
				this.Height = this.MaxHeight;
			}
			else
			{
				var bitmap = new TransformedBitmap(_baseImage,
					new ScaleTransform(xscale, yscale));

				SourceImage.Source = bitmap;
				this.MaxWidth = width;
				this.MaxHeight = height;

				this.Width = this.MaxWidth;
				this.Height = this.MaxHeight;
				
			}

			if (updateSizeLocation_Hook != null)
			{
				int xRenderPos = (int)Canvas.GetLeft(this);
				int yRenderPos = (int)Canvas.GetTop(this);

				updateSizeLocation_Hook(xRenderPos, yRenderPos, this.Width, this.Height);
			}
		}

		private void MoveControl(int x, int y)
		{
			
			if(this.Parent is Canvas parentCanvas)
			{
				Canvas.SetTop(this, y);
				Canvas.SetLeft(this, x);

				this.XPos = x;
				this.YPos = y;

				if(updateSizeLocation_Hook != null)
				{
					updateSizeLocation_Hook(x, y, this.Width, this.Height);
				}
			}
		}

		private static BitmapFrame CreateResizedImage(ImageSource source, int width, int height, int margin)
		{
			var rect = new Rect(margin, margin, width - margin * 2, height - margin * 2);

			var group = new DrawingGroup();
			RenderOptions.SetBitmapScalingMode(group, BitmapScalingMode.HighQuality);
			group.Children.Add(new ImageDrawing(source, rect));

			var drawingVisual = new DrawingVisual();
			using (var drawingContext = drawingVisual.RenderOpen())
				drawingContext.DrawDrawing(group);

			var resizedImage = new RenderTargetBitmap(
					width, height,         // Resized dimensions
					96, 96,                // Default DPI values
					PixelFormats.Default); // Default pixel format
			resizedImage.Render(drawingVisual);

			return BitmapFrame.Create(resizedImage);
		}

		private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (Parent is Canvas parentCanvas)
			{
				_bCanDrag = true;

				// let's get the mouse position and find the offset from top left to mouse.
				_bCanDrag = true;
				int xMousePos = (int)e.GetPosition(parentCanvas).X;
				int yMousePos = (int)e.GetPosition(parentCanvas).Y;

				int xRenderPos = (int)Canvas.GetLeft(this);
				int yRenderPos = (int)Canvas.GetTop(this);

				_xMouseOffset = xMousePos - xRenderPos;
				_yMouseOffset = yMousePos - yRenderPos;
			}

			// Capture the mouse events to allow the child control to continue to receive them
			(sender as UIElement).CaptureMouse();

			// Handle the mouse down event on the child control here
			// Make sure to set e.Handled = true to prevent the event from bubbling up to the parent control
			e.Handled = true;

		}

		private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			_bCanDrag = false;
			_bIsDragging = false;

			_xMouseOffset = 0;
			_yMouseOffset = 0;

			// Release the captured mouse events
			(sender as UIElement).ReleaseMouseCapture();
		}

		private void UserControl_MouseMove(object sender, MouseEventArgs e)
		{
			Console.WriteLine("moving");
			if (_bCanDrag)
			{
				if (Parent is Canvas parentCanvas)
				{
					_bIsDragging = true;

					int xrenderPos = (int)e.GetPosition(parentCanvas).X - _xMouseOffset;
					int yrenderPos = (int)e.GetPosition(parentCanvas).Y - _yMouseOffset;

					// Is this out of bounds?
					if (xrenderPos < 0)
						xrenderPos = 0;
					if (yrenderPos < 0)
						yrenderPos = 0;

					XPos = xrenderPos;
					YPos = yrenderPos;

					Canvas.SetTop(this, yrenderPos);
					Canvas.SetLeft(this, xrenderPos);

					Canvas.SetTop(SourceImage, yrenderPos);
					Canvas.SetLeft(SourceImage, xrenderPos);

					Canvas.SetTop(RootGrid, yrenderPos);
					Canvas.SetLeft(RootGrid, xrenderPos);

					if (updateSizeLocation_Hook != null)
					{
						updateSizeLocation_Hook(xrenderPos, yrenderPos, this.Width, this.Height);
					}
				}
			}
		}

		private void UserControl_MouseLeave(object sender, MouseEventArgs e)
		{
			if(_bIsDragging)
			{
				_bCanDrag = false;
			}
		}

		public void ShowBorder()
		{
			CroppableImage_Border.Visibility = Visibility.Visible;
		}

		public void HideBorder()
		{
			CroppableImage_Border.Visibility = Visibility.Hidden;
		}

		private void ConfirmCrop_BTN_Click(object sender, RoutedEventArgs e)
		{
			// Crop the image
			if (CropService != null)
			{
				if (ResizeService == null)
					ResizeService = new ResizeService(this);

				int xstart = (int)(CropService.GetCroppedArea().CroppedRectAbsolute.Left / xscale);
				int ystart = (int)(CropService.GetCroppedArea().CroppedRectAbsolute.Top / yscale);
				int width = (int)(CropService.GetCroppedArea().CroppedRectAbsolute.Width / xscale);
				int height = (int)(CropService.GetCroppedArea().CroppedRectAbsolute.Height / yscale);

				if (width < 0 || height < 0)
				{ 
					Console.WriteLine("WTF negative height?!?");
					return;
				}

				CropService?.ClearAdorners(this);
				ResizeService?.ClearAdorners(this);
				RenderPointService?.ClearAdorners(this);

				(ResizeService.Adorner as ResizeAdorner).Resize_Hook = ResizeImage;

				try
				{
					_croppedImage = new CroppedBitmap(_croppedImage, new Int32Rect(xstart, ystart, width, height));
					SourceImage.Source = _croppedImage;

					// Update controls that are binded
					if (updateCropLocation_Hook != null)
					{
						updateCropLocation_Hook(xstart, ystart);
					}

					//Crop the control to the cropped image dimensions
					this.Width = (int) CropService.GetCroppedArea().CroppedRectAbsolute.Width;
					this.Height = (int) CropService.GetCroppedArea().CroppedRectAbsolute.Height;
					this.UpdateLayout();

					// Now we need to move the starting drawing point!
					if (this.Parent is Canvas parentCanvas)
					{
						Canvas.SetLeft(this, (Canvas.GetLeft(this) + (int) CropService.GetCroppedArea().CroppedRectAbsolute.Left));
						Canvas.SetTop(this, (Canvas.GetTop(this) + (int) CropService.GetCroppedArea().CroppedRectAbsolute.Top));
					}

					CropService = new CropService(this);
					ResizeService = new ResizeService(this);
					RenderPointService = new RenderPointService(this);
				}
				catch (Exception ex)
				{
					Console.WriteLine("Invalid Crop region given");
				}
				
				// e.Handled = true;

			}
		}

		private void RootGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			// e.Handled = true;
		}

		private void CroppableImage_KeyDown(object sender, KeyEventArgs e)
		{

			if (ResizeService == null)
			{
				Console.WriteLine("Nothing Selected");
			}
			else
			{
				Console.WriteLine(" Selected");

			}

		}

		private void SetRenderPoint_MI_Click(object sender, RoutedEventArgs e)
		{
			RenderPointService?.ClearAdorners(this);
			if (RenderPointService != null)
				RenderPointService = null;
			RenderPointService = new RenderPointService(this);
			RenderPointService.SetRenderPoint_hook = SetRenderPoint_ToControls;
		}

		// We have found the renderpoint we need. So let's stop the service
		private void SetRenderPoint_ToControls(int x, int y)
		{
			// Kill the adorners. kill the service.
			RenderPointService?.ClearAdorners(this);
			if (RenderPointService != null)
				RenderPointService = null;

			if (this.setRenderPoint_Hook != null)
				setRenderPoint_Hook(x, y);
		}

	}
}
