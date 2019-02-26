using System;
using System.Data;

namespace Framework.Data.SQL
{
    public static partial class Mapper
    {
        private struct DeserializerState
        {
            #region| Properties |

            public readonly int Hash;
            public readonly Func<IDataReader, object> Func;

            #endregion

            #region| Constructor |

            public DeserializerState(int hash, Func<IDataReader, object> func)
            {
                Hash = hash;
                Func = func;
            } 

            #endregion
        }
    }
}
