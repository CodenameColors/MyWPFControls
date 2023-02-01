using BixBite.NodeEditor;

namespace BixBite.Resources
{
	interface IStateMachineTraversal
	{


		//Every node block has three states. 
		//Start execution = The start of the execution here is where node references should be checked for. A null will kill/Stop execution flow
		  bool OnStartNodeBlockExecution(ref BaseNodeBlock currentNB);

		//Excute = This state here assumes there is no null ptrs along the data ptrs.
		//				 The state will proprogate down the data ptrs to solve the "equation" and find data value that is assigned to X Node
		  bool NodeBlockExecution(ref BaseNodeBlock currentNB);

		//End Execution = Execution state has completed and returned a VALID value. So set this value to output node. 
		//								Then set execution pointer to the next block (the exit node ptr)
		  void OnEndNodeBlockExecution(ref BaseNodeBlock currentNB);


		//Blocks that need to evaluate expression NEED to run this method.
		//this method is here to check the inputs connected blocks. 
		//If NOT GetConstantBlock then it must set the current node reference to the previous connected node
		  bool OnStartEvaluateInternalData();

		//Blocks that need to evaluate expression NEED to run this method.
		//this method is run when only when the input nodes are connected to a GetConstantBlock
		//OR the block contains an answer value.
		  bool EvaluateInternalData(BaseNodeBlock connectedBlock);

		//Blocks that need to evaluate expression NEED to run this method.
		//the method will take the answer and output it to the connected nodes of the output node.
		  bool OnEndEvaluateInternalData();


	}
}
