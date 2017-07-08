using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace WebSSMS
{
	public static class Utils
	{
		public static async Task<IEnumerable<Dictionary<string, object>>> GetDictsFromQuery(string query, string connStr)
		{
			var result = new List<Dictionary<string, object>>();
			using (SqlConnection conn = new SqlConnection(connStr))
			{
				await conn.OpenAsync();
				using (SqlCommand cmd = new SqlCommand(query, conn))
				{
					using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync())
						{
							var val = new Dictionary<string, object>();
							for (int i = 0; i < reader.FieldCount; i++)
							{
								val[reader.GetName(i)] = reader.GetValue(i);
							}
							result.Add(val);
						}
					}
				}
			}
			return result;
		}


		public static IEnumerable<Dictionary<string, object>> GetDictsFromQuery(SqlDataReader reader)
		{
			var result = new List<Dictionary<string, object>>();
			while (reader.Read())
			{
				var val = new Dictionary<string, object>();
				for (int i = 0; i < reader.FieldCount; i++)
				{
					val[reader.GetName(i)] = reader.GetValue(i);
				}
				result.Add(val);
			}
			return result;
		}


		public static IEnumerable<object[]> GetArraysFromQuery(SqlDataReader reader)
		{
			while (reader.Read())
			{
				var row = new object[reader.FieldCount];
				reader.GetValues(row);
				yield return row;
			}
		}

		public static T ToObject<T>(this IDictionary<string, object> source) where T : class, new()
		{
			T someObject = new T();
			Type someObjectType = someObject.GetType();

			foreach (KeyValuePair<string, object> item in source)
			{
				someObjectType.GetProperties()
					.FirstOrDefault(x=>x.Name == item.Key)
					?.SetValue(someObject, item.Value, null);
				someObjectType.GetFields()
					.FirstOrDefault(x => x.Name == item.Key)
					?.SetValue(someObject, item.Value);

			}

			return someObject;
		}
	}
}