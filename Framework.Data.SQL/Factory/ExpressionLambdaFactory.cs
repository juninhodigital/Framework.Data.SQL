using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq.Expressions;

using Framework.Entity;

namespace Framework.Data.SQL
{
    internal static class ExpressionLambdaFactory
    {
        #region| Fields |

        // Cache store for memorizing the delegate for later use
        static ConcurrentDictionary<Type, Delegate> ExpressionCache = new ConcurrentDictionary<Type, Delegate>();

        #endregion

        #region| Methods |

        // Method for creating the dynamic funtion for setting entity properties
        public static Func<SqlDataReader, T> GetReaderOptimized<T>(T Sender, HashSet<string> schema) where T : BusinessEntityStructure
        {
            Delegate resDelegate;

            if (!ExpressionCache.TryGetValue(typeof(T), out resDelegate))
            {
                // Get the indexer property of SqlDataReader 
                var indexerProperty = typeof(SqlDataReader).GetProperty("Item", new[] { typeof(string) });

                // List of statements in our dynamic method 
                var statements = new List<Expression>();

                // Instance type of target entity class 
                var instanceParam = Expression.Variable(typeof(T));

                // Parameter for the SqlDataReader object
                var readerParam = Expression.Parameter(typeof(SqlDataReader));

                // Create and assign new T to variable. Ex. var instance = new T(); 
                var createInstance = Expression.Assign(instanceParam, Expression.New(typeof(T)));

                statements.Add(createInstance);

                foreach (var property in typeof(T).GetProperties())
                {
                    if (property.Name.Equals("MappedProperties"))
                    {
                        continue;
                    }

                    //if (Sender.MappedProperties.ContainsKey(property.Name) && schema.Contains(property.Name))
                    if (schema.Contains(property.Name))
                    {
                        // instance.Property 
                        MemberExpression getProperty = Expression.Property(instanceParam, property);

                        // row[property] The assumption is, column names are the  same as PropertyInfo names of T 
                        IndexExpression readValue = Expression.MakeIndex(readerParam, indexerProperty, new[] { Expression.Constant(property.Name) });

                        // instance.Property = row[property] 
                        BinaryExpression assignProperty = Expression.Assign(getProperty, Expression.Convert(readValue, property.PropertyType));

                        statements.Add(assignProperty);
                    }
                }

                var returnStatement = instanceParam;
                statements.Add(returnStatement);

                var body = Expression.Block(instanceParam.Type, new[] { instanceParam }, statements.ToArray());

                var lambda = Expression.Lambda<Func<SqlDataReader, T>>(body, readerParam);

                resDelegate = lambda.Compile();

                // Cache the dynamic method into ExpressionCache dictionary
                ExpressionCache[typeof(T)] = resDelegate;
            }
            return (Func<SqlDataReader, T>)resDelegate;
        } 

        #endregion
    }
}
