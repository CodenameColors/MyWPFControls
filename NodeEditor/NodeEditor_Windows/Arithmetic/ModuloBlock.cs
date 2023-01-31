using System.Linq;

namespace BixBite.NodeEditor.Arithmetic
{
	public class ModuloBlock : BaseArithmeticBlock
	{

		public ModuloBlock(bool bInitNodes = true) : base(bInitNodes)
		{

		}
		public override bool OnEndEvaluateInternalData()
		{
			int result = (int)ResultsStack.ToArray().Last();
			ResultsStack.Pop();
			//make sure result stack is not empty!
			if (ResultsStack.Count == 0) return false;
			else
			{
				while (ResultsStack.Count != 0)
				{
					result %= (int)ResultsStack.Pop();
				}
			}
			ResultsStack.Clear();
			AnswerToOutput = result;
			return true;
		}
	}
}
