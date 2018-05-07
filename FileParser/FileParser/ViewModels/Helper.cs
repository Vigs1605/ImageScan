using FileParser.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileParser.ViewModels
{
	public static class Helper
	{

		public static void connect(DynamicObject dynobj)
		{
			var val= dynobj.GetProperty("uri");
			var val2 = dynobj.GetProperty("Password");
		}

		public static ResponseObject Insert(List<List<Field>> rec)
		{
			var objResp = new ResponseObject();
			//Call the service to insert the record
			System.Threading.Thread.Sleep(100);
			return objResp;
		}
	}
}
