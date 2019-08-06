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
		public String Trackname
		{
			get { return (String)GetValue(StartDateProperty); }
			set { SetValue(StartDateProperty, value); }
		}
		// Using a DependencyProperty as the backing store for StartDate.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty StartDateProperty =
				DependencyProperty.Register("Trackname", typeof(String), typeof(TimeBlock),
				new PropertyMetadata( "Emma", new PropertyChangedCallback(OnStartDateChanged)));

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
			DataContext = this;
		}

		private void TimeBlock_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
		{
			Console.WriteLine("moved");
			Trackname = "Antonio";
		}


	}
}
