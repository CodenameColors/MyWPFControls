using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeEditor.Components
{
	public class BaseArithmeticBlock : BaseNodeBlock, INotifyPropertyChanged
	{
		public ConnectionNode Operand1
		{
			get { return InputNodes[0]; }
			set
			{
				InputNodes[0] = value;
			}
		}

		public ConnectionNode Operand2
		{
			get { return InputNodes[1]; }
			set
			{
				InputNodes[1] = value;
				if (InputNodes[1].ConnectedNodes.Count > 0) //its now connected so set display
					NewValConnected = true;
			}
		}
		public ConnectionNode OutValue
		{
			get { return OutputNodes[0]; }
			set
			{
				OutputNodes[0] = value;
			}
		}

		private ECOnnectionType dtype;
		public ECOnnectionType DType
		{
			get { return dtype; }
			set
			{
				dtype = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DType"));

			}
		}

		private bool newvalconnected;
		public bool NewValConnected
		{
			get { return newvalconnected; }
			set
			{
				newvalconnected = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NewValConnected"));

			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Base constructor for XAML PTR
		/// </summary>
		public BaseArithmeticBlock()
		{
			this.InputNodes = new List<ConnectionNode>();
			this.OutputNodes = new List<ConnectionNode>();
			this.InputNodes.Add(new ConnectionNode(this, "InputNode1", ECOnnectionType.Int));
			this.InputNodes.Add(new ConnectionNode(this, "InputNode2", ECOnnectionType.Int));
			this.OutputNodes.Add(new ConnectionNode(this, "OutputNode1", ECOnnectionType.Int));

			dtype = ECOnnectionType.Int;
			newvalconnected = false;
		}

		public override bool EvaluateInternalData(BaseNodeBlock connectedBlock, out object retVal)
		{
			throw new NotImplementedException();
		}

		public override void NodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			throw new NotImplementedException();
		}

		public override void OnEndEvaluateInternalData()
		{
			throw new NotImplementedException();
		}

		public override void OnEndNodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			throw new NotImplementedException();
		}

		public override bool OnStartEvaluateInternalData(ConnectionNode desiredNode, out BaseNodeBlock connectedBlock)
		{
			throw new NotImplementedException();
		}

		public override void OnStartNodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			throw new NotImplementedException();
		}

		public override void DeleteConnection(EConditionalTypes contype, int row)
		{
			throw new NotImplementedException();
		}
	}
}
