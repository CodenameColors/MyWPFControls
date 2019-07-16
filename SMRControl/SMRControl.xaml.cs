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

namespace SMRControl
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
		/// ITS IMPORTANT THAT THIS CONTROL IS ONLY USED FOR CANVASES
    /// </summary>
    public partial class SMRControl : UserControl
    {

		Point MPos = new Point();
		Point InitialPoint = new Point();
		Point InitialDims = new Point();

    public SMRControl()
    {
        InitializeComponent();
    }


		private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
		{
			InitialPoint = Mouse.GetPosition((Canvas)this.Parent);
			InitialDims.X = this.Width; InitialDims.Y = this.Height;
			Console.WriteLine(InitialPoint.ToString());
		}

		private void Rectangle_MouseUp(object sender, MouseButtonEventArgs e)
		{
			InitialPoint = Mouse.GetPosition((Canvas)this.Parent);
			InitialDims.X = this.Width; InitialDims.Y = this.Height;
			Console.WriteLine(InitialPoint.ToString());
		}

		private void Top_MouseMove(object sender, MouseEventArgs e)
		{
			Point CurrentPos = Mouse.GetPosition((Canvas)this.Parent);
			if (e.LeftButton == MouseButtonState.Pressed)
			{
				if (InitialPoint.Y - CurrentPos.Y >= 0)
				{
					this.Height = InitialDims.Y + (InitialPoint.Y - CurrentPos.Y);
					Canvas.SetTop(this, Canvas.GetTop(this) - (1));
					Console.WriteLine("Increase top");
				}
				else
				{
					this.Height = InitialDims.Y - (CurrentPos.Y - InitialPoint.Y);
					Canvas.SetTop(this, Canvas.GetTop(this) + (1));
					Console.WriteLine("Decrease top");
				}
			}
		}

		private void MoveRect_MouseMove(object sender, MouseEventArgs e)
		{
			//if(e.LeftButton == MouseButtonState.Pressed)
			//{
			//	Canvas.SetLeft(this, Canvas.GetLeft(this) + 1);
			//	Canvas.SetTop(this, Canvas.GetTop(this) + 1);
			//	Console.WriteLine("Moved");
			//}
		}
	}
}
