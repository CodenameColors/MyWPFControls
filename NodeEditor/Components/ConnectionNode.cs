using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;

namespace NodeEditor.Components
{
	public enum ECOnnectionType
	{
		NONE,
		Enter,
		Exit,
		Int,
		Bool
	}


	public class ConnectionNode : Button
	{
		public String Name { get; set; }

		public List<ConnectionNode> ConnectedNodes = new List<ConnectionNode>();
		public Point NodeLocation;
		public List<Path> Curves = new List<Path>();
		public ECOnnectionType NodeType;
		public BaseNodeBlock ParentBlock = new BaseNodeBlock();

		public ConnectionNode()
		{

		}

		public ConnectionNode(BaseNodeBlock pblock, String Name, Point p, ECOnnectionType nodetype)
		{
			this.ParentBlock = pblock;
			this.Name = Name;
			this.NodeLocation = p;
			this.NodeType = nodetype;
		}

	}
}
