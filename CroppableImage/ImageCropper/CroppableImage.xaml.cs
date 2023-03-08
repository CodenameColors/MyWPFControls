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

		private bool _bHasFocus = false;

		public bool bHasFocus
		{
			get => _bHasFocus;
			set
			{
				_bHasFocus = value;
				if (!_bHasFocus)
				{
					CropService?.ClearAdorners(this);
					if (CropService == null)
						CropService = null;
					if (ResizeService == null)
						ResizeService = null;
				}
			}
		}

		public CroppableImage()
		{
			InitializeComponent();
			CropService = null;
			ResizeService = null;
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
			_baseImage.BeginInit();
			_baseImage.UriSource = new Uri(bitmapImagePath, UriKind.Absolute);
			_baseImage.EndInit();


			SourceImage.Source = _baseImage;
			if (bChangeSize)
			{
				if (CropService == null)
					CropService = new CropService(this);

				this.Width = _baseImage.Width;
				this.Height = _baseImage.Height;
				CropService.Adorner.Width = this.Width;
				CropService.Adorner.Height = this.Height;
				//ImageBorder.Width = this.Width;
				//ImageBorder.Height = this.Height;

				RootGrid.Height = _baseImage.Height;
				RootGrid.Width = _baseImage.Width;

				this.UpdateLayout();
				CropService.ClearAdorners(this);
				CropService = new CropService(this);
				CropService.Adorner.UpdateLayout();
				UpdateDefaultStyle();

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
			throw new NotImplementedException();
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
			ResizeService = new ResizeService(this);
			(ResizeService.Adorner as ResizeAdorner).Resize_Hook = ResizeImage;

		}

		private void ResizeImage(int width, int height)
		{

			var bitmap = new TransformedBitmap(_baseImage,
				new ScaleTransform(
				(double)width / _baseImage.PixelWidth,
				(double)height / _baseImage.PixelHeight));

			SourceImage.Source = bitmap;
			//this.Width = width;
			//this.Height = height;
			
			//ImageBorder.Width = width;
			//ImageBorder.Height = height;

			//RootGrid.Height = width;
			//RootGrid.Width = height;

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

	}

}
