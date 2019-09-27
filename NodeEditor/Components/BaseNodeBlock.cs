using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeEditor.Components
{
	public class BaseNodeBlock : System.Windows.Controls.Button
	{
		public ConnectionNode EntryNode;
		public ConnectionNode ExitNode;

		public List<ConnectionNode> InputNodes;
		public List<ConnectionNode> OutputNodes;

		public String Header { get; set; }

	}
}
