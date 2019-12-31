using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeEditor.Components.Logic;

namespace NodeEditor.Components
{
	class ORBlock : BaseLogicNodeBlock
	{
		public ORBlock(bool bInitNodes = true) : base(bInitNodes)
		{

		}

		public override bool OnEndEvaluateInternalData()
		{
			bool result = false;
			//make sure result stack is not empty!
			if (ResultsStack.Count == 0) return false;
			else
			{
				while (ResultsStack.Count != 0)
				{
					result |= (bool)ResultsStack.Pop();
				}
			}
			AnswerToOutput = result;
			return true;
		}


	}
}
