using System.Data;

namespace Framework.Data.SQL
{
    internal abstract class XmlTypeHandler<T> : StringTypeHandler<T>
    {
        #region| Methods |

        public override void SetValue(IDbDataParameter parameter, T value)
        {
            base.SetValue(parameter, value);
            parameter.DbType = DbType.Xml;
        } 

        #endregion
    }
}
