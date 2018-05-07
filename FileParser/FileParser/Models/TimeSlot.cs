using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileParser.Models
{
	public class TimeSlot
	{

		private string _starttime = string.Empty;
		private string _endtime = string.Empty;


		public string StartTime
		{
			get { return this._starttime; }
			set
			{
				this._starttime = value;
				//NotifyPropertyChanged("StartTime");
			}
		}
		

        public string EndTime
		{

			get { return this._endtime; }
			set
			{
				this._endtime = value;
				//NotifyPropertyChanged("EndTime");
			}

		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void NotifyPropertyChanged(string info)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(info));
			}
		}

	}
	
}
