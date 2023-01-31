using System.Linq;

namespace BixBite.NodeEditor.Arithmetic
{
	public class MultiplyBlock : BaseArithmeticBlock
	{

		public MultiplyBlock(bool bInitNodes = true) : base(bInitNodes)
		{
		}
		public override bool OnEndEvaluateInternalData()
		{
			int result = (int)ResultsStack.ToArray().First();
			ResultsStack.Pop();
			//make sure result stack is not empty!
			if (ResultsStack.Count == 0) return false;
			else
			{
				while (ResultsStack.Count != 0)
				{
					result *= (int)ResultsStack.Pop();
				}
			}
			AnswerToOutput = result;
			return true;
		}
	}
}
