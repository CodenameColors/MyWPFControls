using System;
using System.ComponentModel;
using BixBite.NodeEditor.Logic;
using BixBite.Resources;

namespace BixBite.NodeEditor
{
	/// <summary>
	/// This is the starting pointer for a given graph
	/// CONTAINS ONE OUTPUT (EXIT NODE)
	/// </summary>
	public partial class StartBlockNode : BaseNodeBlock, INotifyPropertyChanged, IStateMachineTraversal
	{
		public StartBlockNode()
		{
			this.ExitNode = new ConnectionNode(this, "ExitNode", 0, 0, ECOnnectionType.Exit);
			DType = ECOnnectionType.Enter;
		}

		public override bool OnStartEvaluateInternalData()
		{
			throw new NotImplementedException();
		}

		public override bool EvaluateInternalData(BaseNodeBlock connectedBlock)
		{
			throw new NotImplementedException();
		}

		public override bool OnEndEvaluateInternalData()
		{
			throw new NotImplementedException();
		}

		public override void DeleteConnection(EConditionalTypes contype, int row)
		{
			throw new NotImplementedException();
		}

		public override bool OnStartNodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			//check to make sure the exit node is connected.
			if (this.ExitNode.ConnectedNodes.Count == 0)
			{
				this.ActiveStatus = EActiveStatus.Error;
				return false;
			}

			this.NodeBlockExecution(ref currentNB);
			this.ActiveStatus = EActiveStatus.Active;
			return true;
		}

		public override bool NodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			return true;
		}

		public override void OnEndNodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			if (this.ExitNode.ConnectedNodes.Count > 0)
			{
				currentNB = this.ExitNode.ConnectedNodes[0].ParentBlock;
				this.ActiveStatus = EActiveStatus.Disabled;
				//currentNB.OnStartNodeBlockExecution(ref currentNB);
			}
		}
	}
}	

