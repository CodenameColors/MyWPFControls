﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Converters;
using NodeEditor.Components.Logic;
using NodeEditor.Resources;

namespace NodeEditor.Components
{
	public class DialogueNodeBlock : BaseNodeBlock
	{
		public int ChoiceVar = 0;
		public object UnlockingVar = null;
		public DialogueNodeBlock(String Header)
		{
			this.Header = Header;
			InputNodes = new List<ConnectionNode>();
			OutputNodes = new List<ConnectionNode>();
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
					Console.WriteLine(@"Dialogue Eval failed");
					this.ActiveStatus = EActiveStatus.Error;
					return false;
				}
			}
			return temp;
		}

		public override void OnEndNodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			//TODO: make the unlocking variables work
			currentNB = this.OutputNodes[ChoiceVar].ConnectedNodes[0].ParentBlock;
			this.ActiveStatus = EActiveStatus.Disabled;
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
			{
				this.ActiveStatus = EActiveStatus.Error;
				return temp;
			}
			else
			{
				//no error found we can evaluate
				foreach (ConnectionNode cn in this.InputNodes)
				{
					temp &= EvaluateInternalData(cn.ConnectedNodes[0].ParentBlock);
				}

				if (!temp)
				{
					this.ActiveStatus = EActiveStatus.Error;
					return temp;
				}
				else
				{
					temp &= OnEndEvaluateInternalData();
				}
			}
			return temp;
		} 


		/// <summary>
		/// we need to determine what state the expression is in
		/// if connectedBlock is GetConstantBlock then we don't need to do any equations.
		/// </summary>
		/// <param name="connectedBlock"></param>
		public override bool EvaluateInternalData(BaseNodeBlock connectedBlock)
		{
			Console.WriteLine(String.Format("From {0} -> {1}", this.GetType().Name, connectedBlock.GetType().Name));
			bool temp = true;
			if (connectedBlock is GetConstantNodeBlock block)
			{
				ResultsStack.Push(block?.InternalData.VarData);
				Console.WriteLine(String.Format("Result: {0}", ResultsStack.Peek()));
				return true;
			}
			else
			{
				if (connectedBlock.AnswerToOutput != null)
				{
					ResultsStack.Push(connectedBlock.AnswerToOutput);
					Console.WriteLine(String.Format("Result: {0}", ResultsStack.Peek()));
					connectedBlock.AnswerToOutput = null;
				}
				//else if (connectedBlock.NewValue_Constant != null && !(connectedBlock as BaseArithmeticBlock).NewValConnected)
				//{
				//	ResultsStack.Push(Int32.Parse(connectedBlock.NewValue_Constant));
				//	Console.WriteLine(String.Format("Result: {0}", ResultsStack.Peek()));
				//}
				else
				{
					temp &= connectedBlock.OnStartEvaluateInternalData(); //it's not a constant thus we MUST evaluate this node.
					if (temp)
					{
						this.ResultsStack.Push(connectedBlock.AnswerToOutput);
						Console.WriteLine(String.Format("Result: {0}", ResultsStack.Peek()));
						connectedBlock.AnswerToOutput = null;
					}
				}
			}
			return temp;
		}

		public override bool OnEndEvaluateInternalData()
		{
			if (ResultsStack.Count == 2)
			{
				UnlockingVar = ResultsStack.Pop();
				ChoiceVar = (int)ResultsStack.Pop();
			}
			else if (ResultsStack.Count == 1) ChoiceVar = (int) ResultsStack.Pop();

			if (ChoiceVar > this.OutputNodes.Count - 1)
			{
				ErrorStack.Push(new DialogueChoiceInvalidException(this.ChoiceVar, this.OutputNodes));
				return false;
			}
			return true;
		}

		public override void DeleteConnection(EConditionalTypes contype, int row)
		{
			throw new NotImplementedException();
		}

	}
}
