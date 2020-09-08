using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("name", "year", "month", "date", "nested")]
	public class ES3UserType_BattleLog : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_BattleLog() : base(typeof(BattleLog)){ Instance = this; priority = 1; }


		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (BattleLog)obj;
			
			writer.WriteProperty("name", instance.name, ES3Type_string.Instance);
			writer.WriteProperty("year", instance.year, ES3Type_int.Instance);
			writer.WriteProperty("month", instance.month, ES3Type_int.Instance);
			writer.WriteProperty("date", instance.date, ES3Type_int.Instance);
			writer.WriteProperty("nested", instance.nested);
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (BattleLog)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "name":
						instance.name = reader.Read<System.String>(ES3Type_string.Instance);
						break;
					case "year":
						instance.year = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "month":
						instance.month = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "date":
						instance.date = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "nested":
						instance.nested = reader.Read<BattleLog.Nested>();
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var instance = new BattleLog();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}


	public class ES3UserType_BattleLogArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_BattleLogArray() : base(typeof(BattleLog[]), ES3UserType_BattleLog.Instance)
		{
			Instance = this;
		}
	}
}