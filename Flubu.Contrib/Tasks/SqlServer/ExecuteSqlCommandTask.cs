namespace Flubu.Tasks.SqlServer
{
    using System.Data.SqlClient;
    using System.Globalization;
    using Flubu;
    using Microsoft.SqlServer.Management.Common;
    using Microsoft.SqlServer.Management.Smo;

    /// <summary>
    /// Execute SQL command.
    /// </summary>
    public class ExecuteSqlCommandTask : TaskBase
    {
        private string ConnectionString { get; set; }

        /// <summary>
        /// Initializes new instance of <see cref="ExecuteSqlCommandTask"/>
        /// </summary>
        /// <param name="connectionString">Connection string to use.</param>
        /// <param name="commandText">Command to execute.</param>
        public ExecuteSqlCommandTask(string connectionString, string commandText)
        {
            FailOnSqlError = true;
            ConnectionString = connectionString;
            CommandText = commandText;
        }

        /// <summary>
        /// Gets or sets whether build should fail on sql error or not.
        /// </summary>
        public bool FailOnSqlError { get; set; }

        /// <summary>
        /// Sezs whether build should fail on error or not.
        /// </summary>
        /// <param name="fail"></param>
        /// <returns>This instance of <see cref="ExecuteSqlCommandTask"/></returns>
        public ExecuteSqlCommandTask SetFailOnSqlError(bool fail)
        {
            FailOnSqlError = fail;
            return this;
        }

        /// <summary>
        /// Execute SQL command on sql server.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="commandText"></param>
        public static ExecuteSqlCommandTask New(string connectionString, string commandText)
        {
            return new ExecuteSqlCommandTask(connectionString, commandText);
        }

        private string CommandText { get; set; }

        public override string Description
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "Execute SQL command '{0}'", CommandText);
            }
        }

        protected override void DoExecute(ITaskContext context)
        {
            SqlConnectionStringBuilder connStringBuilder = new SqlConnectionStringBuilder(ConnectionString);
            using (SqlConnection conn = new SqlConnection(connStringBuilder.ConnectionString))
            {
                Server server = new Server(new ServerConnection(conn));
                try
                {
                    server.ConnectionContext.ExecuteNonQuery(CommandText);
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