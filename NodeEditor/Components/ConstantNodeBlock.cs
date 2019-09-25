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
			output = (new ConnectionNode() { Name = "Output1", NodeType = ECOnnectionType.Bool });
		}

		public ConstantNodeBlock(ECOnnectionType type)
		{
			output = (new ConnectionNode() { Name = "Output1", NodeType = ECOnnectionType.Bool });
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
