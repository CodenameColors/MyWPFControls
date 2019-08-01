using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimelinePlayer.Components
{
	public class TrackItem
	{

		#region fields
			
		private String name { get; set; }
		public String Name
		{
			get { return name; }
			set { name = value; }
		}

		private String imagepath { get; set; }
		public String ImagePath
		{
			get { return imagepath; }
			set { imagepath = value; }
		}

		private Uri image { get; set; }
		public Uri Image { get { return image; } }

		#endregion

		public TrackItem(String name, String ImagePath)
		{
			this.name = name;
			this.imagepath = ImagePath;
			SetImage(ImagePath);
		}

		private void SetImage(String ImagePath)
		{
			image = new Uri(ImagePath, UriKind.RelativeOrAbsolute);
		}

	}
}
