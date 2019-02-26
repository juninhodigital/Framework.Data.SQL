using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Text;

using static Framework.Core.Extensions;
using Framework.Entity;
using System.Linq;
using System.Threading;
using System.Globalization;

namespace Framework.Data.SQL
{
    /// <summary>
    /// Base class that provides methods to handle SQL Server statements
    /// <seealso cref="https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/try-finally"/>
    /// </summary>
    [Serializable]
    public class SQLDatabaseRepository : IDatabaseRepository, IDisposable
    {
        #region| Properties |

        /// <summary>
        /// Provides information returned by the print output from the SQL Server
        /// </summary>
        public string InfoMessage { get; set; }

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
        public SQLDatabaseRepository()
        {
            // Your code goes here...
        }

        #endregion

        #region| Destructor |

        /// <summary>
        /// Default Destructor
        /// </summary>
        ~SQLDatabaseRepository()
        {
            this.Dispose();
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

            var oParam = new SqlParameter();

            oParam.Direction = ParameterDirection.Input;
            oParam.ParameterName = parameterName;
            oParam.Value = GetParameterValue(parameterValue);

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
        /// <example>
        /// <code>
        ///     InOut("ParameterName", "ParameterValue");
        /// </code>
        /// </example>
        public void InOut(string parameterName, object parameterValue)
        {
            parameterName = CheckParameterName(parameterName);

            var oParam = new SqlParameter();

            oParam.Direction = ParameterDirection.InputOutput;
            oParam.ParameterName = parameterName;
            oParam.Value = GetParameterValue(parameterValue);

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
        public void Out(string parameterName, SqlDbType sqlDbType, object parameterValue = null)
        {
            parameterName = CheckParameterName(parameterName);

            var oParam = new SqlParameter();

            oParam.Direction = ParameterDirection.InputOutput;
            oParam.SqlDbType = sqlDbType;
            oParam.ParameterName = parameterName;
            oParam.Value = GetParameterValue(parameterValue);

            AddParam(oParam);
        }

        /// <summary>
        /// Adds the specified IDbDataParameter object to the parameter collection
        /// </summary>
        /// <param name="dbDataParameter">IDbDataParameter</param>
        public void AddParam(IDbDataParameter dbDataParameter)
        {
            if (Command == null)
            {
                Command = new SqlCommand();
            }

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
            if (parameterValue.IsNull())
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
                    if (DateTime.Parse(parameterValue.ToString()) == DateTime.MinValue)
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

                    if (oDataTable.IsNull())
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
            var profilePath = GetAppSettings("FRAMEWORK.PROFILE.PATH");

            if (profilePath.IsNotNull())
            {
                var SQL = PreviewSQL();

                File.AppendAllText(profilePath, SQL + Environment.NewLine);

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
                if (Command.IsNotNull() && Command.Parameters != null && Command.Parameters.Count > 0)
                {
                    var HasTVP = false;

                    for (int i = 0; i < Command.Parameters.Count; i++)
                    {
                        var oParam = Command.Parameters[i];

                        if (oParam.SqlDbType == SqlDbType.Structured)
                        {
                            HasTVP = true;

                            var DT = oParam.Value as DataTable;

                            if (DT.IsNotNull() && DT.Rows.Count > 0)
                            {
                                oStringBuilder.Append("DECLARE " + oParam.ParameterName + " " + oParam.ParameterName.Replace("@", "") + BR + BR);

                                for (int z = 0; z < DT.Rows.Count; z++)
                                {
                                    oStringBuilder.Append("INSERT " + oParam.ParameterName);

                                    DataRow oRow = DT.Rows[z];

                                    for (int y = 0; y < oRow.ItemArray.Length; y++)
                                    {
                                        if (y == 0)
                                        {
                                            oStringBuilder.Append(BR);
                                            oStringBuilder.Append("SELECT ");
                                        }

                                        oStringBuilder.Append("'" + oRow.ItemArray[y].ToString() + "'");

                                        if (y != oRow.ItemArray.Length - 1)
                                        {
                                            oStringBuilder.Append(",");
                                        }
                                    }
                                }

                                oStringBuilder.Append(BR);
                            }

                            oStringBuilder.Append(BR);
                        }
                    }

                    if (HasTVP)
                    {
                        oStringBuilder.Append(BR);
                    }
                }

                oStringBuilder.Append("EXEC " + this.CommandText + " " + BR);

                if (Command.IsNotNull() && Command.Parameters != null && Command.Parameters.Count > 0)
                {
                    for (int i = 0; i < Command.Parameters.Count; i++)
                    {
                        var oParam = Command.Parameters[i];

                        if (oParam.Direction == ParameterDirection.InputOutput || oParam.Direction == ParameterDirection.Output)
                        {
                            if (oParam.Direction == ParameterDirection.InputOutput)
                            {
                                if (oParam.Value.IsNotNull())
                                {
                                    oStringBuilder.Append(oParam.ParameterName + " = " + oParam.Value.ToString() + " OUTPUT");
                                }
                                else
                                {
                                    oStringBuilder.Append(oParam.ParameterName + " = NULL OUTPUT");
                                }
                            }
                            else
                            {
                                oStringBuilder.Append(oParam.ParameterName + " = NULL OUTPUT");
                            }
                        }
                        else
                        {
                            if (oParam.Value.IsNull())
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
            if (databaseContext.IsNull())
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
        /// <returns>
        /// The first column of the first row in the result set, or a null reference if the result set is empty. 
        /// Returns a maximum of 2033 characters.
        /// </returns>
        public T GetScalar<T>()
        {
            T output = default(T);

            try
            {
                this.Prepare();

                output = (T)Command.ExecuteScalar();

            }
            finally
            {
                this.Release();
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
        public DataTable GetDataTable()
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
                this.Release();
            }

            return output;
        }

        /// <summary>
        /// Adds or refreshes rows in the System.Data.DataSet.
        /// </summary>
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
        public DataSet GetDataSet()
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
                this.Release();
            }

            return output;
        }

        /// <summary>
        ///  Get a IDataReader based on the System.Data.CommandType and the given parameters
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
        public IEnumerable<T> Query<T>()
        {
            this.Prepare();

            return SqlMapper.Query<T>(this.Connection, this.Command);
        }

        /// <summary>
        ///  Get a IDataReader based on the System.Data.CommandType and the given parameters
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
        public IDataReader GetReader()
        {
            SqlDataReader output = null;

            this.Prepare();

            this.Command.Prepare();

            output = this.Command.ExecuteReader(CommandBehavior.CloseConnection);

            return output;

        }

        /// <summary>
        /// Executes a Transact-SQL statement against the connection and returns the number of rows affected 
        /// </summary>
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
        public int Execute()
        {
            var output = 0;

            try
            {
                this.Prepare();

                output = this.Command.ExecuteNonQuery();
            }
            finally
            {
                this.Release();
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

            if (this.Command.IsNull())
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
        /// Closes the connection to the database. This is the preferred method of closing any open connection.
        /// Closes the command used to execute statements on the database
        /// </summary>
        public void Release()
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

                //this.oSqlCommand.Dispose();
                //this.oSqlCommand = null;
            }


        }

        /// <summary>
        /// Get the info message from the underlying database raised by the PRINT statement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void GetInfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            if (e.Errors.IsNotNull())
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

        /// <summary>
        /// Returns an instance of the Business Entity Structured class whose properties will be filled with the information from the Database
        /// </summary>
        /// <param name="dataReader">IDataReader</param>
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
        public T Map<T>(IDataReader dataReader = null, bool isUsingNextResult = false) where T : BusinessEntityStructure
        {
            if (dataReader.IsNull())
            {
                dataReader = GetReader();
            }

            if (dataReader.IsNotNull() && dataReader.IsClosed == false && ((SqlDataReader)dataReader).HasRows)
            {
                var items = GetList<T>(dataReader, isUsingNextResult);

                if (items != null && items.Any())
                {
                    return items.FirstOrDefault();
                }
                else
                {
                    return default(T); //Activator.CreateInstance<T>();
                }
            }
            else
            {
                return default(T); //Activator.CreateInstance<T>();
            }

        }

        /// <summary>
        /// Returns a generic collection list with instances of the Business Entity Structured class 
        /// whose properties will be filled with the information from the Database
        /// </summary>
        /// <param name="oIDataReader">IDataReader</param>
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
        public IEnumerable<T> GetList<T>(IDataReader oIDataReader = null, bool IsUsingNextResult = false) where T : BusinessEntityStructure
        {
            T Sender = Activator.CreateInstance<T>();

            List<T> Aux = null;

            if (Sender != null && Sender.MappedProperties.IsNotNull())
            {
                Sender.Dispose();

                if (oIDataReader == null)
                {
                    oIDataReader = GetReader();
                }

                try
                {
                    if (oIDataReader.IsNotNull() && oIDataReader.IsClosed == false && ((SqlDataReader)oIDataReader).HasRows)
                    {
                        Aux = new List<T>();

                        var oSchema = GetSchema(oIDataReader);
                        var oType = typeof(T);
                        var TypeName = oType.Name;
                        var MustRaiseException = GetAppSettings("FRAMEWORK.RAISE.EXCEPTION");

                        if (MustRaiseException.IsNull())
                        {
                            MustRaiseException = "false";
                        }

                        while (oIDataReader.Read())
                        {
                            Sender = Activator.CreateInstance<T>();
                            BindList(oIDataReader, Sender, oType, TypeName, oSchema, bool.Parse(MustRaiseException));

                            Sender.MappedProperties = null;

                            Aux.Add(Sender);
                        }

                        oSchema = null;
                        oType = null;
                    }
                    else
                    {
                        Aux = new List<T>();
                    }
                }
                catch (Exception)
                {
                    if (Aux.IsNull())
                    {
                        Aux = new List<T>();
                    }
                    else
                    {
                        Aux.Clear();
                    }

                    oIDataReader.Close();
                    oIDataReader.Dispose();

                    throw;
                }
                finally
                {
                    if (!IsUsingNextResult)
                    {
                        if (oIDataReader != null)
                        {
                            oIDataReader.Close();
                            oIDataReader.Dispose();
                        }

                        this.Release();
                    }
                }
            }

            return Aux;
        }

        /// <summary>
        /// Retuns a generic list whose datatype will be filled with the information from the Database        
        /// </summary>
        /// <param name="oIDataReader">IDataReader</param>
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
        public List<T> GetPrimitiveList<T>(IDataReader oIDataReader = null) where T : IComparable
        {
            List<T> output = null;

            if (oIDataReader == null)
            {
                oIDataReader = GetReader();
            }

            try
            {
                if (oIDataReader.IsNotNull() && oIDataReader.IsClosed == false && ((SqlDataReader)oIDataReader).HasRows)
                {
                    output = new List<T>();

                    var MustRaiseException = GetAppSettings("FRAMEWORK.RAISE.EXCEPTION");

                    if (MustRaiseException.IsNull())
                    {
                        MustRaiseException = "false";
                    }

                    while (oIDataReader.Read())
                    {
                        T PropertyValue = Activator.CreateInstance<T>();

                        if (oIDataReader[0] != null || oIDataReader[0] == DBNull.Value)
                        {
                            if (oIDataReader[0] == DBNull.Value)
                            {
                                PropertyValue = default(T);
                            }
                            else
                            {
                                PropertyValue = (T)Convert.ChangeType(oIDataReader[0], typeof(T));
                            }
                        }
                        else
                        {
                            PropertyValue = default(T);
                        }

                        if (PropertyValue.IsNotNull())
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
                if (output.IsNull())
                {
                    output = new List<T>();
                }
                else
                {
                    output.Clear();
                }

                oIDataReader.Close();
                oIDataReader.Dispose();

                throw;
            }
            finally
            {
                if (oIDataReader != null)
                {
                    oIDataReader.Close();
                    oIDataReader.Dispose();
                }

                this.Release();
            }

            return output;
        }


        /// <summary>
        /// Fill the property value of the Business Entity Structured Class with the information in the IDataReader
        /// </summary>
        /// <param name="oIDataReader">IDataReader</param>
        /// <param name="Sender">Class derived from the Framework.Entity.BussinessEntityStructure class</param>
        /// <param name="oType">Type of Sender</param>
        /// <param name="TypeName">Gets the name of the current member.</param>
        /// <param name="oSchema">List of the columns avaiable in the SqlDataReader</param>
        /// <param name="MustRaiseException">Indicates whether an exception will be throw in case of failure</param>
        public void BindList<T>(IDataReader oIDataReader, T Sender, Type oType, string TypeName, HashSet<string> oSchema, bool MustRaiseException) where T : BusinessEntityStructure
        {
            //mappedProperty.Key   PropertyName;
            //mappedProperty.Value ColumnName;

            foreach (var mappedProperty in Sender.MappedProperties)
            {
                if (oSchema.Contains(mappedProperty.Key))
                {
                    var oPropertyInfo = oType.GetProperty(mappedProperty.Key, BindingFlags.Public | BindingFlags.Instance);

                    if (oPropertyInfo.IsNull())
                    {
                        if (MustRaiseException)
                        {
                            throw new ArgumentException(string.Format("A propriedade '{0}' não existe na classe '{1}'.", mappedProperty.Key, TypeName));
                        }
                    }
                    else
                    {
                        object PropertyValue = null;

                        // If the underlying type is nullable
                        if (oPropertyInfo.PropertyType.IsGenericType && oPropertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            var oPropertyDescriptors = TypeDescriptor.GetProperties(typeof(T));
                            var oPropertyDescriptor = oPropertyDescriptors.Find(mappedProperty.Key, false);
                            var oUnderlyingType = Nullable.GetUnderlyingType(oPropertyDescriptor.PropertyType);

                            if (oUnderlyingType.IsNotNull())
                            {
                                var oConverter = oPropertyDescriptor.Converter;

                                if (oConverter.IsNotNull())
                                {
                                    if (oIDataReader[mappedProperty.Value] != null || oIDataReader[mappedProperty.Value] == DBNull.Value)
                                    {
                                        if (oIDataReader[mappedProperty.Value] == DBNull.Value)
                                        {
                                            PropertyValue = null;
                                        }
                                        else
                                        {
                                            PropertyValue = oConverter.ConvertFrom(oIDataReader[mappedProperty.Value]);
                                        }
                                    }
                                    else
                                    {
                                        PropertyValue = null;
                                    }

                                    if (PropertyValue.IsNotNull())
                                    {
                                        oPropertyInfo.SetValue(Sender, PropertyValue, null);
                                    }
                                }
                            }

                            oPropertyDescriptors = null;
                            oPropertyDescriptor = null;
                            oUnderlyingType = null;
                        }
                        else
                        {
                            if (oIDataReader[mappedProperty.Value] != null)
                            {
                                if (oIDataReader[mappedProperty.Value] != null || oIDataReader[mappedProperty.Value] == DBNull.Value)
                                {
                                    if (oIDataReader[mappedProperty.Value] == DBNull.Value)
                                    {
                                        PropertyValue = null;
                                    }
                                    else
                                    {
                                        PropertyValue = Convert.ChangeType((oIDataReader[mappedProperty.Value]), oPropertyInfo.PropertyType);
                                    }
                                }
                                else
                                {
                                    PropertyValue = null;
                                }

                                if (PropertyValue.IsNotNull())
                                {
                                    oPropertyInfo.SetValue(Sender, PropertyValue, null);
                                }
                            }
                            else
                            {
                                throw new ArgumentException(string.Format("NullReferenceException - A propriedade '{0}' da classe '{1}' está mapeada para o campo '{2}' que nao existe IDataReader.", mappedProperty.Key, TypeName, mappedProperty.Value));
                            }
                        }
                    }
                }
                else
                {
                    if (MustRaiseException)
                    {
                        throw new ArgumentException(string.Format("NullReferenceException - A propriedade '{0}' da classe '{1}' está mapeada para o campo '{2}' que nao existe IDataReader.", mappedProperty.Key, TypeName, mappedProperty.Value));
                    }
                }
            }
        }

        /// <summary>
        /// Returns a System.Data.DataTable that describes the column metadata of the IDataReader
        /// </summary>
        /// <param name="oIDataReader"></param>
        /// <returns></returns>
        public HashSet<string> GetSchema(IDataReader oIDataReader)
        {
            var oList = new HashSet<string>();
            var oSchema = oIDataReader.GetSchemaTable();

            foreach (DataRow oRow in oSchema.Rows)
            {
                var ColumnName = oRow["ColumnName"].ToString();

                oList.Add(ColumnName);
            }

            oSchema.Dispose();

            return oList;
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

        #endregion

    }
}
