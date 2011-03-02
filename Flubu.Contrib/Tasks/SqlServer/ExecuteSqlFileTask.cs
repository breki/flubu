namespace Flubu.Tasks.SqlServer
{
    using System.Data.SqlClient;
    using System.Globalization;
    using System.IO;

    using Flubu;

    using Microsoft.SqlServer.Management.Common;
    using Microsoft.SqlServer.Management.Smo;

    /// <summary>
    /// Executes sql script file.
    /// </summary>
    public class ExecuteSqlFileTask : TaskBase
    {
        private string ConnectionString { get; set; }
        private string ScriptFilePath { get; set; }

        /// <summary>
        /// Gets or sets whether build should fail on sql error or not.
        /// </summary>
        public bool FailOnSqlError { get; set; }

        /// <summary>
        /// Sezs whether build should fail on error or not.
        /// </summary>
        /// <param name="fail"></param>
        /// <returns>This instance of <see cref="ExecuteSqlFileTask"/></returns>
        public ExecuteSqlFileTask SetFailOnSqlError(bool fail)
        {
            FailOnSqlError = fail;
            return this;
        }

        /// <summary>
        /// Initializes new instance of <see cref="ExecuteSqlFileTask"/>.
        /// </summary>
        /// <param name="connectionString">Connection string to use.</param>
        /// <param name="fileName">SQL script file name.</param>
        public static ExecuteSqlFileTask New(string connectionString, string fileName)
        {
            return new ExecuteSqlFileTask(connectionString, fileName);
        }

        /// <summary>
        /// Initializes new instance of <see cref="ExecuteSqlFileTask"/>.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="fileName"></param>
        public ExecuteSqlFileTask(string connectionString, string fileName)
        {
            FailOnSqlError = true;
            ConnectionString = connectionString;
            ScriptFilePath = fileName;
        }

        public override string Description
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "Execute SQL script '{0}'", ScriptFilePath);
            }
        }

        protected override void DoExecute(ITaskContext context)
        {
            string script = File.ReadAllText(ScriptFilePath);
            SqlConnectionStringBuilder connStringBuilder = new SqlConnectionStringBuilder(ConnectionString);
            using (SqlConnection conn = new SqlConnection(connStringBuilder.ConnectionString))
            {
                Server server = new Server(new ServerConnection(conn));
                try
                {
                    server.ConnectionContext.ExecuteNonQuery(script);
                }
                catch (ExecutionFailureException e)
                {
                    if (FailOnSqlError)
                        throw;
                    context.WriteError("SQL execution failed. Execution continues. {0}", e);
                }
            }
        }
    }
}