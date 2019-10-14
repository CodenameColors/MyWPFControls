using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeEditor.Components
{
	public abstract class BaseNodeBlock : System.Windows.Controls.Button
	{
		public ConnectionNode EntryNode;
		public ConnectionNode ExitNode;

		public List<ConnectionNode> InputNodes;
		public List<ConnectionNode> OutputNodes;

		public String Header { get; set; }

		//Every node block has three startes. 
		//Start execution = The start of the execution here is where node references should be checked for. A null will kill/Stop execution flow
		public abstract void OnStartNodeBlockExecution();

		//Excute = This state here assumes there is no null ptrs along the data ptrs.
		//				 The state will proprogate down the data ptrs to solve the "equation" and find data value that is assigned to X Node
		public abstract void NodeBlockExecution();

		//End Execution = Execution state has completed and returned a VALID value. So set this value to output node. 
		//								Then set excution pointer to the next block (the exit node ptr)
		public abstract void OnEndNodeBlockExecution();

		//Blocks that need to evalute expression NEED to run this method.
		public abstract void OnStartEvaulatInternalData();

		//Blocks that need to evalute expression NEED to run this method.
		public abstract void EvaulatInternalData();

		//Blocks that need to evalute expression NEED to run this method.
		public abstract void OnEndEvaulatInternalData();


		//every block node that is inherited will be able to delete connections. 
		public abstract void DeleteConnection(EConditionalTypes contype, int row);

	}
}
