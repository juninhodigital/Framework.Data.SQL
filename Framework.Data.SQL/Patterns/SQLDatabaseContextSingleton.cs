using System;

namespace Framework.Data.SQL
{
    /// <summary>
    /// Implements the singleton design pattern for the SQL Database context
    /// </summary>
    public class SQLDatabaseContextSingleton
    {
        #region| Properties |

        private static string connectionString;

        private static readonly Lazy<SQLDatabaseContext> instance = new Lazy<SQLDatabaseContext>(() => new SQLDatabaseContext(connectionString), true);

        /// <summary>
        /// Create thread-safe Singeton instance 
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static SQLDatabaseContext Instance(string connection)
        {
            connectionString = connection;
            return instance.Value;
            
        }

        #endregion

        #region| Constructor |

        /// <summary>
        /// Static constructor
        /// </summary>
        static SQLDatabaseContextSingleton()
        {
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        private SQLDatabaseContextSingleton()
        {
            
        } 

        #endregion
    }
}
