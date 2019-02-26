using System.Collections.Generic;
using System.Reflection;

namespace Framework.Data.SQL
{
    public static partial class SqlMapper
    {
        private class PropertyInfoByNameComparer : IComparer<PropertyInfo>
        {
            #region| Methods |

            public int Compare(PropertyInfo x, PropertyInfo y) => string.CompareOrdinal(x.Name, y.Name); 

            #endregion
        }
    }
}
