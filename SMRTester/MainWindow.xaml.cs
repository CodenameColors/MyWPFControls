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

namespace SMRTester
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			SMRCon.MouseUp += SMRCon_MouseUp;
		}

		private void SMRCon_MouseUp(object sender, MouseButtonEventArgs e)
		{
			Console.WriteLine("MUP");
		}

		private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{

			Console.WriteLine("MW MBLD");

			//get position
			Point Mpos = Mouse.GetPosition(BackCanvas);

			int x0 = (int)Canvas.GetLeft(SMRCon);
			int x1 = (int)Canvas.GetRight(SMRCon);
			int y0 = (int)Canvas.GetTop(SMRCon);
			int y1 = (int)Canvas.GetBottom(SMRCon);


			

			SMRCon.SetVisibility(Visibility.Hidden);

			//if (Mpos.X >= x0 && Mpos.X <= x1)
			//{
			//	if (Mpos.Y >= y0 && Mpos.Y <= y1)
			//	{
			//		Console.WriteLine("clicked in control");
			//	}
			//}

		}
	}
}
