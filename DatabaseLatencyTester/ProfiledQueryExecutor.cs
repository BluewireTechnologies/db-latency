using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Transactions;
using log4net;

namespace DatabaseLatencyTester
{
    public class ProfiledQueryExecutor
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ProfiledQueryExecutor));

        private readonly string connectionString;

        public ProfiledQueryExecutor(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public void ExecuteQuery(QueryDefinition definition)
        {
            var succeeded = true;
            var infoMessages = new List<string>();
            try
            {
                using (QueryMetrics.Total.NewContext(definition.Name))
                {
                    using (var cn = new SqlConnection(connectionString))
                    using (var scope = new TransactionScope(TransactionScopeOption.Required))
                    {
                        cn.InfoMessage += (s, e) => infoMessages.Add(e.Message);
                        using (QueryMetrics.OpenConnection.NewContext(definition.Name))
                        {
                            cn.Open();
                        }

                        var cmd = cn.CreateCommand();
                        cmd.CommandText = $"set statistics time on;\n{definition.Text}";
                        cmd.CommandType = CommandType.Text;

                        using (var reader = QueryMetrics.ExecuteReader.Time(() => cmd.ExecuteReader(), definition.Name))
                        using (QueryMetrics.ConsumeReader.NewContext(definition.Name))
                        {
                            do
                            {
                                ConsumeResultSet(reader);
                            } while(reader.NextResult());
                        }

                        using (QueryMetrics.CompleteTransaction.NewContext(definition.Name))
                        {
                            scope.Complete();
                            scope.Dispose();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                succeeded = false;
                log.Error(ex);
            }
            finally
            {
                if (succeeded) QueryMetrics.Succeeded.Mark();
                else QueryMetrics.Failed.Mark();
            }

            var databaseTime = GetDatabaseTime(infoMessages);
            if (databaseTime != null)
            {
                QueryMetrics.CPUTime.Record(databaseTime.CpuNs, Metrics.TimeUnit.Nanoseconds, definition.Name);
                QueryMetrics.ElapsedTime.Record(databaseTime.ElapsedNs, Metrics.TimeUnit.Nanoseconds, definition.Name);
            }
        }

        private static SqlTimeStatistics GetDatabaseTime(IEnumerable<string> messages)
        {
            var parser = new SqlInfoMessageParser();
            var time = new SqlTimeStatistics();
            var timingsCount = 0;
            foreach (var message in messages)
            {
                if (parser.TryParse(message, ref time)) timingsCount++;
            }
            return timingsCount == 0 ? null : time;
        }

        private static void ConsumeResultSet(IDataReader reader)
        {
            var fields = new string[reader.FieldCount];
            var types = new Type[reader.FieldCount];
            for (var i = 0; i < reader.FieldCount; ++i)
            {
                var name = reader.GetName(i);
                types[i] = reader.GetFieldType(i);
                fields[i] = name;
            }
            while (reader.Read())
            {
                var data = new object[fields.Length];
                for (var index = 0; index < reader.FieldCount; index++)
                {
                    data[index] = reader[index];
                }
            }
        }
    }
}
