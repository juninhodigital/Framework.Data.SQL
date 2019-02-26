using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;

namespace Framework.Data.SQL
{
    public static partial class SqlMapper
    {
        private partial class TypeDeserializerCache
        {
            #region| Constructor |

            private TypeDeserializerCache(Type type)
            {
                this.type = type;
            }

            #endregion

            #region| Fields |

            private static readonly Hashtable byType = new Hashtable();
            private readonly Type type;
            private readonly Dictionary<DeserializerKey, Func<IDataReader, object>> readers = new Dictionary<DeserializerKey, Func<IDataReader, object>>();

            #endregion

            #region| Methods |

            internal static void Purge(Type type)
            {
                lock (byType)
                {
                    byType.Remove(type);
                }
            }

            internal static void Purge()
            {
                lock (byType)
                {
                    byType.Clear();
                }
            }

            internal static Func<IDataReader, object> GetReader(Type type, IDataReader reader, int startBound, int length, bool returnNullIfFirstMissing)
            {
                var found = (TypeDeserializerCache)byType[type];
                if (found == null)
                {
                    lock (byType)
                    {
                        found = (TypeDeserializerCache)byType[type];
                        if (found == null)
                        {
                            byType[type] = found = new TypeDeserializerCache(type);
                        }
                    }
                }
                return found.GetReader(reader, startBound, length, returnNullIfFirstMissing);
            }

            private Func<IDataReader, object> GetReader(IDataReader reader, int startBound, int length, bool returnNullIfFirstMissing)
            {
                if (length < 0)
                {
                    length = reader.FieldCount - startBound;
                }

                int hash = GetColumnHash(reader, startBound, length);

                if (returnNullIfFirstMissing)
                {
                    hash *= -27;
                }

                // get a cheap key first: false means don't copy the values down
                var key = new DeserializerKey(hash, startBound, length, returnNullIfFirstMissing, reader, false);

                Func<IDataReader, object> deser;

                lock (readers)
                {
                    if (readers.TryGetValue(key, out deser)) return deser;
                }

                deser = GetTypeDeserializerImpl(type, reader, startBound, length, returnNullIfFirstMissing);

                // get a more expensive key: true means copy the values down so it can be used as a key later
                key = new DeserializerKey(hash, startBound, length, returnNullIfFirstMissing, reader, true);

                lock (readers)
                {
                    return readers[key] = deser;
                }
            } 
            
            #endregion
        }
    }
}
