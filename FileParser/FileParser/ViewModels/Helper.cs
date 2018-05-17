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
	public class Helper
	{
		public dynamic Obj;

		public bool ConnectionStatus;

		public Helper(DynamicObject dynobj)
		{
			this.Obj = dynobj;
			
		}

		public ResponseObject connect(DynamicObject dynobj)
		{
			ResponseObject obj = new ResponseObject();
			
			return obj;
			
		}

		public ResponseObject Insert(List<Field> rec)
		{
			var objResp = new ResponseObject();
			var uri = Obj.GetProperty("URI");
			var user = Obj.GetProperty("UserName");
			var pwd = Obj.GetProperty("Password");
			var domain = Obj.GetProperty("Domain");
			var DocClass = Obj.GetProperty("DocClass");
			var P8Connector = new P8QueryServer(P8QueryServer.RunMode.CONNECTED);
			P8Connector.CEURI = uri;
			var success = P8Connector.login(user, pwd, domain);
			if (success) { obj.ResultCode = true; obj.AdditionalInfo = "Success"; }
			else
			{ obj.ResultCode = false; obj.AdditionalInfo = P8Connector.getErrorString(); }

			CommittalRecord comRec = new CommittalRecord(DocClass);
			foreach (Field field in rec)
			{
				P8QueryServer.DocumentProperty prop;
				prop = new P8QueryServer.DocumentProperty();
				prop.Name = field.Name;
				prop.Value = field.Value;
				prop.Type = P8QueryServer.DocumentProperty.propType.TYPE_STRING;
				comRec.Properties.Add(prop);
				if (field.Name == "ImagePath")
				{
					prop = new P8QueryServer.DocumentProperty();
					prop.Name = "DocumentTitle";
					prop.Value = Path.GetFileName(field.Value);
					prop.Type = P8QueryServer.DocumentProperty.propType.TYPE_STRING;
					comRec.Properties.Add(prop);
					comRec.Contents.Add(field.Value);
				}
			}

			Microsoft.VisualBasic.Collection docCollection = new Microsoft.VisualBasic.Collection();
			docCollection.Add(comRec);

			success = P8Connector.commitDocuments(ref docCollection);

			//log the id created if success, or the error if it failed
			if (success == true)
			{
				var sNewID = comRec.Id;
			}
			else
			{
				var sError = P8Connector.getErrorString();
			}



			return objResp;
			/*

			//add whatever properties are needed for document
			

			prop = new P8QueryConnectorLib.P8QueryServer.DocumentProperty();
			prop.Name = "fieldB";
			prop.Value = "bbb";
			prop.Type = P8QueryConnectorLib.P8QueryServer.DocumentProperty.propType.TYPE_STRING;
			comRec.Properties.Add(prop);

			prop = new P8QueryConnectorLib.P8QueryServer.DocumentProperty();
			prop.Name = "fieldC";
			prop.Value = "ccc";
			prop.Type = P8QueryConnectorLib.P8QueryServer.DocumentProperty.propType.TYPE_STRING;
			comRec.Properties.Add(prop);

			//the DocumentTitle property is needed so that the proper mime type is displayed in filenet
			prop = new P8QueryConnectorLib.P8QueryServer.DocumentProperty();
			prop.Name = "DocumentTitle";
			prop.Value = "myfile.pdf";
			prop.Type = P8QueryConnectorLib.P8QueryServer.DocumentProperty.propType.TYPE_STRING;
			comRec.Properties.Add(prop);


			//attach the file to the commital record
			comRec.Contents.Add("C:\\path\\myfile.pdf");

			//commit the document
			Microsoft.VisualBasic.Collection docCollection = new Microsoft.VisualBasic.Collection();
			docCollection.Add(comRec);

			success = P8Connector.commitDocuments(ref docCollection);

			//log the id created if success, or the error if it failed
			if (success == true)
			{
				var sNewID = comRec.Id;
			}
			else
			{
				var sError = P8Connector.getErrorString();
			}

*/
		}
	}
}
