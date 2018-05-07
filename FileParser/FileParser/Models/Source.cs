using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileParser.Models
{
	public class Source
	{
		private string _Name = string.Empty;
		private string _Path = string.Empty;
		public string BatchName
		{
			get { return this._Name; }
			set
			{
				this._Name = value;
			}
		}
		public string SourceFile
		{
			get { return this._Path; }
			set
			{
				this._Path = value;
			}
		}

	}

}
