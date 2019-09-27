using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NodeEditor.Components
{
	public class DialogueNodeBlock : BaseNodeBlock, INotifyPropertyChanged
	{
		public List<object> NodeData { get; set; }



		public DialogueNodeBlock(String Header)
		{
			this.Header = Header;
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
