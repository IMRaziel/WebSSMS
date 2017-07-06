/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSSMS
{
	public interface IQueryService
	{
		string[] GetConnectionNames();
		string[] GetTableNames(string connectionName);
		FieldInfo[] GetFields(string connectionName, string tableName);
		string GetSavedQueryList();
		string GetSavedQuery(string name);
		void SaveQuery(string name, string query);
		QueryReturn[] RunQuery(string connectionName, string query);
		string[] GetAsCSV(QueryReturn data);
	}
}*/