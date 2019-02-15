using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

using Framework.Core;

namespace Framework.Data.SQL
{
    /// <summary>
    /// Handles Datababases Connections and operations
    /// </summary>
    [Serializable]
    public sealed class SQLDatabaseManager: DataBaseManager 
    {
        #region| Constructor |

        public SQLDatabaseManager()
        {

        }
    
        /// <summary>
        /// Provides T-SQL statement execution in only on SQL Server Database
        /// </summary>
        /// <param name="ExecutionType">ExecutionType</param>
        internal SQLDatabaseManager(ExecutionType ExecutionType) : base(ExecutionType) { }

        /// <summary>
        /// Provides T-SQL statement execution in only on SQL Server Database
        /// </summary>
        /// <param name="oDataBase">DataBase</param>
        /// <param name="ExecutionType">ExecutionType</param>
        internal SQLDatabaseManager(IDatabaseContext oDataBase, ExecutionType ExecutionType) : base(oDataBase, ExecutionType) { }

        /// <summary>
        /// Provides T-SQL statement execution in on a SQL Server Database Collection
        /// </summary>
        /// <param name="oDataBases">List of DataBases</param>
        /// <param name="ExecutionType">ExecutionType</param>
        internal SQLDatabaseManager(List<IDatabaseContext> oDataBases, ExecutionType ExecutionType) : base(oDataBases, ExecutionType) { }
        

        #endregion      

        #region| Destructor |

        /// <summary>
        /// Default destructor
        /// </summary>
        ~SQLDatabaseManager()
        {
            this.Dispose();
        }
        
        #endregion

    }
}
