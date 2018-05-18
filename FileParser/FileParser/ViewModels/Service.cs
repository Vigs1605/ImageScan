using FileParser.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using P8QueryConnectorLib;
using System.IO;

namespace FileParser.ViewModels
{

	public class Service
	{
		public dynamic Obj;

		public string ConnectionStatus;

		public P8QueryServer P8Connector;

		public Service(DynamicObject dynobj)
		{
			this.Obj = dynobj;

			var user = this.Obj.GetProperty("UserName");
			var pwd = this.Obj.GetProperty("Password");
			var domain = this.Obj.GetProperty("Domain");
			var DocClass = this.Obj.GetProperty("DocClass");
			this.P8Connector = new P8QueryServer(P8QueryServer.RunMode.CONNECTED)
			{
				CEURI = this.Obj.GetProperty("URI")
			};
			this.ConnectionStatus = this.P8Connector.login(user, pwd, domain) ? "Connected" : this.P8Connector.getErrorString();

		}

		public ResponseObject Insert(List<Field> rec)
		{
			var objResp = new ResponseObject();
			CommittalRecord comRec = new CommittalRecord(this.Obj.GetProperty("DocClass"));
			foreach (Field field in rec)
			{
				P8QueryServer.DocumentProperty prop;
				prop = new P8QueryServer.DocumentProperty
				{
					Name = field.Name,
					Value = field.Value,
					Type = field.Type == "Date" ? P8QueryServer.DocumentProperty.propType.TYPE_DATETIME : (field.Type == "Number" ? P8QueryServer.DocumentProperty.propType.TYPE_INTEGER : P8QueryServer.DocumentProperty.propType.TYPE_STRING)
				};
				comRec.Properties.Add(prop);
				if (field.Name == "ImagePath")
				{
					prop = new P8QueryServer.DocumentProperty
					{
						Name = "DocumentTitle",
						Value = Path.GetFileName(field.Value),
						Type = P8QueryServer.DocumentProperty.propType.TYPE_STRING
					};
					comRec.Properties.Add(prop);
					comRec.Contents.Add(field.Value);
				}
			}

			Microsoft.VisualBasic.Collection docCollection = new Microsoft.VisualBasic.Collection();
			docCollection.Add(comRec);

			var success = this.P8Connector.commitDocuments(ref docCollection);

			//log the id created if success, or the error if it failed
			if (success == true)
			{
				objResp.AdditionalInfo = comRec.Id;
				objResp.ResultCode = true;
			}
			else
			{
				objResp.AdditionalInfo= P8Connector.getErrorString();
				objResp.ResultCode = false;
			}



			return objResp;
		}

	}

	
}

