using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSSMS
{
	public static class ConnectionStringsProvider
	{
		public struct ConnectionString
		{
			public string value;
			public string label;
			public Guid id;
		}

		public static ConnectionString GetById(string id) {
			return connStrings.First(x => x.id == Guid.Parse(id));
		}

		public static readonly ConnectionString[] connStrings = new[] {
			new ConnectionString{ value = @"Data Source=HAL-9000\SQLEXPRESS;Initial Catalog=master;Persist Security Info=True;User ID=sa;Password=zasazz", label = "Default" , id = Guid.Parse("fe622713-ec41-405a-94d4-e8afdf6a626a")}
		};

	}
}