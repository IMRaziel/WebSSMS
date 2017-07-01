using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace WebSSMS.Controllers
{
	public class WebSsmsController : ApiController
	{

		[HttpGet]
		public HttpResponseMessage Index()
		{
			var response = Request.CreateResponse(HttpStatusCode.Found);
			response.Headers.Location = new Uri(Request.RequestUri, "/client/dist/index.html");
			return response;
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