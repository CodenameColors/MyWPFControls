using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TimelinePlayer.Components
{
	public class Timeline : Canvas
	{

		LinkedList<TimeBlock> timeBlocksLL = new LinkedList<TimeBlock>();

		public Timeline()
		{

		}

		/// <summary>
		/// this method is here to INSERT the time block into the timeline's linked list in the CORRECT spot
		/// </summary>
		public void AddTimeBlock(TimeBlock timeBlock)
		{

		}



	}
}
