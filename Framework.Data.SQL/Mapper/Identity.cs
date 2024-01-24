using System;
using System.Data;

namespace Framework.Data.SQL
{
    public static partial class Mapper
    {
        /// <summary>
        /// Identity of a cached query, used for extensibility.
        /// </summary>
        public class Identity : IEquatable<Identity>
        {
            #region| Properties |

            /// <summary>
            /// Whether this <see cref="Identity"/> equals another.
            /// </summary>
            /// <param name="obj">The other <see cref="object"/> to compare to.</param>
            public override bool Equals(object? obj)
            {
                if (obj != null)
                {
                    if (Equals((Identity)obj))
                    {
                        return true;
                    }
                }

                return false;
            }
            

            /// <summary>
            /// The raw SQL command.
            /// </summary>
            public readonly string sql;

            /// <summary>
            /// The SQL command type.
            /// </summary>
            public readonly CommandType? commandType;

            /// <summary>
            /// The hash code of this Identity.
            /// </summary>
            public readonly int hashCode;

            /// <summary>
            /// The grid index (position in the reader) of this Identity.
            /// </summary>
            public readonly int gridIndex;

            /// <summary>
            /// This <see cref="Type"/> of this Identity.
            /// </summary>
            public readonly Type type;

            /// <summary>
            /// The connection string for this Identity.
            /// </summary>
            public readonly string connectionString;

            /// <summary>
            /// The type of the parameters object for this Identity.
            /// </summary>
            public readonly Type parametersType; 
            #endregion

            #region| Constructor |

            public Identity(string statement, CommandType? commandType, string connectionString, Type type)
            {
                this.sql = statement;

                this.commandType = commandType;
                this.connectionString = connectionString;
                this.type = type;

                unchecked
                {
                    hashCode = 17; // we *know* we are using this in a dictionary, so pre-compute this
                    hashCode = (hashCode * 23) + commandType.GetHashCode();
                    hashCode = (hashCode * 23) + gridIndex.GetHashCode();
                    hashCode = (hashCode * 23) + (sql?.GetHashCode() ?? 0);
                    hashCode = (hashCode * 23) + (type?.GetHashCode() ?? 0);
                    hashCode = (hashCode * 23) + (connectionString == null ? 0 : connectionStringComparer.GetHashCode(connectionString));

                }
            }

            #endregion

            #region| Methods |

            /// <summary>
            /// Gets the hash code for this identity.
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode() => hashCode;

            /// <summary>
            /// Compare 2 Identity objects
            /// </summary>
            /// <param name="other">The other <see cref="Identity"/> object to compare.</param>
            /// <returns>Whether the two are equal</returns>
            public bool Equals(Identity other)
            {
                return other != null
                    && gridIndex == other.gridIndex
                    && type == other.type
                    && sql == other.sql
                    && commandType == other.commandType
                    && connectionStringComparer.Equals(connectionString, other.connectionString)
                    && parametersType == other.parametersType;
            } 

            #endregion
        }
    }
}
