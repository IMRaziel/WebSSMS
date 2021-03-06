﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.Caching;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.IO;
using System.Collections.Concurrent;

namespace WebSSMS
{
	static public class SqlQueryRunner
	{
		static ObjectCache FinishedQueries = MemoryCache.Default;
		private static ConcurrentDictionary<Guid, SqlQuery> Queries = new ConcurrentDictionary<Guid, SqlQuery>();

		public class RunQueryResults
		{
			public string error;
			public ParseError[] parseErrors;
			public SqlQuery[] queries;
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
			SqlQuery tmp;
			Queries.TryRemove(query.id, out tmp);

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

			SqlQuery tmp;
			Queries.TryRemove(query.id, out tmp);
			return query;
		}

		public static SqlQuery GetFinishedSqlQueryResult(string id) {
			return FinishedQueries.Get(id) as SqlQuery;
		}

		public static async Task<RunQueryResults> RunQuery(ConnectionStringsProvider.ConnectionString connString, string queryText, bool slow = false)
		{
			var result = new RunQueryResults();
			SqlConnection sqlConnection = new SqlConnection(connString.value);

			var splitResult = SqlSplitter.Split(queryText);
			if(splitResult.errors.Count()!=0)
			{
				result.parseErrors = splitResult.errors;
				result.error = "Syntax error in SQL query";
				return result;
			}

			SqlConnection conn = null;
			try
			{
				conn = new SqlConnection(connString.value);
				conn.StatisticsEnabled = true;
				conn.Open();

				result.queries = splitResult.queries.Select(x => {
					var id = Guid.NewGuid();
					var query = new SqlQuery
					{
						id = id,
						ConnectionId = connString.id,
						ConnectionName = connString.label,
						QueryStatus = SqlQuery.Status.Running,
						SqlText = x,
						connection = conn,
						command = new SqlCommand(x, conn),
						data = new object[0][]
					};
					Queries[id] = query;

					// store in cache for 10 minutes in case someone wantes to download results
					var p = new CacheItemPolicy();
					p.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(10);
					FinishedQueries.Add(new CacheItem(query.id.ToString(), query), p);

					return query;
				}).ToArray();


#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				// should run in background. no need for await here
				Task.Run(async () =>
				{
					try
					{
						foreach (var query in result.queries)
						{
							SqlDataReader reader = null;
							try
							{
								// for testing of query cancelation
								if (slow)
								{
									await Task.Delay(10000);
								}
								reader = await query.command.ExecuteReaderAsync();
								query.Columns = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToArray();
								query.data = Utils.GetArraysFromQuery(reader).ToArray();
								query.Stats = conn.RetrieveStatistics();
								conn.ResetStatistics();
								query.QueryStatus = SqlQuery.Status.Finished;
							}
							catch (Exception e)
							{
								query.QueryStatus = SqlQuery.Status.Error;
								query.Error = e.Message;
							}
							finally
							{
								reader?.Close();
							}
						}
					}
					finally
					{
						conn.Close();
					}
				});
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			}
			catch (Exception e)
			{
				result.error = e.Message;
				conn?.Close();
			}

			return result;
		}
	}
}