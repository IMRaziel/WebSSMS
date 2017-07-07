using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace WebSSMS
{
	public class SqlQuery
	{
		public enum Status {
			Running,
			Finished,
			Error,
			Cancelled
		}

		public Guid id;
		public string ConnectionName;
		public Guid ConnectionId;
		public string SqlText;
		public Status QueryStatus;
		public IDictionary Stats;
		public List<Dictionary<string, object>> data;
		public string Error;
		public SqlQuery NextQuery;

		[ScriptIgnore, JsonIgnore]
		public SqlCommand command;
		[ScriptIgnore, JsonIgnore]
		public SqlConnection connection;
	}
}