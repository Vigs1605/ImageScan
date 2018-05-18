using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using FileParser.Models;


namespace FileParser.ViewModels
{
	public class Presenter : ObservableObject
	{
		private string _configFile;
		private ObservableCollection<Batch> _BatchFiles = new ObservableCollection<Batch>();
		private Dictionary<string, List<TimeSlot>> _schedules = new Dictionary<string, List<TimeSlot>>();
		private string _status;
		private string _hyperlinktext;
		private string _ButtonText;
		private ICommand _ButtonClickCommand;
		private string _eta;
		

		public ObservableCollection<Batch> BatchFiles
		{
			get
			{
				return _BatchFiles;
			}
			set
			{
				_BatchFiles = value;
				RaisePropertyChangedEvent("BatchFiles");
			}
		}
		public Dictionary<string, List<TimeSlot>> Schedules
		{
			get
			{
				return _schedules;
			}
			set
			{
				_schedules = value;
				RaisePropertyChangedEvent("Schedules");
			}
		}

		public char delimiter;
		public List<Field> fields = new List<Field>();

		public BackgroundWorker workerThread = new BackgroundWorker()
		{
			WorkerSupportsCancellation = true,
			WorkerReportsProgress = true
		};
		public Timer timer = new Timer() { Interval = 360000, Enabled = true, AutoReset = true };

		public dynamic ConObj = new Models.DynamicObject();

		private Service service;

		public string ConfigFile
		{
			get { return _configFile; }
			set
			{
				_configFile = value;
				RaisePropertyChangedEvent("ConfigFile");
			}
		}

		public string HyperlinkText
		{
			get { return _hyperlinktext ?? (_hyperlinktext = "<<Select File Name>>"); }
			set
			{
				_hyperlinktext = value;
				RaisePropertyChangedEvent("HyperlinkText");
			}
		}

		public string Status
		{
			get { return _status; }
			set
			{
				_status = value;
				RaisePropertyChangedEvent("Status");
			}
		}

		public string ButtonText
		{
			get { return _ButtonText ?? (_ButtonText = "Start"); }
			set
			{
				_ButtonText = value;
				RaisePropertyChangedEvent("ButtonText");
			}
		}

		public ICommand BrowseCommand
        {
            get { return new ActionCommand(p => LoadConfig(ConfigFile));  }
        }

		public ICommand StartCommand
		{
			get { return new ActionCommand(p => ProcessFiles()); }
		}

		public ICommand CancelCommand
		{
			get { return new ActionCommand(p => Cancel()); }
		}

		public ICommand ButtonClickCommand
		{
			get { return _ButtonClickCommand ?? (_ButtonClickCommand = StartCommand); }
			set
			{
				_ButtonClickCommand = value;
				RaisePropertyChangedEvent("ButtonClickCommand");
			}
		}

		public string ETA
		{
			get { return _eta; }
			set
			{
				_eta = value;
				RaisePropertyChangedEvent("ETA");
			}

		}

		public void LoadConfig(string ConfigFile)
		{
			Reset();
			XmlDocument map = new XmlDocument();
			#region SetXMLReader
			try
			{
				XmlReaderSettings readerSettings = new XmlReaderSettings();
				readerSettings.IgnoreWhitespace = true;
				readerSettings.IgnoreComments = true;
				readerSettings.CheckCharacters = true;
				readerSettings.CloseInput = true;
				readerSettings.IgnoreProcessingInstructions = false;
				readerSettings.ValidationFlags = System.Xml.Schema.XmlSchemaValidationFlags.None;
				readerSettings.ValidationType = ValidationType.None;
				XmlReader reader = XmlReader.Create(ConfigFile, readerSettings);
				
				map.Load(reader);
			}
			catch
			{
				Status = "Error Loading Config File. Please select a config file";
				return;
			}
			#endregion

			#region loadinputfile_and_counters

			XmlNodeList bl = map.SelectNodes("configuration/Source/Batch");
			foreach (XmlNode batchfile in bl)
			{
				Batch B = new Batch();
				B.BatchName = batchfile.Attributes["Name"].Value;
				B.SourceFile = batchfile.Attributes["Path"].Value;
				B.SuccessLog = B.SourceFile.Insert(B.SourceFile.IndexOf("."), "_Success");
				B.ErrorLog = B.SourceFile.Insert(B.SourceFile.IndexOf("."), "_Error");

				bool duplicate = BatchFiles.Any(item => item.SourceFile == B.SourceFile);
				if (duplicate) { Status = "Duplicate Batch Files"; Reset() ; return; }
				else { BatchFiles.Add(B); }
			}
			
			#endregion

			#region loadDelimiter
			var delimnode = map.SelectSingleNode("configuration/Delimiter/Delimiter");
			delimiter = delimnode.Attributes["Character"].Value.ToCharArray().First();
			#endregion

			#region loadFields
			XmlNodeList fieldNodes = map.SelectNodes("configuration/FileMap/Field");
			//Loop through the nodes and create a field object
			// for each one.
			foreach (XmlNode fieldNode in fieldNodes)
			{
				Field field = new Field
				{

					//Set the field's name
					Name = fieldNode.Attributes["Name"].Value,

					//Set the field's type
					Type = fieldNode.Attributes["Type"].Value,


				};
				bool duplicate = fields.Any(item => item.Name == field.Name);
				if (duplicate) { Status = "Duplicate Field Names"; Reset(); return; }
				else { fields.Add(field); }
			
			
			}

			#endregion

			#region loadTimes
			
			Schedules.Clear();
			XmlNodeList b2 = map.SelectNodes("configuration/schedules");
			foreach (XmlNode node in b2)
			{
				foreach (XmlNode childNode in node.ChildNodes)
				{
					List<TimeSlot> slots = new List<TimeSlot>();

					foreach (XmlNode gchildNode in childNode)
					{
						
						TimeSlot t = new TimeSlot();
						t.StartTime = gchildNode.Attributes["Start"].Value;
						t.EndTime = gchildNode.Attributes["End"].Value;
						if (Convert.ToInt32(t.StartTime) > Convert.ToInt32(t.EndTime)) { Status = "Start Time cannot be greater than End Time"; BatchFiles.Clear();Schedules.Clear();fields.Clear();   return; }
						else { slots.Add(t); }
					}
					if (Schedules.ContainsKey(childNode.Name.ToString()))
					{
						Schedules[childNode.Name.ToString()] = slots;
						
					}
					else
					{
						Schedules.Add(childNode.Name.ToString(), slots);
												
					}
				}
			}
			#endregion

			#region LoadConnection
			var connname = map.SelectSingleNode("configuration/System/connection");
			string connectionid = connname.Attributes["Name"].Value.ToString();
			XmlNode conn = map.SelectSingleNode("configuration/Connections/connection[@Name='"+connectionid+"']");

			
			foreach (XmlNode childnode in conn.ChildNodes)
			{
				var  Param = childnode.Name;
				var ParamVal = childnode.InnerText.ToString();
				ConObj.AddProperty(Param, ParamVal);
				
			}
			
			#endregion

		}


