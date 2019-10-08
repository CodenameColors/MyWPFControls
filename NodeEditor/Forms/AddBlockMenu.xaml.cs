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
using System.Windows.Shapes;

namespace NodeEditor.Forms
{
	/// <summary>
	/// Interaction logic for AddBlockMenu.xaml
	/// </summary>
	public partial class AddBlockMenu : Window
	{
		public AddBlockMenu()
		{
			InitializeComponent();
		}

		private void Grid_GotFocus(object sender, RoutedEventArgs e)
		{

		}

		private void Grid_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			this.Close();
		}
	}
}
