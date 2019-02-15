using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Text;

using Framework.Core;
using Framework.Entity;

namespace Framework.Data.SQL
{
    /// <summary>
    /// Base class that provides methods to handle SQL Server statements
    /// </summary>
    [Serializable]
    public class SQLDatabaseRepository: IDatabaseRepository, IDisposable
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
        public SqlConnection oSqlConnection;

        /// <summary>
        /// Instance of System.Data.SqlClient.SqlCommand
        /// </summary>
        public SqlCommand oSqlCommand;

        /// <summary>
        /// Gets or sets the Transact-SQL statement, table name or stored procedure to execute at the data source.
        /// </summary>
        public string oCommandText;

        /// <summary>
        /// Specifies how a command string is interpreted.
        /// </summary>
        public CommandType oCommandType;

        /// <summary>
        /// Instance of Framework.Data.SQL.DataBaseManager
        /// </summary>
        public IDatabaseManager oDataBaseManager;

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

        #region| Parameters 

        /// <summary>
        /// Adds the specified parameter object to the parameter collection (INPUT)
        /// </summary>
        /// <param name="ParameterName">Parameter Name</param>
        /// <param name="ParameterValue">Parameter Value</param>
        /// <example>
        /// <code>
        ///     In("ParameterName", "ParameterValue");
        /// </code>
        /// </example>
        public void In(string ParameterName, object ParameterValue)
        {
            ParameterName = CheckParameterName(ParameterName);

            var oParam = new SqlParameter();

            oParam.Direction     = ParameterDirection.Input;
            oParam.ParameterName = ParameterName;
            oParam.Value         = GetParameterValue(ParameterValue);

            AddParam(oParam);
        }

        /// <summary>
        /// Adds the specified parameter object to the parameter collection (INPUT)
        /// </summary>
        /// <param name="ParameterName">Parameter Name</param>
        /// <param name="ParameterValue">Parameter Value</param>
        /// <param name="ParameterType">System.Data.SqlDbType</param>
        public void In(string ParameterName, object ParameterValue, SqlDbType ParameterType)
        {
            In(ParameterName, ParameterValue);

            if (oSqlCommand.Parameters != null && !oSqlCommand.Parameters.Contains(ParameterName))
            {
                oSqlCommand.Parameters[ParameterName].SqlDbType = ParameterType;
            }
        }

        /// <summary>
        /// Adds the specified parameter object to the parameter collection (INPUT / OUTPUT)
        /// </summary>
        /// <param name="ParameterName">Parameter Name</param>
        /// <param name="ParameterValue">Parameter Value</param>
        /// <example>
        /// <code>
        ///     InOut("ParameterName", "ParameterValue");
        /// </code>
        /// </example>
        public void InOut(string ParameterName, object ParameterValue)
        {
            ParameterName = CheckParameterName(ParameterName);

            var oParam = new SqlParameter();

            oParam.Direction     = ParameterDirection.InputOutput;
            oParam.ParameterName = ParameterName;
            oParam.Value         = GetParameterValue(ParameterValue);

            AddParam(oParam);
        }

        /// <summary>
        /// Adds the specified parameter object to the parameter collection (OUTPUT)
        /// </summary>
        /// <param name="ParameterName">Parameter Name</param>
        /// <param name="ParameterType">System.Data.DbType</param>
        /// <param name="ParameterValue">ParameterValue</param>
        /// <example>
        /// <code>
        ///     Out("ParameterName", DbType.Int32);
        /// </code>
        /// </example>
        public void Out(string ParameterName, SqlDbType ParameterType, object ParameterValue = null)
        {
            ParameterName = CheckParameterName(ParameterName);

            var oParam = new SqlParameter();

            oParam.Direction     = ParameterDirection.InputOutput;
            oParam.SqlDbType     = ParameterType;
            oParam.ParameterName = ParameterName;
            oParam.Value         = GetParameterValue(ParameterValue);

            AddParam(oParam);
        }

