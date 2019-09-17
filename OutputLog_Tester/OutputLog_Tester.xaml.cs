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

namespace OutputLog_Tester
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class OutputLogTester : Window
	{
		public OutputLogTester()
		{
			InitializeComponent();
		}

		private void AddItem_BTN_Click(object sender, RoutedEventArgs e)
		{
			outputlog.AddErrorLogItem(0, "Compile Successful!", "Cuprite", false);
			outputlog.AddErrorLogItem(19, "Check sum Error", "FOE", false);
			outputlog.AddErrorLogItem(0, "Move successful, but slow", "COE", true);

			outputlog.AddLogItem("It did the thing!");

		}

		private void AddItem_BTN_Copy_Click(object sender, RoutedEventArgs e)
		{
			outputlog.ClearErrorLog();
		}
	}
}
