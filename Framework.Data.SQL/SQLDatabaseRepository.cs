using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Text;

using static Framework.Core.Extensions;
using Framework.Entity;

namespace Framework.Data.SQL
{
    /// <summary>
    /// Base class that provides methods to handle SQL Server statements
    /// <seealso cref="https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/try-finally"/>
    /// </summary>
    public class SQLDatabaseRepository : IDatabaseRepository, IDisposable
    {
        #region| Properties |

        /// <summary>
        /// Provides information returned by the print output from the SQL Server
        /// </summary>
        public string InfoMessage { get; set; }

        /// <summary>
        /// Binding Flags
        /// </summary>
        private const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty;

        /// <summary>
        /// Indicates the local path to store the executed T-SQL statements
        /// </summary>
        public string ProfilePath { get; private set; } = null;

        /// <summary>
        /// Indicates whether an exception will be throw in case of failure
        /// </summary>
        public readonly bool MustThrowMappingExceptions = false;

        #endregion

        #region| Fields |

        /// <summary>
        /// Initializes a new instance of the System.Data.SqlClient.SqlConnection class.
        /// </summary>
        public SqlConnection Connection;

        /// <summary>
        /// Instance of System.Data.SqlClient.SqlCommand
        /// </summary>
        public SqlCommand Command;

        /// <summary>
        /// Gets or sets the Transact-SQL statement, table name or stored procedure to execute at the data source.
        /// </summary>
        public string CommandText;

        /// <summary>
        /// Specifies how a command string is interpreted.
        /// </summary>
        public CommandType CommandType;

        /// <summary>
        /// Instance of Framework.Data.SQL.DataBaseManager
        /// </summary>
        public IDatabaseContext DatabaseContext;

        #endregion

        #region| Constructor |

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="mustThrowMappingExceptions">File path to log the T-SQL Statements</param>
        public SQLDatabaseRepository(bool mustThrowMappingExceptions = false, string profilePath = "")
        {
            this.MustThrowMappingExceptions = mustThrowMappingExceptions;
            this.ProfilePath                = profilePath;
        }

        #endregion

        #region| Destructor |

        /// <summary>
        /// Default Destructor
        /// </summary>
        ~SQLDatabaseRepository()
        {

            /*
            
            CAUTION: Do not call Close or Dispose on a Connection, a DataReader, or any other managed object in the Finalize method of your class. 
            In a finalizer, you should only release unmanaged resources that your class owns directly.
            If your class does not own any unmanaged resources, do not include a Finalize method in your class definition. 
            
            */

            // Commented the line below based on the MSDN article http://msdn2.microsoft.com/en-us/library/system.data.sqlclient.sqldatareader.close.aspx
            //this.Dispose();
        }

        #endregion

        #region| Methods |   

        #region| Parameters |

        /// <summary>
        /// Adds the specified parameter object to the parameter collection (INPUT)
        /// </summary>
        /// <param name="parameterName">Parameter Name</param>
        /// <param name="parameterValue">Parameter Value</param>
        /// <example>
        /// <code>
        ///     In("ParameterName", "ParameterValue");
        /// </code>
        /// </example>
        public void In(string parameterName, object parameterValue)
        {
            parameterName = CheckParameterName(parameterName);

            var oParam = new SqlParameter
            {
                Direction     = ParameterDirection.Input,
                ParameterName = parameterName,
                Value         = GetParameterValue(parameterValue)
             };

            AddParam(oParam);
        }

        /// <summary>
        /// Adds the specified parameter object to the parameter collection (INPUT)
        /// </summary>
        /// <param name="parameterName">Parameter Name</param>
        /// <param name="parameterValue">Parameter Value</param>
        /// <param name="sqlDbType">System.Data.SqlDbType</param>
        public void In(string parameterName, object parameterValue, SqlDbType sqlDbType)
        {
            In(parameterName, parameterValue);

            if (Command.Parameters != null && !Command.Parameters.Contains(parameterName))
            {
                Command.Parameters[parameterName].SqlDbType = sqlDbType;
            }
        }

        /// <summary>
        /// Adds the specified parameter object to the parameter collection (INPUT / OUTPUT)
        /// </summary>
        /// <param name="parameterName">Parameter Name</param>
        /// <param name="parameterValue">Parameter Value</param>
        /// <param name="sqlDbType">SqlDbType</param>
        /// <param name="size">Size</param>
        /// <example>
        /// <code>
        ///     InOut("ParameterName", "ParameterValue");
        /// </code>
        /// </example>
        public void InOut(string parameterName, object parameterValue, SqlDbType sqlDbType, int size)
        {
            parameterName = CheckParameterName(parameterName);

            var oParam = new SqlParameter
            {
                ParameterName = parameterName,
                SqlDbType     = sqlDbType,
                Value         = GetParameterValue(parameterValue),
                Direction     = ParameterDirection.InputOutput,
                Size          = size
            };
                        
            AddParam(oParam);
        }

        /// <summary>
        /// Adds the specified parameter object to the parameter collection (OUTPUT)
        /// </summary>
        /// <param name="parameterName">Parameter Name</param>
        /// <param name="sqlDbType">System.Data.DbType</param>
        /// <param name="parameterValue">ParameterValue</param>
        /// <example>
        /// <code>
        ///     Out("ParameterName", DbType.Int32);
        /// </code>
        /// </example>
        public void Out(string parameterName, SqlDbType sqlDbType, object? parameterValue = null)
        {
            if (parameterValue != null)
            {
                parameterName = CheckParameterName(parameterName);

                var oParam = new SqlParameter
                {
                    Direction = ParameterDirection.InputOutput,
                    SqlDbType = sqlDbType,
                    ParameterName = parameterName,
                    Value = GetParameterValue(parameterValue)
                };

                AddParam(oParam);
            }
        }

        /// <summary>
        /// Adds the specified IDbDataParameter object to the parameter collection
        /// </summary>
        /// <param name="dbDataParameter">IDbDataParameter</param>
        public void AddParam(IDbDataParameter dbDataParameter)
        {
            Command  = Command ?? new SqlCommand();
          
            if (Command.Parameters != null && !Command.Parameters.Contains(dbDataParameter.ParameterName))
            {
                Command.Parameters.Add(dbDataParameter);
            }
        }

