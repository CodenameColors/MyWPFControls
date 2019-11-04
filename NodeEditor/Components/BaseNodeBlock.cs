using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeEditor.Resources;

namespace NodeEditor.Components
{
	public enum EActiveStatus
	{
		Active = 0,
		Disabled = 1,
		Error = 2
	}

	public abstract class BaseNodeBlock : System.Windows.Controls.Button, INotifyPropertyChanged
	{
		public String Header { get; set; }

		//for displaying active status
		private EActiveStatus activeStatus = EActiveStatus.Disabled;
		public EActiveStatus ActiveStatus
		{
			get => activeStatus;
			set
			{
				activeStatus = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("BIsEActive"));
			}
		}


		public ConnectionNode EntryNode;
		public ConnectionNode ExitNode;

		public List<ConnectionNode> InputNodes;
		public List<ConnectionNode> OutputNodes;

		public object AnswerToOutput = null;

		public Stack<NodeEditorException> ErrorStack = new Stack<NodeEditorException>();

		public event PropertyChangedEventHandler PropertyChanged;


		//Every node block has three startes. 
		//Start execution = The start of the execution here is where node references should be checked for. A null will kill/Stop execution flow
		public abstract bool OnStartNodeBlockExecution(ref BaseNodeBlock currentNB);

		//Excute = This state here assumes there is no null ptrs along the data ptrs.
		//				 The state will proprogate down the data ptrs to solve the "equation" and find data value that is assigned to X Node
		public abstract bool NodeBlockExecution(ref BaseNodeBlock currentNB);

		//End Execution = Execution state has completed and returned a VALID value. So set this value to output node. 
		//								Then set execution pointer to the next block (the exit node ptr)
		public abstract void OnEndNodeBlockExecution(ref BaseNodeBlock currentNB);


		//Blocks that need to evaluate expression NEED to run this method.
		//this method is here to check the inputs connected blocks. 
		//If NOT GetConstantBlock then it must set the current node reference to the previous connected node
		public abstract bool OnStartEvaluateInternalData();

		//Blocks that need to evaluate expression NEED to run this method.
		//this method is run when only when the input nodes are connected to a GetConstantBlock
		//OR the block contains an answer value.
		public abstract bool EvaluateInternalData(BaseNodeBlock connectedBlock);

		//Blocks that need to evaluate expression NEED to run this method.
		//the method will take the answer and output it to the connected nodes of the output node.
		public abstract void OnEndEvaluateInternalData();


		//every block node that is inherited will be able to delete connections. 
		public abstract void DeleteConnection(EConditionalTypes contype, int row);

	}
}
