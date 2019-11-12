using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
			NodeEditor.CurrentErrors.CollectionChanged += CurrentErrorsOnCollectionChanged;
		}

		private void CurrentErrorsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			foreach (Exception ex in e.NewItems)
			{
				if (ex.Message == "Dialogue Scene Completed!")
				{
					OutputLog.AddLogItem("Dialogue Scene Completed! :D");
					OutputLog.AddErrorLogItem(0, ex.Message, "BlockNodeEditor", false);
				}
				else if (ex.Message.Contains("Updated Runtime Var"))
				{
					OutputLog.AddLogItem("Updated Global Runtime Var");
					OutputLog.AddErrorLogItem(0, ex.Message, "BlockNodeEditor", true);
				}
				else
				{
					OutputLog.AddErrorLogItem(-1, ex.Message, "BlockNodeEditor", false);
					OutputLog.AddLogItem("Dialogue Error Found! Check Error Log for details.");
				
				}
			}
		}

		private void BaseNode_BTN_Click(object sender, RoutedEventArgs e)
		{
			nodes.Add(new DialogueNodeBlock("Emma"));
		}
		private void BaseNode_BTN_Click1(object sender, RoutedEventArgs e)
		{
			nodes.Add(new GetConstantNodeBlock(ECOnnectionType.Int));
		}
		private void BaseNode_BTN_Click2(object sender, RoutedEventArgs e)
		{

			//nodes.Add(new SetConstantNodeBlock(ECOnnectionType.Int));
		}


	}
}
