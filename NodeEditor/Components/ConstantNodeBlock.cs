using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NodeEditor.Components
{
	public class GetConstantNodeBlock : BaseNodeBlock, INotifyPropertyChanged
	{
		public ConnectionNode output
		{
			get { return OutputNodes[0]; } //there is only one output node 
			set
			{
				OutputNodes[0] = value;
			}
		}

		private ECOnnectionType dtype;
		public ECOnnectionType DType
		{
			get { return dtype; }
			set
			{
				dtype = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DType"));

			}
		}

		private RuntimeVars data = new RuntimeVars();
		public RuntimeVars InternalData
		{
			get { return data; }
			set
			{
				data = value;
			}
		}

		public GetConstantNodeBlock(ECOnnectionType type)
		{
			dtype = type;
			Header = String.Format("Get Constant [{0}]", type.ToString());
			OutputNodes = new List<ConnectionNode>(); //There is only ONE output
			OutputNodes.Add(new ConnectionNode() { Name = "Output1", NodeType = type });
		}

		public GetConstantNodeBlock(ECOnnectionType type, ref RuntimeVars varptr)
		{
			dtype = type;
			Header = String.Format("Get Constant [{0}]", type.ToString());
			OutputNodes = new List<ConnectionNode>(); //There is only ONE output
			OutputNodes.Add(new ConnectionNode() { Name = "Output1", NodeType = type });

			this.InternalData = varptr;
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}

	public class SetConstantNodeBlock : BaseNodeBlock, INotifyPropertyChanged
	{
		ConnectionNode OldValue
		{
			get { return InputNodes[0]; }
			set
			{
				InputNodes[0] = value;
			}
		}

		ConnectionNode NewValue
		{
			get { return InputNodes[1]; }
			set
			{
				InputNodes[1] = value;
				if (InputNodes[1].ConnectedNodes.Count > 0) //its now connected so set display
					NewValConnected = false;
			}
		}
		ConnectionNode OutValue
		{
			get { return OutputNodes[0]; }
			set
			{
				OutputNodes[0] = value;
			}
		}

		private ECOnnectionType dtype;
		public ECOnnectionType DType
		{
			get { return dtype; }
			set
			{
				dtype = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DType"));

			}
		}

		private bool newvalconnected;
		public bool NewValConnected
		{
			get { return newvalconnected; }
			set
			{
				newvalconnected = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NewValConnected"));

			}
		}

		public SetConstantNodeBlock()
		{
			this.EntryNode = new ConnectionNode() { Name = "EntryNode", NodeType = ECOnnectionType.Enter };
			this.ExitNode = new ConnectionNode() { Name = "ExitNode", NodeType = ECOnnectionType.Exit };
			this.InputNodes.Add(new ConnectionNode() { Name = "InputNode1", NodeType = ECOnnectionType.Bool });
			this.InputNodes.Add(new ConnectionNode() { Name = "InputNode2", NodeType = ECOnnectionType.Bool });
			this.OutputNodes.Add(new ConnectionNode() { Name = "OutputNode1", NodeType = ECOnnectionType.Bool });
		}

		public SetConstantNodeBlock(ECOnnectionType nodetype)
		{
			this.InputNodes = new List<ConnectionNode>();
			this.OutputNodes = new List<ConnectionNode>();
			this.EntryNode = new ConnectionNode() { Name = "EntryNode", NodeType = ECOnnectionType.Enter };
			this.ExitNode = new ConnectionNode() { Name = "ExitNode", NodeType = ECOnnectionType.Exit };
			this.InputNodes.Add(new ConnectionNode() { Name = "InputNode1", NodeType = nodetype });
			this.InputNodes.Add(new ConnectionNode() { Name = "InputNode2", NodeType = nodetype });
			this.OutputNodes.Add(new ConnectionNode() { Name = "OutputNode1", NodeType = nodetype });

			dtype = nodetype;
			newvalconnected = false;
		}


		public event PropertyChangedEventHandler PropertyChanged;
		

	}




}