        /// <summary>
        /// Adds the specified IDbDataParameter object to the parameter collection
        /// </summary>
        /// <param name="oDbParameter">IDbDataParameter</param>
        public void AddParam(IDbDataParameter oDbParameter)
        {
            if (oSqlCommand == null)
            {
                oSqlCommand = new SqlCommand();
            }

            if (oSqlCommand.Parameters != null && !oSqlCommand.Parameters.Contains(oDbParameter.ParameterName))
            {
                oSqlCommand.Parameters.Add(oDbParameter);
            }
        }

        /// <summary>
        /// Check if the ParameterName is null or empty
        /// </summary>
        /// <param name="ParameterName"></param>
        /// <returns></returns>
        public string CheckParameterName(string ParameterName)
        {
            if (string.IsNullOrEmpty(ParameterName))
            {
                throw new Exception("Framework: O nome do parâmetro não pode ser nulo ou vazio");
            }
            else
            {
                ParameterName = ParameterName.Replace("@", "");
                ParameterName = "@" + ParameterName;
            }

            return ParameterName;
        }

        /// <summary>
        /// Check the parameter value
        /// </summary>
        /// <param name="ParameterValue">ParameterValue</param>
        /// <returns>object</returns>
        public object GetParameterValue(object ParameterValue)
        {
            if (ParameterValue.IsNull())
            {
                return DBNull.Value;
            }
            else
            {
                if (ParameterValue is String)
                {
                    if (string.IsNullOrEmpty(ParameterValue.ToString()))
                    {
                        return DBNull.Value;
                    }
                    else
                    {
                        return ParameterValue;
                    }
                }

                if (ParameterValue is DateTime)
                {
                    if (DateTime.Parse(ParameterValue.ToString()) == DateTime.MinValue)
                    {
                        return DBNull.Value;
                    }
                    else
                    {
                        return ParameterValue;
                    }
                }

                if (ParameterValue is Int32 || ParameterValue is int || ParameterValue is Int64)
                {
                    if (ParameterValue.ToString().ToInt() == int.MinValue)
                    {
                        return DBNull.Value;
                    }
                    else
                    {
                        return ParameterValue;
                    }
                }

                if (ParameterValue is float)
                {
                    if (ParameterValue.ToString().ToFloat() == float.MinValue)
                    {
                        return DBNull.Value;
                    }
                    else
                    {
                        return ParameterValue;
                    }
                }

                if (ParameterValue is decimal)
                {
                    if (ParameterValue.ToString().ToDecimal() == decimal.MinValue)
                    {
                        return DBNull.Value;
                    }
                    else
                    {
                        return ParameterValue;
                    }
                }

                if (ParameterValue is byte)
                {
                    if (byte.Parse(ParameterValue.ToString()) == byte.MinValue)
                    {
                        return DBNull.Value;
                    }
                    else
                    {
                        return ParameterValue;
                    }
                }

                if (ParameterValue is char)
                {
                    if (ParameterValue.ToString().Trim().Length == 0)
                    {
                        return DBNull.Value;
                    }
                    else
                    {
                        if (char.Parse(ParameterValue.ToString()) == char.MinValue)
                        {
                            return DBNull.Value;
                        }
                        else
                        {
                            return ParameterValue;
                        }
                    }
                }

                if (ParameterValue is DataTable)
                {
                    var oDataTable = ParameterValue as DataTable;

                    if (oDataTable.IsNull())
                    {
                        return DBNull.Value;
                    }
                    else
                    {
                        return ParameterValue;
                    }
                }

                if (ParameterValue is Nullable<bool>)
                {
                    var oNullable = (Nullable<bool>)ParameterValue;

                    if (oNullable.HasValue == false)
                    {
                        return DBNull.Value;
                    }
                    else
                    {
                        return oNullable.Value;
                    }
                }

                return ParameterValue;
            }
        }

        #endregion

