using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.IO;
using System.Linq;

namespace WebSSMS
{
	public static class SqlSplitter
	{
		public class SplitResult
		{
			public ParseError[] errors = new ParseError[0];
			public string[] queries = new string[0];
		}

		public static SplitResult Split(string t)
		{
			var result = new SplitResult();

			var parser = new TSql120Parser(false);
			var sg = new Sql120ScriptGenerator();

			using (StringReader sr = new StringReader(t))
			{
				IList<ParseError> err = new List<ParseError>();
				var root = parser.Parse(sr, out err);
				if (err.Count() != 0)
				{
					result.errors = err.ToArray();
					result.queries = new string[0];
				}
				result.queries = GetStatements(root).ToArray();

			}
			return result;
		}

		private static IEnumerable<string> GetStatements(TSqlFragment fragment)
		{
			Sql120ScriptGenerator sg = new Sql120ScriptGenerator();
			TSqlScript script = fragment as TSqlScript;
			if (script != null)
			{
				return script.Batches.SelectMany(x => x.Statements, (_, x) => ScriptFragment(sg, x));
			}
			else
			{
				// TSqlFragment is a TSqlBatch or a TSqlStatement
				return new[] { ScriptFragment(sg, fragment) };
			}
		}

		private static string ScriptFragment(SqlScriptGenerator sg, TSqlFragment fragment)
		{
			string resultString;
			sg.GenerateScript(fragment, out resultString);
			return resultString;
		}
	}
}