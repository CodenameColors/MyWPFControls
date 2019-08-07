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

		public Timeline TimelineParent;

		#region Sprite

		//Sprite Name
		#region TrackName
		public String Trackname
		{
			get { return (String)GetValue(TrackNameProperty); }
			set { SetValue(TrackNameProperty, value); }
		}
		// Using a DependencyProperty as the backing store for StartDate.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty TrackNameProperty =
				DependencyProperty.Register("Trackname", typeof(String), typeof(TimeBlock),
				new PropertyMetadata("NameHere", new PropertyChangedCallback(OnTrackNameChanged)));

		public event PropertyChangedEventHandler PropertyChanged;

		private static void OnTrackNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Console.WriteLine("changed time");
		}
		#endregion

		//sprite image path
		public String TrackSpritePath
		{
			get { return (String)GetValue(TrackSpritePathProperty); }
			set { SetValue(TrackSpritePathProperty, value); }
		}

		public static readonly DependencyProperty TrackSpritePathProperty =
			DependencyProperty.Register("TrackSpritePath", typeof(String), typeof(TimeBlock),
				new PropertyMetadata("Images/speech-bubbles-png.png", new PropertyChangedCallback(OnSpritePathChange)));

		private static void OnSpritePathChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Console.WriteLine("Changed Image Path");
		}


		//Srpite Image

		#endregion

		#region Dialogue

		//dialogue text (full)

		//dialogue text (text crawling)
		public String CurrentDialogue
		{
			get { return (String)GetValue(CurrentDialogueProperty); }
			set { SetValue(CurrentDialogueProperty, value); }
		}

		public static readonly DependencyProperty CurrentDialogueProperty =
			DependencyProperty.Register("CurrentDialogue", typeof(String), typeof(TimeBlock),
				new PropertyMetadata("Default Text", new PropertyChangedCallback(OnCurrentDialogueChanged)));
		private static void OnCurrentDialogueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Console.WriteLine("Changed Dialogue");
		}

		#endregion

		#region Audio
		//audio file path
		public String AudioFile
		{
			get { return (String)GetValue(AudioFileProperty); }
			set { SetValue(AudioFileProperty, value); }
		}

		public static readonly DependencyProperty AudioFileProperty =
			DependencyProperty.Register("AudioFile", typeof(String), typeof(TimeBlock),
				new PropertyMetadata("Audio FIle.mp3", new PropertyChangedCallback(OnAudioFilePropertyChanged)));
		private static void OnAudioFilePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Console.WriteLine("Changed Audio File");
		}


		//audio start offset

		#endregion

		#region Time

		public double StartTime { get; set; }
			public double EndTime { get; set; }
			public double Duration { get; set; }

		#endregion

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			this.MouseMove += TimeBlock_MouseMove;
		}

		public TimeBlock(Timeline parent)
		{
			TimelineParent = parent;
			DataContext = this;
		}

		private void TimeBlock_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
		{
			Console.WriteLine("moved");
			Trackname = "Emma";
			TrackSpritePath = "C:/Users/amorales/Documents/Visual Studio 2017/Projects/ProjectEE/AmethystEngine/images/emma_colors_oc.png";
			CurrentDialogue = "Sup, Nerd?";


			double left = Canvas.GetLeft(this);
			if (left + this.Width > TimelineParent.ActualWidth) //expands the time line if the block goes off screen
				TimelineParent.Width = left + this.Width;
		}


	}
}
