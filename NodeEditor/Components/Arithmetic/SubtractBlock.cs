using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeEditor.Components
{
	public class SubtractBlock : BaseArithmeticBlock
	{

		public SubtractBlock() : base()
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
					result -= (int)ResultsStack.Pop(); 
					
				}
			}
			ResultsStack.Clear();
			AnswerToOutput = result;
			return true;
		}
	}
}
