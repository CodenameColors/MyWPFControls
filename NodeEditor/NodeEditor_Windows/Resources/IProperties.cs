using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BixBite.Resources
{
	interface IProperties
	{
		/// <summary>
		/// This method will SET the new properties.(ALL)
		/// </summary>
		/// <param name="NewProperties"></param>
		void SetNewProperties(ObservableCollection<Tuple<String, object>> NewProperties);

		/// <summary>
		/// Clears the current properties for the object.
		/// </summary>
		void ClearProperties();

		/// <summary>
		/// Searches for the given Key in the collection. If it found it will replace it
		/// </summary>
		/// <param name="Key">Property Name</param>
		/// <param name="Property">Property Data.</param>
		void SetProperty(String Key, object Property);

		/// <summary>
		/// Adds a new Property to the collection.
		/// </summary>
		/// <param name="Key">name of Property</param>
		/// <param name="data">Data of the property</param>
		void AddProperty(String Key, object data);

		/// <summary>
		/// Gets the property data
		/// </summary>
		/// <returns>the property data as an object.</returns>
		object GetPropertyData(String Key);

		/// <summary>
		/// Returns the Tuple data for the property
		/// </summary>
		/// <returns>Returns the Tuple of the property</returns>
		Tuple<String, object> GetProperty(String Key);

		/// <summary>
		/// Returns the whole property Collection
		/// </summary>
		/// <returns></returns>
		ObservableCollection<Tuple<String, object>> GetProperties();

		/// <summary>
		/// Returns the index that this key is found at.
		/// Helper function because tuple
		/// </summary>
		/// <param name="Key"></param>
		/// <returns></returns>
		int GetPropertyIndex(String Key);


	}
}
