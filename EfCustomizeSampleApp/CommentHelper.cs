using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace EfCustomizeSampleApp
{
    internal class CommentHelper
    {
        private const string ConnectionString =
            @"Server=your server;Database=your database;Trusted_Connection=True";

        internal static string GetComment(string tableName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("SELECT");
            sb.AppendLine("  t.name as TABLE_NAME");
            sb.AppendLine("  , ep.value as COMMENT ");
            sb.AppendLine("FROM");
            sb.AppendLine("  sys.tables t");
            sb.AppendLine("  , sys.extended_properties ep ");
            sb.AppendLine("WHERE");
            sb.AppendLine($"  t.name = '{tableName}' ");
            sb.AppendLine("  AND t.object_id = ep.major_id");
            sb.AppendLine("  AND ep.minor_id = 0");

            var sql = sb.ToString();

            var results = ExecuteSql(sql).ToDictionary(x => x.Key, x => x.Value);

            if (results.Count > 0)
            {
                return results.FirstOrDefault().Value;
            }
            return null;
        }

        internal static Dictionary<string, string> GetComments(string tableName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("SELECT");
            sb.AppendLine("  c.name as COLUMN_NAME");
            sb.AppendLine("  , ep.value as COMMENT ");
            sb.AppendLine("FROM");
            sb.AppendLine("  sys.tables t");
            sb.AppendLine("  , sys.columns c");
            sb.AppendLine("  , sys.extended_properties ep ");
            sb.AppendLine("WHERE");
            sb.AppendLine($"  t.name = '{tableName}' ");
            sb.AppendLine("  AND t.object_id = c.object_id ");
            sb.AppendLine("  AND c.object_id = ep.major_id ");
            sb.AppendLine("  AND c.column_id = ep.minor_id");

            var sql = sb.ToString();

            return ExecuteSql(sql).ToDictionary(x => x.Key, x => x.Value);
        }

        private static IEnumerable<KeyValuePair<string, string>> ExecuteSql(string sql)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var name = reader.GetString(0);
                                var comment = reader.GetString(1);
                                var res = new KeyValuePair<string, string>(name, comment);
                                yield return res;
                            }
                        }
                    }
                }
            }
        }

    }
}
