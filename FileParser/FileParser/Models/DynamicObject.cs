using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileParser.Models
{
	public class DynamicObject
	{
		public dynamic Instance = new ExpandoObject();

		public void AddProperty(string name, object value)
		{
			((IDictionary<string, object>)this.Instance).Add(name, value);
		}

		public dynamic GetProperty(string name)
		{
			if (((IDictionary<string, object>)this.Instance).ContainsKey(name))
				return ((IDictionary<string, object>)this.Instance)[name];
			else
				return null;
		}

		public void Clear()
		{
			((IDictionary<string, object>)this.Instance).Clear();
		}
	}
}
