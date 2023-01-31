namespace BixBite.NodeEditor.Arithmetic
{
	public class AddBlock : BaseArithmeticBlock
	{

		public AddBlock(bool bInitNodes = true) : base(bInitNodes)
		{
			

		}

		public override bool OnEndEvaluateInternalData()
		{
			int result = 0;
			//make sure result stack is not empty!
			if (ResultsStack.Count == 0) return false;
			else
			{
				while(ResultsStack.Count != 0)
				{
					result += (int)ResultsStack.Pop();
				}
			}
			AnswerToOutput = result;
			return true;
		}
	}


}
