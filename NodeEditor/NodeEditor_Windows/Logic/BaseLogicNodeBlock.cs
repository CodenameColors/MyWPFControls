using System;
using System.Collections.Generic;
using System.ComponentModel;
using BixBite.Resources;

namespace BixBite.NodeEditor.Logic
{

	public enum EConditionalTypes
	{
		Equals,
		NotEquals,
		Greater,
		Less,
		GreaterEquals,
		LessEquals,
	};

	public class BaseLogicNodeBlock : BaseNodeBlock , INotifyPropertyChanged
	{
		public ConnectionNode Operand1
		{
			get { return this.InputNodes[0]; }
			set { this.InputNodes[0] = value; }
		}

		public ConnectionNode Operand2
		{
			get { return this.InputNodes[1]; }
			set { this.InputNodes[1] = value; }
		}

		public ConnectionNode Output
		{
			get { return OutputNodes[0]; }
			set { OutputNodes[0] = value; }
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

		private bool newvalconnected;

		public override event PropertyChangedEventHandler PropertyChanged;

		public new bool NewValConnected
		{
			get { return newvalconnected; }
			set
			{
				newvalconnected = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NewValConnected"));

			}
		}


		/// <summary>
		/// Constructor. it will always have this setup.
		/// </summary>
		/// <param name="nodetype"></param>
		public BaseLogicNodeBlock(bool bInitNodes = true)
		{
			this.InputNodes = new List<ConnectionNode>();
			this.OutputNodes = new List<ConnectionNode>();
			if (bInitNodes)
			{
				this.InputNodes.Add(new ConnectionNode(this, "InputNode1", ECOnnectionType.Bool));
				this.InputNodes.Add(new ConnectionNode(this, "InputNode2", ECOnnectionType.Bool));
				this.OutputNodes.Add(new ConnectionNode(this, "OutputNode1", ECOnnectionType.Bool));
			}

			dtype = ECOnnectionType.Bool;
			newvalconnected = false;
		}

		/// <summary>
		/// NA if you ask for this... the algorithm you are writing is wrong
		/// </summary>
		/// <param name="currentNB"></param>
		/// <returns></returns>
		public override bool OnStartNodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// NA if you ask for this... the algorithm you are writing is wrong
		/// </summary>
		/// <param name="currentNB"></param>
		/// <returns></returns>
		public override bool NodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// NA if you ask for this... the algorithm you are writing is wrong
		/// </summary>
		/// <param name="currentNB"></param>
		/// <returns></returns>
		public override void OnEndNodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// This method is here to check to make sure that all the inputs on this current block
		/// are/have a valid connection.
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
					if (InputNodes.Count - 1 == i && (!newvalconnected && NewValue_Constant != ""))
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
						if (this.NewValue_Constant == "T")
							ResultsStack.Push(true);
						else if (this.NewValue_Constant == "F")
							ResultsStack.Push(false);
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

		public static implicit operator BaseLogicNodeBlock(ExitBlockNode v)
		{
			throw new NotImplementedException();
		}
	}
}
