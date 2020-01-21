using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using Framework.Core;

namespace Framework.Data.SQL
{
    public static class DataRecordExtensions
    {
        #region| Methods |

        /// <summary>
        /// Check whether the SqlDataReader has a given column name
        /// </summary>
        /// <param name="dataReader">SqlDataReader</param>
        /// <param name="columnName">Column name</param>
        /// <returns></returns>
        public static bool HasColumn(this SqlDataReader dataReader, string columnName)
        {
            if (dataReader.IsNotNull())
            {
                for (int i = 0; i < dataReader.FieldCount; i++)
                {
                    if (dataReader.GetName(i) == columnName)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Check whether the SqlDataReader has rows
        /// </summary>
        /// <param name="dataReader"></param>
        /// <returns>true if has rows, otherwise false</returns>
        public static bool HasRows(this SqlDataReader dataReader)
        {
            if (dataReader.IsNotNull() && dataReader.IsClosed == false && dataReader.HasRows)
            {
                return true;
            }

            return false;
        }



        /// <summary>
        /// Returns a System.Data.DataTable that describes the column metadata of the SqlDataReader
        /// </summary>
        /// <param name="dataReader">SqlDataReader</param>
        /// <returns></returns>
        public static HashSet<string> GetSchema(this SqlDataReader dataReader)
        {
            var oList = new HashSet<string>();

            if (dataReader.IsNotNull())

                for (int i = 0; i < dataReader.FieldCount; i++)
                {
                    oList.Add(dataReader.GetName(i));
                }

            return oList;
        }

        #endregion
    }
}
