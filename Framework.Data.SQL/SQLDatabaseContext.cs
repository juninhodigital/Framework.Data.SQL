using System;
using System.Data.SqlClient;

namespace Framework.Data.SQL
{
    /// <summary>
    /// It handles Database Connection and sets the time in seconds to wait for the command to execute
    /// </summary>
    [Serializable]
    public class SQLDatabaseContext : DatabaseContext
    {
        #region| Constructor |

        /// <summary>
        /// DataBaseManager constructor used to set the if the Transaction will be used by the application and the ConnectionString and the CommandTimeout
        /// </summary>
        /// <param name="connectionString">The string used to open the database.</param>
        /// <param name="CommandTimeout">The time in seconds to wait for the command to execute. The default is 30 seconds (if null).</param>
        /// <param name="connectionTimeout"> The value of the System.Data.SqlClient.SqlConnectionStringBuilder.ConnectTimeout property, or 15 seconds if no value has been supplied.</param>
        public SQLDatabaseContext(string connectionString, int?commandTimeout = null, int? connectionTimeout = null) : base(connectionString, commandTimeout, connectionTimeout)
        {
        }

        #endregion

        #region| Methods |

        /// <summary>
        /// DataBaseManager constructor used to set the if the Transaction will be used by the application and the ConnectionString and the CommandTimeout
        /// </summary>
        /// <param name="connectionString">The string used to open a SQL Server database.</param>
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. The default is 30 seconds (if null).</param>
        /// <param name="connectionTimeout"> The value of the System.Data.SqlClient.SqlConnectionStringBuilder.ConnectTimeout property, or 15 seconds if no value has been supplied.</param>
        public override void BuildConnectionString(string connectionString, int? commandTimeout, int? connectionTimeout)
        {
            this.ConnectionString = connectionString ?? throw new ArgumentNullException("The connection string is null or empty");

            this.CommandTimeout = commandTimeout.HasValue ? commandTimeout : 30;
            this.ConnectTimeout = connectionTimeout.HasValue ? connectionTimeout : 15;

            var oSqlConnectionBuilder = new SqlConnectionStringBuilder(this.ConnectionString)
            {
                ConnectTimeout = this.ConnectTimeout.Value
            };

            this.ConnectionString = oSqlConnectionBuilder.ToString();

            oSqlConnectionBuilder = null;
        }

        #endregion
    }
}
