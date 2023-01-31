using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BixBite.NodeEditor.Logic;

namespace BixBite.NodeEditor
{
	/// <summary>
	/// THis is the Stopping pointer for a given graph
	/// CONTAINS ONE INPUT (ENTRY NODE)
	/// </summary>
	public partial class ExitBlockNode : BaseNodeBlock
	{
		public ExitBlockNode()
		{
			this.EntryNode = new ConnectionNode(this, "EntryNode", 0, 0, ECOnnectionType.Enter);
			DType = ECOnnectionType.Exit;
		}

		public override void DeleteConnection(EConditionalTypes contype, int row)
		{
			throw new NotImplementedException();
		}

		public override bool EvaluateInternalData(BaseNodeBlock connectedBlock)
		{
			throw new NotImplementedException();
		}

		public override bool NodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			return true;
		}

		public override bool OnEndEvaluateInternalData()
		{
			return true;
		}

		public override void OnEndNodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			this.ActiveStatus = EActiveStatus.Done;
			return;
		}

		public override bool OnStartEvaluateInternalData()
		{
			throw new NotImplementedException();
		}

		public override bool OnStartNodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			//check to make sure the exit node is connected.
			if (this.EntryNode.ConnectedNodes.Count == 0)
			{
				this.ActiveStatus = EActiveStatus.Error;
				return false;
			}

			//this.NodeBlockExecution(ref currentNB);
			this.ActiveStatus = EActiveStatus.Active;
			return true;
		}
	}
}
