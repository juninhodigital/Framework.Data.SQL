using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

using Framework.Core;

namespace Framework.Data.SQL
{
    /// <summary>
    /// This class is responsible to set the database connections and its behavior
    /// </summary>
    public class SQLConnectionManager: IConnectionManager
    {
        #region| Methods | 

        /// <summary>
        /// Provides T-SQL statement execution in one or more databases
        /// This constructor will try to get the list of servers in the Framework.config file
        /// </summary>
        /// <returns></returns>
        public IDatabaseManager Configure(bool LoadFromXML = false, ExecutionType ExecutionType = ExecutionType.ExecuteOnFirst)
        {
            if (LoadFromXML)
            {
                return ConfigureFromXML(ExecutionType);
            }
            else
            {
                return new SQLDatabaseManager(ExecutionType);
            }

        }

        /// <summary>
        /// Provides T-SQL statement execution in the specific database
        /// using the default command timeout (30) seconds
        /// </summary>
        /// <param name="Connection">The string used to open a SQL Server database.</param>
        /// <returns>DataBaseManager</returns>
        public IDatabaseManager Configure(string Connection)
        {
            return Configure(Connection, 30);
        }

        /// <summary>
        /// Provides T-SQL statement execution in the specific database
        /// </summary>
        /// <param name="Connection">The string used to open a SQL Server database.</param>
        /// <param name="CommandTimeout">The time in seconds to wait for the command to execute.</param>
        /// <returns>DataBaseManager</returns>
        public IDatabaseManager Configure(string Connection, int CommandTimeout)
        {
            return Configure(Connection, CommandTimeout, ExecutionType.ExecuteOnFirst);
        }

        /// <summary>
        /// Provides T-SQL statement execution in the specific database
        /// </summary>
        /// <param name="Connection">The string used to open a SQL Server database.</param>
        /// <param name="CommandTimeout">The time in seconds to wait for the command to execute.</param>
        /// <param name="ExecutionType">ExecutionType</param>
        /// <returns>DataBaseManager</returns>
        public IDatabaseManager Configure(string Connection, int CommandTimeout, ExecutionType ExecutionType)
        {
            var oDataBaseManager = new SQLDatabaseManager(ExecutionType);

            var oDataBase = new SQLDatabaseContext(Connection, CommandTimeout);

            oDataBaseManager.Databases.Add(oDataBase);

            return oDataBaseManager;
        }

        /// <summary>
        /// Provides T-SQL statement execution in one or more databases
        /// using the default command timeout (30) seconds.
        /// 
        /// The T-SQL statement will be executed in the first server of the list, 
        /// if it runs sucessfully, the execution stops and it will not be propagated to the remaining servers.
        /// Otherwise, the execution will continue
        /// </summary>
        /// <param name="Connections">The SQL Server Connection string param array</param>
        /// <returns>DataBaseManager</returns>
        public IDatabaseManager Configure(params string[] Connections)
        {
            return Configure(ExecutionType.ExecuteOnFirst, Connections);
        }

        /// <summary>
        /// Provides T-SQL statement execution in one or more databases
        /// using the default command timeout (30) seconds.
        /// </summary>
        /// <param name="Connections">The SQL Server Connection string param array</param>
        /// <param name="ExecutionType">
        ///     if the ExecutionType = ExecutionType.ExecuteOnFirst:
        ///         The T-SQL statement will be executed in the first server of the list, 
        ///         if it runs sucessfully, the execution stops and it will not be propagated to the remaining servers.
        ///         Otherwise, the execution will continue
        ///     
        ///     if the ExecutionType = ExecutionType.ExecuteOnAll:
        ///         The T-SQL statement will be executed in the all of the server of the list
        /// </param>
        /// <returns>DataBaseManager</returns>
        public IDatabaseManager Configure(ExecutionType ExecutionType, params string[] Connections)
        {
            return Configure(ExecutionType, 30, Connections);
        }

        /// <summary>
        /// Provides T-SQL statement execution in one or more databases
        /// </summary>
        /// <param name="ExecutionType">
        ///     if the ExecutionType = ExecutionType.ExecuteOnFirst:
        ///         The T-SQL statement will be executed in the first server of the list, 
        ///         if it runs sucessfully, the execution stops and it will not be propagated to the remaining servers.
        ///         Otherwise, the execution will continue
        ///     
        ///     if the ExecutionType = ExecutionType.ExecuteOnAll:
        ///         The T-SQL statement will be executed in the all of the server of the list
        /// </param>
        /// <param name="CommandTimeout">The time in seconds to wait for the command to execute.</param>
        /// <param name="Connections">The SQL Server Connection string param array</param>
        /// <returns>DataBaseManager</returns>
        public IDatabaseManager Configure(ExecutionType ExecutionType, int CommandTimeout, params string[] Connections)
        {
            var oConnections = GetConnections().ToList();

            return Configure(ExecutionType, CommandTimeout, oConnections);
        }

        /// <summary>
        /// Provides T-SQL statement execution in one or more databases
        /// using the default command timeout (30) seconds.
        /// </summary>
        /// <param name="Connections">A List of The SQL Server Connection</param>
        /// <returns>DataBaseManager</returns>
        public IDatabaseManager Configure(List<string> Connections)
        {
            return Configure(ExecutionType.ExecuteOnFirst, 30, Connections);
        }

