using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeEditor.Components.Logic;
using NodeEditor.Resources;

namespace NodeEditor.Components
{
	public enum EActiveStatus
	{
		Active,
		Disabled,
		Error,
		Done,
		Eval

	}

	public abstract class BaseNodeBlock : System.Windows.Controls.Button, INotifyPropertyChanged, IStateMachineTraversal
	{
		public String Header { get; set; }
		public String NewValue_Constant { get; set; }

		public double LocX = 0.0;
		public double LocY = 0.0;

		//for displaying active status
		private bool _bChoice = false;
		public bool bChoice
		{
			get => _bChoice;
			set
			{
				_bChoice = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("bChoice"));
			}
		}

		//for displaying active status
		private EActiveStatus activeStatus = EActiveStatus.Disabled;
		public EActiveStatus ActiveStatus
		{
			get => activeStatus;
			set
			{
				activeStatus = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ActiveStatus"));
			}
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

		private bool newvalconnected;
		public bool NewValConnected
		{
			get { return newvalconnected; }
			set
			{
				newvalconnected = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NewValConnected"));

			}
		}

		public ConnectionNode EntryNode;
		public ConnectionNode ExitNode;

		public List<ConnectionNode> InputNodes;
		public List<ConnectionNode> OutputNodes;

		public Stack<object> ResultsStack = new Stack<object>();
		public object AnswerToOutput = null;

		public Stack<NodeEditorException> ErrorStack = new Stack<NodeEditorException>();

		public virtual event PropertyChangedEventHandler PropertyChanged;


		////Every node block has three states. 
		////Start execution = The start of the execution here is where node references should be checked for. A null will kill/Stop execution flow
		//public abstract bool OnStartNodeBlockExecution(ref BaseNodeBlock currentNB);

		////Excute = This state here assumes there is no null ptrs along the data ptrs.
		////				 The state will proprogate down the data ptrs to solve the "equation" and find data value that is assigned to X Node
		//public abstract bool NodeBlockExecution(ref BaseNodeBlock currentNB);

		////End Execution = Execution state has completed and returned a VALID value. So set this value to output node. 
		////								Then set execution pointer to the next block (the exit node ptr)
		//public abstract void OnEndNodeBlockExecution(ref BaseNodeBlock currentNB);


		////Blocks that need to evaluate expression NEED to run this method.
		////this method is here to check the inputs connected blocks. 
		////If NOT GetConstantBlock then it must set the current node reference to the previous connected node
		//public abstract bool OnStartEvaluateInternalData();

		////Blocks that need to evaluate expression NEED to run this method.
		////this method is run when only when the input nodes are connected to a GetConstantBlock
		////OR the block contains an answer value.
		//public abstract bool EvaluateInternalData(BaseNodeBlock connectedBlock);

		////Blocks that need to evaluate expression NEED to run this method.
		////the method will take the answer and output it to the connected nodes of the output node.
		//public abstract bool OnEndEvaluateInternalData();


		//every block node that is inherited will be able to delete connections. 
		public abstract void DeleteConnection(EConditionalTypes contype, int row);

		public virtual bool OnStartNodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			throw new NotImplementedException();
		}

		public virtual bool NodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			throw new NotImplementedException();
		}

		public virtual void OnEndNodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			throw new NotImplementedException();
		}

		public virtual bool OnStartEvaluateInternalData()
		{
			throw new NotImplementedException();
		}

		public virtual bool EvaluateInternalData(BaseNodeBlock connectedBlock)
		{
			throw new NotImplementedException();
		}

		public virtual bool OnEndEvaluateInternalData()
		{
			throw new NotImplementedException();
		}
	}
}
