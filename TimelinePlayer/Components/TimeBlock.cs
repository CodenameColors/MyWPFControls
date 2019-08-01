using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TimelinePlayer.Components
{
	public class TimeBlock : Button
	{

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			this.MouseMove += TimeBlock_MouseMove;
		}

		private void TimeBlock_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
		{
			throw new NotImplementedException();
		}
	}
}
