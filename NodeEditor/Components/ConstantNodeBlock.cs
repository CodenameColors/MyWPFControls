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

		public override void OnStartNodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			throw new NotImplementedException();
		}

		public override void NodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			throw new NotImplementedException();
		}

		public override void OnEndNodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			throw new NotImplementedException();
		}

		public override bool OnStartEvaluateInternalData(ConnectionNode desiredNode, out BaseNodeBlock connectedBlock)
		{
			throw new NotImplementedException();
		}

		public override bool EvaluateInternalData(BaseNodeBlock connectedBlock, out object retVal)
		{
			throw new NotImplementedException();
		}

		public override void OnEndEvaluateInternalData()
		{
			throw new NotImplementedException();
		}

		public override void DeleteConnection(EConditionalTypes contype, int row)
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
					NewValConnected = true;
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

		public SetConstantNodeBlock(ECOnnectionType nodetype)
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

		public override void OnStartNodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			throw new NotImplementedException();
		}

		public override void NodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			throw new NotImplementedException();
		}

		public override void OnEndNodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			throw new NotImplementedException();
		}

		public override bool OnStartEvaluateInternalData(ConnectionNode desiredNode, out BaseNodeBlock connectedBlock)
		{
			throw new NotImplementedException();
		}

		public override bool EvaluateInternalData(BaseNodeBlock connectedBlock, out object retVal)
		{
			throw new NotImplementedException();
		}

		public override void OnEndEvaluateInternalData()
		{
			throw new NotImplementedException();
		}

		public override void DeleteConnection(EConditionalTypes contype, int row)
		{
			throw new NotImplementedException();
		}
	}




}
