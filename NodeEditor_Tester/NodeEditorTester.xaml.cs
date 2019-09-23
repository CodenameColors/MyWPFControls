using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using NodeEditor;
using NodeEditor.Components;

namespace NodeEditor_Tester
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class NodeEditorTester : Window
	{

		ObservableCollection<object> nodes = new ObservableCollection<object>();

		public NodeEditorTester()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			NodeEditor.ItemsSource = nodes;
		}

		private void BaseNode_BTN_Click(object sender, RoutedEventArgs e)
		{
			nodes.Add(new DialogueNodeBlock() { Header = "Emma" });
			nodes.Add(new ConstantNodeBlock());
		}

		
	}
}