        /// <summary>
        /// Check if the ParameterName is null or empty
        /// </summary>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public string CheckParameterName(string parameterName)
        {
            if (string.IsNullOrEmpty(parameterName))
            {
                throw new Exception("Framework: O nome do parâmetro não pode ser nulo ou vazio");
            }
            else
            {
                parameterName = parameterName.Replace("@", "");
                parameterName = "@" + parameterName;
            }

            return parameterName;
        }

        /// <summary>
        /// Check the parameter value
        /// </summary>
        /// <param name="parameterValue">ParameterValue</param>
        /// <returns>object</returns>
        public object GetParameterValue(object parameterValue)
        {
            if (parameterValue==null || parameterValue==DBNull.Value)
            {
                return DBNull.Value;
            }
            else
            {
                if (parameterValue is String)
                {
                    if (string.IsNullOrEmpty(parameterValue.ToString()))
                    {
                        return DBNull.Value;
                    }
                    else
                    {
                        return parameterValue;
                    }
                }

                if (parameterValue is DateTime)
                {
                    if (DateTime.Parse(parameterValue.ToString()??string.Empty) == DateTime.MinValue)
                    {
                        return DBNull.Value;
                    }
                    else
                    {
                        return parameterValue;
                    }
                }

                if (parameterValue is Int32 || parameterValue is int || parameterValue is Int64)
                {
                    if (parameterValue.ToString().ToInt() == int.MinValue)
                    {
                        return DBNull.Value;
                    }
                    else
                    {
                        return parameterValue;
                    }
                }

                if (parameterValue is float)
                {
                    if (parameterValue.ToString().ToFloat() == float.MinValue)
                    {
                        return DBNull.Value;
                    }
                    else
                    {
                        return parameterValue;
                    }
                }

                if (parameterValue is decimal)
                {
                    if (parameterValue.ToString().ToDecimal() == decimal.MinValue)
                    {
                        return DBNull.Value;
                    }
                    else
                    {
                        return parameterValue;
                    }
                }

                if (parameterValue is byte)
                {
                    if (byte.Parse(parameterValue.ToString()) == byte.MinValue)
                    {
                        return DBNull.Value;
                    }
                    else
                    {
                        return parameterValue;
                    }
                }

                if (parameterValue is char)
                {
                    if (parameterValue.ToString().Trim().Length == 0)
                    {
                        return DBNull.Value;
                    }
                    else
                    {
                        if (char.Parse(parameterValue.ToString()) == char.MinValue)
                        {
                            return DBNull.Value;
                        }
                        else
                        {
                            return parameterValue;
                        }
                    }
                }

                if (parameterValue is DataTable)
                {
                    var oDataTable = parameterValue as DataTable;

                    if (oDataTable==null)
                    {
                        return DBNull.Value;
                    }
                    else
                    {
                        return parameterValue;
                    }
                }

                if (parameterValue is Nullable<bool>)
                {
                    var oNullable = (Nullable<bool>)parameterValue;

                    if (oNullable.HasValue == false)
                    {
                        return DBNull.Value;
                    }
                    else
                    {
                        return oNullable.Value;
                    }
                }

                return parameterValue;
            }
        }

        #endregion

