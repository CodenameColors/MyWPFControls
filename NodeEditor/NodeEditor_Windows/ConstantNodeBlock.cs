using System;
using System.Collections.Generic;
using System.ComponentModel;
using BixBite.NodeEditor.Logic;
using BixBite.NodeEditor;
using BixBite.Resources;

namespace BixBite.NodeEditor
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


		private RuntimeVars data = new RuntimeVars();
		public RuntimeVars InternalData
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
			get => data.VarName;
			set => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("VarHeader"));
		}

		public GetConstantNodeBlock(ECOnnectionType type, bool bInitNodes = true)
		{
			DType = type;
			Header = String.Format("Get Constant [{0}]", type.ToString());
			OutputNodes = new List<ConnectionNode>(); //There is only ONE output
			if(bInitNodes)
				this.OutputNodes.Add(new ConnectionNode(this, "OutputNode1", type));
		}

		public GetConstantNodeBlock(ECOnnectionType type, ref RuntimeVars varptr, bool bInitNodes = true)
		{
			DType = type;
			Header = String.Format("Get Constant [{0}]", type.ToString());
			OutputNodes = new List<ConnectionNode>(); //There is only ONE output
			if(bInitNodes)
				this.OutputNodes.Add(new ConnectionNode(this, "OutputNode1", type));
			this.InternalData = varptr;
		}

		public override event PropertyChangedEventHandler PropertyChanged;

		public override bool OnStartNodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			throw new NotImplementedException();
		}

		public override bool NodeBlockExecution(ref BaseNodeBlock currentNB)
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
	}

	public class SetConstantNodeBlock : BaseNodeBlock
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


		public SetConstantNodeBlock(ECOnnectionType nodetype, bool bInitNNodes = true)
		{
			this.InputNodes = new List<ConnectionNode>();
			this.OutputNodes = new List<ConnectionNode>();
			this.EntryNode = new ConnectionNode(this, "EntryNode", ECOnnectionType.Enter);
			this.ExitNode = new ConnectionNode(this, "ExitNode", ECOnnectionType.Exit);
			if (bInitNNodes)
			{
				this.InputNodes.Add(new ConnectionNode(this, "InputNode1", nodetype));
				this.InputNodes.Add(new ConnectionNode(this, "InputNode2", nodetype));
				this.OutputNodes.Add(new ConnectionNode(this, "OutputNode1", nodetype));
			}
			DType = nodetype;
			NewValConnected = false;
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
				if (cn.ConnectedNodes.Count == 0)
				{
					if (InputNodes.Count - 1 == i && (!NewValConnected && NewValue_Constant != ""))
						continue;
					temp = false;
					ErrorStack.Push(new InputNodeConnectionException(i, this.GetType().Name));
				}
				i++;
			}


			if (EntryNode.ConnectedNodes.Count == 0)
			{
				temp = false;
				ErrorStack.Push(new EntryNodeConnectionException(this.GetType().Name));
			}

			if(ExitNode.ConnectedNodes.Count == 0)
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
			currentNB = this.ExitNode.ConnectedNodes[0].ParentBlock;
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
				if(OldValue.ConnectedNodes[0].ParentBlock is GetConstantNodeBlock)
					temp &= EvaluateInternalData(OldValue.ConnectedNodes[0].ParentBlock);
				else
				{
					temp = false;
					ErrorStack.Push(new InputNodeConnectionException(0, this.GetType().Name));
				}
				if (NewValue.ConnectedNodes.Count != 0)
					temp &= EvaluateInternalData(NewValue.ConnectedNodes[0].ParentBlock);
				else //this is here for the constants that one can manually enter.
				{
					if (DType == ECOnnectionType.Int)
					{
						ResultsStack.Push(Int32.Parse(this.NewValue_Constant));
						Console.WriteLine(String.Format("Result: {0}", ResultsStack.Peek()));
					}
					else if(DType == ECOnnectionType.Bool)
					{
						if (this.NewValue_Constant == "T")
							ResultsStack.Push(true);
						else if (this.NewValue_Constant == "F")
							ResultsStack.Push(false);
						Console.WriteLine(String.Format("Result: {0}", ResultsStack.Peek()));
					}
					else
					{
						temp = false;
						ErrorStack.Push(new InputNodeConnectionException(i, this.GetType().Name));
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

			if (OldValue.ConnectedNodes != null)
			{
				RuntimeVars rtv = new RuntimeVars()
				{
					VarData = in2,
					VarName = ((GetConstantNodeBlock)OldValue.ConnectedNodes[0].ParentBlock).InternalData.VarName,
					OrginalVarData = ((GetConstantNodeBlock)OldValue.ConnectedNodes[0].ParentBlock).InternalData.OrginalVarData,
					Type = ((GetConstantNodeBlock)OldValue.ConnectedNodes[0].ParentBlock).InternalData.Type
				};
				ErrorStack.Push(new ChangeVarFlag(((GetConstantNodeBlock)OldValue.ConnectedNodes[0].ParentBlock).InternalData, rtv));
				((GetConstantNodeBlock) OldValue.ConnectedNodes[0].ParentBlock).InternalData = rtv;
				AnswerToOutput = in2;
			}

			return true;
		}

		public override void DeleteConnection(EConditionalTypes contype, int row)
		{
			throw new NotImplementedException();
		}
	}




}
