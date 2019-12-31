using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeEditor.Components.Logic;

namespace NodeEditor.Components
{
	class NOTBlock : BaseLogicNodeBlock
	{
		public NOTBlock(bool bInitNodes = true) : base(bInitNodes)
		{
			this.InputNodes = new List<ConnectionNode>();
			this.OutputNodes = new List<ConnectionNode>();
			if (bInitNodes)
			{
				this.InputNodes.Add(new ConnectionNode(this, "InputNode1", ECOnnectionType.Bool));
				this.OutputNodes.Add(new ConnectionNode(this, "OutputNode1", ECOnnectionType.Bool));
			}
		}

		public override bool OnEndEvaluateInternalData()
		{
			bool result = true;
			//make sure result stack is not empty!
			if (ResultsStack.Count == 0) return false;
			else
			{
				result = !(bool) (ResultsStack.Pop());
			}
			AnswerToOutput = result;
			return true;
		}

	}
}
