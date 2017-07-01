using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;

namespace WebSSMS.Controllers
{
	[EnableCors(origins: "*", headers: "*", methods: "*")]
	public class WebSsmsController : ApiController
	{
		public struct ConnectionString
		{
			public string value;
			public string label;
			public Guid id;
		}

		private static readonly ConnectionString[] connStrings = new [] {
			new ConnectionString{ value = @"Data Source=HAL-9000\SQLEXPRESS;Initial Catalog=master;Persist Security Info=True;User ID=sa;Password=zasazz", label = "Default" , id = Guid.NewGuid()}
		};

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
			return connStrings.Select(x=> new {
				value = x.id,
				x.label
			}).ToArray();
		}

		[Route("api/tables")]
		[HttpGet]
		public async Task<Table[]> GetConnectionStrings([FromUri] string conn_string_id)
		{
			var query = @"
					SELECT TABLE_SCHEMA + '.' + TABLE_NAME as table_name, COLUMN_NAME as name, DATA_TYPE as type
					FROM INFORMATION_SCHEMA.COLUMNS
			";
			var cs = connStrings.First(x => x.id == Guid.Parse(conn_string_id)).value;
			return (await Utils.GetDictsFromQuery(query, cs))
					.GroupBy(x=>x["table_name"] as string)
					.Select(x => new Table {
						name = x.Key,
						fields = x.Select(f => f.ToObject<Field>()).ToArray()
					})
					.ToArray();
		}

		public class Table
		{
			public string name;
			public Field[] fields;
		}

		public class Field {
			public string name;
			public string type;
		}

		[Route("api/meta")]
		[HttpGet]
		public async Task<string> GetMeta()
		{
			var val = "";
			var connStr = @"Data Source=HAL-9000\SQLEXPRESS;Initial Catalog=master;Persist Security Info=True;User ID=sa;Password=zasazz";
			using (SqlConnection conn = new SqlConnection(connStr))
			{
				await conn.OpenAsync();
				using (SqlCommand cmd = new SqlCommand("SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'", conn))
				{
					using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync()) {
							val = reader["TABLE_NAME"].ToString();
						}
					}
				}
			}
			return val;
		}

	}
}