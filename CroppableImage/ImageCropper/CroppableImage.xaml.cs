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
		public CropService CropService { get; private set; }
		public ResizeService ResizeService { get; private set; }
		private BitmapImage _baseImage = new BitmapImage();
		private CroppedBitmap _croppedImage = null;
		bool _bCanDrag = false;
		bool _bIsDragging = false;
		int _xMouseOffset = 0;
		int _yMouseOffset = 0;

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
					CropService?.ClearAdorners(this);
					ResizeService?.ClearAdorners(this);
					if (CropService != null)
						CropService = null;
					if (ResizeService != null)
						ResizeService = null;
				}
			}
		}

		public CroppableImage()
		{
			InitializeComponent();
			CropService = null;
			ResizeService = null;
			IsHitTestVisible = true;
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{

			var image = new BitmapImage(new Uri("Resources/defaultimage.jpg", UriKind.Relative));
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

		public void SetImage(String bitmapImagePath, bool bChangeSize)
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
					ResizeService = new ResizeService(this);

				this.UpdateLayout();

				xscale = SourceImage.Source.Width / _baseImage.PixelWidth;
				yscale = SourceImage.Source.Height / _baseImage.PixelHeight;

				this.Width = _baseImage.Width;
				this.Height = _baseImage.Height;

				ResizeService?.ClearAdorners(this);
				CropService = new CropService(this);
				CropService?.ClearAdorners(this);

				ResizeService = new ResizeService(this);
				(ResizeService.Adorner as ResizeAdorner).Resize_Hook = ResizeImage;
				_croppedImage = null;

				// Reset the cropped data
				_croppedImage = new CroppedBitmap(_baseImage, new Int32Rect(0, 0, (int)_baseImage.Width, (int)_baseImage.Height));

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

			this.Width = _baseImage.Width;
			this.Height = _baseImage.Height;

			ResizeService?.ClearAdorners(this);
			CropService = new CropService(this);
			CropService?.ClearAdorners(this);

			ResizeService = new ResizeService(this);
			(ResizeService.Adorner as ResizeAdorner).Resize_Hook = ResizeImage;
			_croppedImage = null;

			// Reset the cropped data
			_croppedImage = new CroppedBitmap(_baseImage, new Int32Rect(0, 0, (int)_baseImage.Width, (int)_baseImage.Height));

			
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

		private void ResizeImage(int width, int height)
		{
			xscale = (double)width / _baseImage.PixelWidth;
			yscale = (double)height / _baseImage.PixelHeight;
			if (_croppedImage != null)
			{
				var bitmap = new TransformedBitmap(_croppedImage,
					new ScaleTransform(xscale, yscale));

				SourceImage.Source = bitmap;
			}
			else
			{
				var bitmap = new TransformedBitmap(_baseImage,
					new ScaleTransform(xscale, yscale));

				SourceImage.Source = bitmap;
			}

		}

		private void MoveControl(int x, int y)
		{
			
			if(this.Parent is Canvas parentCanvas)
			{
				Canvas.SetTop(this, y);
				Canvas.SetLeft(this, x);
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
		}

		private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			_bCanDrag = false;
			_bIsDragging = false;

			_xMouseOffset = 0;
			_yMouseOffset = 0;
		}

		private void UserControl_MouseMove(object sender, MouseEventArgs e)
		{
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

					Canvas.SetTop(this, yrenderPos);
					Canvas.SetLeft(this, xrenderPos);

					Canvas.SetTop(SourceImage, yrenderPos);
					Canvas.SetLeft(SourceImage, xrenderPos);

					Canvas.SetTop(RootGrid, yrenderPos);
					Canvas.SetLeft(RootGrid, xrenderPos);
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
				
				(ResizeService.Adorner as ResizeAdorner).Resize_Hook = ResizeImage;

				_croppedImage = new CroppedBitmap(_croppedImage, new Int32Rect(xstart, ystart, width, height));

				SourceImage.Source = _croppedImage;

				//Crop the control to the cropped image dimensions
				this.Width = (int)CropService.GetCroppedArea().CroppedRectAbsolute.Width;
				this.Height = (int)CropService.GetCroppedArea().CroppedRectAbsolute.Height;
				this.UpdateLayout();

				// Now we need to move the starting drawing point!
				if (this.Parent is Canvas parentCanvas)
				{
					Canvas.SetLeft(this, (Canvas.GetLeft(this) + (int)CropService.GetCroppedArea().CroppedRectAbsolute.Left));
					Canvas.SetTop(this, (Canvas.GetTop(this) + (int)CropService.GetCroppedArea().CroppedRectAbsolute.Top));
				}

				CropService = new CropService(this);
				ResizeService = new ResizeService(this);
			}
		}
	}

}
