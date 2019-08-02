using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TimelinePlayer.Components
{
	public class TimeBlock : Button
	{
		#region start date
		public DateTime StartDate
		{
			get { return (DateTime)GetValue(StartDateProperty); }
			set { SetValue(StartDateProperty, value); }
		}

		// Using a DependencyProperty as the backing store for StartDate.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty StartDateProperty =
				DependencyProperty.Register("StartDate", typeof(DateTime), typeof(TrackTimeline),
				new UIPropertyMetadata(DateTime.MinValue,
						new PropertyChangedCallback(OnStartDateChanged)));
		private static void OnStartDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Console.WriteLine("changed time");
		}
		#endregion

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			this.MouseMove += TimeBlock_MouseMove;
		}

		private void TimeBlock_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
		{
			Console.WriteLine("moved");
		}

		

	}
}