        /// <summary>
        /// Check whether the Profiler is enabled or not to log the T-SQL Statements in a log file 
        /// </summary>
        public void IsProfilerEnabled()
        {
            if (this.ProfilePath!=null)
            {
                if (!Directory.Exists(Path.GetDirectoryName(this.ProfilePath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(this.ProfilePath));
                }
               
                var SQL   = PreviewSQL();
                var BREAK = "-".Repeat(100);

                File.AppendAllText(this.ProfilePath, SQL + BREAK + Environment.NewLine);
            }
        }

        /// <summary>
        /// Check whether the Profiler is enabled or not to log the T-SQL Statements in a log file 
        /// </summary>
        public async Task IsProfilerEnabledAsync()
        {
            if (this.ProfilePath!=null)
            {
                if (!Directory.Exists(Path.GetDirectoryName(this.ProfilePath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(this.ProfilePath));
                }

                var SQL = PreviewSQL();
                var BREAK = "-".Repeat(100);

                await File.AppendAllTextAsync(this.ProfilePath, SQL + BREAK + Environment.NewLine);
            }
        }

        /// <summary>
        /// Returns the T-SQL Statement that will be execute on the Database
        /// </summary>
        /// <returns>T-SQL Statement</returns>
        public string PreviewSQL()
        {
            var oStringBuilder = new StringBuilder();
            var BR = Environment.NewLine;

            if (this.CommandType == CommandType.StoredProcedure)
            {
                // Check if exists table value parameters
                if (Command!=null && Command.Parameters != null && Command.Parameters.Count > 0)
                {
                    for (int i = 0; i < Command.Parameters.Count; i++)
                    {
                        var oParam = Command.Parameters[i];

                        if (oParam.SqlDbType == SqlDbType.Structured)
                        {
                            var DT = oParam.Value as DataTable;

                            if (DT!=null && DT.Rows.Count > 0)
                            {
                                oStringBuilder.Append($"DECLARE {oParam.ParameterName} {oParam.ParameterName.Replace("@P_", "").Replace("@", "")} {BR}{BR}");

                                for (int z = 0; z < DT.Rows.Count; z++)
                                {
                                    oStringBuilder.Append($"INSERT {oParam.ParameterName}");

                                    DataRow oRow = DT.Rows[z];

                                    for (int y = 0; y < oRow.ItemArray.Length; y++)
                                    {
                                        if (y == 0)
                                        {
                                            oStringBuilder.Append(BR);
                                            oStringBuilder.Append("SELECT ");
                                        }

                                        oStringBuilder.Append(oRow.ItemArray[y].ToString().AddSingleQuotes());

                                        if (y != oRow.ItemArray.Length - 1)
                                        {
                                            oStringBuilder.Append(",");
                                        }
                                    }

                                    oStringBuilder.Append(BR);
                                    oStringBuilder.Append(BR);
                                }
                            }
                        }
                    }
                }

                oStringBuilder.Append($"EXEC {this.CommandText} {BR}");

                if (Command!=null && Command.Parameters != null && Command.Parameters.Count > 0)
                {
                    for (int i = 0; i < Command.Parameters.Count; i++)
                    {
                        var oParam = Command.Parameters[i];

                        if (oParam.Direction == ParameterDirection.InputOutput || oParam.Direction == ParameterDirection.Output)
                        {
                            if (oParam.Direction == ParameterDirection.InputOutput)
                            {
                                if (oParam.Value!=null && oParam.Value!= DBNull.Value)
                                {
                                    oStringBuilder.Append(oParam.ParameterName + " = " + oParam.Value.ToString() + " OUTPUT");
                                }
                                else
                                {
                                    oStringBuilder.Append(oParam.ParameterName + " = " + oParam.ParameterName + " OUTPUT");
                                }
                            }
                            else
                            {
                                oStringBuilder.Append(oParam.ParameterName + " = NULL OUTPUT");
                            }
                        }
                        else
                        {
                            if (oParam.Value==null || oParam.Value == DBNull.Value)
                            {
                                oStringBuilder.Append(oParam.ParameterName + " = NULL");
                            }
                            else
                            {

                                switch (oParam.SqlDbType)
                                {
                                    case SqlDbType.Decimal:
                                    case SqlDbType.Float:
                                    case SqlDbType.Money:
                                    case SqlDbType.Real:
                                        {
                                            oStringBuilder.Append(oParam.ParameterName + " = " + oParam.Value.ToString().Replace(",", ".") + "");
                                            break;
                                        }
                                    case SqlDbType.Int:
                                    case SqlDbType.SmallInt:
                                    case SqlDbType.BigInt:
                                        {
                                            oStringBuilder.Append(oParam.ParameterName + " = " + oParam.Value.ToString() + "");
                                            break;
                                        }
                                    case SqlDbType.Bit:
                                        {
                                            if (oParam.Value.ToString().ToLower() == "true")
                                            {
                                                oStringBuilder.Append(oParam.ParameterName + " = 1");
                                            }
                                            else
                                            {
                                                oStringBuilder.Append(oParam.ParameterName + " = 0");
                                            }
                                            break;
                                        }
                                    case SqlDbType.Structured:
                                        {
                                            oStringBuilder.Append(oParam.ParameterName + " = " + oParam.ParameterName + "");
                                            break;
                                        }
                                    default:
                                        {
                                            oStringBuilder.Append(oParam.ParameterName + " = '" + oParam.Value.ToString().Replace(",", ".") + "'");
                                            break;
                                        }
                                }
                            }
                        }

                        if (i != Command.Parameters.Count - 1)
                        {
                            oStringBuilder.Append(",");
                        }

                        oStringBuilder.Append(BR);
                    }
                }
            }
            else
            {
                oStringBuilder.Append(this.CommandText);
            }

            return oStringBuilder.ToString();
        }

        /// <summary>
        /// Sets the database context to execute operations against the DataBase
        /// </summary>
        /// <param name="databaseContext">DataBaseManager</param>
        public void SetContext(IDatabaseContext databaseContext)
        {
            if (databaseContext==null)
            {
                throw new ArgumentException("Framework - The database context is null or empty");
            }
            else
            {
                this.DatabaseContext = databaseContext;
            }
        }

        /// <summary>
        /// Configures the System.Data.CommandType and the T-SQL statement that will be executed on the Database
        /// </summary>
        /// <param name="commandType">System.Data.CommandType</param>
        /// <param name="statement">T-SQL Statement</param>
        public void Run(string statement, CommandType commandType = CommandType.StoredProcedure)
        {
            this.CommandText = statement;
            this.CommandType = commandType;
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the
        /// result set returned by the query. Additional columns or rows are ignored.
        /// </summary>
        /// <param name="stopExecutionImmediately">If true, the connection will be released immediately after the t-sql statement execution. Otherwise, it will wait for the next one</param>
        /// <returns>
        /// The first column of the first row in the result set, or a null reference if the result set is empty. 
        /// Returns a maximum of 2033 characters.
        /// </returns>
        public T GetScalar<T>(bool stopExecutionImmediately = true)
        {
            T output = default(T);

            try
            {
                this.Prepare();

                output = (T)Command.ExecuteScalar();

            }
            finally
            {
                this.Release(stopExecutionImmediately);
            }

            return output;
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the
        /// result set returned by the query. Additional columns or rows are ignored.
        /// </summary>
        /// <param name="stopExecutionImmediately">If true, the connection will be released immediately after the t-sql statement execution. Otherwise, it will wait for the next one</param>
        /// <returns>
        /// The first column of the first row in the result set, or a null reference if the result set is empty. 
        /// Returns a maximum of 2033 characters.
        /// </returns>
        public async Task<T> GetScalarAsync<T>(bool stopExecutionImmediately = true)
        {
            T output = default(T);

            try
            {
                await this.PrepareAsync();

                output = (T)await Command.ExecuteScalarAsync();

            }
            finally
            {
                this.Release(stopExecutionImmediately);
            }

            return output;
        }

        /// <summary>
        /// Gets the output of the parameter value
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="ParameterName">ParameterName</param>
        /// <returns>An System.Object that is the value of the parameter. The default value is null.</returns>
        /// <example>
        /// <code>
        ///     public void Inserir(UsuarioBES oUsuario)
        ///     {
        ///         this.Run("STORED_PROCEDURE_NAME");
        /// 
        ///	        InOut("ID", DbType.Int32);
        ///         In("Nome",UsuarioI.Nome);
        ///         In("Mail",UsuarioI.Mail);
        /// 
        ///         this.Execute();
        ///         
        ///         oUsuario.ID = this.GetValue<![CDATA[<int]]>("ID");
        ///     }
        /// </code>
        /// </example>
        public T GetValue<T>(string ParameterName)
        {
            ParameterName = CheckParameterName(ParameterName);

            return (T)Command.Parameters[ParameterName].Value;
        }

        /// <summary>
        ///  Adds or refreshes rows in a specified range in the System.Data.DataSet to match those in the data source using the System.Data.DataTable name.
        /// </summary>
        /// <param name="stopExecutionImmediately">If true, the connection will be released immediately after the t-sql statement execution. Otherwise, it will wait for the next one</param>
        /// <returns>System.Data.DataTable</returns>
        /// <example>
        /// <code>
        /// public DataTable FillDataTable()
        /// {
        ///     this.Run("STORED_PROCEDURE_NAME");
        /// 
        ///     In("ACTION", "SELECT");
        /// 
        ///     return GetDataTable();
        /// }
        /// </code>
        /// </example>
        public DataTable GetDataTable(bool stopExecutionImmediately = true)
        {
            var output = new DataTable();

            try
            {
                this.Prepare();

                this.Command.Prepare();

                using (var oSqlDataReader = this.Command.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    output.Load(oSqlDataReader);

                    Command.Dispose();
                }
            }
            finally
            {
                this.Release(stopExecutionImmediately);
            }

            return output;
        }

        /// <summary>
        ///  Adds or refreshes rows in a specified range in the System.Data.DataSet to match those in the data source using the System.Data.DataTable name.
        /// </summary>
        /// <param name="stopExecutionImmediately">If true, the connection will be released immediately after the t-sql statement execution. Otherwise, it will wait for the next one</param>
        /// <returns>System.Data.DataTable</returns>
        /// <example>
        /// <code>
        /// public DataTable FillDataTable()
        /// {
        ///     this.Run("STORED_PROCEDURE_NAME");
        /// 
        ///     In("ACTION", "SELECT");
        /// 
        ///     return GetDataTable();
        /// }
        /// </code>
        /// </example>
        public async Task<DataTable> GetDataTableAsync(bool stopExecutionImmediately = true)
        {
            var output = new DataTable();

            try
            {
                await this.PrepareAsync();

                this.Command.Prepare();

                using (var oSqlDataReader = await this.Command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                {
                    output.Load(oSqlDataReader);
                    
                    Command.Dispose();
                }
            }
            finally
            {
                this.Release(stopExecutionImmediately);
            }

            return output;
        }

        /// <summary>
        /// Adds or refreshes rows in the System.Data.DataSet.
        /// </summary>
        /// <param name="stopExecutionImmediately">If true, the connection will be released immediately after the t-sql statement execution. Otherwise, it will wait for the next one</param>
        /// <returns>System.Data.DataSet</returns>
        /// <example>
        /// <code>
        ///  public DataSet FillDataSet()
        ///  {
        ///     this.Run("STORED_PROCEDURE_NAME");
        ///  
        ///     In("ACTION", "SELECT");
        ///  
        ///     return GetDataSet();
        ///  }
        /// </code>
        /// </example>
        public DataSet GetDataSet(bool stopExecutionImmediately = true)
        {
            var output = new DataSet();

            try
            {
                this.Prepare();

                var oSqlDataAdapter = new SqlDataAdapter(this.Command);

                oSqlDataAdapter.Fill(output);

                oSqlDataAdapter.Dispose();
                Command.Dispose();

            }
            finally
            {
                this.Release(stopExecutionImmediately);
            }

            return output;
        }

        /// <summary>
        /// Adds or refreshes rows in the System.Data.DataSet.
        /// </summary>
        /// <param name="stopExecutionImmediately">If true, the connection will be released immediately after the t-sql statement execution. Otherwise, it will wait for the next one</param>
        /// <returns>System.Data.DataSet</returns>
        /// <example>
        /// <code>
        ///  public DataSet FillDataSet()
        ///  {
        ///     this.Run("STORED_PROCEDURE_NAME");
        ///  
        ///     In("ACTION", "SELECT");
        ///  
        ///     return GetDataSet();
        ///  }
        /// </code>
        /// </example>
        public async Task<DataSet> GetDataSetAsync(bool stopExecutionImmediately = true)
        {
            var output = new DataSet();

            try
            {
                await this.PrepareAsync();

                var oSqlDataAdapter = new SqlDataAdapter(this.Command);

                oSqlDataAdapter.Fill(output);

                oSqlDataAdapter.Dispose();
                Command.Dispose();

            }
            finally
            {
                this.Release(stopExecutionImmediately);
            }

            return output;
        }

        /// <summary>
        ///  Get a SqlDataReader based on the System.Data.CommandType and the given parameters
        /// </summary>
        /// <returns>System.Data.SqlClient.SqlDataReader</returns>
        /// <example>
        /// <code>
        ///  public SqlDataReader FillDataReader()
        ///  {
        ///     this.Run("STORED_PROCEDURE_NAME");
        ///  
        ///     In("ACTION", "SELECT");
        ///  
        ///     return GetReader();
        ///  }
        /// </code>
        /// </example>
        public SqlDataReader GetReader()
        {
            SqlDataReader output = null;

            this.Prepare();

            this.Command.Prepare();
            
            output = this.Command.ExecuteReader(CommandBehavior.CloseConnection);

            return output;
        }

        /// <summary>
        ///  Get a SqlDataReader based on the System.Data.CommandType and the given parameters
        /// </summary>
        /// <returns>System.Data.SqlClient.SqlDataReader</returns>
        /// <example>
        /// <code>
        ///  public SqlDataReader FillDataReader()
        ///  {
        ///     this.Run("STORED_PROCEDURE_NAME");
        ///  
        ///     In("ACTION", "SELECT");
        ///  
        ///     return GetReader();
        ///  }
        /// </code>
        /// </example>
        public async Task<SqlDataReader> GetReaderAsync()
        {
            SqlDataReader output = null;

            await this.PrepareAsync();

            this.Command.Prepare();
            
            output = await this.Command.ExecuteReaderAsync(CommandBehavior.CloseConnection);

            return output;

        }

        /// <summary>
        /// Executes a Transact-SQL statement against the connection and returns the number of rows affected 
        /// </summary>
        /// <param name="stopExecutionImmediately">If true, the connection will be released immediately after the t-sql statement execution. Otherwise, it will wait for the next one</param>
        /// <example>
        /// <code>
        ///     public void Inserir(UsuarioBES UsuarioI)
        ///     {
        ///         this.Run("STORED_PROCEDURE_NAME");
        /// 
        ///	        InOut("ID", DbType.Int32);
        ///         In("Nome",UsuarioI.Nome);
        ///         In("Mail",UsuarioI.Mail);
        /// 
        ///         this.Execute();
        ///     }
        /// </code>
        /// </example>
        /// <returns>The number of rows affected</returns>
        public int Execute(bool stopExecutionImmediately = true)
        {
            var output = 0;

            try
            {
                this.Prepare();

                output = this.Command.ExecuteNonQuery();
            }
            finally
            {
                this.Release(stopExecutionImmediately);
            }

            return output;
        }

        /// <summary>
        /// Executes a Transact-SQL statement against the connection and returns the number of rows affected 
        /// </summary>
        /// <param name="stopExecutionImmediately">If true, the connection will be released immediately after the t-sql statement execution. Otherwise, it will wait for the next one</param>
        /// <example>
        /// <code>
        ///     public void Inserir(UsuarioBES UsuarioI)
        ///     {
        ///         this.Run("STORED_PROCEDURE_NAME");
        /// 
        ///	        InOut("ID", DbType.Int32);
        ///         In("Nome",UsuarioI.Nome);
        ///         In("Mail",UsuarioI.Mail);
        /// 
        ///         this.Execute();
        ///     }
        /// </code>
        /// </example>
        /// <returns>The number of rows affected</returns>
        public async Task<int> ExecuteAsync(bool stopExecutionImmediately = true)
        {
            var output = 0;

            try
            {
                await this.PrepareAsync();

                output = await this.Command.ExecuteNonQueryAsync();
            }
            finally
            {
                this.Release(stopExecutionImmediately);
            }

            return output;
        }

        /// <summary>
        /// Executes a Transact-SQL statement against the connection and returns the number of rows affected 
        /// </summary>
        /// <param name="outputParameterName">Parameter name to returned</param>
        /// <param name="stopExecutionImmediately">If true, the connection will be released immediately after the t-sql statement execution. Otherwise, it will wait for the next one</param>
        /// <example>
        /// <code>
        ///     public void Inserir(UsuarioBES UsuarioI)
        ///     {
        ///         this.Run("STORED_PROCEDURE_NAME");
        /// 
        ///	        InOut("ID", DbType.Int32);
        ///         In("Nome",UsuarioI.Nome);
        ///         In("Mail",UsuarioI.Mail);
        /// 
        ///         this.Execute();
        ///     }
        /// </code>
        /// </example>
        /// <returns>The number of rows affected</returns>
        public int Execute(string outputParameterName, bool stopExecutionImmediately = true)
        {
            var output = 0;

            try
            {
                this.Prepare();

                this.Command.ExecuteNonQuery();

                output = this.GetValue<int>(outputParameterName);
            }
            finally
            {
                this.Release(stopExecutionImmediately);
            }

            return output;
        }

        /// <summary>
        /// Executes a Transact-SQL statement against the connection and returns the number of rows affected 
        /// </summary>
        /// <param name="outputParameterName">Parameter name to returned</param>
        /// <param name="stopExecutionImmediately">If true, the connection will be released immediately after the t-sql statement execution. Otherwise, it will wait for the next one</param>
        /// <example>
        /// <code>
        ///     public void Inserir(UsuarioBES UsuarioI)
        ///     {
        ///         this.Run("STORED_PROCEDURE_NAME");
        /// 
        ///	        InOut("ID", DbType.Int32);
        ///         In("Nome",UsuarioI.Nome);
        ///         In("Mail",UsuarioI.Mail);
        /// 
        ///         this.Execute();
        ///     }
        /// </code>
        /// </example>
        /// <returns>The number of rows affected</returns>
        public async Task<int> ExecuteAsync(string outputParameterName, bool stopExecutionImmediately = true)
        {
            var output = 0;

            try
            {
                await this.PrepareAsync();

                await this.Command.ExecuteNonQueryAsync();

                output = this.GetValue<int>(outputParameterName);
            }
            finally
            {
                this.Release(stopExecutionImmediately);
            }

            return output;
        }

        /// <summary>
        /// Opens a database connection with the property settings specified by the System.Data.SqlClient.SqlConnection.ConnectionString.
        /// </summary>
        public void Prepare()
        {
            this.InfoMessage = string.Empty;

            this.Connection = new SqlConnection(DatabaseContext.ConnectionString);
            this.Connection.InfoMessage += new SqlInfoMessageEventHandler(GetInfoMessage);

            if (this.Command==null)
            {
                this.Command = Connection.CreateCommand();
            }

            this.Command.CommandType = CommandType;
            this.Command.CommandText = CommandText;
            this.Command.Connection = Connection;

            if (DatabaseContext.CommandTimeout.HasValue && DatabaseContext.CommandTimeout.Value > 0)
            {
                this.Command.CommandTimeout = DatabaseContext.CommandTimeout.Value;
            }

            if (this.Command.CommandType == CommandType.Text)
            {
                this.Command.Parameters.Clear();
            }

            if (this.Connection != null && this.Connection.State == ConnectionState.Closed)
            {
                this.Connection.Open();
            }

            IsProfilerEnabled();
        }

        /// <summary>
        /// Opens a database connection with the property settings specified by the System.Data.SqlClient.SqlConnection.ConnectionString.
        /// </summary>
        public async Task PrepareAsync()
        {
            this.InfoMessage = string.Empty;

            this.Connection = new SqlConnection(DatabaseContext.ConnectionString);
            this.Connection.InfoMessage += new SqlInfoMessageEventHandler(GetInfoMessage);

            if (this.Command==null)
            {
                this.Command = Connection.CreateCommand();
            }

            this.Command.CommandType = CommandType;
            this.Command.CommandText = CommandText;
            this.Command.Connection = Connection;

            if (DatabaseContext.CommandTimeout.HasValue && DatabaseContext.CommandTimeout.Value > 0)
            {
                this.Command.CommandTimeout = DatabaseContext.CommandTimeout.Value;
            }

            if (this.Command.CommandType == CommandType.Text)
            {
                this.Command.Parameters.Clear();
            }

            if (this.Connection != null && this.Connection.State == ConnectionState.Closed)
            {
                await this.Connection.OpenAsync();
            }

            await IsProfilerEnabledAsync();
        }

        /// <summary>
        /// Clear all parameter from the existing SQL Command
        /// </summary>
        public void ResetParameters()
        {
            if (this.Command!=null)
            {
                this.Command.Parameters.Clear();
            }
        }

        /// <summary>
        ///  Removes all the System.Data.SqlClient.SqlParameter objects from the System.Data.SqlClient.SqlParameterCollection
        /// </summary>
        public void ClearParameters()
        {
            if(this.Command!=null)
            {
                this.Command.Parameters.Clear();
            }
        }

        /// <summary>
        /// Closes the connection to the database. This is the preferred method of closing any open connection.
        /// Closes the command used to execute statements on the database
        /// </summary>
        /// <param name="stopExecutionImmediately">If true, the connection will be released immediately after the t-sql statement execution. Otherwise, it will wait for the next one</param>
        public void Release(bool stopExecutionImmediately = true)
        {
            if (stopExecutionImmediately)
            {
                if (this.Connection != null)
                {
                    if (this.Connection.State == ConnectionState.Open)
                    {
                        this.Connection.Close();
                    }

                    //this.SqlConnection.ClearPool(Command.Connection);
                    this.Connection.Dispose();
                    this.Connection = null;
                }

                if (this.Command != null)
                {
                    if (this.Command.Connection != null)
                    {
                        if (this.Command.Connection.State == ConnectionState.Open)
                        {
                            this.Command.Connection.Close();
                        }

                        //this.SqlConnection.ClearPool(Command.Connection);
                        this.Command.Connection.Dispose();
                        this.Command.Connection = null;
                    }

                    this.Command.Dispose();
                    this.Command = null;
                }
            }
        }

        /// <summary>
        /// Get the info message from the underlying database raised by the PRINT statement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void GetInfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            if (e.Errors!=null)
            {
                foreach (SqlError oError in e.Errors)
                {
                    InfoMessage += string.Format("INFO: {0}", oError.Message);
                }
            }

            InfoMessage += e.Message;
        }

        #endregion

        #region| Mapper |

        /// <summar
        /// Returns an instance of the Business Entity Structured class whose properties will be filled with the information from the Database
        /// </summary>
        /// <param name="dataReader">SqlDataReader</param>
        /// <param name="isUsingNextResult">Indicates if is using multiple resultsets</param>
        /// <returns>An instance of the Business Entity Structured class</returns>
        /// <example>
        ///     <code>
        ///     public UsuarioInfo Pesquisar(int ID)
        ///     {
        ///         this.Run("SPS_PROCEDURENAME");
        ///
        ///         In("PARAMETERNAME_ID", ID);
        ///
        ///         return Mapper.Map<![CDATA[<UsuarioInfo>]]>();
        ///     }
        ///     </code>
        /// </example>
        public T Map<T>(SqlDataReader? dataReader = null, bool isUsingNextResult = false) where T : BusinessEntityStructure, new()
        {
            if (dataReader==null)
            {
                dataReader = GetReader();
            }

            if (dataReader.HasRows())
            {
                var items = GetList<T>(dataReader, isUsingNextResult).ToList();

                if (items != null && items.Count > 0)
                {
                    var output = items.FirstOrDefault();

                    if(output != null)
                    {
                        return output;
                    }

                    return new();
                }
                else
                {
                    return new(); //Activator.CreateInstance<T>();
                }
            }
            else
            {
                return new(); //Activator.CreateInstance<T>();
            }

        }

        /// <summar
        /// Returns an instance of the Business Entity Structured class whose properties will be filled with the information from the Database
        /// </summary>
        /// <param name="dataReader">SqlDataReader</param>
        /// <param name="isUsingNextResult">Indicates if is using multiple resultsets</param>
        /// <returns>An instance of the Business Entity Structured class</returns>
        /// <example>
        ///     <code>
        ///     public UsuarioInfo Pesquisar(int ID)
        ///     {
        ///         this.Run("SPS_PROCEDURENAME");
        ///
        ///         In("PARAMETERNAME_ID", ID);
        ///
        ///         return Mapper.Map<![CDATA[<UsuarioInfo>]]>();
        ///     }
        ///     </code>
        /// </example>
        public async Task<T> MapAsync<T>(SqlDataReader? dataReader = null, bool isUsingNextResult = false) where T : BusinessEntityStructure, new()
        {
            if (dataReader==null)
            {
                dataReader = await GetReaderAsync();
            }

            if (dataReader.HasRows())
            {
                var items = GetList<T>(dataReader, isUsingNextResult).ToList();

                if (items != null && items.Any())
                {
                    var output = items.FirstOrDefault();

                    if (output != null)
                    {
                        return output;
                    }

                    return new();
                }
                else
                {
                    return new(); //Activator.CreateInstance<T>();
                }
            }
            else
            {
                return new(); //Activator.CreateInstance<T>();
            }

        }

        /// <summary>
        ///  Get a SqlDataReader based on the System.Data.CommandType and the given parameters  (using Reflection.Emit)
        /// </summary>
        /// <returns>System.Data.SqlClient.SqlDataReader</returns>
        /// <example>
        /// <code>
        ///  public SqlDataReader FillDataReader()
        ///  {
        ///     this.Run("STORED_PROCEDURE_NAME");
        ///  
        ///     In("ACTION", "SELECT");
        ///  
        ///     return GetReader();
        ///  }
        /// </code>
        /// </example>
        public IEnumerable<T> Query<T>() where T: new()
        {
            this.Prepare();

            return Mapper.Query<T>(this.Connection, this.Command);
        }

        /// <summary>
        /// Returns a generic collection list with instances of the Business Entity Structured class 
        /// whose properties will be filled with the information from the Database  (using Reflection.Emit)
        /// </summary>
        /// <param name="dataReader">SqlDataReader</param>
        /// <param name="IsUsingNextResult">Indicates if is using multiple resultsets</param>
        /// <returns>Generic Collection List</returns>
        /// <example>
        ///     <code>
        ///     public List<![CDATA[<UsuarioInfo>]]> Pesquisar()
        ///     {
        ///         this.Run("SPS_PROCEDURENAME");
        ///
        ///         In("PARAMETERNAME_ID", ID);
        ///
        ///         return Mapper.GetList<![CDATA[<UsuarioInfo>]]>();
        ///     }
        ///     </code>
        /// </example>
        public List<T> GetListOptimized<T>(SqlDataReader? dataReader = null, bool isUsingNextResult = false) where T : BusinessEntityStructure, new()
        {
            List<T>? output = null;

            T Sender = ActivatorFactory.CreateInstance<T>();

            if (Sender != null && Sender.MappedProperties!=null)
            {
                if (dataReader == null)
                {
                    dataReader = GetReader();
                }

                try
                {
                    if (dataReader.HasRows())
                    {
                        output = new List<T>();

                        var schema = dataReader.GetSchema();

                        var function = ExpressionLambdaFactory.GetReaderOptimized<T>(Sender, schema);

                        while (dataReader.Read())
                        {
                            var item = function(dataReader);

                            output.Add(item);
                        }

                        schema = null;
                    }
                }
                finally
                {
                    if (!isUsingNextResult)
                    {
                        if (dataReader != null)
                        {
                            dataReader.Close();
                            dataReader.Dispose();
                        }

                        this.Release();
                    }

                    Sender.Dispose();
                }
            }

            return output??new();

        }

        /// <summary>
        /// Returns a generic collection list with instances of the Business Entity Structured class 
        /// whose properties will be filled with the information from the Database  (using Reflection.Emit)
        /// </summary>
        /// <param name="dataReader">SqlDataReader</param>
        /// <param name="IsUsingNextResult">Indicates if is using multiple resultsets</param>
        /// <returns>Generic Collection List</returns>
        /// <example>
        ///     <code>
        ///     public List<![CDATA[<UsuarioInfo>]]> Pesquisar()
        ///     {
        ///         this.Run("SPS_PROCEDURENAME");
        ///
        ///         In("PARAMETERNAME_ID", ID);
        ///
        ///         return Mapper.GetList<![CDATA[<UsuarioInfo>]]>();
        ///     }
        ///     </code>
        /// </example>
        public async Task<List<T>> GetListOptimizedAsync<T>(SqlDataReader? dataReader = null, bool isUsingNextResult = false) where T : BusinessEntityStructure, new()
        {
            List<T>? output = null;

            T Sender = ActivatorFactory.CreateInstance<T>();

            if (Sender != null && Sender.MappedProperties!=null)
            {
                if (dataReader == null)
                {
                    dataReader = await GetReaderAsync();
                }

                try
                {
                    if (dataReader.HasRows())
                    {
                        output = new List<T>();

                        var schema = dataReader.GetSchema();

                        var function = ExpressionLambdaFactory.GetReaderOptimized<T>(Sender, schema);

                        while (await dataReader.ReadAsync())
                        {
                            var item = function(dataReader);

                            output.Add(item);
                        }

                        schema = null;
                    }
                }
                finally
                {
                    if (!isUsingNextResult)
                    {
                        if (dataReader != null)
                        {
                            dataReader.Close();
                            dataReader.Dispose();
                        }

                        this.Release();
                    }

                    Sender.Dispose();
                }
            }

            return output??new();

        }

        /// Returns a generic collection list with instances of the Business Entity Structured class 
        /// whose properties will be filled with the information from the Database
        /// </summary>
        /// <param name="dataReader">SqlDataReader</param>
        /// <param name="IsUsingNextResult">Indicates if is using multiple resultsets</param>
        /// <returns>Generic Collection List</returns>
        /// <example>
        ///     <code>
        ///     public List<![CDATA[<UsuarioInfo>]]> Pesquisar()
        ///     {
        ///         this.Run("SPS_PROCEDURENAME");
        ///
        ///         In("PARAMETERNAME_ID", ID);
        ///
        ///         return Mapper.GetList<![CDATA[<UsuarioInfo>]]>();
        ///     }
        ///     </code>
        /// </example>
        public List<T> GetList<T>(SqlDataReader? dataReader = null, bool isUsingNextResult = false) where T : BusinessEntityStructure, new()
        {
            List<T>? output = null;

            T Sender = Activator.CreateInstance<T>();

            if (Sender != null && Sender.MappedProperties!=null)
            {
                Sender.Dispose();

                if (dataReader == null)
                {
                    dataReader = GetReader();
                }

                try
                {
                    if (dataReader.HasRows())
                    {
                        output = new List<T>();

                        var schema = dataReader.GetSchema();
                        var oType = typeof(T);
                        var TypeName = oType.Name;
                       
                        while (dataReader.Read())
                        {
                            Sender = Activator.CreateInstance<T>();
                            BindList(dataReader, Sender, oType, TypeName, schema, this.MustThrowMappingExceptions);

                            Sender.MappedProperties = null;

                            output.Add(Sender);
                        }

                        schema = null;
                        oType = null;
                    }
                }
                finally
                {
                    if (!isUsingNextResult)
                    {
                        if (dataReader != null)
                        {
                            dataReader.Close();
                            dataReader.Dispose();

                            dataReader = null;
                        }

                        this.Release();
                    }

                    Sender.Dispose();
                }
            }

            return output??new();
        }

        /// Returns a generic collection list with instances of the Business Entity Structured class 
        /// whose properties will be filled with the information from the Database
        /// </summary>
        /// <param name="dataReader">SqlDataReader</param>
        /// <param name="IsUsingNextResult">Indicates if is using multiple resultsets</param>
        /// <returns>Generic Collection List</returns>
        /// <example>
        ///     <code>
        ///     public List<![CDATA[<UsuarioInfo>]]> Pesquisar()
        ///     {
        ///         this.Run("SPS_PROCEDURENAME");
        ///
        ///         In("PARAMETERNAME_ID", ID);
        ///
        ///         return Mapper.GetList<![CDATA[<UsuarioInfo>]]>();
        ///     }
        ///     </code>
        /// </example>
        public async Task<List<T>> GetListAsync<T>(SqlDataReader? dataReader = null, bool isUsingNextResult = false) where T : BusinessEntityStructure, new()
        {
            List<T>? output = null;

            T Sender = Activator.CreateInstance<T>();

            if (Sender != null && Sender.MappedProperties!=null)
            {
                Sender.Dispose();

                if (dataReader == null)
                {
                    dataReader = await GetReaderAsync();
                }

                try
                {
                    if (dataReader.HasRows())
                    {
                        output = new List<T>();

                        var schema = dataReader.GetSchema();
                        var oType = typeof(T);
                        var TypeName = oType.Name;

                        while (await dataReader.ReadAsync())
                        {
                            Sender = Activator.CreateInstance<T>();
                            BindList(dataReader, Sender, oType, TypeName, schema, this.MustThrowMappingExceptions);

                            Sender.MappedProperties = null;

                            output.Add(Sender);
                        }

                        schema = null;
                        oType = null;
                    }
                }
                finally
                {
                    if (!isUsingNextResult)
                    {
                        if (dataReader != null)
                        {
                            dataReader.Close();
                            dataReader.Dispose();

                            dataReader = null;
                        }

                        this.Release();
                    }

                    Sender.Dispose();
                }
            }

            return output??new();
        }

        /// <summary>
        /// Retuns a generic list whose datatype will be filled with the information from the Database        
        /// </summary>
        /// <param name="dataReader">SqlDataReader</param>
        /// <example>
        ///     <code>
        ///     public List<![CDATA[<UsuarioInfo>]]> Pesquisar()
        ///     {
        ///         this.Run("SPS_PROCEDURENAME");
        ///
        ///         In("PARAMETERNAME_ID", ID);
        ///
        ///         return Mapper.GetList<![CDATA[<int>]]>();
        ///     }
        ///     </code>
        /// </example>
        public List<T> GetPrimitiveList<T>(SqlDataReader? dataReader = null) where T : IComparable, new()
        {
            List<T>? output = null;

            if (dataReader == null)
            {
                dataReader = GetReader();
            }

            try
            {
                if (dataReader!=null && dataReader.IsClosed == false && dataReader.HasRows)
                {
                    output = new List<T>();

                    while (dataReader.Read())
                    {
                        T PropertyValue = ActivatorFactory.CreateInstance<T>();

                        if (dataReader[0] != null || dataReader[0] == DBNull.Value)
                        {
                            if (dataReader[0] == DBNull.Value)
                            {
                                PropertyValue = new();
                            }
                            else
                            {
                                PropertyValue = (T)Convert.ChangeType(dataReader[0], typeof(T));
                            }
                        }
                        else
                        {
                            PropertyValue = new();
                        }

                        if (PropertyValue!=null)
                        {
                            output.Add(PropertyValue);
                        }
                    }
                }
                else
                {
                    output = new List<T>();
                }
            }
            catch (Exception)
            {
                if (output==null)
                {
                    output = new List<T>();
                }
                else
                {
                    output.Clear();
                }

                dataReader.Close();
                dataReader.Dispose();

                throw;
            }
            finally
            {
                if (dataReader != null)
                {
                    dataReader.Close();
                    dataReader.Dispose();
                }

                this.Release();
            }

            return output;
        }

        /// <summary>
        /// Retuns a generic list whose datatype will be filled with the information from the Database        
        /// </summary>
        /// <param name="dataReader">SqlDataReader</param>
        /// <example>
        ///     <code>
        ///     public List<![CDATA[<UsuarioInfo>]]> Pesquisar()
        ///     {
        ///         this.Run("SPS_PROCEDURENAME");
        ///
        ///         In("PARAMETERNAME_ID", ID);
        ///
        ///         return Mapper.GetList<![CDATA[<int>]]>();
        ///     }
        ///     </code>
        /// </example>
        public async Task<List<T>> GetPrimitiveListAsync<T>(SqlDataReader? dataReader = null) where T : IComparable, new()
        {
            List<T>? output = null;

            if (dataReader == null)
            {
                dataReader = await GetReaderAsync();
            }

            try
            {
                if (dataReader!=null && dataReader.IsClosed == false && dataReader.HasRows)
                {
                    output = new List<T>();

                    while (await dataReader.ReadAsync())
                    {
                        T PropertyValue = ActivatorFactory.CreateInstance<T>();

                        if (dataReader[0] != null || dataReader[0] == DBNull.Value)
                        {
                            if (dataReader[0] == DBNull.Value)
                            {
                                PropertyValue = new();
                            }
                            else
                            {
                                PropertyValue = (T)Convert.ChangeType(dataReader[0], typeof(T));
                            }
                        }
                        else
                        {
                            PropertyValue = new();
                        }

                        if (PropertyValue!=null)
                        {
                            output.Add(PropertyValue);
                        }
                    }
                }
                else
                {
                    output = new List<T>();
                }
            }
            catch (Exception)
            {
                if (output==null)
                {
                    output = new List<T>();
                }
                else
                {
                    output.Clear();
                }

                dataReader.Close();
                dataReader.Dispose();

                throw;
            }
            finally
            {
                if (dataReader != null)
                {
                    dataReader.Close();
                    dataReader.Dispose();
                }

                this.Release();
            }

            return output;
        }

        /// <summary>
        /// Fill the property value of the Business Entity Structured Class with the information in the SqlDataReader
        /// </summary>
        /// <param name="dataReader">SqlDataReader</param>
        /// <param name="Sender">Class derived from the Framework.Entity.BussinessEntityStructure class</param>
        /// <param name="type">Type of Sender</param>
        /// <param name="typeName">Gets the name of the current member.</param>
        /// <param name="schema">List of the columns avaiable in the SqlDataReader</param>
        /// <param name="mustRaiseException">Indicates whether an exception will be throw in case of failure</param>
        public void BindList<T>(SqlDataReader dataReader, T Sender, Type type, string typeName, HashSet<string> schema, bool mustRaiseException) where T : BusinessEntityStructure
        {
            if (Sender != null && Sender.MappedProperties!=null)
            {
                foreach (var mappedProperty in Sender.MappedProperties)
                {
                    if (schema!=null && schema.Contains(mappedProperty.Key))
                    {
                        if (dataReader[mappedProperty.Value] != null)
                        {
                            object? PropertyValue = null;

                            if (dataReader[mappedProperty.Value] == DBNull.Value)
                            {
                                PropertyValue = null;
                            }
                            else
                            {
                                //PropertyValue = Convert.ChangeType((oIDataReader[mappedProperty.Value]), oPropertyInfo.PropertyType);
                                PropertyValue = dataReader[mappedProperty.Value];
                            }

                            if (PropertyValue!=null)
                            {
                                type.InvokeMember(mappedProperty.Key, bindingFlags, Type.DefaultBinder, Sender, new object[] { PropertyValue });
                            }
                        }
                        else
                        {
                            if (mustRaiseException)
                            {
                                throw new ArgumentException($"NullReferenceException - A propriedade '{mappedProperty.Key}' da classe '{typeName}' está mapeada para o campo '{mappedProperty.Value}' que nao existe IDataReader.");
                            }
                        }

                    }
                    else
                    {
                        // Do nothing
                    }
                }
            }
        }

        #endregion

        #region| IDisposable Members |

        /// <summary>
        /// Release allocated resources
        /// </summary>
        public void Dispose()
        {
            this.Release();

            if (this.Command != null)
            {
                this.Command.Dispose();
                this.Command = null;
            }

            if (this.DatabaseContext != null)
            {
                this.DatabaseContext = null;
            }

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Returns a System.Data.DataTable that describes the column metadata of the SqlDataReader
        /// </summary>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        public HashSet<string> GetSchema(SqlDataReader dataReader)
        {
            return dataReader.GetSchema();
        }

        #endregion

    }
}