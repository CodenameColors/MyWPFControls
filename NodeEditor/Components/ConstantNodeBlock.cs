using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NodeEditor.Components
{
	public class GetConstantNodeBlock : Button, INotifyPropertyChanged
	{
		public ConnectionNode output = new ConnectionNode();
		public  object data = new object();

		public GetConstantNodeBlock()
		{
			output = (new ConnectionNode() { Name = "Output1", NodeType = ECOnnectionType.Bool });
		}

		public GetConstantNodeBlock(ECOnnectionType type)
		{
			output = (new ConnectionNode() { Name = "Output1", NodeType = ECOnnectionType.Bool });
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}

	public class SetConstantNodeBlock : Button, INotifyPropertyChanged
	{
		ConnectionNode EntryNode;
		ConnectionNode ExitNode;
		ConnectionNode DesiredValue;
		ConnectionNode NewValue;

		private bool dtype;
		public bool DType
		{
			get { return dtype; }
			set
			{
				dtype = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DType"));

			}
		}

		public SetConstantNodeBlock()
		{
			this.DataContext = dtype;
			DType = true;
			this.EntryNode = new ConnectionNode() { Name = "EntryNode", NodeType = ECOnnectionType.Enter };
			this.ExitNode = new ConnectionNode() { Name = "ExitNode", NodeType = ECOnnectionType.Exit };
			this.DesiredValue = new ConnectionNode() { Name = "InputNode1", NodeType = ECOnnectionType.Bool };
			this.NewValue = new ConnectionNode() { Name = "OutputNode1", NodeType = ECOnnectionType.Bool };
		}

		public SetConstantNodeBlock(ECOnnectionType nodetype)
		{
			this.EntryNode = new ConnectionNode() { Name = "EntryNode", NodeType = ECOnnectionType.Enter };
			this.ExitNode = new ConnectionNode() { Name = "ExitNode", NodeType = ECOnnectionType.Exit };
			this.DesiredValue = new ConnectionNode() { Name = "InputNode1", NodeType = nodetype };
			this.NewValue = new ConnectionNode() { Name = "OutputNode1", NodeType = nodetype };

			if (nodetype == ECOnnectionType.Bool)
				dtype = false;
			else dtype = true;
		}


		public event PropertyChangedEventHandler PropertyChanged;
		

	}




}
