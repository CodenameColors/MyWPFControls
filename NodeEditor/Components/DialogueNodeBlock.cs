using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NodeEditor.Components
{
	public class DialogueNodeBlock : Button, INotifyPropertyChanged
	{
		public String Header { get; set; }
		public List<ConnectionNode> InputNodes { get; set; }
		public List<ConnectionNode> OutputNodes { get; set; }
		public List<object> NodeData { get; set; }



		public DialogueNodeBlock()
		{
			Header = String.Empty;
			InputNodes = new List<ConnectionNode>();
			OutputNodes = new List<ConnectionNode>();
			NodeData = new List<object>();
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
