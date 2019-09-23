using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NodeEditor.Components
{
	public class ConstantNodeBlock : Button, INotifyPropertyChanged
	{
		public ConnectionNode output = new ConnectionNode();
		public  object data = new object();

		public ConstantNodeBlock()
		{

		}

		public ConstantNodeBlock(ECOnnectionType type)
		{
			output.ConnectedNodes.Add(new ConnectionNode() { Name = "Output1", NodeType = ECOnnectionType.Exit });
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
