using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

		#region PTR
		/// <summary>
		/// this var is a String Name to serve as a pointer to all external settings
		/// </summary>
		public String LinkedTextBoxName { get; set; }
		#endregion

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

		public double StartTime
		{
			get { return (double)GetValue(StartTimeProperty); }
			set { SetValue(StartTimeProperty, value); }
		}

	public static readonly DependencyProperty StartTimeProperty =
				DependencyProperty.Register("StartTime", typeof(double), typeof(TimeBlock),
					new PropertyMetadata(0.0, new PropertyChangedCallback(OnStartTimeChange)));
		private static void OnStartTimeChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			TimeBlock tb = (TimeBlock)d;
			tb.EndTime = tb.StartTime + (tb.ActualWidth * tb.TimelineParent.TimePerPixel);
			tb.Duration = tb.EndTime - tb.StartTime;
			Canvas.SetLeft(tb, (1 / tb.TimelineParent.TimePerPixel) * (double)e.NewValue);
		}

		public double EndTime
		{
			get { return (double)GetValue(EndTimeProperty); }
			set { SetValue(EndTimeProperty, value); }
		}
		public static readonly DependencyProperty EndTimeProperty =
			DependencyProperty.Register("EndTime", typeof(double), typeof(TimeBlock),
				new PropertyMetadata(0.0, new PropertyChangedCallback(OnEndtTimeChange)));

		public static void OnEndtTimeChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			TimeBlock tb = (TimeBlock)d;
			//tb.EndTime = tb.StartTime + (tb.ActualWidth * tb.TimelineParent.TimePerPixel);
			tb.Duration = tb.EndTime - tb.StartTime;
			tb.Width = (1 / tb.TimelineParent.TimePerPixel) * (tb.EndTime - tb.StartTime);
		}

		private double duration;
		public double Duration
		{
			get { return duration; }
			set
			{
				duration = value;
				EndTime = StartTime + value;
			}
		}

		#endregion

		public object LinkedDialogueBlock = null;

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			this.MouseMove += TimeBlock_MouseMove;
		}

		public TimeBlock(Timeline parent, double starttime)
		{
			TimelineParent = parent;
			DataContext = this;
			this.Loaded += TimeBlock_Loaded;
			this.StartTime = starttime;
		}

		private void TimeBlock_Loaded(object sender, RoutedEventArgs e)
		{
			
			this.EndTime = StartTime + (this.ActualWidth * TimelineParent.TimePerPixel);
			this.Duration = EndTime - StartTime;
		}

		public void ScaleToTimeline()
		{
			//start
			Canvas.SetLeft(this, StartTime / TimelineParent.TimePerPixel);
			//duration
			this.Width = Duration / TimelineParent.TimePerPixel;
		}

		private void TimeBlock_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
		{
		}

		static bool readWav(string filename, out float[] L, out float[] R)
		{
			L = R = null;
			//float [] left = new float[1];

			//float [] right;
			try
			{
				using (FileStream fs = File.Open(filename, FileMode.Open))
				{
					BinaryReader reader = new BinaryReader(fs);

					// chunk 0
					int chunkID = reader.ReadInt32();
					int fileSize = reader.ReadInt32();
					int riffType = reader.ReadInt32();


					// chunk 1
					int fmtID = reader.ReadInt32();
					int fmtSize = reader.ReadInt32(); // bytes for this chunk
					int fmtCode = reader.ReadInt16();
					int channels = reader.ReadInt16();
					int sampleRate = reader.ReadInt32();
					int byteRate = reader.ReadInt32();
					int fmtBlockAlign = reader.ReadInt16();
					int bitDepth = reader.ReadInt16();

					if (fmtSize == 18)
					{
						// Read any extra values
						int fmtExtraSize = reader.ReadInt16();
						reader.ReadBytes(fmtExtraSize);
					}

					// chunk 2
					int dataID = reader.ReadInt32();
					int bytes = reader.ReadInt32();

					// DATA!
					byte[] byteArray = reader.ReadBytes(bytes);

					int bytesForSamp = bitDepth / 8;
					int samps = bytes / bytesForSamp;


					float[] asFloat = null;
					switch (bitDepth)
					{
						case 64:
							double[]
							asDouble = new double[samps];
							Buffer.BlockCopy(byteArray, 0, asDouble, 0, bytes);
							asFloat = Array.ConvertAll(asDouble, e => (float)e);
							break;
						case 32:
							asFloat = new float[samps];
							Buffer.BlockCopy(byteArray, 0, asFloat, 0, bytes);
							break;
						case 16:
							Int16[]
							asInt16 = new Int16[samps];
							Buffer.BlockCopy(byteArray, 0, asInt16, 0, bytes);
							asFloat = Array.ConvertAll(asInt16, e => e / (float)Int16.MaxValue);
							break;
						default:
							return false;
					}

					switch (channels)
					{
						case 1:
							L = asFloat;
							R = null;
							return true;
						case 2:
							L = new float[samps];
							R = new float[samps];
							for (int i = 0, s = 0; i < samps; i++)
							{
								L[i] = asFloat[s++];
								R[i] = asFloat[s++];
							}
							return true;
						default:
							return false;
					}
				}
			}
			catch
			{
				Console.WriteLine("...Failed to load note: " + filename);
				return false;
				//left = new float[ 1 ]{ 0f };
			}

			return false;
		}


		public override string ToString()
		{
			return String.Format("StartTime: {0} EndTime: [1}", StartTime, EndTime);
		}


	}
}