        /// <summary>
        /// Provides T-SQL statement execution in one or more databases
        /// using the default command timeout (30) seconds.
        /// </summary>
        /// <param name="Connections">A List of The SQL Server Connection</param>
        /// <param name="ExecutionType">
        ///     if the ExecutionType = ExecutionType.ExecuteOnFirst:
        ///         The T-SQL statement will be executed in the first server of the list, 
        ///         if it runs sucessfully, the execution stops and it will not be propagated to the remaining servers.
        ///         Otherwise, the execution will continue
        ///     
        ///     if the ExecutionType = ExecutionType.ExecuteOnAll:
        ///         The T-SQL statement will be executed in the all of the server of the list
        /// </param> T-SQL statement will be executed in the all of the server of the list
        /// <returns>DataBaseManager</returns>
        public IDatabaseManager Configure(ExecutionType ExecutionType, List<string> Connections)
        {
            return Configure(ExecutionType, 30, Connections);
        }

        /// <summary>
        /// PProvides T-SQL statement execution in one or more databases
        /// </summary>
        /// <param name="ExecutionType">
        ///     if the ExecutionType = ExecutionType.ExecuteOnFirst:
        ///         The T-SQL statement will be executed in the first server of the list, 
        ///         if it runs sucessfully, the execution stops and it will not be propagated to the remaining servers.
        ///         Otherwise, the execution will continue
        ///     
        ///     if the ExecutionType = ExecutionType.ExecuteOnAll:
        ///         The T-SQL statement will be executed in the all of the server of the list
        /// </param>
        /// <param name="CommandTimeout">The time in seconds to wait for the command to execute.</param>
        /// <param name="Connections">The SQL Server Connection string param array</param>
        /// <returns>DataBaseManager</returns>
        public IDatabaseManager Configure(ExecutionType ExecutionType, int CommandTimeout, List<string> Connections)
        {
            if (Connections.IsNotNull())
            {
                var oDataBaseManager = new SQLDatabaseManager(ExecutionType);

                foreach (var Connection in Connections)
                {
                    var oDataBase = new SQLDatabaseContext(Connection, CommandTimeout);

                    oDataBaseManager.Databases.Add(oDataBase);
                }

                return oDataBaseManager;
            }
            else
            {
                throw new Exception("Framework: A List<string> ou a cole��o de param string[] de ConnectionString n�o contem elementos ou est� vazia.");
            }
        }

        /// <summary>
        /// Provides T-SQL statement execution in one or more databases
        /// This constructor will try to get the list of servers in the Framework.config file
        /// </summary>
        public IDatabaseManager ConfigureFromXML(ExecutionType ExecutionType)
        {
            var oDataBaseManager = new SQLDatabaseManager(ExecutionType);

            var XML_Path = string.Concat(Environment.CurrentDirectory, @"\Framework.config");
            var XSD_Path = string.Concat(Environment.CurrentDirectory, @"\Framework.XSD");

            if (File.Exists(XML_Path) && File.Exists(XSD_Path))
            {
                if ("".ValidateSchema(XML_Path, XSD_Path))
                {
                    var oXml = new XmlDocument();

                    oXml.Load(XML_Path);

                    var oFrameworkNode = oXml.SelectSingleNode("Framework");

                    var oServersNode = oFrameworkNode.SelectSingleNode("Databases");

                    if (oServersNode.IsNotNull())
                    {

                        for (int i = 0; i < oServersNode.ChildNodes.Count; i++)
                        {
                            XmlNode oChildNode = oServersNode.ChildNodes[i];

                            var DataSource     = oChildNode.GetAttribute("DataSource");
                            var UserID         = oChildNode.GetAttribute("UserID");
                            var Password       = oChildNode.GetAttribute("Password");
                            var InitialCatalog = oChildNode.GetAttribute("InitialCatalog");
                            var CommandTimeout = oChildNode.GetAttribute("CommandTimeout").ToInt();
                            var ConnectTimeout = oChildNode.GetAttribute("ConnectTimeout").ToInt();
                            var UseEncryption  = oChildNode.GetAttribute("UseEncryption").ToBool();

                            if (UseEncryption)
                            {
                                Password = Framework.Cryptography.Cryptography.Decrypt(Password, "SampleKey");
                            }

                            var ConnectionString = string.Format("Password={0};User ID={1};Initial Catalog={2};Data Source={3};Connect Timeout={4}", Password, UserID, InitialCatalog, DataSource, ConnectTimeout);

                            var oDataBase = new SQLDatabaseContext(ConnectionString, CommandTimeout);

                            oDataBaseManager.Databases.Add(oDataBase);
                        }
                    }
                    else
                    {
                        throw new Exception("Framework: No one database server has been specified in the Framework.config file");
                    }
                }
                else
                {
                    throw new Exception("Framework: It was not possible to validate the 'Framework.config' using the schema 'Framework.XSD'");
                }
            }
            else
            {
                throw new Exception("Framework: It was not possible to find the files 'Framework.config' and 'Framework.XSD'");
            }

            return oDataBaseManager;
        }

        /// <summary>
        /// Retuns an IEnumerable collections of ConnectionString
        /// </summary>
        /// <param name="Connections">Connections</param>
        /// <returns>IEnumerable</returns>
        public IEnumerable<string> GetConnections(params string[] Connections)
        {
            for (int i = 0; i < Connections.Length; i++)
            {
                yield return Connections[i];
            }
        }

        #endregion
    }
}
