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
		public NOTBlock() 
		{
			this.InputNodes = new List<ConnectionNode>();
			this.OutputNodes = new List<ConnectionNode>();
			this.InputNodes.Add(new ConnectionNode(this, "InputNode1", ECOnnectionType.Bool));
			this.OutputNodes.Add(new ConnectionNode(this, "OutputNode1", ECOnnectionType.Bool));
		}
	}
}
