using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace WebSSMS
{
	static public class SqlQueryRunner
	{
		private static Dictionary<Guid, SqlQuery> Queries = new Dictionary<Guid, SqlQuery>();

		public static async Task<SqlQuery> GetQueryResult(Guid queryId)
		{
			var query = Queries[queryId];
			while (query.QueryStatus == SqlQuery.Status.Running)
			{
				// if cancelled, return empty object
				if (!Queries.ContainsKey(queryId)) return new SqlQuery();
				await Task.Delay(250);
			}
			Queries.Remove(query.id);

			return query;
		}

		public static SqlQuery CancelQuery(Guid queryId)
		{
			var query = Queries[queryId];
			try
			{
				query.command.Cancel();
			}
			catch { }
			try
			{
				query.connection.Close();
			}
			catch { }

			query.QueryStatus = SqlQuery.Status.Cancelled;
			Queries.Remove(query.id);
			return query;
		}

		public static async Task<SqlQuery> RunQuery(ConnectionStringsProvider.ConnectionString connString, string queryText, bool slow = false)
		{

			var query = new SqlQuery
			{
				id = Guid.NewGuid(),
				ConnectionId = connString.id,
				ConnectionName = connString.label,
				QueryStatus = SqlQuery.Status.Running,
				SqlText = queryText,
			};
			SqlConnection conn = null;
			SqlCommand cmd = null;

			try
			{
				conn = new SqlConnection(connString.value);
				conn.StatisticsEnabled = true;
				query.connection = conn;

				cmd = new SqlCommand(queryText, conn);
				query.command = cmd;

				conn.Open();
				SqlDataReader reader = await cmd.ExecuteReaderAsync();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				// should run in background. no need for await here
				Task.Run(async () =>
				{
					// for testing of query cancelation
					if (slow)
					{
						await Task.Delay(10000);
					}
					try
					{
						query.data = Utils.GetDictsFromQuery(reader);
						query.Stats = conn.RetrieveStatistics();
						query.QueryStatus = SqlQuery.Status.Finished;
					}
					catch (Exception e)
					{
						query.QueryStatus = SqlQuery.Status.Error;
						query.Error = e.Message;
					}
				});
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				Queries[query.id] = query;

			}
			catch (Exception e)
			{
				query.QueryStatus = SqlQuery.Status.Error;
				query.Error = e.Message;

				conn?.Close();
			}

			return query;
		}


	}
}