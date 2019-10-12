using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeEditor.Components
{

	public enum EConditionalTypes
	{
		Equals,
		NotEquals,
		Greater,
		Less,
		GreaterEquals,
		LessEquals,
	};

	public class BaseLogicNodeBlock : BaseNodeBlock
	{
		public EConditionalTypes CondType = EConditionalTypes.Equals;

		public ConnectionNode Cond1
		{
			get { return this.InputNodes[0]; }
			set { this.InputNodes[0] = value; }
		}

		public ConnectionNode Cond2
		{
			get { return this.InputNodes[1]; }
			set { this.InputNodes[1] = value; }
		}

		public ConnectionNode TrueOutput
		{
			get { return OutputNodes[0]; }
			set { OutputNodes[0] = value; }
		}
		public ConnectionNode FalseOutput
		{
			get { return OutputNodes[1]; }
			set { OutputNodes[1] = value; }
		}

		/// <summary>
		/// Constructor. it will always have this setup.
		/// </summary>
		/// <param name="nodetype"></param>
		public BaseLogicNodeBlock(ECOnnectionType nodetype)
		{
			this.InputNodes = new List<ConnectionNode>();
			this.OutputNodes = new List<ConnectionNode>();
			this.EntryNode = new ConnectionNode(this, "EntryNode", ECOnnectionType.Enter);
			this.ExitNode = new ConnectionNode(this, "ExitNode", ECOnnectionType.Exit);
			this.InputNodes.Add(new ConnectionNode(this, "InputNode1", nodetype));
			this.InputNodes.Add(new ConnectionNode(this, "InputNode2", nodetype));
			this.OutputNodes.Add(new ConnectionNode(this, "OutputNode1", nodetype ));
			this.OutputNodes.Add(new ConnectionNode(this, "OutputNode2", nodetype ));
		}

	}
}
