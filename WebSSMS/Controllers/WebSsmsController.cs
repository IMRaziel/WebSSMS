using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;

namespace WebSSMS.Controllers
{
	[EnableCors(origins: "*", headers: "*", methods: "*")]
	public class WebSsmsController : ApiController
	{

		[HttpGet]
		public HttpResponseMessage Index()
		{
			var response = Request.CreateResponse(HttpStatusCode.Found);
			response.Headers.Location = new Uri("http://localhost:8080/");
			//			response.Headers.Location = new Uri(Request.RequestUri, "/client/dist/index.html");
			return response;
		}


		[Route("api/conn_strings")]
		[HttpGet]
		public async Task<object[]> GetConnectionStrings()
		{
			return ConnectionStringsProvider.connStrings.Select(x => new
			{
				value = x.id,
				x.label
			}).ToArray();
		}

		public class SelectFieldModel
		{
			public string value { get; set; }
			public string label { get; set; }
			public Guid id { get; set; }
		}

		private static List<SelectFieldModel> savedQueries = new List<SelectFieldModel>();

		[Route("api/query_list")]
		[HttpGet]
		public async Task<SelectFieldModel[]> GetSavedQueryList()
		{
			return savedQueries.ToArray();
		}

		[Route("api/query_list/save")]
		[HttpPost]
		public async Task<SelectFieldModel> SaveQuery([FromBody] SelectFieldModel query)
		{
			query.id = query.id == Guid.Empty ? Guid.NewGuid() : query.id;
			savedQueries = savedQueries.Where(x => x.id != query.id).ToList();
			savedQueries.Add(query);
			return query;
		}


		public class Table
		{
			public string name;
			public Field[] fields;

			public class Field
			{
				public string name;
				public string type;
			}
		}



		[Route("api/tables")]
		[HttpGet]
		public async Task<Table[]> GetConnectionStrings([FromUri] string conn_string_id)
		{
			var query = @"
					SELECT '['  +TABLE_CATALOG + '].['  + TABLE_SCHEMA + '].[' + TABLE_NAME + ']' as table_name, '[' + COLUMN_NAME + ']' as name, DATA_TYPE as type
					FROM INFORMATION_SCHEMA.COLUMNS
					ORDER BY table_name
			";
			var cs = ConnectionStringsProvider.GetById(conn_string_id).value;
			return (await Utils.GetDictsFromQuery(query, cs))
					.GroupBy(x => x["table_name"] as string)
					.Select(x => new Table
					{
						name = x.Key,
						fields = x.Select(f => f.ToObject<Table.Field>()).ToArray()
					})
					.ToArray();
		}


		public class RunQueryParams {
			public string conn_string_id { get; set; }
			public string query_text { get; set; }
			public bool slow { get; set; }
		}

		[Route("api/query_runner/run")]
		[HttpPost]
		public async Task<SqlQueryRunner.RunQueryResults> RunQuery([FromBody] RunQueryParams param)
		{
			var cs = ConnectionStringsProvider.GetById(param.conn_string_id);
			var query = await SqlQueryRunner.RunQuery(cs, param.query_text, param.slow);
			return query;
		}

		[Route("api/query_runner/results")]
		[HttpGet]
		public async Task<SqlQuery> GetQueryResults([FromUri] Guid query_id)
		{
			var result = await SqlQueryRunner.GetQueryResult(query_id);
			return result;
		}

		[Route("api/query_runner/cancel")]
		[HttpGet]
		public SqlQuery CancelQuery([FromUri] Guid query_id)
		{
			return SqlQueryRunner.CancelQuery(query_id);
		}

		[Route("api/query_runner/download_as_csv")]
		[HttpGet]
		public HttpResponseMessage DownloadAsCsv([FromUri] string query_id, [FromUri] string name)
		{
			
			var query = SqlQueryRunner.GetFinishedSqlQueryResult(query_id);
			var sb = new StringBuilder();
			sb.AppendLine(String.Join(",", query.Columns));

			foreach(var row in query.data)
			{
				sb.AppendLine(String.Join(
					",",
					row.Select(JsonConvert.SerializeObject)
				));
			}

			var stream = new MemoryStream();
			var bytes = Encoding.UTF8.GetBytes(sb.ToString());
			var result = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new ByteArrayContent(bytes)
			};
			result.Content.Headers.ContentDisposition =
				new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
				{
					FileName = name + ".csv"
				};
			result.Content.Headers.ContentType =
				new MediaTypeHeaderValue("application/octet-stream");

			return result;
		}
	}
}