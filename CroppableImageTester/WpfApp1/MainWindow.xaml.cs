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
using ImageCropper;

namespace CroppableImageTester
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void ImportNewImage_BTN_Click(object sender, RoutedEventArgs e)
		{
			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
			{
				Title = "Import PNG File",
				FileName = "", //default file name
				Filter = "Dialogue Scene files (*.png)|*.png",
				RestoreDirectory = true
			};

			Nullable<bool> result = dlg.ShowDialog();
			// Process save file dialog box results
			string filename = "";
			if (result == true)
			{
				// Save document
				filename = dlg.FileName;
				filename = filename.Substring(0, filename.LastIndexOfAny(new Char[] { '/', '\\' }));
			}
			else return; //invalid name

			Console.WriteLine(dlg.FileName);
			int charcnt = 0;
			//DIALOGUE SCENE HOOKS

			CroppableImage.SetImage(dlg.FileName, true);

			//Rectangle r = new Rectangle()
			//{
			//	Width = (int)sprite.GetPropertyData("width"),
			//	Height = (int)sprite.GetPropertyData("height"),
			//	Fill = new ImageBrush(img.Source)
			//}; //Make a rectange the size of the image

			//ContentControl CC = ((ContentControl)this.TryFindResource("MoveableImages_Template"));
			//CC.Width = (int)sprite.GetPropertyData("width");
			//CC.Height = (int)sprite.GetPropertyData("height");

			//Canvas.SetLeft(CC, (int)sprite.GetPropertyData("x"));
			//Canvas.SetTop(CC, (int)sprite.GetPropertyData("y"));
			//Canvas.SetZIndex(CC, Zindex);
			//Selector.SetIsSelected(CC, false);
			//CC.MouseRightButtonDown += ContentControl_MouseLeftButtonDown;
			//((Rectangle)CC.Content).Fill = new ImageBrush(img.Source);
			//LevelEditor_Canvas.Children.Add(CC);
		}

		private void PrintOutAreaToLog_BTN_Click(object sender, RoutedEventArgs e)
		{
			CropArea ca = CroppableImage.CropService.GetCroppedArea();
			
			outputlog.AddErrorLogItem(0, String.Format("Area Found: Xpos:{0}, Ypos:{1} | [Width:{2}, Height:{3}]", ca.CroppedRectAbsolute.X, ca.CroppedRectAbsolute.Y,
				ca.CroppedRectAbsolute.Width, ca.CroppedRectAbsolute.Height), "Croppable Image", false);
		}

		private void Canvas_On_Click(object sender, MouseButtonEventArgs e)
		{
			Canvas c = sender as Canvas;
			if (c != null)
			{
				ClearAdorners(c);
				CroppableImage.bHasFocus = false;
			}
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
