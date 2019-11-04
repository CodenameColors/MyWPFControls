using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeEditor.Components
{
	public class ConditionalNodeBlock : BaseNodeBlock, INotifyPropertyChanged
	{
		public EConditionalTypes CondType = EConditionalTypes.Equals;

		public ConnectionNode Cond1
		{
			get { return this.InputNodes[0]; }
			set { this.InputNodes[0] = value; }
		}

		public ConnectionNode Cond2
		{
			get { return this.InputNodes[1]; }
			set { this.InputNodes[1] = value; }
		}

		public ConnectionNode TrueOutput
		{
			get { return OutputNodes[0]; }
			set { OutputNodes[0] = value; }
		}

		public ConnectionNode FalseOutput
		{
			get { return OutputNodes[1]; }
			set { OutputNodes[1] = value; }
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

		public event PropertyChangedEventHandler PropertyChanged;

		public ConditionalNodeBlock(ECOnnectionType nodetype)
		{
			this.InputNodes = new List<ConnectionNode>();
			this.OutputNodes = new List<ConnectionNode>();
			this.EntryNode = new ConnectionNode(this, "EntryNode", ECOnnectionType.Enter);
			this.InputNodes.Add(new ConnectionNode(this, "InputNode1", nodetype));
			this.InputNodes.Add(new ConnectionNode(this, "InputNode2", nodetype));
			this.OutputNodes.Add(new ConnectionNode(this, "OutputNode1", ECOnnectionType.Exit));
			this.OutputNodes.Add(new ConnectionNode(this, "OutputNode2", ECOnnectionType.Exit));
			this.DType = nodetype;
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
		}

		public override bool EvaluateInternalData(BaseNodeBlock connectedBlock)
		{
			throw new NotImplementedException();
		}

		public override boolean NodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			throw new NotImplementedException();
		}

		public override void OnEndEvaluateInternalData()
		{
			throw new NotImplementedException();
		}

		public override void OnEndNodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			throw new NotImplementedException();
		}

		public override bool OnStartEvaluateInternalData()
		{
			throw new NotImplementedException();
		}

		public override bool OnStartNodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			throw new NotImplementedException();
		}

		public override void DeleteConnection(EConditionalTypes contype, int row)
		{
			throw new NotImplementedException();
		}

	}
}