		public void ProcessFiles()
		{
			if (ConfigFile == null)
			{
				Status = "Please load the config file..";
				return;
			}
			else
			{
				if (CanRunNow())
				{
					service = new Service(ConObj);
					if (service.ConnectionStatus == "Connected")
					{
						ButtonText = "Pause";
						ButtonClickCommand = CancelCommand;
						workerThread.DoWork += new DoWorkEventHandler(WorkerThread_DoWork);
						workerThread.ProgressChanged += worker_ProgressChanged;
						workerThread.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerThread_RunWorkerCompleted);
						workerThread.RunWorkerAsync();
					}
					else
					{
						Status = "Login Failed " + service.ConnectionStatus;
						return;
					}
				}
				else
				{
					timer.Elapsed+= OnTimedEvent;
					timer.Start();
					//Status = "Process will start at "+ Status;

				}
				
			}

		}

		private void WorkerThread_DoWork(object sender, DoWorkEventArgs e)
		{
			
			foreach (Batch batch in BatchFiles)
			{
				
				int cursor = 0;
				if (batch.TotalRecords == batch.Success + batch.Failed || batch.TotalRecords == batch.Success || batch.TotalRecords == batch.Failed) { Status = "All Files Processed! "; }
				else
				{
					// Start Processing
					//Create Files if they dont exist

					if (!File.Exists(batch.SuccessLog))
					{
						try
						{
							File.Create(batch.SuccessLog).Close();

						}
						catch
						{
							Status = "Could not create Success Log";
							Cancel();
						}
					}
					if (!File.Exists(batch.ErrorLog))
					{
						try
						{
							File.Create(batch.ErrorLog).Close();

						}
						catch
						{
							Status = "Could not create Error Log";
							Cancel();
						}
					}
					var sourcelist = File.ReadAllLines(batch.SourceFile).ToList();
					var successlist = File.ReadAllLines(batch.SuccessLog).ToList();
					var errorlist = File.ReadAllLines(batch.ErrorLog).ToList();

					if (batch.Failed == 0 && batch.Success == 0)
					{
						//Start Processing from scratch
						cursor = 0;

					}
					else
					{
						if (batch.Failed == 0)
						{
							/*take the line number from SuccessLog*/
							cursor = Convert.ToInt32((successlist.Last()).Split(new char[] { delimiter }).First());
						}
						else if (batch.Success == 0)
						{        /*take the line number from Error Log */
							cursor = Convert.ToInt32((errorlist.Last()).Split(new char[] { delimiter }).First());
						}
						else
						{ /*compare the line numbers*/
							cursor = (Convert.ToInt32((successlist.Last()).Split(new char[] { delimiter }).First()) > Convert.ToInt32((errorlist.Last()).Split(new char[] { delimiter }).First())) ? Convert.ToInt32((successlist.Last()).Split(new char[] { delimiter }).First()) : Convert.ToInt32((errorlist.Last()).Split(new char[] { delimiter }).First());
						}
					}
					//Take the sourcelist and start processing    

					sourcelist = sourcelist.Skip(cursor).ToList();
					List<List<Field>> records = new List<List<Field>>();
					List<string> PLs = new List<string>();
					var startTime = DateTime.Now;
					for (int i = 0, lineno = cursor + 1; i < sourcelist.Count; i++, lineno++)
					{
						//check for string not null

						List<Field> record = new List<Field>();
						var templist = sourcelist[i].Split(new char[] { delimiter, '|' }).ToList();
						foreach (Field field in fields)
						{
							Field fileField = new Field();
							fileField.Name = field.Name;
							fileField.Type = field.Type;
							if (templist.First().StartsWith("\""))
							{
								int end = templist.FindIndex(item => item.EndsWith("\""));
								fileField.Value = string.Join(" ", templist.Skip(0).Take(end + 1).ToArray());
								templist.RemoveRange(0, end + 1);
							}
							else
							{
								fileField.Value = templist.First();
								templist.RemoveAt(0);
							}

							record.Add(fileField);

						} //each record is a line 
						ResponseObject Res = service.Insert(record);
						PLs.Add(sourcelist[i].Replace(sourcelist[i], lineno.ToString() + " " + "," + sourcelist[i]) + Res.AdditionalInfo);
						File.AppendAllLines(Res.ResultCode ? batch.SuccessLog : batch.ErrorLog, PLs.ToArray());
						PLs.Clear();
						// Need to add Logic to write into Error or Success Logs
						batch.Failed = File.ReadAllLines(batch.ErrorLog).Length;
						batch.Success = File.ReadAllLines(batch.SuccessLog).Length;
						batch.Pending = batch.TotalRecords - batch.Success + batch.Failed;
						records.Add(record);
						if (workerThread.CancellationPending == true)
						{
							e.Cancel = true;
							return;
						}
						if (!CanRunNow()) { Pause(); }
						int percents = (batch.TotalRecords - batch.Pending) * 100/batch.TotalRecords;
						batch.Progress = percents.ToString();
						workerThread.ReportProgress((percents));
						TimeSpan timeRemaining = TimeSpan.FromTicks(DateTime.Now.Subtract(startTime).Ticks * (sourcelist.Count - (i + 1)) / (i + 1));
						ETA = "Estimated Time Left : "+ String.Format("{0} days, {1} hours, {2} minutes, {3} seconds", timeRemaining.Days, timeRemaining.Hours, timeRemaining.Minutes, timeRemaining.Seconds);
						
					}


				}
			}
			


		}

