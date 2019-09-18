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
	public class ConnectionNode : Button
	{
		public String Name { get; set; }

		public List<BaseNode> ConnectedNodes = new List<BaseNode>();
		public Point NodeLocation;
		public List<Path> Curves = new List<Path>(); 


		public ConnectionNode()
		{

		}

		public ConnectionNode(String Name, Point p)
		{
			this.Name = Name;
			this.NodeLocation = p;
		}

	}
}
