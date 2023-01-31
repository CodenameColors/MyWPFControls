using System;
using System.Collections.Generic;
using BixBite.NodeEditor.Logic;
using BixBite.Resources;

namespace BixBite.NodeEditor
{
	public class ConditionalNodeBlock : BaseNodeBlock
	{
		/// <summary>
		/// If true then this if statement will continue down the true path connection. else false
		/// </summary>
		public bool bOutputTrue = true;
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

		private RuntimeVars data = new RuntimeVars();

		public ConditionalNodeBlock(ECOnnectionType nodetype, EConditionalTypes condType, bool bInitNode = true)
		{
			this.InputNodes = new List<ConnectionNode>();
			this.OutputNodes = new List<ConnectionNode>();
			this.EntryNode = new ConnectionNode(this, "EntryNode", ECOnnectionType.Enter);
			if (bInitNode)
			{
				this.InputNodes.Add(new ConnectionNode(this, "InputNode1", nodetype));
				this.InputNodes.Add(new ConnectionNode(this, "InputNode2", nodetype));
				this.OutputNodes.Add(new ConnectionNode(this, "OutputNode1", ECOnnectionType.Exit));
				this.OutputNodes.Add(new ConnectionNode(this, "OutputNode2", ECOnnectionType.Exit));
			}

			this.DType = nodetype;
			this.CondType = condType;
		}

		//public override void OnApplyTemplate()
		//{
		//	base.OnApplyTemplate();
		//}

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
					if (InputNodes.Count - 1 == i && (!NewValConnected && NewValue_Constant != ""))
						continue;
					temp = false;
					ErrorStack.Push(new InputNodeConnectionException(i, this.GetType().Name));
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

		/// <summary>
		/// This method is here to check where or not execution can occur.
		/// It also checks every eval node before to make sure the "expression" is valid
		/// </summary>
		/// <param name="currentNB"></param>
		/// <returns></returns>
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
			if (bOutputTrue)
				currentNB = TrueOutput.ConnectedNodes[0].ParentBlock;
			else
				currentNB = FalseOutput.ConnectedNodes[0].ParentBlock;
			this.ActiveStatus = EActiveStatus.Disabled;
		}

		/// <summary>
		/// make sure there are connections
		/// </summary>
		/// <returns></returns>
		public override bool OnStartEvaluateInternalData()
		{
			bool temp = true;
			int i = 0;
			this.ActiveStatus = EActiveStatus.Active;
			foreach (ConnectionNode cn in InputNodes)
			{
				if (!(cn.ConnectedNodes.Count > 0))
				{
					if (InputNodes.Count - 1 == i && (!NewValConnected && NewValue_Constant != ""))
						continue;
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
					if (cn.ConnectedNodes.Count != 0)
						temp &= EvaluateInternalData(cn.ConnectedNodes[0].ParentBlock);
					else //this is here for the constants that one can manually enter.
					{
						if(this.NewValue_Constant == "T")
							ResultsStack.Push(true);
						else if (this.NewValue_Constant == "F")
							ResultsStack.Push(false);
						else
						{
							ResultsStack.Push(Int32.Parse(this.NewValue_Constant));
						}
						Console.WriteLine(String.Format("Result: {0}", ResultsStack.Peek()));
					}
					//temp &= EvaluateInternalData(cn.ConnectedNodes[0].ParentBlock);
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
			object in1, in2 = null;
			//there should ALWAYS be 2 in the result stack
			in2 = ResultsStack.Pop();
			in1 = ResultsStack.Pop();

			if (DType == ECOnnectionType.Bool)
			{
				if (CondType == EConditionalTypes.Equals)
				{
					bOutputTrue = (bool) in1 == (bool) in2;
				}
				else if (CondType == EConditionalTypes.NotEquals)
				{
					bOutputTrue = (bool)in1 != (bool)in2;
				}
				else
				{
					ErrorStack.Push(new NodeEditorException("INVALID Conditional type for conditional block: Expected"));
					return false;
				}
			}
			else if (DType == ECOnnectionType.Int)
			{
				switch (CondType)
				{
					case (EConditionalTypes.Equals):
						bOutputTrue = (int)in1 == (int)in2;
						break;
					case (EConditionalTypes.NotEquals):
						bOutputTrue = (int)in1 != (int)in2;
						break;
					case (EConditionalTypes.Greater):
						bOutputTrue = (int)in1 > (int)in2;
						break;
					case (EConditionalTypes.GreaterEquals):
						bOutputTrue = (int)in1 >= (int)in2;
						break;
					case (EConditionalTypes.Less):
						bOutputTrue = (int)in1 < (int)in2;
						break;
					case (EConditionalTypes.LessEquals):
						bOutputTrue = (int)in1 <= (int)in2;
						break;
					default:
						ErrorStack.Push(new NodeEditorException("INVALID Conditional type for conditional block: Expected"));
						return false;
				}
			}
			else
			{
				ErrorStack.Push(new NodeEditorException("INVALID data type for conditional block: [NOT SET]"));
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
