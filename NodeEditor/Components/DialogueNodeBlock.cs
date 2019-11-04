using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NodeEditor.Components
{
	public class DialogueNodeBlock : BaseNodeBlock, INotifyPropertyChanged
	{
		public List<object> NodeData { get; set; }



		public DialogueNodeBlock(String Header)
		{
			this.Header = Header;
			InputNodes = new List<ConnectionNode>();
			OutputNodes = new List<ConnectionNode>();
			NodeData = new List<object>();
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
		}

		public override void OnStartNodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			currentNB.bIsActive = true;
		}

		public override void NodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			//what state are we in?
			if(this.InputNodes.Count == 0)
			{
				//there are no input nodes, thus we are in a single dialogue option. so no eval needed
				
			}
			else
			{
				//there are input nodes so we need to determine what state the block is
				if(this.InputNodes.Count == 1)
				{
					//we do not have an unlocking variable we need to check for.
					//only check for the FIRST input 
					if (this.OnStartEvaluateInternalData(this.InputNodes[0], out BaseNodeBlock connectedBlock))
					{
						EvaluateInternalData(connectedBlock, out TODO);

					}
				}
				else
				{

				}
			}
		}

		public override void OnEndNodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			throw new NotImplementedException();
		}

		//make sure there are connections
		public override bool OnStartEvaluateInternalData(ConnectionNode desiredNode, out BaseNodeBlock connectedBlock)
		{
			bool isValid = true; //by default
			connectedBlock = null;
			if (desiredNode.ConnectedNodes.Count > 0)
			{
				//assume there is only one connection.
				connectedBlock = desiredNode.ConnectedNodes[0].ParentBlock;
			}
			else isValid = false;
			return isValid;
		}


		/// <summary>
		/// we need to determine what state the expression is in
		/// if connectedBlock is GetConstantBlock then we don't need to do any equations.
		/// </summary>
		/// <param name="connectedBlock"></param>
		/// <param name="retVal"></param>
		public override bool EvaluateInternalData(BaseNodeBlock connectedBlock, out object retVal)
		{
			bool ret = true;
			retVal = null; 
			if (connectedBlock is GetConstantNodeBlock)
			{
				retVal = (connectedBlock as GetConstantNodeBlock).InternalData.VarData;
			}
			else if (connectedBlock is SetConstantNodeBlock || connectedBlock is BaseLogicNodeBlock
			                                                || connectedBlock is BaseArithmeticBlock)
			{
				//this state means that we need to evaluate the next connected node.
				
			}

			return ret;
		}

		public override void OnEndEvaluateInternalData()
		{
			throw new NotImplementedException();
		}

		public override void DeleteConnection(EConditionalTypes contype, int row)
		{
			throw new NotImplementedException();
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