        /// <summary>
        /// Check whether the Profiler is enabled or not to log the T-SQL Statements in a log file 
        /// </summary>
        public void IsProfilerEnabled()
        {
            var ProfileEnable = this.GetAppSettings("FRAMEWORK.PROFILE.ENABLED");

            if (ProfileEnable.IsNotNull() && ProfileEnable.IsEqual("true"))
            {
                var SQL = PreviewSQL();

                File.AppendAllText(@"C:\FRAMEWORK_PROFILER_LOG.txt", SQL + Environment.NewLine);

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

            if (this.oCommandType == CommandType.StoredProcedure)
            {
                // Check if exists table value parameters
                if (oSqlCommand.IsNotNull() && oSqlCommand.Parameters != null && oSqlCommand.Parameters.Count > 0)
                {
                    var HasTVP = false;

                    for (int i = 0; i < oSqlCommand.Parameters.Count; i++)
                    {
                        var oParam = oSqlCommand.Parameters[i];

                        if (oParam.SqlDbType == SqlDbType.Structured)
                        {
                            HasTVP = true;

                            var DT = oParam.Value as DataTable;

                            if (DT.IsNotNull() && DT.Rows.Count > 0)
                            {
                                oStringBuilder.Append("DECLARE " + oParam.ParameterName + " " + oParam.ParameterName.Replace("@","") + BR + BR);

                                oStringBuilder.Append("INSERT " + oParam.ParameterName);

                                for (int z = 0; z < DT.Rows.Count ; z++)
                                {
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
                
                oStringBuilder.Append("EXEC " + this.oCommandText + " " + BR);

                if (oSqlCommand.IsNotNull() && oSqlCommand.Parameters != null && oSqlCommand.Parameters.Count > 0)
                {
                    for (int i = 0; i < oSqlCommand.Parameters.Count; i++)
                    {
                        var oParam = oSqlCommand.Parameters[i];

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

                        if (i != oSqlCommand.Parameters.Count - 1)
                        {
                            oStringBuilder.Append(",");
                        }

                        oStringBuilder.Append(BR);
                    }
                }
            }
            else
            {
                oStringBuilder.Append(this.oCommandText);
            }

            return oStringBuilder.ToString();
        }
        
        /// <summary>
        /// Sets the DataBaseManager to execute operations against the DataBase
        /// </summary>
        /// <param name="oDM">DataBaseManager</param>
        public void SetManager(IDatabaseManager oDM)
        {
            if (oDM.IsNull() || oDM.Databases.IsNull())
            {
                throw new ArgumentException("Framework - DataBaseManager está null ou não contém databases adicionados.");
            }
            else
            {
                this.oDataBaseManager = oDM;
            }
            
        }

        /// <summary>
        /// Configures the System.Data.CommandType and the T-SQL statement that will be executed on the Database
        /// </summary>
        /// <param name="CommandType">System.Data.CommandType</param>
        /// <param name="Statement">T-SQL Statement</param>
        public void Run(string Statement, CommandType CommandType = CommandType.StoredProcedure)
        {
            this.oCommandText = Statement;
            this.oCommandType = CommandType;
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
            T Aux = default(T);

            for (int i = 0; i < oDataBaseManager.Databases.Count; i++)
            {
                var oDataBase = oDataBaseManager.Databases[i];

                try
                {
                    this.Prepare(oDataBase);

                    Aux = (T)oSqlCommand.ExecuteScalar();

                    break;
                }
                catch (Exception Error)
                {
                    LogError(oDataBaseManager, oDataBase, Error);
                }
                finally
                {
                    this.Release();
                }
            }

            CheckErrorsOnAll();

            return Aux;
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

            return (T)oSqlCommand.Parameters[ParameterName].Value;
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
            var oDataTable = new DataTable();

            for (int i = 0; i < oDataBaseManager.Databases.Count; i++)
            {
                var oDataBase = oDataBaseManager.Databases[i];

                try
                {
                    this.Prepare(oDataBase);

                    this.oSqlCommand.Prepare();

                    using (var oSqlDataReader = this.oSqlCommand.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        oDataTable.Load(oSqlDataReader);

                        oSqlCommand.Dispose();
                    }

                    break;
                }
                catch (Exception Error)
                {
                    LogError(oDataBaseManager, oDataBase, Error);
                }
                finally
                {
                    this.Release();
                }
            }

            CheckErrorsOnAll();

            return oDataTable;
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
            var oDataSet = new DataSet();

            for (int i = 0; i < oDataBaseManager.Databases.Count; i++)
            {
                var oDataBase = oDataBaseManager.Databases[i];

                try
                {
                    this.Prepare(oDataBase);

                    var oSqlDataAdapter = new SqlDataAdapter(this.oSqlCommand);

                    oSqlDataAdapter.Fill(oDataSet);

                    oSqlDataAdapter.Dispose();
                    oSqlCommand.Dispose();

                    break;
                }
                catch (Exception Error)
                {
                    LogError(oDataBaseManager, oDataBase, Error);
                }
                finally
                {
                    this.Release();
                }
            }

            CheckErrorsOnAll();
            
            return oDataSet;
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
            SqlDataReader oSqlDataReader = null;

            for (int i = 0; i < oDataBaseManager.Databases.Count; i++)
            {
                var oDataBase = oDataBaseManager.Databases[i];

                try
                {
                    this.Prepare(oDataBase);

                    this.oSqlCommand.Prepare();

                    oSqlDataReader = this.oSqlCommand.ExecuteReader(CommandBehavior.CloseConnection);

                    break;
                }
                catch (Exception Error)
                {
                    LogError(oDataBaseManager, oDataBase, Error);
                }
            }

            CheckErrorsOnAll();

            return oSqlDataReader;
          
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
            for (int i = 0; i < oDataBaseManager.Databases.Count; i++)
            {
                var oDataBase = oDataBaseManager.Databases[i];

                try
                {
                    this.Prepare(oDataBase);

                    this.oSqlCommand.ExecuteNonQuery();

                    if (oDataBaseManager.ExecutionType == ExecutionType.ExecuteOnFirst)
                    {
                        break;
                    }

                }
                catch (Exception Error)
                {
                    LogError(oDataBaseManager, oDataBase, Error);
                }
                finally
                {
                    this.Release();
                }
            }

            CheckErrorsOnAll();

            return 0;
        }

        /// <summary>
        /// Opens a database connection with the property settings specified by the System.Data.SqlClient.SqlConnection.ConnectionString.
        /// </summary>
        public void Prepare(IDatabaseContext oDataBase)
        {
            this.InfoMessage = string.Empty;

            this.oSqlConnection = new SqlConnection(oDataBase.ConnectionString);
            this.oSqlConnection.InfoMessage += new SqlInfoMessageEventHandler(GetInfoMessage);

            if (this.oSqlCommand == null)
            {
                this.oSqlCommand = oSqlConnection.CreateCommand();
            }

            this.oSqlCommand.CommandType = oCommandType;
            this.oSqlCommand.CommandText = oCommandText;
            this.oSqlCommand.Connection = oSqlConnection;

            if (oDataBase.CommandTimeout.HasValue && oDataBase.CommandTimeout.Value > 0)
            {
                this.oSqlCommand.CommandTimeout = oDataBase.CommandTimeout.Value;
            }
 
            if (this.oSqlCommand.CommandType == CommandType.Text)
            {
                this.oSqlCommand.Parameters.Clear();
            }

            if (this.oSqlConnection != null && this.oSqlConnection.State == ConnectionState.Closed)
            {
                this.oSqlConnection.Open();
            }

            IsProfilerEnabled();
        }

        /// <summary>
        /// Closes the connection to the database. This is the preferred method of closing any open connection.
        /// Closes the command used to execute statements on the database
        /// </summary>
        public void Release()
        {
            if (this.oSqlConnection != null)
            {
                if (this.oSqlConnection.State == ConnectionState.Open)
                {
                    this.oSqlConnection.Close();
                }

                //this.SqlConnection.ClearPool(Command.Connection);
                this.oSqlConnection.Dispose();
                this.oSqlConnection = null;
            }

            if (this.oSqlCommand!=null)
            {
                if (this.oSqlCommand.Connection != null)
                {
                    if (this.oSqlCommand.Connection.State == ConnectionState.Open)
                    {
                        this.oSqlCommand.Connection.Close();
                    }

                    //this.SqlConnection.ClearPool(Command.Connection);
                    this.oSqlCommand.Connection.Dispose();
                    this.oSqlCommand.Connection = null;
                }

                //this.oSqlCommand.Dispose();
                //this.oSqlCommand = null;
            }

           
        }

        /// <summary>
        /// Logs the errors into the Database class
        /// </summary>
        /// <param name="oDataBaseManager">DataBaseManager</param>
        /// <param name="oDataBase">DataBase</param>
        /// <param name="Error">Exception</param>
        public void LogError(IDatabaseManager oDataBaseManager, IDatabaseContext oDataBase, Exception Error)
        {
            // oDataBaseManager.errorMessage += oDataBase.DataSource + " - " + Error.Message + Environment.NewLine;
            oDataBaseManager.ErrorMessage += Error.Message + Environment.NewLine;

            oDataBaseManager.HasError = true;
            oDataBase.HasError = true;

        }

        /// <summary>
        /// Check whether an error occured in all database collection
        /// </summary>
        public void CheckErrorsOnAll()
        {           
            if (oDataBaseManager.Databases.Exists(D => D.HasError == false))
            {
                oDataBaseManager.HasErrorOnAll = false;
            }
            else
            {
                oDataBaseManager.HasErrorOnAll = true;
            }
        }

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
        /// <param name="oIDataReader">IDataReader</param>
        /// <param name="IsUsingNextResult">Indicates if is using multiple resultsets</param>
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
        public T Map<T>(IDataReader oIDataReader = null, bool IsUsingNextResult = false) where T : BusinessEntityStructure
        {
            if (oIDataReader.IsNull())
            {
                oIDataReader = GetReader();
            }

            if (oIDataReader.IsNotNull() && oIDataReader.IsClosed == false && ((SqlDataReader)oIDataReader).HasRows)
            {
                List<T> Aux = GetList<T>(oIDataReader, IsUsingNextResult);

                if (Aux != null && Aux.Count > 0)
                {
                    T Sender = Aux[0] as T;

                    Aux = null;

                    return Sender;

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
        public List<T> GetList<T>(IDataReader oIDataReader = null, bool IsUsingNextResult = false) where T : BusinessEntityStructure
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

                        var oSchema            = GetSchema(oIDataReader);
                        var oType              = typeof(T);
                        var TypeName           = oType.Name;
                        var MustRaiseException = this.GetAppSettings("FRAMEWORK.RAISE.EXCEPTION");

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
                        oType   = null;
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

                    var MustRaiseException = this.GetAppSettings("FRAMEWORK.RAISE.EXCEPTION");

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
        public void BindList<T>(IDataReader oIDataReader, T Sender, Type oType, string TypeName, List<string> oSchema, bool MustRaiseException) where T : BusinessEntityStructure
        {
            #region| OLD 

            //var MustRaiseException = Core.Common.GetAppSettings("FRAMEWORK.RAISE.EXCEPTION");

            //Type oType = Sender.GetType();

            //foreach (var oProperty in Sender.MappedProperties)
            //{
            //    try
            //    {
            //        var oPropertyInfo = oType.GetProperty(oProperty.PropertyName);

            //        if (oPropertyInfo == null)
            //        {
            //            if (MustRaiseException.IsNotNull() && MustRaiseException.IsEqual("true"))
            //            {
            //                throw new ArgumentException(string.Format("A propriedade {0} nao existe na classe {1}.", oProperty.PropertyName, oType.Name));
            //            }
            //        }
            //        else
            //        {
            //            if (oSqlDataReader[oProperty.ColumnName] != null && oSqlDataReader[oProperty.ColumnName] != DBNull.Value)
            //            {
            //                oPropertyInfo.SetValue(Sender, Convert.ChangeType((oSqlDataReader[oProperty.ColumnName]), oPropertyInfo.PropertyType), null);
            //            }
            //        }
            //    }
            //    catch (IndexOutOfRangeException)
            //    {
            //        if (MustRaiseException.IsNotNull() && MustRaiseException.IsEqual("true"))
            //        {
            //            throw new ArgumentException(string.Format("IndexOutOfRangeException - A propriedade {0} da classe {1} está mapeada para o campo {2} que nao existe IDataReader.", oProperty.PropertyName, oType.Name, oProperty.ColumnName));
            //        }
            //    }
            //    catch (NullReferenceException)
            //    {
            //        if (MustRaiseException.IsNotNull() && MustRaiseException.IsEqual("true"))
            //        {
            //            throw new ArgumentException(string.Format("NullReferenceException - A propriedade {0} da classe {1} está mapeada para o campo {2} que nao existe IDataReader.", oProperty.PropertyName, oType.Name, oProperty.ColumnName));
            //        }
            //    }
            //} 
            #endregion

            for (int i = 0; i < Sender.MappedProperties.Count; i++)
            {
                var oProperty = Sender.MappedProperties[i];

                if (oSchema.Exists(C => C.IsEqual(oProperty.ColumnName)))
                {
                    var oPropertyInfo = oType.GetProperty(oProperty.PropertyName, BindingFlags.Public | BindingFlags.Instance);

                    if (oPropertyInfo.IsNull())
                    {
                        if (MustRaiseException)
                        {
                            throw new ArgumentException(string.Format("A propriedade '{0}' não existe na classe '{1}'.", oProperty.PropertyName, TypeName));
                        }
                    }
                    else
                    {
                        object PropertyValue = null;

                        // If the underlying type is nullable
                        if (oPropertyInfo.PropertyType.IsGenericType && oPropertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            var oPropertyDescriptors = TypeDescriptor.GetProperties(typeof(T));
                            var oPropertyDescriptor  = oPropertyDescriptors.Find(oProperty.PropertyName, false);
                            var oUnderlyingType      = Nullable.GetUnderlyingType(oPropertyDescriptor.PropertyType);

                            if (oUnderlyingType.IsNotNull())
                            {
                                var oConverter = oPropertyDescriptor.Converter;

                                if (oConverter.IsNotNull())
                                {
                                    if (oIDataReader[oProperty.ColumnName] != null || oIDataReader[oProperty.ColumnName] == DBNull.Value)
                                    {
                                        if (oIDataReader[oProperty.ColumnName] == DBNull.Value)
                                        {
                                            PropertyValue = null;
                                        }
                                        else
                                        {
                                            PropertyValue = oConverter.ConvertFrom(oIDataReader[oProperty.ColumnName]);
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
                            oPropertyDescriptor  = null;
                            oUnderlyingType      = null;
                        }
                        else
                        {
                            if (oIDataReader[oProperty.ColumnName]!=null)
                            {
                                if (oIDataReader[oProperty.ColumnName] != null || oIDataReader[oProperty.ColumnName] == DBNull.Value)
                                {
                                    if (oIDataReader[oProperty.ColumnName] == DBNull.Value)
                                    {
                                        PropertyValue = null;
                                    }
                                    else
                                    {
                                        PropertyValue = Convert.ChangeType((oIDataReader[oProperty.ColumnName]), oPropertyInfo.PropertyType);
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
                                throw new ArgumentException(string.Format("NullReferenceException - A propriedade '{0}' da classe '{1}' está mapeada para o campo '{2}' que nao existe IDataReader.", oProperty.PropertyName, TypeName, oProperty.ColumnName));
                            }
                        }
                    }
                }
                else
                {
                    if (MustRaiseException)
                    {
                        throw new ArgumentException(string.Format("NullReferenceException - A propriedade '{0}' da classe '{1}' está mapeada para o campo '{2}' que nao existe IDataReader.", oProperty.PropertyName, TypeName, oProperty.ColumnName));
                    }
                }
            }
        }

        /// <summary>
        /// Returns a System.Data.DataTable that describes the column metadata of the IDataReader
        /// </summary>
        /// <param name="oIDataReader"></param>
        /// <returns></returns>
        public List<string> GetSchema(IDataReader oIDataReader)
        {
            var oList = new List<string>();
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

            if (this.oSqlCommand != null)
            {
                this.oSqlCommand.Dispose();
                this.oSqlCommand = null;
            }

            if (this.oDataBaseManager != null)
            {
                this.oDataBaseManager.Dispose();
                this.oDataBaseManager = null;
            }

            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
