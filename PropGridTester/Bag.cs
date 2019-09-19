using DrWPF.Windows.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;


namespace PropGridTester
{
	class Bag
	{
		public ObservableCollection<Tuple<String, object, Control>> DictionaryValues = new ObservableCollection<Tuple<String, object, Control>>();
		private ObservableDictionary<String, object> observableDictionary = new ObservableDictionary<string, object>();

		public Bag()
		{
			observableDictionary.Add("combo box", new List<String>() { "one", "two", "three" }); //combo box
			observableDictionary.Add("Text", "Default Text"); //textblock and or label
			observableDictionary.Add("Check Box", true); //check box
			observableDictionary.Add("Text Box", "Writable Text");
			observableDictionary.Add("Custom Controls", new DropDownCustomColorPicker.ColorPicker());
			//DictionaryValues = new ObservableCollection<object>(observableDictionary.Values); //sync
		}

		public ObservableDictionary<String, object> GetObsDict()
		{
			return observableDictionary;
		}

		public void SetobsDict (ObservableDictionary<String, object> newobsdict)
		{
			observableDictionary = newobsdict;
			//DictionaryValues = new ObservableCollection<object>(newobsdict.Values);
		}

		public void Setdictval (string key, object data)
		{
			if (observableDictionary.ContainsKey(key))
			{
				int index = observableDictionary.Keys.ToList().IndexOf(key);
				observableDictionary[key] = data;

				//the obs dict does a weird remove and add thing... so i do too.
				object o = DictionaryValues[index];
				DictionaryValues.RemoveAt(index);
				//DictionaryValues.Add(data);
			}
		}

		public void AddDictVal (string key, object data)
		{
			observableDictionary.Add(key, data);
			//DictionaryValues.Add(data);
		}

	}
}
