using System;
using System.Data;
using System.Threading;

namespace Framework.Data.SQL
{
    public static partial class SqlMapper
    {
        private class CacheInfo
        {
            #region| Fields |

            private int hitCount; 

            #endregion

            #region| Properties |

            public DeserializerState Deserializer { get; set; } 
            public Func<IDataReader, object>[] OtherDeserializers { get; set; }
            public Action<IDbCommand, object> ParamReader { get; set; }

            #endregion

            #region| Methods |

            public int GetHitCount()
            {
                return Interlocked.CompareExchange(ref hitCount, 0, 0);
            }

            public void RecordHit()
            {
                Interlocked.Increment(ref hitCount);
            } 

            #endregion
        }
    }
}
