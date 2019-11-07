using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeEditor.Components
{
	public class AddBlock : BaseArithmeticBlock
	{

		public AddBlock() : base()
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
