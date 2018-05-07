using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileParser.Models;

namespace FileParser.ViewModels
{
	public class Batch : Source,INotifyPropertyChanged
	{
		private string _SuccessLog = string.Empty;
		private string _ErrorLog = string.Empty;
		private int _TotalRecords = 0;
		private int _Pending = 0;
		private int _Success = 0;
		private int _Failed = 0;
		private string _progress = string.Empty;

		
		public string SuccessLog
		{
			get { return this._SuccessLog; }
			set
			{
				this._SuccessLog = value;
				NotifyPropertyChanged("SuccessLog");
			}
		}
		public string ErrorLog
		{
			get { return this._ErrorLog; }
			set
			{
				this._ErrorLog = value;
				NotifyPropertyChanged("ErrorLog");
			}
		}
		public int TotalRecords
		{
			get { return (File.Exists(SourceFile)) ? WriteSafeReadAllLines(SourceFile).Length : 0; }
			set
			{
				this._TotalRecords = value;
				NotifyPropertyChanged("TotalRecords");
			}
		}
		public int Pending
		{
			get { return TotalRecords - Success + Failed; }
			set
			{
				this._Pending = value;
				NotifyPropertyChanged("Pending");
			}
		}
		public int Success
		{
			get { return (File.Exists(SuccessLog)) ? WriteSafeReadAllLines(SuccessLog).Length : 0; }
			set
			{
				this._Success = value;
				NotifyPropertyChanged("Success");
			}
		}
		public int Failed
		{
			get { return (File.Exists(ErrorLog)) ? WriteSafeReadAllLines(ErrorLog).Length : 0; }
			set
			{
				this._Failed = value;
				NotifyPropertyChanged("Failed");
			}
		}
		public string Progress
		{
			get { return this._progress; }
			set
			{
				this._progress = value;
				NotifyPropertyChanged("Progress");
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

		private string[] WriteSafeReadAllLines(String path)
		{
			using (var csv = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			using (var sr = new StreamReader(csv))
			{
				List<string> file = new List<string>();
				while (!sr.EndOfStream)
				{
					file.Add(sr.ReadLine());
				}

				return file.ToArray();
			}
		}
	}

}
