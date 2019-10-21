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

		public override void OnStartNodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			currentNB.bIsActive = true;
		}

		public override void NodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			throw new NotImplementedException();
		}

		public override void OnEndNodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			throw new NotImplementedException();
		}

		public override void OnStartEvaulatInternalData()
		{
			throw new NotImplementedException();
		}

		public override void EvaulatInternalData()
		{
			throw new NotImplementedException();
		}

		public override void OnEndEvaulatInternalData()
		{
			throw new NotImplementedException();
		}

		public override void DeleteConnection(EConditionalTypes contype, int row)
		{
			throw new NotImplementedException();
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
