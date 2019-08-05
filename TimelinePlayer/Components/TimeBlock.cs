using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TimelinePlayer.Components
{
	public class TimeBlock : Button, INotifyPropertyChanged
	{
		#region start date
		public String StartDate
		{
			get { return (String)GetValue(StartDateProperty); }
			set { SetValue(StartDateProperty, value); }
		}

		public String Test { get { return "settest"; } set { Test = value; } }

		// Using a DependencyProperty as the backing store for StartDate.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty StartDateProperty =
				DependencyProperty.Register("StartDate", typeof(String), typeof(TrackTimeline),
				new UIPropertyMetadata("test",
						new PropertyChangedCallback(OnStartDateChanged)));

		public event PropertyChangedEventHandler PropertyChanged;

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

		public TimeBlock()
		{
			StartDate = "60";
		}

		private void TimeBlock_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
		{
			Console.WriteLine("moved");
			StartDate = "70";
		}


	}
}
