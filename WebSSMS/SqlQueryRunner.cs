using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.IO;

namespace WebSSMS
{
	static public class SqlQueryRunner
	{
		private static Dictionary<Guid, SqlQuery> Queries = new Dictionary<Guid, SqlQuery>();

		public static void s(string t)
		{
			TSql120Parser parser = new TSql120Parser(false);
			IList<ParseError> errors;
			using (StringReader sr = new StringReader(t))
			using (StringReader sr2 = new StringReader(t))
			{

				// TODO: first parse batches, then parse statements list for each batch
				TSqlFragment fragment = parser.ParseStatementList(sr, out errors);
				var f2 = parser.Parse(sr2, out errors);
				IEnumerable<string> batches = GetBatches(fragment);
				foreach (var batch in batches)
				{
					Console.WriteLine(batch);
				}
			}
		}

		private static IEnumerable<string> GetBatches(TSqlFragment fragment)
		{
			Sql120ScriptGenerator sg = new Sql120ScriptGenerator();
			TSqlScript script = fragment as TSqlScript;
			if (script != null)
			{
				foreach (var batch in script.Batches)
				{
					yield return ScriptFragment(sg, batch);
				}
			}
			else
			{
				// TSqlFragment is a TSqlBatch or a TSqlStatement
				yield return ScriptFragment(sg, fragment);
			}
		}

		private static string ScriptFragment(SqlScriptGenerator sg, TSqlFragment fragment)
		{
			string resultString;
			sg.GenerateScript(fragment, out resultString);
			return resultString;
		}
	

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

			string connectionString, scriptText;
			SqlConnection sqlConnection = new SqlConnection(connString.value);
			ServerConnection svrConnection = new ServerConnection(sqlConnection);
			Server server = new Server(svrConnection);

			s(queryText);

			// server.ConnectionContext.MultipleActiveResultSets = true;
			server.ConnectionContext.SqlExecutionModes = SqlExecutionModes.ExecuteAndCaptureSql;
			var res = server.ConnectionContext.ExecuteWithResults(queryText);
//			var a1 = res.Tables[0].Rows[0];
//			var a2 = res.Tables[1].Rows[0];

			var query = new SqlQuery
			{
				id = Guid.NewGuid(),
				ConnectionId = connString.id,
				ConnectionName = connString.label,
				QueryStatus = SqlQuery.Status.Running,
				SqlText = queryText,
				data = new List<Dictionary<string, object>>()
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
				var currentQuery = query;
				Task.Run(async () =>
				{
					
					// for testing of query cancelation
					try
					{
						while(true)
						{
							if (slow)
							{
								await Task.Delay(10000);
							}
							currentQuery.data = Utils.GetDictsFromQuery(reader);
							currentQuery.Stats = conn.RetrieveStatistics();
							currentQuery.QueryStatus = SqlQuery.Status.Finished;

							if (!await reader.NextResultAsync()) break;

							currentQuery.NextQuery = new SqlQuery
							{
								ConnectionId = connString.id,
								ConnectionName = connString.label,
								QueryStatus = SqlQuery.Status.Running,
								SqlText = queryText,
								data = new List<Dictionary<string, object>>(),
								id = Guid.NewGuid()
							};
							currentQuery = currentQuery.NextQuery;
							Queries[currentQuery.id] = currentQuery;
						};
					}
					catch (Exception e)
					{
						query.QueryStatus = SqlQuery.Status.Error;
						query.Error = e.Message;
					} finally
					{
						conn.Close();
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