using System;
using System.Data.SqlClient;

using Framework.Core;

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
        /// DataBaseManager default constructor
        /// </summary>
        public SQLDatabaseContext() : this(string.Empty, null)
        {
        }

        /// <summary>
        /// DataBaseManager constructor used to set the Command Timeout
        /// </summary>
        /// <param name="CommandTimeout">The time in seconds to wait for the command to execute. The default is 30 seconds (if null).</param>
        public SQLDatabaseContext(int? CommandTimeout) : this(string.Empty, CommandTimeout)
        {
        }

        /// <summary>
        /// DataBaseManager constructor used to set the ConnectionString
        /// </summary>
        /// <param name="ConnectionString">The string used to open the database.</param>
        public SQLDatabaseContext(string ConnectionString) : this(ConnectionString, null)
        {
        }

        /// <summary>
        /// DataBaseManager constructor used to set the if the Transaction will be used by the application and the ConnectionString and the CommandTimeout
        /// </summary>
        /// <param name="ConnectionString">The string used to open the database.</param>
        /// <param name="CommandTimeout">The time in seconds to wait for the command to execute. The default is 30 seconds (if null).</param>
        public SQLDatabaseContext(string ConnectionString, int? CommandTimeout) : base(ConnectionString, CommandTimeout)
        {
        }
     
        #endregion

        #region| Methods |

        /// <summary>
        /// DataBaseManager constructor used to set the if the Transaction will be used by the application and the ConnectionString and the CommandTimeout
        /// </summary>
        /// <param name="ConnectionString">The string used to open a SQL Server database.</param>
        /// <param name="CommandTimeout">The time in seconds to wait for the command to execute. The default is 30 seconds (if null).</param>
        public override void BuildConnectionString(string ConnectionString, int? CommandTimeout)
        {
            if (ConnectionString.IsNull())
            {
                var connectionKey = this.GetAppSettings("DB_CONN_KEY");
                var connection    = string.Empty;

                connectionKey.ThrowIfNull("The database connection string key cannot be null or empty");

                connection = this.GetAppSettings(connectionKey);

                connection.ThrowIfNull("The database connection string cannot be null or empty");

                // Decrypts the connection string
                this.ConnectionString = Cryptography.Cryptography.DecryptUsingTripleDES(connection);

            }
            else
            {
                // Decrypts the connection string
                this.ConnectionString = Cryptography.Cryptography.DecryptUsingTripleDES(ConnectionString);
            }

            var oSqlConnectionBuilder = new SqlConnectionStringBuilder(this.ConnectionString);

            this.UserID         = oSqlConnectionBuilder.UserID;
            this.Password       = oSqlConnectionBuilder.Password;
            this.InitialCatalog = oSqlConnectionBuilder.InitialCatalog;
            this.DataSource     = oSqlConnectionBuilder.DataSource;
            this.ConnectTimeout = oSqlConnectionBuilder.ConnectTimeout;

            var COMMAND_TIMEOUT_CONFIG = this.GetAppSettings("FRAMEWORK.COMMAND.TIMEOUT");
            var CONNECT_TIMEOUT_CONFIG = this.GetAppSettings("FRAMEWORK.CONNECTION.TIMEOUT");

            if (COMMAND_TIMEOUT_CONFIG.IsNotNull())
            {
                this.CommandTimeout = COMMAND_TIMEOUT_CONFIG.ToInt();
            }
            else
            {
                if (CommandTimeout.HasValue)
                {
                    this.CommandTimeout = CommandTimeout;
                }
                else
                {
                    this.CommandTimeout = 30;
                }
            }

            if (CONNECT_TIMEOUT_CONFIG.IsNotNull() && CONNECT_TIMEOUT_CONFIG.IsInt())
            {
                this.ConnectTimeout = CONNECT_TIMEOUT_CONFIG.ToInt();
                oSqlConnectionBuilder.ConnectTimeout = this.ConnectTimeout.Value;

                this.ConnectionString = oSqlConnectionBuilder.ToString();
            }

            oSqlConnectionBuilder = null;

            if (string.IsNullOrEmpty(this.ConnectionString))
            {
                throw new Exception("Framework: The connection string has not been initialized.");
            }
        } 

        #endregion
    }
}
