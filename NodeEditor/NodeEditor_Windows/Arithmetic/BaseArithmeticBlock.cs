using System;
using System.Collections.Generic;
using System.ComponentModel;
using BixBite.NodeEditor.Logic;
using BixBite.Resources;

namespace BixBite.NodeEditor.Arithmetic
{
	public class BaseArithmeticBlock : BaseNodeBlock, INotifyPropertyChanged
	{
		public ConnectionNode Operand1
		{
			get { return InputNodes[0]; }
			set
			{
				InputNodes[0] = value;
			}
		}

		public ConnectionNode Operand2
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
		public new ECOnnectionType DType
		{
			get { return dtype; }
			set
			{
				dtype = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DType"));

			}
		}

		public override event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Base constructor for XAML PTR
		/// </summary>
		public BaseArithmeticBlock( bool bInitNodes = true )
		{	
			this.InputNodes = new List<ConnectionNode>();
			this.OutputNodes = new List<ConnectionNode>();
			if (bInitNodes)
			{
				this.InputNodes.Add(new ConnectionNode(this, "InputNode1", ECOnnectionType.Int));
				this.InputNodes.Add(new ConnectionNode(this, "InputNode2", ECOnnectionType.Int));
				this.OutputNodes.Add(new ConnectionNode(this, "OutputNode1", ECOnnectionType.Int));
			}

			dtype = ECOnnectionType.Int;
			NewValConnected = false;
		}

		/// <summary>
		/// NA DO NOT USE
		/// </summary>
		/// <param name="currentNB"></param>
		/// <returns></returns>
		public override bool OnStartNodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// NA DO NOT USE
		/// </summary>
		/// <param name="currentNB"></param>
		/// <returns></returns>
		public override bool NodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// NA Do not use
		/// </summary>
		/// <param name="currentNB"></param>
		public override void OnEndNodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			throw new NotImplementedException();
		}


		/// <summary>
		/// This method will check all the inputs and outputs of this block.
		/// used for data nodes
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
					if(InputNodes.Count-1 == i && (!NewValConnected && NewValue_Constant != ""))
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
					if(cn.ConnectedNodes.Count != 0)
						temp &= EvaluateInternalData(cn.ConnectedNodes[0].ParentBlock);
					else //this is here for the constants that one can manually enter.
					{
						ResultsStack.Push(Int32.Parse(this.NewValue_Constant));
						Console.WriteLine(String.Format("Result: {0}", ResultsStack.Peek()));
					}
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
		/// This method will go through the inputs, and determine if each node needs further evaluation.
		/// If not then we will fill the result stack and get ready for expression evaluation
		/// </summary>
		/// <param name="connectedBlock"></param>
		/// <returns></returns>
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

		/// <summary>
		/// This method will take the data that has been received from the inputs and placed in the stack.
		/// and evaluate them.  IF YOU ARE READING THIS, you need to define this in the sub classes!
		/// </summary>
		/// <returns></returns>
		public override bool OnEndEvaluateInternalData()
		{
			throw new NotImplementedException();
		}


		public override void DeleteConnection(EConditionalTypes contype, int row)
		{
			throw new NotImplementedException();
		}
	}
}
