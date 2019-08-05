using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimelinePlayer.Components
{
	class TimeDataType
	{
		public long? StartTime { get; set; }
		public long? EndTime { get; set; }
		public Boolean TimelineViewExpanded { get; set; }
		public String Name { get; set; }
	}
}