		private void workerThread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Cancelled && !CanRunNow())
			{
				Status = "Process will resume as per schedule";
			}
			else if(e.Cancelled)
			{
				Status = "Cancelled by user...";
			}
			else
			{
				//lblStatus.Foreground = Brushes.Green;
				Status = "Work Completed";
			}
		}

		public void Cancel()
		{
			ButtonText = "Start";
			ButtonClickCommand = StartCommand;
			workerThread.CancelAsync();
		}

		public void Pause()
		{
			timer.Start();
			workerThread.CancelAsync();
		}

		void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
		{
			if (CanRunNow())
			{
				service = new Service(ConObj);
				if (service.ConnectionStatus == "Connected")
				{
					timer.Stop();
					ButtonText = "Pause";
					ButtonClickCommand = CancelCommand;
					workerThread.DoWork += new DoWorkEventHandler(WorkerThread_DoWork);
					workerThread.ProgressChanged += worker_ProgressChanged;
					workerThread.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerThread_RunWorkerCompleted);
					workerThread.RunWorkerAsync();
				}
				else
				{
					Status = "Login Failed " + service.ConnectionStatus;
					return;
				}
			}
			
		}

		void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			
			Status = "Working... (" + e.ProgressPercentage + "%)";
		}

		
		public bool CanRunNow()
		{

			List<TimeSlot> value = new List<TimeSlot>();
			var CurrDt = DateTime.Now;
			int noOfdays = 0;
			var day = CurrDt.ToString("dddd");
			foreach (KeyValuePair<string, List<TimeSlot>> pair in Schedules)
			{
				if (Schedules.TryGetValue(CurrDt.ToString("dddd").ToString(), out value))
				{
					foreach (TimeSlot ts in value)
					{
						var starttime = DateTime.ParseExact(ts.StartTime, "HHmm", null, System.Globalization.DateTimeStyles.None).AddDays(noOfdays);
						var endtime = DateTime.ParseExact(ts.EndTime, "HHmm", null, System.Globalization.DateTimeStyles.None).AddDays(noOfdays);
						if (DateTime.Now >= starttime && DateTime.Now <= endtime)
						{

							return true;
						}
						else if (DateTime.Now < starttime)
						{
							Status = "Process will start at "+starttime.ToString();
							return false;
						}
					}
				}
				noOfdays++;
				CurrDt=CurrDt.AddDays(noOfdays);

			}
			Status = "Schedule Not Known";
			return false;

		}

		public void Reset()
		{
			BatchFiles.Clear();
			Status = " ";
			Schedules.Clear();
			fields.Clear();
			ConObj.Clear();
			
		}

	}
}

