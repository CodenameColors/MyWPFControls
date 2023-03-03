using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CroppingImageLibrary.Services;

namespace ImageCropper
{
    /// <summary>
    /// Interaction logic for CroppableImage.xaml
    /// </summary>
    public partial class CroppableImage : UserControl
    {
				public CropService CropService { get; private set; }
				private BitmapImage _baseImage = new BitmapImage();

				private bool _bHasFocus = false;

				public bool bHasFocus
				{
					get => _bHasFocus;
					set
					{
						_bHasFocus = value;
						if(!_bHasFocus)
						{
							CropService?.ClearAdorners(this);
							if(CropService == null)
								CropService = new CropService(this);
						}
					}
				}
	
				public CroppableImage()
        {
            InitializeComponent();
            CropService = null;
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
					if(bChangeSize)
					{
						if(CropService == null)
							CropService = new CropService(this);

						this.Width = _baseImage.Width;
						this.Height = _baseImage.Height;
						CropService.Adorner.Width = this.Width;
						CropService.Adorner.Height = this.Height;
						ImageBorder.Width = this.Width;
						ImageBorder.Height = this.Height;

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
    }
}
