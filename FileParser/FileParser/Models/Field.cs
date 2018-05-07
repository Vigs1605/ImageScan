using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileParser.Models
{
	public class Field
	{
		private string _name;
		private string _type;
		private string _value;

		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public string Type
		{
			get { return _type; }
			set { _type = value; }
		}
		public string Value
		{
			get { return _value; }
			set { _value = value; }
		}

	}
}

