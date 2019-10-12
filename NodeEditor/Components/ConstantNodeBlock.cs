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

		private NodeEditor.RuntimeVars data = new NodeEditor.RuntimeVars();
		public NodeEditor.RuntimeVars InternalData
		{
			get { return data; }
			set
			{
				data = value;
				VarHeader = value.VarName;
			}
		}

		public String VarHeader
		{
			get { return data.VarName; }
			set
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("VarHeader"));
			}
		}

		public GetConstantNodeBlock(ECOnnectionType type)
		{
			dtype = type;
			Header = String.Format("Get Constant [{0}]", type.ToString());
			OutputNodes = new List<ConnectionNode>(); //There is only ONE output
			this.OutputNodes.Add(new ConnectionNode(this, "OutputNode1", type));
		}

		public GetConstantNodeBlock(ECOnnectionType type, ref NodeEditor.RuntimeVars varptr)
		{
			dtype = type;
			Header = String.Format("Get Constant [{0}]", type.ToString());
			OutputNodes = new List<ConnectionNode>(); //There is only ONE output
			this.OutputNodes.Add(new ConnectionNode(this, "OutputNode1", type));

			this.InternalData = varptr;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public override void OnStartNodeBlockExecution()
		{
			throw new NotImplementedException();
		}

		public override void NodeBlockExecution()
		{
			throw new NotImplementedException();
		}

		public override void OnEndNodeBlockExecution()
		{
			throw new NotImplementedException();
		}

		public override void OnStartEvaulatInternalData()
		{
			throw new NotImplementedException();
		}

		public override void EvaulatInternalData()
		{
			throw new NotImplementedException();
		}

		public override void OnEndEvaulatInternalData()
		{
			throw new NotImplementedException();
		}
	}

	public class SetConstantNodeBlock : BaseNodeBlock, INotifyPropertyChanged
	{
		public ConnectionNode OldValue
		{
			get { return InputNodes[0]; }
			set
			{
				InputNodes[0] = value;
			}
		}

		public ConnectionNode NewValue
		{
			get { return InputNodes[1]; }
			set
			{
				InputNodes[1] = value;
				if (InputNodes[1].ConnectedNodes.Count > 0) //its now connected so set display
					NewValConnected = false;
			}
		}
		public ConnectionNode OutValue
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

		public SetConstantNodeBlock(ECOnnectionType nodetype, ref NodeEditor.RuntimeVars varptr)
		{
			this.InputNodes = new List<ConnectionNode>();
			this.OutputNodes = new List<ConnectionNode>();
			this.EntryNode = new ConnectionNode(this, "EntryNode", ECOnnectionType.Enter);
			this.ExitNode = new ConnectionNode(this, "ExitNode", ECOnnectionType.Exit);
			this.InputNodes.Add(new ConnectionNode(this, "InputNode1", nodetype));
			this.InputNodes.Add(new ConnectionNode(this, "InputNode2", nodetype));
			this.OutputNodes.Add(new ConnectionNode(this, "OutputNode1", nodetype));

			dtype = nodetype;
			newvalconnected = false;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public override void OnStartNodeBlockExecution()
		{
			throw new NotImplementedException();
		}

		public override void NodeBlockExecution()
		{
			throw new NotImplementedException();
		}

		public override void OnEndNodeBlockExecution()
		{
			throw new NotImplementedException();
		}

		public override void OnStartEvaulatInternalData()
		{
			throw new NotImplementedException();
		}

		public override void EvaulatInternalData()
		{
			throw new NotImplementedException();
		}

		public override void OnEndEvaulatInternalData()
		{
			throw new NotImplementedException();
		}
	}




}
