using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BixBite.Resources
{
	class EE_Exceptions
	{

	}

	public class PropertyNotFoundException : Exception
	{
		public PropertyNotFoundException()
		{

		}

		public PropertyNotFoundException(string PName)
				: base(String.Format("Property {0} Not found in Collection", PName))
		{

		}

	}

}
