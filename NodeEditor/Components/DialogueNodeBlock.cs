using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using NodeEditor.Resources;

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

		/// <summary>
		/// check to make sure all the nodes on this block have valid connection points.
		/// </summary>
		/// <param name="currentNB"></param>
		/// <returns></returns>
		public override bool OnStartNodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			bool temp = true;
			int i = 0;
			this.ActiveStatus = EActiveStatus.Active;
			foreach (ConnectionNode cn in InputNodes)
			{
				if (!(cn.ConnectedNodes.Count > 0))
				{
					temp = false;
					ErrorStack.Push(new InputNodeConnectionException(i, this.GetType().Name ));
				}
				i++;
			}

			i = 0;
			foreach (ConnectionNode cn in OutputNodes)
			{
				if (!(cn.ConnectedNodes.Count > 0))
				{
					temp = false;
					ErrorStack.Push(new OutputNodeConnectionException(i, this.GetType().Name));
				}
				i++;
			}

			if (!(EntryNode.ConnectedNodes.Count > 0))
			{
				temp = false;
				ErrorStack.Push(new EntryNodeConnectionException(this.GetType().Name));
			}


			if (!temp)
				this.ActiveStatus = EActiveStatus.Error;
			return temp;
		}

		public override bool NodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			bool temp = true;
			if (this.InputNodes.Count == 0)
			{
				return true;
			}
			else
			{
				//we need to evaluate the inputs to determine the calculated output.
				if (!this.OnStartEvaluateInternalData())
				{
					//error found
				}
				else
				{
					//no error found we can evaluate
					foreach (ConnectionNode cn in InputNodes)
					{
						temp &= EvaluateInternalData(cn.ParentBlock);
					}

					if (temp)
					{
						OnEndEvaluateInternalData();
					}
				}
			}
		}

		public override void OnEndNodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			throw new NotImplementedException();
		}

		//make sure there are connections
		public override bool OnStartEvaluateInternalData()
		{
			bool temp = true;
			int i = 0;
			this.ActiveStatus = EActiveStatus.Active;
			foreach (ConnectionNode cn in InputNodes)
			{
				if (!(cn.ConnectedNodes.Count > 0))
				{
					temp = false;
					ErrorStack.Push(new InputNodeConnectionException(i, this.GetType().Name));
				}
				i++;
			}

			if (!temp)
				this.ActiveStatus = EActiveStatus.Error;
			return temp;
		}


		/// <summary>
		/// we need to determine what state the expression is in
		/// if connectedBlock is GetConstantBlock then we don't need to do any equations.
		/// </summary>
		/// <param name="connectedBlock"></param>
		public override bool EvaluateInternalData(BaseNodeBlock connectedBlock)
		{
			foreach (ConnectionNode cn in InputNodes)
			{
				if (!(cn.ConnectedNodes[0].ParentBlock is GetConstantNodeBlock))
				{
					//it's not a constant thus we MUST evaluate this node.
					cn.ConnectedNodes[0].ParentBlock.OnStartEvaluateInternalData();
				}
			}
			return false;
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
