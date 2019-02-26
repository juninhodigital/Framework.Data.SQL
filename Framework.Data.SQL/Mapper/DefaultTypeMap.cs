using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Framework.Data.SQL
{
    /// <summary>
    /// Represents default type mapping strategy
    /// </summary>
    public sealed class DefaultTypeMap : SqlMapper.ITypeMap
    {
        #region| Fields |

        private readonly List<FieldInfo> fields;
        private readonly Type type;

        #endregion

        #region| Properties |

        /// <summary>
        /// Should column names like User_Id be allowed to match properties/fields like UserId ?
        /// </summary>
        public static bool MatchNamesWithUnderscores { get; set; }

        /// <summary>
        /// The settable properties for this typemap
        /// </summary>
        public List<PropertyInfo> Properties { get; }

        #endregion

        #region| Constructor |

        /// <summary>
        /// Creates default type map
        /// </summary>
        /// <param name="type">Entity type</param>
        public DefaultTypeMap(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            fields = GetSettableFields(type);
            Properties = GetSettableProps(type);

            this.type = type;
        }

        #endregion

        #region| Methods |

        /// <summary>
        /// GetPropertySetter
        /// </summary>
        internal static MethodInfo GetPropertySetter(PropertyInfo propertyInfo, Type type)
        {
            if (propertyInfo.DeclaringType == type)
            {
                return propertyInfo.GetSetMethod(true);
            }

            return propertyInfo.DeclaringType.GetProperty(
                   propertyInfo.Name,
                   BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                   Type.DefaultBinder,
                   propertyInfo.PropertyType,
                   propertyInfo.GetIndexParameters().Select(p => p.ParameterType).ToArray(),
                   null).GetSetMethod(true);
        }

        /// <summary>
        /// GetSettableProps
        /// </summary>
        internal static List<PropertyInfo> GetSettableProps(Type @type)
        {
            return @type
                  .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                  .Where(p => GetPropertySetter(p, @type) != null)
                  .ToList();
        }

        internal static List<FieldInfo> GetSettableFields(Type @type)
        {
            return @type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).ToList();
        }

        /// <summary>
        /// Finds best constructor
        /// </summary>
        /// <param name="names">DataReader column names</param>
        /// <param name="types">DataReader column types</param>
        /// <returns>Matching constructor or default one</returns>
        public ConstructorInfo FindConstructor(string[] names, Type[] types)
        {
            var constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (ConstructorInfo ctor in constructors.OrderBy(c => c.IsPublic ? 0 : (c.IsPrivate ? 2 : 1)).ThenBy(c => c.GetParameters().Length))
            {
                ParameterInfo[] ctorParameters = ctor.GetParameters();

                if (ctorParameters.Length == 0)
                {
                    return ctor;
                }

                if (ctorParameters.Length != types.Length)
                {
                    continue;
                }

                int i = 0;

                for (; i < ctorParameters.Length; i++)
                {
                    if (!string.Equals(ctorParameters[i].Name, names[i], StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }

                    if (types[i] == typeof(byte[]) && ctorParameters[i].ParameterType.FullName == SqlMapper.LinqBinary)
                    {
                        continue;
                    }

                    var unboxedType = Nullable.GetUnderlyingType(ctorParameters[i].ParameterType) ?? ctorParameters[i].ParameterType;

                    if ((unboxedType != types[i])
                        && !(unboxedType.IsEnum() && Enum.GetUnderlyingType(unboxedType) == types[i])
                        && !(unboxedType == typeof(char) && types[i] == typeof(string))
                        && !(unboxedType.IsEnum() && types[i] == typeof(string)))
                    {
                        break;
                    }
                }

                if (i == ctorParameters.Length)
                {
                    return ctor;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the constructor, if any, that has the ExplicitConstructorAttribute on it.
        /// </summary>
        public ConstructorInfo FindExplicitConstructor()
        {
            var constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            var withAttr = constructors.Where(c => c.GetCustomAttributes(typeof(ExplicitConstructorAttribute), true).Length > 0).ToList();

            if (withAttr.Count == 1)
            {
                return withAttr[0];
            }

            return null;
        }

        /// <summary>
        /// Gets mapping for constructor parameter
        /// </summary>
        /// <param name="constructor">Constructor to resolve</param>
        /// <param name="columnName">DataReader column name</param>
        /// <returns>Mapping implementation</returns>
        public SqlMapper.IMemberMap GetConstructorParameter(ConstructorInfo constructor, string columnName)
        {
            var parameters = constructor.GetParameters();

            return new SimpleMemberMap(columnName, parameters.FirstOrDefault(p => string.Equals(p.Name, columnName, StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>
        /// Gets member mapping for column
        /// </summary>
        /// <param name="columnName">DataReader column name</param>
        /// <returns>Mapping implementation</returns>
        public SqlMapper.IMemberMap GetMember(string columnName)
        {
            var property = Properties.Find(p => string.Equals(p.Name, columnName, StringComparison.Ordinal))
               ?? Properties.Find(p => string.Equals(p.Name, columnName, StringComparison.OrdinalIgnoreCase));

            if (property == null && MatchNamesWithUnderscores)
            {
                property = Properties.Find(p => string.Equals(p.Name, columnName.Replace("_", ""), StringComparison.Ordinal))
                    ?? Properties.Find(p => string.Equals(p.Name, columnName.Replace("_", ""), StringComparison.OrdinalIgnoreCase));
            }

            if (property != null)
            {
                return new SimpleMemberMap(columnName, property);
            }

            // roslyn automatically implemented properties, in particular for get-only properties: <{Name}>k__BackingField;
            var backingFieldName = "<" + columnName + ">k__BackingField";

            // preference order is:
            // exact match over underscre match, exact case over wrong case, backing fields over regular fields, match-inc-underscores over match-exc-underscores
            var field = fields.Find(p => string.Equals(p.Name, columnName, StringComparison.Ordinal))
                ?? fields.Find(p => string.Equals(p.Name, backingFieldName, StringComparison.Ordinal))
                ?? fields.Find(p => string.Equals(p.Name, columnName, StringComparison.OrdinalIgnoreCase))
                ?? fields.Find(p => string.Equals(p.Name, backingFieldName, StringComparison.OrdinalIgnoreCase));

            if (field == null && MatchNamesWithUnderscores)
            {
                var effectiveColumnName = columnName.Replace("_", "");
                backingFieldName = "<" + effectiveColumnName + ">k__BackingField";

                field = fields.Find(p => string.Equals(p.Name, effectiveColumnName, StringComparison.Ordinal))
                    ?? fields.Find(p => string.Equals(p.Name, backingFieldName, StringComparison.Ordinal))
                    ?? fields.Find(p => string.Equals(p.Name, effectiveColumnName, StringComparison.OrdinalIgnoreCase))
                    ?? fields.Find(p => string.Equals(p.Name, backingFieldName, StringComparison.OrdinalIgnoreCase));
            }

            if (field != null)
            {
                return new SimpleMemberMap(columnName, field);
            }

            return null;
        } 
        #endregion

    }
}
