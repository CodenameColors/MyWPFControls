using System;
using System.Collections.Generic;
using System.ComponentModel;
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

	public class BaseLogicNodeBlock : BaseNodeBlock , INotifyPropertyChanged
	{
		public ConnectionNode Operand1
		{
			get { return this.InputNodes[0]; }
			set { this.InputNodes[0] = value; }
		}

		public ConnectionNode Operand2
		{
			get { return this.InputNodes[1]; }
			set { this.InputNodes[1] = value; }
		}

		public ConnectionNode Output
		{
			get { return OutputNodes[0]; }
			set { OutputNodes[0] = value; }
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

		public event PropertyChangedEventHandler PropertyChanged;

		public bool NewValConnected
		{
			get { return newvalconnected; }
			set
			{
				newvalconnected = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NewValConnected"));

			}
		}


		/// <summary>
		/// Constructor. it will always have this setup.
		/// </summary>
		/// <param name="nodetype"></param>
		public BaseLogicNodeBlock()
		{
			this.InputNodes = new List<ConnectionNode>();
			this.OutputNodes = new List<ConnectionNode>();
			this.InputNodes.Add(new ConnectionNode(this, "InputNode1", ECOnnectionType.Bool));
			this.InputNodes.Add(new ConnectionNode(this, "InputNode2", ECOnnectionType.Bool));
			this.OutputNodes.Add(new ConnectionNode(this, "OutputNode1", ECOnnectionType.Bool));

			dtype = ECOnnectionType.Bool;
			newvalconnected = false;
		}

		public override bool OnStartNodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			throw new NotImplementedException();
		}

		public override boolean NodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			throw new NotImplementedException();
		}

		public override void OnEndNodeBlockExecution(ref BaseNodeBlock currentNB)
		{
			throw new NotImplementedException();
		}

		public override bool OnStartEvaluateInternalData()
		{
			throw new NotImplementedException();
		}

		public override bool EvaluateInternalData(BaseNodeBlock connectedBlock)
		{
			throw new NotImplementedException();
		}

		public override void OnEndEvaluateInternalData()
		{
			throw new NotImplementedException();
		}

		public override void DeleteConnection(EConditionalTypes contype, int row)
		{
			throw new NotImplementedException();
		}
	}
}
