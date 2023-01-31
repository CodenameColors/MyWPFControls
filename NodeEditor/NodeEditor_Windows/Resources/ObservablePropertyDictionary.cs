using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace BixBite.Resources
{
	public class ObservablePropertyDictionary : ObservableSortedDictionary<string, Tuple<String, String>>
	{
		#region constructors

		#region public

		public ObservablePropertyDictionary() : base(new KeyComparer())
		{
		}

		#endregion public

		#endregion constructors

		#region key comparer class

		private class KeyComparer : IComparer<DictionaryEntry>
		{
			public int Compare(DictionaryEntry entry1, DictionaryEntry entry2)
			{
				return string.Compare((string)entry1.Key, (string)entry2.Key, StringComparison.InvariantCultureIgnoreCase);
			}
		}

		#endregion key comparer class


	}
}
