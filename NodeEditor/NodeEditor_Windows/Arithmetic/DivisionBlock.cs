using System.Linq;

namespace BixBite.NodeEditor.Arithmetic
{
	public class DivisionBlock : BaseArithmeticBlock
	{

		public DivisionBlock(bool bInitNodes = true) : base(bInitNodes)
		{

		}
		public override bool OnEndEvaluateInternalData()
		{
			int result = (int)ResultsStack.ToArray().Last();
			//make sure result stack is not empty!
			if (ResultsStack.Count == 0) return false;
			else
			{
				while (ResultsStack.Count > 1)
				{
					result /= (int) ResultsStack.Pop();
				}
			}
			ResultsStack.Clear();
			AnswerToOutput = result;
			return true;
		}
	}
}
