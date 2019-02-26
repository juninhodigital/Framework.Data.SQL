﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

#if NETSTANDARD1_3
using DataException = System.InvalidOperationException;
#endif

namespace Framework.Data.SQL
{
    /// <summary>
    /// SQL Mapper class based on the Dapper
    /// </summary>
    public static partial class SqlMapper
    {
        private class PropertyInfoByNameComparer : IComparer<PropertyInfo>
        {
            public int Compare(PropertyInfo x, PropertyInfo y) => string.CompareOrdinal(x.Name, y.Name);
        }
        private static int GetColumnHash(IDataReader reader, int startBound = 0, int length = -1)
        {
            unchecked
            {
                int max = length < 0 ? reader.FieldCount : startBound + length;
                int hash = (-37 * startBound) + max;
                for (int i = startBound; i < max; i++)
                {
                    object tmp = reader.GetName(i);
                    hash = (-79 * ((hash * 31) + (tmp?.GetHashCode() ?? 0))) + (reader.GetFieldType(i)?.GetHashCode() ?? 0);
                }
                return hash;
            }
        }

        /// <summary>
        /// Called if the query cache is purged via PurgeQueryCache
        /// </summary>
        public static event EventHandler QueryCachePurged;
        private static void OnQueryCachePurged()
        {
            var handler = QueryCachePurged;
            handler?.Invoke(null, EventArgs.Empty);
        }

        private static readonly System.Collections.Concurrent.ConcurrentDictionary<Identity, CacheInfo> _queryCache = new System.Collections.Concurrent.ConcurrentDictionary<Identity, CacheInfo>();
        private static void SetQueryCache(Identity key, CacheInfo value)
        {
            if (Interlocked.Increment(ref collect) == COLLECT_PER_ITEMS)
            {
                CollectCacheGarbage();
            }
            _queryCache[key] = value;
        }

        private static void CollectCacheGarbage()
        {
            try
            {
                foreach (var pair in _queryCache)
                {
                    if (pair.Value.GetHitCount() <= COLLECT_HIT_COUNT_MIN)
                    {
                        _queryCache.TryRemove(pair.Key, out CacheInfo cache);
                    }
                }
            }

            finally
            {
                Interlocked.Exchange(ref collect, 0);
            }
        }

        private const int COLLECT_PER_ITEMS = 1000, COLLECT_HIT_COUNT_MIN = 0;
        private static int collect;
        private static bool TryGetQueryCache(Identity key, out CacheInfo value)
        {
            if (_queryCache.TryGetValue(key, out value))
            {
                value.RecordHit();
                return true;
            }
            value = null;
            return false;
        }

        /// <summary>
        /// Purge the query cache
        /// </summary>
        public static void PurgeQueryCache()
        {
            _queryCache.Clear();
            TypeDeserializerCache.Purge();
            OnQueryCachePurged();
        }

        private static void PurgeQueryCacheByType(Type type)
        {
            foreach (var entry in _queryCache)
            {
                if (entry.Key.type == type)
                    _queryCache.TryRemove(entry.Key, out CacheInfo cache);
            }
            TypeDeserializerCache.Purge(type);
        }

        /// <summary>
        /// Return a count of all the cached queries
        /// </summary>
        /// <returns></returns>
        public static int GetCachedSQLCount()
        {
            return _queryCache.Count;
        }

        /// <summary>
        /// Return a list of all the queries cached
        /// </summary>
        /// <param name="ignoreHitCountAbove"></param>
        /// <returns></returns>
        public static IEnumerable<Tuple<string, string, int>> GetCachedSQL(int ignoreHitCountAbove = int.MaxValue)
        {
            var data = _queryCache.Select(pair => Tuple.Create(pair.Key.connectionString, pair.Key.sql, pair.Value.GetHitCount()));
            return (ignoreHitCountAbove < int.MaxValue)
                    ? data.Where(tuple => tuple.Item3 <= ignoreHitCountAbove)
                    : data;
        }

        /// <summary>
        /// Deep diagnostics only: find any hash collisions in the cache
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Tuple<int, int>> GetHashCollissions()
        {
            var counts = new Dictionary<int, int>();
            foreach (var key in _queryCache.Keys)
            {
                if (!counts.TryGetValue(key.hashCode, out int count))
                {
                    counts.Add(key.hashCode, 1);
                }
                else
                {
                    counts[key.hashCode] = count + 1;
                }
            }
            return from pair in counts
                   where pair.Value > 1
                   select Tuple.Create(pair.Key, pair.Value);
        }

        private static Dictionary<Type, DbType> typeMap;

        static SqlMapper()
        {
            typeMap = new Dictionary<Type, DbType>
            {
                [typeof(byte)] = DbType.Byte,
                [typeof(sbyte)] = DbType.SByte,
                [typeof(short)] = DbType.Int16,
                [typeof(ushort)] = DbType.UInt16,
                [typeof(int)] = DbType.Int32,
                [typeof(uint)] = DbType.UInt32,
                [typeof(long)] = DbType.Int64,
                [typeof(ulong)] = DbType.UInt64,
                [typeof(float)] = DbType.Single,
                [typeof(double)] = DbType.Double,
                [typeof(decimal)] = DbType.Decimal,
                [typeof(bool)] = DbType.Boolean,
                [typeof(string)] = DbType.String,
                [typeof(char)] = DbType.StringFixedLength,
                [typeof(Guid)] = DbType.Guid,
                [typeof(DateTime)] = DbType.DateTime,
                [typeof(DateTimeOffset)] = DbType.DateTimeOffset,
                [typeof(TimeSpan)] = DbType.Time,
                [typeof(byte[])] = DbType.Binary,
                [typeof(byte?)] = DbType.Byte,
                [typeof(sbyte?)] = DbType.SByte,
                [typeof(short?)] = DbType.Int16,
                [typeof(ushort?)] = DbType.UInt16,
                [typeof(int?)] = DbType.Int32,
                [typeof(uint?)] = DbType.UInt32,
                [typeof(long?)] = DbType.Int64,
                [typeof(ulong?)] = DbType.UInt64,
                [typeof(float?)] = DbType.Single,
                [typeof(double?)] = DbType.Double,
                [typeof(decimal?)] = DbType.Decimal,
                [typeof(bool?)] = DbType.Boolean,
                [typeof(char?)] = DbType.StringFixedLength,
                [typeof(Guid?)] = DbType.Guid,
                [typeof(DateTime?)] = DbType.DateTime,
                [typeof(DateTimeOffset?)] = DbType.DateTimeOffset,
                [typeof(TimeSpan?)] = DbType.Time,
                [typeof(object)] = DbType.Object
            };
            ResetTypeHandlers(false);
        }

        /// <summary>
        /// Clear the registered type handlers.
        /// </summary>
        public static void ResetTypeHandlers() => ResetTypeHandlers(true);

        private static void ResetTypeHandlers(bool clone)
        {
            typeHandlers = new Dictionary<Type, ITypeHandler>();
#if !NETSTANDARD1_3
            //Do nothing..
#endif
            AddTypeHandlerImpl(typeof(XmlDocument), new XmlDocumentHandler(), clone);
            AddTypeHandlerImpl(typeof(XDocument), new XDocumentHandler(), clone);
            AddTypeHandlerImpl(typeof(XElement), new XElementHandler(), clone);
        }

        /// <summary>
        /// Configure the specified type to be mapped to a given db-type.
        /// </summary>
        /// <param name="type">The type to map from.</param>
        /// <param name="dbType">The database type to map to.</param>
        public static void AddTypeMap(Type type, DbType dbType)
        {
            // use clone, mutate, replace to avoid threading issues
            var snapshot = typeMap;

            if (snapshot.TryGetValue(type, out DbType oldValue) && oldValue == dbType) return; // nothing to do

            typeMap = new Dictionary<Type, DbType>(snapshot) { [type] = dbType };
        }

        /// <summary>
        /// Removes the specified type from the Type/DbType mapping table.
        /// </summary>
        /// <param name="type">The type to remove from the current map.</param>
        public static void RemoveTypeMap(Type type)
        {
            // use clone, mutate, replace to avoid threading issues
            var snapshot = typeMap;

            if (!snapshot.ContainsKey(type)) return; // nothing to do

            var newCopy = new Dictionary<Type, DbType>(snapshot);
            newCopy.Remove(type);

            typeMap = newCopy;
        }

        /// <summary>
        /// Configure the specified type to be processed by a custom handler.
        /// </summary>
        /// <param name="type">The type to handle.</param>
        /// <param name="handler">The handler to process the <paramref name="type"/>.</param>
        public static void AddTypeHandler(Type type, ITypeHandler handler) => AddTypeHandlerImpl(type, handler, true);

        internal static bool HasTypeHandler(Type type) => typeHandlers.ContainsKey(type);

        /// <summary>
        /// Configure the specified type to be processed by a custom handler.
        /// </summary>
        /// <param name="type">The type to handle.</param>
        /// <param name="handler">The handler to process the <paramref name="type"/>.</param>
        /// <param name="clone">Whether to clone the current type handler map.</param>
        public static void AddTypeHandlerImpl(Type type, ITypeHandler handler, bool clone)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            Type secondary = null;
            if (type.IsValueType())
            {
                var underlying = Nullable.GetUnderlyingType(type);
                if (underlying == null)
                {
                    secondary = typeof(Nullable<>).MakeGenericType(type); // the Nullable<T>
                    // type is already the T
                }
                else
                {
                    secondary = type; // the Nullable<T>
                    type = underlying; // the T
                }
            }

            var snapshot = typeHandlers;
            if (snapshot.TryGetValue(type, out ITypeHandler oldValue) && handler == oldValue) return; // nothing to do

            var newCopy = clone ? new Dictionary<Type, ITypeHandler>(snapshot) : snapshot;

#pragma warning disable 618
            typeof(TypeHandlerCache<>).MakeGenericType(type).GetMethod(nameof(TypeHandlerCache<int>.SetHandler), BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { handler });
            if (secondary != null)
            {
                typeof(TypeHandlerCache<>).MakeGenericType(secondary).GetMethod(nameof(TypeHandlerCache<int>.SetHandler), BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { handler });
            }
#pragma warning restore 618
            if (handler == null)
            {
                newCopy.Remove(type);
                if (secondary != null) newCopy.Remove(secondary);
            }
            else
            {
                newCopy[type] = handler;
                if (secondary != null) newCopy[secondary] = handler;
            }
            typeHandlers = newCopy;
        }

        /// <summary>
        /// Configure the specified type to be processed by a custom handler.
        /// </summary>
        /// <typeparam name="T">The type to handle.</typeparam>
        /// <param name="handler">The handler for the type <typeparamref name="T"/>.</param>
        public static void AddTypeHandler<T>(TypeHandler<T> handler) => AddTypeHandlerImpl(typeof(T), handler, true);

        private static Dictionary<Type, ITypeHandler> typeHandlers;

        internal const string LinqBinary = "System.Data.Linq.Binary";

        private const string ObsoleteInternalUsageOnly = "This method is for internal use only";

        /// <summary>
        /// Get the DbType that maps to a given value.
        /// </summary>
        /// <param name="value">The object to get a corresponding database type for.</param>
        [Obsolete(ObsoleteInternalUsageOnly, false)]
#if !NETSTANDARD1_3
        [Browsable(false)]
#endif
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static DbType GetDbType(object value)
        {
            if (value == null || value is DBNull) return DbType.Object;

            return LookupDbType(value.GetType(), "n/a", false, out ITypeHandler handler);
        }

        /// <summary>
        /// OBSOLETE: For internal usage only. Lookup the DbType and handler for a given Type and member
        /// </summary>
        /// <param name="type">The type to lookup.</param>
        /// <param name="name">The name (for error messages).</param>
        /// <param name="demand">Whether to demand a value (throw if missing).</param>
        /// <param name="handler">The handler for <paramref name="type"/>.</param>
        [Obsolete(ObsoleteInternalUsageOnly, false)]
#if !NETSTANDARD1_3
        [Browsable(false)]
#endif
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static DbType LookupDbType(Type type, string name, bool demand, out ITypeHandler handler)
        {
            handler = null;
            var nullUnderlyingType = Nullable.GetUnderlyingType(type);
            if (nullUnderlyingType != null) type = nullUnderlyingType;
            if (type.IsEnum() && !typeMap.ContainsKey(type))
            {
                type = Enum.GetUnderlyingType(type);
            }
            if (typeMap.TryGetValue(type, out DbType dbType))
            {
                return dbType;
            }
            if (type.FullName == LinqBinary)
            {
                return DbType.Binary;
            }
            if (typeHandlers.TryGetValue(type, out handler))
            {
                return DbType.Object;
            }

            if (demand)
                throw new NotSupportedException($"The member {name} of type {type.FullName} cannot be used as a parameter value");
            return DbType.Object;
        }

        /// <summary>
        /// Obtains the data as a list; if it is *already* a list, the original object is returned without
        /// any duplication; otherwise, ToList() is invoked.
        /// </summary>
        /// <typeparam name="T">The type of element in the list.</typeparam>
        /// <param name="source">The enumerable to return as a list.</param>
        public static List<T> AsList<T>(this IEnumerable<T> source) =>
            (source == null || source is List<T>) ? (List<T>)source : source.ToList();

     

        /// <summary>
        /// Executes a query, returning the data typed as <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of results to return.</typeparam>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sql">The SQL to execute for the query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="buffered">Whether to buffer results in memory.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <returns>
        /// A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first column in assumed, otherwise an instance is
        /// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public static IEnumerable<T> Query<T>(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            var command = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, buffered ? CommandFlagsEnumerator.Buffered : CommandFlagsEnumerator.None);
            var data = QueryImpl<T>(cnn, command, typeof(T));
            return command.Buffered ? data.ToList() : data;
        }

        private static IDataReader ExecuteReaderWithFlagsFallback(IDbCommand cmd, bool wasClosed, CommandBehavior behavior)
        {
            try
            {
                return cmd.ExecuteReader(GetBehavior(wasClosed, behavior));
            }
            catch (ArgumentException ex)
            { // thanks, Sqlite!
                if (Settings.DisableCommandBehaviorOptimizations(behavior, ex))
                {
                    // we can retry; this time it will have different flags
                    return cmd.ExecuteReader(GetBehavior(wasClosed, behavior));
                }
                throw;
            }
        }

        private static IEnumerable<T> QueryImpl<T>(this IDbConnection cnn, CommandDefinition command, Type effectiveType)
        {
            object param = command.Parameters;
            var identity = new Identity(command.CommandText, command.CommandType, cnn.ConnectionString, effectiveType);
            var info = GetCacheInfo(identity, param, command.AddToCache);

            IDbCommand cmd = null;
            IDataReader reader = null;

            bool wasClosed = cnn.State == ConnectionState.Closed;
            try
            {
                cmd = command.SetupCommand(cnn, info.ParamReader);

                if (wasClosed) cnn.Open();
                reader = ExecuteReaderWithFlagsFallback(cmd, wasClosed, CommandBehavior.SequentialAccess | CommandBehavior.SingleResult);
                wasClosed = false; // *if* the connection was closed and we got this far, then we now have a reader
                // with the CloseConnection flag, so the reader will deal with the connection; we
                // still need something in the "finally" to ensure that broken SQL still results
                // in the connection closing itself
                var tuple = info.Deserializer;
                int hash = GetColumnHash(reader);
                if (tuple.Func == null || tuple.Hash != hash)
                {
                    if (reader.FieldCount == 0) 
                        yield break;
                    tuple = info.Deserializer = new DeserializerState(hash, GetDeserializer(effectiveType, reader, 0, -1, false));
                    if (command.AddToCache) SetQueryCache(identity, info);
                }

                var func = tuple.Func;
                var convertToType = Nullable.GetUnderlyingType(effectiveType) ?? effectiveType;
                while (reader.Read())
                {
                    object val = func(reader);
                    if (val == null || val is T)
                    {
                        yield return (T)val;
                    }
                    else
                    {
                        yield return (T)Convert.ChangeType(val, convertToType, CultureInfo.InvariantCulture);
                    }
                }
                while (reader.NextResult()) { /* ignore subsequent result sets */ }
                // happy path; close the reader cleanly - no
                // need for "Cancel" etc
                reader.Dispose();
                reader = null;

                
            }
            finally
            {
                if (reader != null)
                {
                    if (!reader.IsClosed)
                    {
                        try { cmd.Cancel(); }
                        catch { /* don't spoil the existing exception */ }
                    }
                    reader.Dispose();
                }
                if (wasClosed) cnn.Close();
                cmd?.Dispose();
            }
        }

        [Flags]
        internal enum Row
        {
            First = 0,
            FirstOrDefault = 1, //  & FirstOrDefault != 0: allow zero rows
            Single = 2, // & Single != 0: demand at least one row
            SingleOrDefault = 3
        }

        private static readonly int[] ErrTwoRows = new int[2], ErrZeroRows = new int[0];
        private static void ThrowMultipleRows(Row row)
        {
            switch (row)
            {  // get the standard exception from the runtime
                case Row.Single: ErrTwoRows.Single(); break;
                case Row.SingleOrDefault: ErrTwoRows.SingleOrDefault(); break;
                default: throw new InvalidOperationException();
            }
        }

        private static void ThrowZeroRows(Row row)
        {
            switch (row)
            { // get the standard exception from the runtime
                case Row.First: ErrZeroRows.First(); break;
                case Row.Single: ErrZeroRows.Single(); break;
                default: throw new InvalidOperationException();
            }
        }

        private static T QueryRowImpl<T>(IDbConnection cnn, Row row, ref CommandDefinition command, Type effectiveType)
        {
            object param = command.Parameters;
            var identity = new Identity(command.CommandText, command.CommandType, cnn.ConnectionString, effectiveType);
            var info = GetCacheInfo(identity, param, command.AddToCache);

            IDbCommand cmd = null;
            IDataReader reader = null;

            bool wasClosed = cnn.State == ConnectionState.Closed;
            try
            {
                cmd = command.SetupCommand(cnn, info.ParamReader);

                if (wasClosed) cnn.Open();
                reader = ExecuteReaderWithFlagsFallback(cmd, wasClosed, (row & Row.Single) != 0
                    ? CommandBehavior.SequentialAccess | CommandBehavior.SingleResult // need to allow multiple rows, to check fail condition
                    : CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow);
                wasClosed = false; // *if* the connection was closed and we got this far, then we now have a reader

                T result = default(T);
                if (reader.Read() && reader.FieldCount != 0)
                {
                    // with the CloseConnection flag, so the reader will deal with the connection; we
                    // still need something in the "finally" to ensure that broken SQL still results
                    // in the connection closing itself
                    var tuple = info.Deserializer;
                    int hash = GetColumnHash(reader);
                    if (tuple.Func == null || tuple.Hash != hash)
                    {
                        tuple = info.Deserializer = new DeserializerState(hash, GetDeserializer(effectiveType, reader, 0, -1, false));
                        if (command.AddToCache) SetQueryCache(identity, info);
                    }

                    var func = tuple.Func;
                    object val = func(reader);
                    if (val == null || val is T)
                    {
                        result = (T)val;
                    }
                    else
                    {
                        var convertToType = Nullable.GetUnderlyingType(effectiveType) ?? effectiveType;
                        result = (T)Convert.ChangeType(val, convertToType, CultureInfo.InvariantCulture);
                    }
                    if ((row & Row.Single) != 0 && reader.Read()) ThrowMultipleRows(row);
                    while (reader.Read()) { /* ignore subsequent rows */ }
                }
                else if ((row & Row.FirstOrDefault) == 0) // demanding a row, and don't have one
                {
                    ThrowZeroRows(row);
                }
                while (reader.NextResult()) { /* ignore subsequent result sets */ }
                // happy path; close the reader cleanly - no
                // need for "Cancel" etc
                reader.Dispose();
                reader = null;

                
                return result;
            }
            finally
            {
                if (reader != null)
                {
                    if (!reader.IsClosed)
                    {
                        try { cmd.Cancel(); }
                        catch { /* don't spoil the existing exception */ }
                    }
                    reader.Dispose();
                }
                if (wasClosed) cnn.Close();
                cmd?.Dispose();
            }
        }
       
        private static CommandBehavior GetBehavior(bool close, CommandBehavior @default)
        {
            return (close ? (@default | CommandBehavior.CloseConnection) : @default) & Settings.AllowedCommandBehaviors;
        }
             
        private static Func<IDataReader, TReturn> GenerateMapper<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(Func<IDataReader, object> deserializer, Func<IDataReader, object>[] otherDeserializers, object map)
        {
            switch (otherDeserializers.Length)
            {
                case 1:
                    return r => ((Func<TFirst, TSecond, TReturn>)map)((TFirst)deserializer(r), (TSecond)otherDeserializers[0](r));
                case 2:
                    return r => ((Func<TFirst, TSecond, TThird, TReturn>)map)((TFirst)deserializer(r), (TSecond)otherDeserializers[0](r), (TThird)otherDeserializers[1](r));
                case 3:
                    return r => ((Func<TFirst, TSecond, TThird, TFourth, TReturn>)map)((TFirst)deserializer(r), (TSecond)otherDeserializers[0](r), (TThird)otherDeserializers[1](r), (TFourth)otherDeserializers[2](r));
                case 4:
                    return r => ((Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>)map)((TFirst)deserializer(r), (TSecond)otherDeserializers[0](r), (TThird)otherDeserializers[1](r), (TFourth)otherDeserializers[2](r), (TFifth)otherDeserializers[3](r));
                case 5:
                    return r => ((Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>)map)((TFirst)deserializer(r), (TSecond)otherDeserializers[0](r), (TThird)otherDeserializers[1](r), (TFourth)otherDeserializers[2](r), (TFifth)otherDeserializers[3](r), (TSixth)otherDeserializers[4](r));
                case 6:
                    return r => ((Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>)map)((TFirst)deserializer(r), (TSecond)otherDeserializers[0](r), (TThird)otherDeserializers[1](r), (TFourth)otherDeserializers[2](r), (TFifth)otherDeserializers[3](r), (TSixth)otherDeserializers[4](r), (TSeventh)otherDeserializers[5](r));
                default:
                    throw new NotSupportedException();
            }
        }

        private static Func<IDataReader, TReturn> GenerateMapper<TReturn>(int length, Func<IDataReader, object> deserializer, Func<IDataReader, object>[] otherDeserializers, Func<object[], TReturn> map)
        {
            return r =>
            {
                var objects = new object[length];
                objects[0] = deserializer(r);

                for (var i = 1; i < length; ++i)
                {
                    objects[i] = otherDeserializers[i - 1](r);
                }

                return map(objects);
            };
        }

        private static Func<IDataReader, object>[] GenerateDeserializers(Type[] types, string splitOn, IDataReader reader)
        {
            var deserializers = new List<Func<IDataReader, object>>();
            var splits = splitOn.Split(',').Select(s => s.Trim()).ToArray();
            bool isMultiSplit = splits.Length > 1;
            if (types[0] == typeof(object))
            {
                // we go left to right for dynamic multi-mapping so that the madness of TestMultiMappingVariations
                // is supported
                bool first = true;
                int currentPos = 0;
                int splitIdx = 0;
                string currentSplit = splits[splitIdx];
                foreach (var type in types)
                {
                    int splitPoint = GetNextSplitDynamic(currentPos, currentSplit, reader);
                    if (isMultiSplit && splitIdx < splits.Length - 1)
                    {
                        currentSplit = splits[++splitIdx];
                    }
                    deserializers.Add(GetDeserializer(type, reader, currentPos, splitPoint - currentPos, !first));
                    currentPos = splitPoint;
                    first = false;
                }
            }
            else
            {
                // in this we go right to left through the data reader in order to cope with properties that are
                // named the same as a subsequent primary key that we split on
                int currentPos = reader.FieldCount;
                int splitIdx = splits.Length - 1;
                var currentSplit = splits[splitIdx];
                for (var typeIdx = types.Length - 1; typeIdx >= 0; --typeIdx)
                {
                    var type = types[typeIdx];
                  
                    int splitPoint = 0;
                    if (typeIdx > 0)
                    {
                        splitPoint = GetNextSplit(currentPos, currentSplit, reader);
                        if (isMultiSplit && splitIdx > 0)
                        {
                            currentSplit = splits[--splitIdx];
                        }
                    }

                    deserializers.Add(GetDeserializer(type, reader, splitPoint, currentPos - splitPoint, typeIdx > 0));
                    currentPos = splitPoint;
                }

                deserializers.Reverse();
            }

            return deserializers.ToArray();
        }

        private static int GetNextSplitDynamic(int startIdx, string splitOn, IDataReader reader)
        {
            if (startIdx == reader.FieldCount)
            {
                throw MultiMapException(reader);
            }

            if (splitOn == "*")
            {
                return ++startIdx;
            }

            for (var i = startIdx + 1; i < reader.FieldCount; ++i)
            {
                if (string.Equals(splitOn, reader.GetName(i), StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return reader.FieldCount;
        }

        private static int GetNextSplit(int startIdx, string splitOn, IDataReader reader)
        {
            if (splitOn == "*")
            {
                return --startIdx;
            }

            for (var i = startIdx - 1; i > 0; --i)
            {
                if (string.Equals(splitOn, reader.GetName(i), StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            throw MultiMapException(reader);
        }

        private static CacheInfo GetCacheInfo(Identity identity, object exampleParameters, bool addToCache)
        {
            if (!TryGetQueryCache(identity, out CacheInfo info))
            {
                info = new CacheInfo();
                
                if (addToCache) SetQueryCache(identity, info);
            }
            return info;
        }

        private static bool ShouldPassByPosition(string sql)
        {
            return sql?.IndexOf('?') >= 0 && pseudoPositional.IsMatch(sql);
        }

        private static void PassByPosition(IDbCommand cmd)
        {
            if (cmd.Parameters.Count == 0) return;

            Dictionary<string, IDbDataParameter> parameters = new Dictionary<string, IDbDataParameter>(StringComparer.Ordinal);

            foreach (IDbDataParameter param in cmd.Parameters)
            {
                if (!string.IsNullOrEmpty(param.ParameterName)) parameters[param.ParameterName] = param;
            }
            HashSet<string> consumed = new HashSet<string>(StringComparer.Ordinal);
            bool firstMatch = true;
            cmd.CommandText = pseudoPositional.Replace(cmd.CommandText, match =>
            {
                string key = match.Groups[1].Value;
                if (!consumed.Add(key))
                {
                    throw new InvalidOperationException("When passing parameters by position, each parameter can only be referenced once");
                }
                else if (parameters.TryGetValue(key, out IDbDataParameter param))
                {
                    if (firstMatch)
                    {
                        firstMatch = false;
                        cmd.Parameters.Clear(); // only clear if we are pretty positive that we've found this pattern successfully
                    }
                    // if found, return the anonymous token "?"
                    cmd.Parameters.Add(param);
                    parameters.Remove(key);
                    consumed.Add(key);
                    return "?";
                }
                else
                {
                    // otherwise, leave alone for simple debugging
                    return match.Value;
                }
            });
        }

        private static Func<IDataReader, object> GetDeserializer(Type type, IDataReader reader, int startBound, int length, bool returnNullIfFirstMissing)
        {
            Type underlyingType = null;
            if (!(typeMap.ContainsKey(type) || type.IsEnum() || type.FullName == LinqBinary
                || (type.IsValueType() && (underlyingType = Nullable.GetUnderlyingType(type)) != null && underlyingType.IsEnum())))
            {
                if (typeHandlers.TryGetValue(type, out ITypeHandler handler))
                {
                    return GetHandlerDeserializer(handler, type, startBound);
                }
                return GetTypeDeserializer(type, reader, startBound, length, returnNullIfFirstMissing);
            }
            return GetStructDeserializer(type, underlyingType ?? type, startBound);
        }

        private static Func<IDataReader, object> GetHandlerDeserializer(ITypeHandler handler, Type type, int startBound)
        {
            return reader => handler.Parse(type, reader.GetValue(startBound));
        }

        private static Exception MultiMapException(IDataRecord reader)
        {
            bool hasFields = false;
            try { hasFields = reader != null && reader.FieldCount != 0; }
            catch { /* don't throw when trying to throw */ }
            if (hasFields)
                return new ArgumentException("When using the multi-mapping APIs ensure you set the splitOn param if you have keys other than Id", "splitOn");
            else
                return new InvalidOperationException("No columns were selected");
        }
      
        /// <summary>
        /// Internal use only.
        /// </summary>
        /// <param name="value">The object to convert to a character.</param>
#if !NETSTANDARD1_3
        [Browsable(false)]
#endif
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete(ObsoleteInternalUsageOnly, false)]
        public static char ReadChar(object value)
        {
            if (value == null || value is DBNull) throw new ArgumentNullException(nameof(value));
            var s = value as string;
            if (s == null || s.Length != 1) throw new ArgumentException("A single-character was expected", nameof(value));
            return s[0];
        }

        /// <summary>
        /// Internal use only.
        /// </summary>
        /// <param name="value">The object to convert to a character.</param>
#if !NETSTANDARD1_3
        [Browsable(false)]
#endif
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete(ObsoleteInternalUsageOnly, false)]
        public static char? ReadNullableChar(object value)
        {
            if (value == null || value is DBNull) return null;
            var s = value as string;
            if (s == null || s.Length != 1) throw new ArgumentException("A single-character was expected", nameof(value));
            return s[0];
        }

        /// <summary>
        /// Internal use only.
        /// </summary>
        /// <param name="parameters">The parameter collection to search in.</param>
        /// <param name="command">The command for this fetch.</param>
        /// <param name="name">The name of the parameter to get.</param>
#if !NETSTANDARD1_3
        [Browsable(false)]
#endif
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete(ObsoleteInternalUsageOnly, true)]
        public static IDbDataParameter FindOrAddParameter(IDataParameterCollection parameters, IDbCommand command, string name)
        {
            IDbDataParameter result;
            if (parameters.Contains(name))
            {
                result = (IDbDataParameter)parameters[name];
            }
            else
            {
                result = command.CreateParameter();
                result.ParameterName = name;
                parameters.Add(result);
            }
            return result;
        }

        internal static int GetListPaddingExtraCount(int count)
        {
            switch (count)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                    return 0; // no padding
            }
            if (count < 0) return 0;

            int padFactor;
            if (count <= 150) padFactor = 10;
            else if (count <= 750) padFactor = 50;
            else if (count <= 2000) padFactor = 100; // note: max param count for SQL Server
            else if (count <= 2070) padFactor = 10; // try not to over-pad as we approach that limit
            else if (count <= 2100) return 0; // just don't pad between 2070 and 2100, to minimize the crazy
            else padFactor = 200; // above that, all bets are off!

            // if we have 17, factor = 10; 17 % 10 = 7, we need 3 more
            int intoBlock = count % padFactor;
            return intoBlock == 0 ? 0 : (padFactor - intoBlock);
        }

        private static string GetInListRegex(string name, bool byPosition) => byPosition
            ? (@"(\?)" + Regex.Escape(name) + @"\?(?!\w)(\s+(?i)unknown(?-i))?")
            : ("([?@:]" + Regex.Escape(name) + @")(?!\w)(\s+(?i)unknown(?-i))?");

        /// <summary>
        /// Internal use only.
        /// </summary>
        /// <param name="command">The command to pack parameters for.</param>
        /// <param name="namePrefix">The name prefix for these parameters.</param>
        /// <param name="value">The parameter value can be an <see cref="IEnumerable{T}"/></param>
#if !NETSTANDARD1_3
        [Browsable(false)]
#endif

        private static bool TryStringSplit(ref IEnumerable list, int splitAt, string namePrefix, IDbCommand command, bool byPosition)
        {
            if (list == null || splitAt < 0) return false;
            switch (list)
            {
                case IEnumerable<int> l:
                    return TryStringSplit(ref l, splitAt, namePrefix, command, "int", byPosition, (sb, i) => sb.Append(i.ToString(CultureInfo.InvariantCulture)));
                case IEnumerable<long> l:
                    return TryStringSplit(ref l, splitAt, namePrefix, command, "bigint", byPosition, (sb, i) => sb.Append(i.ToString(CultureInfo.InvariantCulture)));
                case IEnumerable<short> l:
                    return TryStringSplit(ref l, splitAt, namePrefix, command, "smallint", byPosition, (sb, i) => sb.Append(i.ToString(CultureInfo.InvariantCulture)));
                case IEnumerable<byte> l:
                    return TryStringSplit(ref l, splitAt, namePrefix, command, "tinyint", byPosition, (sb, i) => sb.Append(i.ToString(CultureInfo.InvariantCulture)));
            }
            return false;
        }

        private static bool TryStringSplit<T>(ref IEnumerable<T> list, int splitAt, string namePrefix, IDbCommand command, string colType, bool byPosition,
            Action<StringBuilder, T> append)
        {
            var typed = list as ICollection<T>;
            if (typed == null)
            {
                typed = list.ToList();
                list = typed; // because we still need to be able to iterate it, even if we fail here
            }
            if (typed.Count < splitAt) return false;

            string varName = null;
            var regexIncludingUnknown = GetInListRegex(namePrefix, byPosition);
            var sql = Regex.Replace(command.CommandText, regexIncludingUnknown, match =>
            {
                var variableName = match.Groups[1].Value;
                if (match.Groups[2].Success)
                {
                    // looks like an optimize hint; leave it alone!
                    return match.Value;
                }
                else
                {
                    varName = variableName;
                    return "(select cast([value] as " + colType + ") from string_split(" + variableName + ",','))";
                }
            }, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant);
            if (varName == null) return false; // couldn't resolve the var!

            command.CommandText = sql;
            var concatenatedParam = command.CreateParameter();
            concatenatedParam.ParameterName = namePrefix;
            concatenatedParam.DbType = DbType.AnsiString;
            concatenatedParam.Size = -1;
            string val;
            using (var iter = typed.GetEnumerator())
            {
                if (iter.MoveNext())
                {
                    var sb = GetStringBuilder();
                    append(sb, iter.Current);
                    while (iter.MoveNext())
                    {
                        append(sb.Append(','), iter.Current);
                    }
                    val = sb.ToString();
                }
                else
                {
                    val = "";
                }
            }
            concatenatedParam.Value = val;
            command.Parameters.Add(concatenatedParam);
            return true;
        }

        /// <summary>
        /// OBSOLETE: For internal usage only. Sanitizes the paramter value with proper type casting.
        /// </summary>
        /// <param name="value">The value to sanitize.</param>
        [Obsolete(ObsoleteInternalUsageOnly, false)]
        public static object SanitizeParameterValue(object value)
        {
            if (value == null) return DBNull.Value;
            if (value is Enum)
            {
                TypeCode typeCode;
                if (value is IConvertible)
                {
                    typeCode = ((IConvertible)value).GetTypeCode();
                }
                else
                {
                    typeCode = TypeExtensions.GetTypeCode(Enum.GetUnderlyingType(value.GetType()));
                }
                switch (typeCode)
                {
                    case TypeCode.Byte: return (byte)value;
                    case TypeCode.SByte: return (sbyte)value;
                    case TypeCode.Int16: return (short)value;
                    case TypeCode.Int32: return (int)value;
                    case TypeCode.Int64: return (long)value;
                    case TypeCode.UInt16: return (ushort)value;
                    case TypeCode.UInt32: return (uint)value;
                    case TypeCode.UInt64: return (ulong)value;
                }
            }
            return value;
        }

        private static IEnumerable<PropertyInfo> FilterParameters(IEnumerable<PropertyInfo> parameters, string sql)
        {
            var list = new List<PropertyInfo>(16);
            foreach (var p in parameters)
            {
                if (Regex.IsMatch(sql, @"[?@:]" + p.Name + @"([^\p{L}\p{N}_]+|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant))
                    list.Add(p);
            }
            return list;
        }

        // look for ? / @ / : *by itself*
        private static readonly Regex smellsLikeOleDb = new Regex(@"(?<![\p{L}\p{N}@_])[?@:](?![\p{L}\p{N}@_])", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled),
            literalTokens = new Regex(@"(?<![\p{L}\p{N}_])\{=([\p{L}\p{N}_]+)\}", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled),
            pseudoPositional = new Regex(@"\?([\p{L}_][\p{L}\p{N}_]*)\?", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        internal static readonly MethodInfo format = typeof(SqlMapper).GetMethod("Format", BindingFlags.Public | BindingFlags.Static);

        /// <summary>
        /// Convert numeric values to their string form for SQL literal purposes.
        /// </summary>
        /// <param name="value">The value to get a string for.</param>
        [Obsolete(ObsoleteInternalUsageOnly)]
        public static string Format(object value)
        {
            if (value == null)
            {
                return "null";
            }
            else
            {
                switch (TypeExtensions.GetTypeCode(value.GetType()))
                {
#if !NETSTANDARD1_3
                    case TypeCode.DBNull:
                        return "null";
#endif
                    case TypeCode.Boolean:
                        return ((bool)value) ? "1" : "0";
                    case TypeCode.Byte:
                        return ((byte)value).ToString(CultureInfo.InvariantCulture);
                    case TypeCode.SByte:
                        return ((sbyte)value).ToString(CultureInfo.InvariantCulture);
                    case TypeCode.UInt16:
                        return ((ushort)value).ToString(CultureInfo.InvariantCulture);
                    case TypeCode.Int16:
                        return ((short)value).ToString(CultureInfo.InvariantCulture);
                    case TypeCode.UInt32:
                        return ((uint)value).ToString(CultureInfo.InvariantCulture);
                    case TypeCode.Int32:
                        return ((int)value).ToString(CultureInfo.InvariantCulture);
                    case TypeCode.UInt64:
                        return ((ulong)value).ToString(CultureInfo.InvariantCulture);
                    case TypeCode.Int64:
                        return ((long)value).ToString(CultureInfo.InvariantCulture);
                    case TypeCode.Single:
                        return ((float)value).ToString(CultureInfo.InvariantCulture);
                    case TypeCode.Double:
                        return ((double)value).ToString(CultureInfo.InvariantCulture);
                    case TypeCode.Decimal:
                        return ((decimal)value).ToString(CultureInfo.InvariantCulture);
                    default:
                        throw new NotSupportedException($"The type '{value.GetType().Name}' is not supported for SQL literals.");
                }
            }
        }

        private static bool IsValueTuple(Type type) => type?.IsValueType() == true && type.FullName.StartsWith("System.ValueTuple`", StringComparison.Ordinal);

        private static List<IMemberMap> GetValueTupleMembers(Type type, string[] names)
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            var result = new List<IMemberMap>(names.Length);
            for (int i = 0; i < names.Length; i++)
            {
                FieldInfo field = null;
                string name = "Item" + (i + 1).ToString(CultureInfo.InvariantCulture);
                foreach (var test in fields)
                {
                    if (test.Name == name)
                    {
                        field = test;
                        break;
                    }
                }
                result.Add(field == null ? null : new SimpleMemberMap(string.IsNullOrWhiteSpace(names[i]) ? name : names[i], field));
            }
            return result;
        }

        private static readonly Dictionary<TypeCode, MethodInfo> toStrings = new[]
        {
            typeof(bool), typeof(sbyte), typeof(byte), typeof(ushort), typeof(short),
            typeof(uint), typeof(int), typeof(ulong), typeof(long), typeof(float), typeof(double), typeof(decimal)
        }.ToDictionary(x => TypeExtensions.GetTypeCode(x), x => x.GetPublicInstanceMethod(nameof(object.ToString), new[] { typeof(IFormatProvider) }));

        private static MethodInfo GetToString(TypeCode typeCode)
        {
            return toStrings.TryGetValue(typeCode, out MethodInfo method) ? method : null;
        }

        private static readonly MethodInfo StringReplace = typeof(string).GetPublicInstanceMethod(nameof(string.Replace), new Type[] { typeof(string), typeof(string) }),
            InvariantCulture = typeof(CultureInfo).GetProperty(nameof(CultureInfo.InvariantCulture), BindingFlags.Public | BindingFlags.Static).GetGetMethod();

        private static int ExecuteCommand(IDbConnection cnn, ref CommandDefinition command, Action<IDbCommand, object> paramReader)
        {
            IDbCommand cmd = null;
            bool wasClosed = cnn.State == ConnectionState.Closed;
            try
            {
                cmd = command.SetupCommand(cnn, paramReader);
                if (wasClosed) cnn.Open();
                int result = cmd.ExecuteNonQuery();
                
                return result;
            }
            finally
            {
                if (wasClosed) cnn.Close();
                cmd?.Dispose();
            }
        }

      
        private static Func<IDataReader, object> GetStructDeserializer(Type type, Type effectiveType, int index)
        {
            // no point using special per-type handling here; it boils down to the same, plus not all are supported anyway (see: SqlDataReader.GetChar - not supported!)
#pragma warning disable 618
            if (type == typeof(char))
            { // this *does* need special handling, though
                return r => ReadChar(r.GetValue(index));
            }
            if (type == typeof(char?))
            {
                return r => ReadNullableChar(r.GetValue(index));
            }
            if (type.FullName == LinqBinary)
            {
                return r => Activator.CreateInstance(type, r.GetValue(index));
            }
#pragma warning restore 618

            if (effectiveType.IsEnum())
            {   // assume the value is returned as the correct type (int/byte/etc), but box back to the typed enum
                return r =>
                {
                    var val = r.GetValue(index);
                    if (val is float || val is double || val is decimal)
                    {
                        val = Convert.ChangeType(val, Enum.GetUnderlyingType(effectiveType), CultureInfo.InvariantCulture);
                    }
                    return val is DBNull ? null : Enum.ToObject(effectiveType, val);
                };
            }
            if (typeHandlers.TryGetValue(type, out ITypeHandler handler))
            {
                return r =>
                {
                    var val = r.GetValue(index);
                    return val is DBNull ? null : handler.Parse(type, val);
                };
            }
            return r =>
            {
                var val = r.GetValue(index);
                return val is DBNull ? null : val;
            };
        }

        private static T Parse<T>(object value)
        {
            if (value == null || value is DBNull) return default(T);
            if (value is T) return (T)value;
            var type = typeof(T);
            type = Nullable.GetUnderlyingType(type) ?? type;
            if (type.IsEnum())
            {
                if (value is float || value is double || value is decimal)
                {
                    value = Convert.ChangeType(value, Enum.GetUnderlyingType(type), CultureInfo.InvariantCulture);
                }
                return (T)Enum.ToObject(type, value);
            }
            if (typeHandlers.TryGetValue(type, out ITypeHandler handler))
            {
                return (T)handler.Parse(type, value);
            }
            return (T)Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
        }

        private static readonly MethodInfo
                    enumParse = typeof(Enum).GetMethod(nameof(Enum.Parse), new Type[] { typeof(Type), typeof(string), typeof(bool) }),
                    getItem = typeof(IDataRecord).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                        .Where(p => p.GetIndexParameters().Length > 0 && p.GetIndexParameters()[0].ParameterType == typeof(int))
                        .Select(p => p.GetGetMethod()).First();

        /// <summary>
        /// Gets type-map for the given type
        /// </summary>
        /// <returns>Type map instance, default is to create new instance of DefaultTypeMap</returns>
        public static Func<Type, ITypeMap> TypeMapProvider = (Type type) => new DefaultTypeMap(type);

        /// <summary>
        /// Gets type-map for the given <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The type to get a map for.</param>
        /// <returns>Type map implementation, DefaultTypeMap instance if no override present</returns>
        public static ITypeMap GetTypeMap(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            var map = (ITypeMap)_typeMaps[type];
            if (map == null)
            {
                lock (_typeMaps)
                {   // double-checked; store this to avoid reflection next time we see this type
                    // since multiple queries commonly use the same domain-entity/DTO/view-model type
                    map = (ITypeMap)_typeMaps[type];

                    if (map == null)
                    {
                        map = TypeMapProvider(type);
                        _typeMaps[type] = map;
                    }
                }
            }
            return map;
        }

        // use Hashtable to get free lockless reading
        private static readonly Hashtable _typeMaps = new Hashtable();

        /// <summary>
        /// Set custom mapping for type deserializers
        /// </summary>
        /// <param name="type">Entity type to override</param>
        /// <param name="map">Mapping rules impementation, null to remove custom map</param>
        public static void SetTypeMap(Type type, ITypeMap map)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (map == null || map is DefaultTypeMap)
            {
                lock (_typeMaps)
                {
                    _typeMaps.Remove(type);
                }
            }
            else
            {
                lock (_typeMaps)
                {
                    _typeMaps[type] = map;
                }
            }

            PurgeQueryCacheByType(type);
        }

        /// <summary>
        /// Internal use only
        /// </summary>
        /// <param name="type"></param>
        /// <param name="reader"></param>
        /// <param name="startBound"></param>
        /// <param name="length"></param>
        /// <param name="returnNullIfFirstMissing"></param>
        /// <returns></returns>
        public static Func<IDataReader, object> GetTypeDeserializer(
            Type type, IDataReader reader, int startBound = 0, int length = -1, bool returnNullIfFirstMissing = false
        )
        {
            return TypeDeserializerCache.GetReader(type, reader, startBound, length, returnNullIfFirstMissing);
        }

        private static LocalBuilder GetTempLocal(ILGenerator il, ref Dictionary<Type, LocalBuilder> locals, Type type, bool initAndLoad)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            locals = locals ?? new Dictionary<Type, LocalBuilder>();
            if (!locals.TryGetValue(type, out LocalBuilder found))
            {
                found = il.DeclareLocal(type);
                locals.Add(type, found);
            }
            if (initAndLoad)
            {
                il.Emit(OpCodes.Ldloca, (short)found.LocalIndex);
                il.Emit(OpCodes.Initobj, type);
                il.Emit(OpCodes.Ldloca, (short)found.LocalIndex);
                il.Emit(OpCodes.Ldobj, type);
            }
            return found;
        }

        private static Func<IDataReader, object> GetTypeDeserializerImpl(
            Type type, IDataReader reader, int startBound = 0, int length = -1, bool returnNullIfFirstMissing = false
        )
        {
            var returnType = type.IsValueType() ? typeof(object) : type;
            var dm = new DynamicMethod("Deserialize" + Guid.NewGuid().ToString(), returnType, new[] { typeof(IDataReader) }, type, true);
            var il = dm.GetILGenerator();
            il.DeclareLocal(typeof(int));
            il.DeclareLocal(type);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Stloc_0);

            if (length == -1)
            {
                length = reader.FieldCount - startBound;
            }

            if (reader.FieldCount <= startBound)
            {
                throw MultiMapException(reader);
            }

            var names = Enumerable.Range(startBound, length).Select(i => reader.GetName(i)).ToArray();

            ITypeMap typeMap = GetTypeMap(type);

            int index = startBound;
            ConstructorInfo specializedConstructor = null;

#if !NETSTANDARD1_3
            bool supportInitialize = false;
#endif
            Dictionary<Type, LocalBuilder> structLocals = null;
            if (type.IsValueType())
            {
                il.Emit(OpCodes.Ldloca_S, (byte)1);
                il.Emit(OpCodes.Initobj, type);
            }
            else
            {
                var types = new Type[length];
                for (int i = startBound; i < startBound + length; i++)
                {
                    types[i - startBound] = reader.GetFieldType(i);
                }

                var explicitConstr = typeMap.FindExplicitConstructor();
                if (explicitConstr != null)
                {
                    var consPs = explicitConstr.GetParameters();
                    foreach (var p in consPs)
                    {
                        if (!p.ParameterType.IsValueType())
                        {
                            il.Emit(OpCodes.Ldnull);
                        }
                        else
                        {
                            GetTempLocal(il, ref structLocals, p.ParameterType, true);
                        }
                    }

                    il.Emit(OpCodes.Newobj, explicitConstr);
                    il.Emit(OpCodes.Stloc_1);
#if !NETSTANDARD1_3
                    supportInitialize = typeof(ISupportInitialize).IsAssignableFrom(type);
                    if (supportInitialize)
                    {
                        il.Emit(OpCodes.Ldloc_1);
                        il.EmitCall(OpCodes.Callvirt, typeof(ISupportInitialize).GetMethod(nameof(ISupportInitialize.BeginInit)), null);
                    }
#endif
                }
                else
                {
                    var ctor = typeMap.FindConstructor(names, types);
                    if (ctor == null)
                    {
                        string proposedTypes = "(" + string.Join(", ", types.Select((t, i) => t.FullName + " " + names[i]).ToArray()) + ")";
                        throw new InvalidOperationException($"A parameterless default constructor or one matching signature {proposedTypes} is required for {type.FullName} materialization");
                    }

                    if (ctor.GetParameters().Length == 0)
                    {
                        il.Emit(OpCodes.Newobj, ctor);
                        il.Emit(OpCodes.Stloc_1);
#if !NETSTANDARD1_3
                        supportInitialize = typeof(ISupportInitialize).IsAssignableFrom(type);
                        if (supportInitialize)
                        {
                            il.Emit(OpCodes.Ldloc_1);
                            il.EmitCall(OpCodes.Callvirt, typeof(ISupportInitialize).GetMethod(nameof(ISupportInitialize.BeginInit)), null);
                        }
#endif
                    }
                    else
                    {
                        specializedConstructor = ctor;
                    }
                }
            }

            il.BeginExceptionBlock();
            if (type.IsValueType())
            {
                il.Emit(OpCodes.Ldloca_S, (byte)1);// [target]
            }
            else if (specializedConstructor == null)
            {
                il.Emit(OpCodes.Ldloc_1);// [target]
            }

            var members = IsValueTuple(type) ? GetValueTupleMembers(type, names) : ((specializedConstructor != null
                ? names.Select(n => typeMap.GetConstructorParameter(specializedConstructor, n))
                : names.Select(n => typeMap.GetMember(n))).ToList());

            // stack is now [target]

            bool first = true;
            var allDone = il.DefineLabel();
            int enumDeclareLocal = -1, valueCopyLocal = il.DeclareLocal(typeof(object)).LocalIndex;
            bool applyNullSetting = Settings.ApplyNullValues;
            foreach (var item in members)
            {
                if (item != null)
                {
                    if (specializedConstructor == null)
                        il.Emit(OpCodes.Dup); // stack is now [target][target]
                    Label isDbNullLabel = il.DefineLabel();
                    Label finishLabel = il.DefineLabel();

                    il.Emit(OpCodes.Ldarg_0); // stack is now [target][target][reader]
                    EmitInt32(il, index); // stack is now [target][target][reader][index]
                    il.Emit(OpCodes.Dup);// stack is now [target][target][reader][index][index]
                    il.Emit(OpCodes.Stloc_0);// stack is now [target][target][reader][index]
                    il.Emit(OpCodes.Callvirt, getItem); // stack is now [target][target][value-as-object]
                    il.Emit(OpCodes.Dup); // stack is now [target][target][value-as-object][value-as-object]
                    StoreLocal(il, valueCopyLocal);
                    Type colType = reader.GetFieldType(index);
                    Type memberType = item.MemberType;

                    if (memberType == typeof(char) || memberType == typeof(char?))
                    {
                        il.EmitCall(OpCodes.Call, typeof(SqlMapper).GetMethod(
                            memberType == typeof(char) ? nameof(SqlMapper.ReadChar) : nameof(SqlMapper.ReadNullableChar), BindingFlags.Static | BindingFlags.Public), null); // stack is now [target][target][typed-value]
                    }
                    else
                    {
                        il.Emit(OpCodes.Dup); // stack is now [target][target][value][value]
                        il.Emit(OpCodes.Isinst, typeof(DBNull)); // stack is now [target][target][value-as-object][DBNull or null]
                        il.Emit(OpCodes.Brtrue_S, isDbNullLabel); // stack is now [target][target][value-as-object]

                        // unbox nullable enums as the primitive, i.e. byte etc

                        var nullUnderlyingType = Nullable.GetUnderlyingType(memberType);
                        var unboxType = nullUnderlyingType?.IsEnum() == true ? nullUnderlyingType : memberType;

                        if (unboxType.IsEnum())
                        {
                            Type numericType = Enum.GetUnderlyingType(unboxType);
                            if (colType == typeof(string))
                            {
                                if (enumDeclareLocal == -1)
                                {
                                    enumDeclareLocal = il.DeclareLocal(typeof(string)).LocalIndex;
                                }
                                il.Emit(OpCodes.Castclass, typeof(string)); // stack is now [target][target][string]
                                StoreLocal(il, enumDeclareLocal); // stack is now [target][target]
                                il.Emit(OpCodes.Ldtoken, unboxType); // stack is now [target][target][enum-type-token]
                                il.EmitCall(OpCodes.Call, typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle)), null);// stack is now [target][target][enum-type]
                                LoadLocal(il, enumDeclareLocal); // stack is now [target][target][enum-type][string]
                                il.Emit(OpCodes.Ldc_I4_1); // stack is now [target][target][enum-type][string][true]
                                il.EmitCall(OpCodes.Call, enumParse, null); // stack is now [target][target][enum-as-object]
                                il.Emit(OpCodes.Unbox_Any, unboxType); // stack is now [target][target][typed-value]
                            }
                            else
                            {
                                FlexibleConvertBoxedFromHeadOfStack(il, colType, unboxType, numericType);
                            }

                            if (nullUnderlyingType != null)
                            {
                                il.Emit(OpCodes.Newobj, memberType.GetConstructor(new[] { nullUnderlyingType })); // stack is now [target][target][typed-value]
                            }
                        }
                        else if (memberType.FullName == LinqBinary)
                        {
                            il.Emit(OpCodes.Unbox_Any, typeof(byte[])); // stack is now [target][target][byte-array]
                            il.Emit(OpCodes.Newobj, memberType.GetConstructor(new Type[] { typeof(byte[]) }));// stack is now [target][target][binary]
                        }
                        else
                        {
                            TypeCode dataTypeCode = TypeExtensions.GetTypeCode(colType), unboxTypeCode = TypeExtensions.GetTypeCode(unboxType);
                            bool hasTypeHandler;
                            if ((hasTypeHandler = typeHandlers.ContainsKey(unboxType)) || colType == unboxType || dataTypeCode == unboxTypeCode || dataTypeCode == TypeExtensions.GetTypeCode(nullUnderlyingType))
                            {
                                if (hasTypeHandler)
                                {
#pragma warning disable 618
                                    il.EmitCall(OpCodes.Call, typeof(TypeHandlerCache<>).MakeGenericType(unboxType).GetMethod(nameof(TypeHandlerCache<int>.Parse)), null); // stack is now [target][target][typed-value]
#pragma warning restore 618
                                }
                                else
                                {
                                    il.Emit(OpCodes.Unbox_Any, unboxType); // stack is now [target][target][typed-value]
                                }
                            }
                            else
                            {
                                // not a direct match; need to tweak the unbox
                                FlexibleConvertBoxedFromHeadOfStack(il, colType, nullUnderlyingType ?? unboxType, null);
                                if (nullUnderlyingType != null)
                                {
                                    il.Emit(OpCodes.Newobj, unboxType.GetConstructor(new[] { nullUnderlyingType })); // stack is now [target][target][typed-value]
                                }
                            }
                        }
                    }
                    if (specializedConstructor == null)
                    {
                        // Store the value in the property/field
                        if (item.Property != null)
                        {
                            il.Emit(type.IsValueType() ? OpCodes.Call : OpCodes.Callvirt, DefaultTypeMap.GetPropertySetter(item.Property, type));
                        }
                        else
                        {
                            il.Emit(OpCodes.Stfld, item.Field); // stack is now [target]
                        }
                    }

                    il.Emit(OpCodes.Br_S, finishLabel); // stack is now [target]

                    il.MarkLabel(isDbNullLabel); // incoming stack: [target][target][value]
                    if (specializedConstructor != null)
                    {
                        il.Emit(OpCodes.Pop);
                        if (item.MemberType.IsValueType())
                        {
                            int localIndex = il.DeclareLocal(item.MemberType).LocalIndex;
                            LoadLocalAddress(il, localIndex);
                            il.Emit(OpCodes.Initobj, item.MemberType);
                            LoadLocal(il, localIndex);
                        }
                        else
                        {
                            il.Emit(OpCodes.Ldnull);
                        }
                    }
                    else if (applyNullSetting && (!memberType.IsValueType() || Nullable.GetUnderlyingType(memberType) != null))
                    {
                        il.Emit(OpCodes.Pop); // stack is now [target][target]
                        // can load a null with this value
                        if (memberType.IsValueType())
                        { // must be Nullable<T> for some T
                            GetTempLocal(il, ref structLocals, memberType, true); // stack is now [target][target][null]
                        }
                        else
                        { // regular reference-type
                            il.Emit(OpCodes.Ldnull); // stack is now [target][target][null]
                        }

                        // Store the value in the property/field
                        if (item.Property != null)
                        {
                            il.Emit(type.IsValueType() ? OpCodes.Call : OpCodes.Callvirt, DefaultTypeMap.GetPropertySetter(item.Property, type));
                            // stack is now [target]
                        }
                        else
                        {
                            il.Emit(OpCodes.Stfld, item.Field); // stack is now [target]
                        }
                    }
                    else
                    {
                        il.Emit(OpCodes.Pop); // stack is now [target][target]
                        il.Emit(OpCodes.Pop); // stack is now [target]
                    }

                    if (first && returnNullIfFirstMissing)
                    {
                        il.Emit(OpCodes.Pop);
                        il.Emit(OpCodes.Ldnull); // stack is now [null]
                        il.Emit(OpCodes.Stloc_1);
                        il.Emit(OpCodes.Br, allDone);
                    }

                    il.MarkLabel(finishLabel);
                }
                first = false;
                index++;
            }
            if (type.IsValueType())
            {
                il.Emit(OpCodes.Pop);
            }
            else
            {
                if (specializedConstructor != null)
                {
                    il.Emit(OpCodes.Newobj, specializedConstructor);
                }
                il.Emit(OpCodes.Stloc_1); // stack is empty
#if !NETSTANDARD1_3
                if (supportInitialize)
                {
                    il.Emit(OpCodes.Ldloc_1);
                    il.EmitCall(OpCodes.Callvirt, typeof(ISupportInitialize).GetMethod(nameof(ISupportInitialize.EndInit)), null);
                }
#endif
            }
            il.MarkLabel(allDone);
            il.BeginCatchBlock(typeof(Exception)); // stack is Exception
            il.Emit(OpCodes.Ldloc_0); // stack is Exception, index
            il.Emit(OpCodes.Ldarg_0); // stack is Exception, index, reader
            LoadLocal(il, valueCopyLocal); // stack is Exception, index, reader, value
            il.EmitCall(OpCodes.Call, typeof(SqlMapper).GetMethod(nameof(SqlMapper.ThrowDataException)), null);
            il.EndExceptionBlock();

            il.Emit(OpCodes.Ldloc_1); // stack is [rval]
            if (type.IsValueType())
            {
                il.Emit(OpCodes.Box, type);
            }
            il.Emit(OpCodes.Ret);

            var funcType = System.Linq.Expressions.Expression.GetFuncType(typeof(IDataReader), returnType);
            return (Func<IDataReader, object>)dm.CreateDelegate(funcType);
        }

        private static void FlexibleConvertBoxedFromHeadOfStack(ILGenerator il, Type from, Type to, Type via)
        {
            MethodInfo op;
            if (from == (via ?? to))
            {
                il.Emit(OpCodes.Unbox_Any, to); // stack is now [target][target][typed-value]
            }
            else if ((op = GetOperator(from, to)) != null)
            {
                // this is handy for things like decimal <===> double
                il.Emit(OpCodes.Unbox_Any, from); // stack is now [target][target][data-typed-value]
                il.Emit(OpCodes.Call, op); // stack is now [target][target][typed-value]
            }
            else
            {
                bool handled = false;
                OpCode opCode = default(OpCode);
                switch (TypeExtensions.GetTypeCode(from))
                {
                    case TypeCode.Boolean:
                    case TypeCode.Byte:
                    case TypeCode.SByte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                    case TypeCode.Single:
                    case TypeCode.Double:
                        handled = true;
                        switch (TypeExtensions.GetTypeCode(via ?? to))
                        {
                            case TypeCode.Byte:
                                opCode = OpCodes.Conv_Ovf_I1_Un; break;
                            case TypeCode.SByte:
                                opCode = OpCodes.Conv_Ovf_I1; break;
                            case TypeCode.UInt16:
                                opCode = OpCodes.Conv_Ovf_I2_Un; break;
                            case TypeCode.Int16:
                                opCode = OpCodes.Conv_Ovf_I2; break;
                            case TypeCode.UInt32:
                                opCode = OpCodes.Conv_Ovf_I4_Un; break;
                            case TypeCode.Boolean: // boolean is basically an int, at least at this level
                            case TypeCode.Int32:
                                opCode = OpCodes.Conv_Ovf_I4; break;
                            case TypeCode.UInt64:
                                opCode = OpCodes.Conv_Ovf_I8_Un; break;
                            case TypeCode.Int64:
                                opCode = OpCodes.Conv_Ovf_I8; break;
                            case TypeCode.Single:
                                opCode = OpCodes.Conv_R4; break;
                            case TypeCode.Double:
                                opCode = OpCodes.Conv_R8; break;
                            default:
                                handled = false;
                                break;
                        }
                        break;
                }
                if (handled)
                {
                    il.Emit(OpCodes.Unbox_Any, from); // stack is now [target][target][col-typed-value]
                    il.Emit(opCode); // stack is now [target][target][typed-value]
                    if (to == typeof(bool))
                    { // compare to zero; I checked "csc" - this is the trick it uses; nice
                        il.Emit(OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Ceq);
                        il.Emit(OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Ceq);
                    }
                }
                else
                {
                    il.Emit(OpCodes.Ldtoken, via ?? to); // stack is now [target][target][value][member-type-token]
                    il.EmitCall(OpCodes.Call, typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle)), null); // stack is now [target][target][value][member-type]
                    il.EmitCall(OpCodes.Call, typeof(Convert).GetMethod(nameof(Convert.ChangeType), new Type[] { typeof(object), typeof(Type) }), null); // stack is now [target][target][boxed-member-type-value]
                    il.Emit(OpCodes.Unbox_Any, to); // stack is now [target][target][typed-value]
                }
            }
        }

        private static MethodInfo GetOperator(Type from, Type to)
        {
            if (to == null) return null;
            MethodInfo[] fromMethods, toMethods;
            return ResolveOperator(fromMethods = from.GetMethods(BindingFlags.Static | BindingFlags.Public), from, to, "op_Implicit")
                ?? ResolveOperator(toMethods = to.GetMethods(BindingFlags.Static | BindingFlags.Public), from, to, "op_Implicit")
                ?? ResolveOperator(fromMethods, from, to, "op_Explicit")
                ?? ResolveOperator(toMethods, from, to, "op_Explicit");
        }

        private static MethodInfo ResolveOperator(MethodInfo[] methods, Type from, Type to, string name)
        {
            for (int i = 0; i < methods.Length; i++)
            {
                if (methods[i].Name != name || methods[i].ReturnType != to) continue;
                var args = methods[i].GetParameters();
                if (args.Length != 1 || args[0].ParameterType != from) continue;
                return methods[i];
            }
            return null;
        }

        private static void LoadLocal(ILGenerator il, int index)
        {
            if (index < 0 || index >= short.MaxValue) throw new ArgumentNullException(nameof(index));
            switch (index)
            {
                case 0: il.Emit(OpCodes.Ldloc_0); break;
                case 1: il.Emit(OpCodes.Ldloc_1); break;
                case 2: il.Emit(OpCodes.Ldloc_2); break;
                case 3: il.Emit(OpCodes.Ldloc_3); break;
                default:
                    if (index <= 255)
                    {
                        il.Emit(OpCodes.Ldloc_S, (byte)index);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldloc, (short)index);
                    }
                    break;
            }
        }

        private static void StoreLocal(ILGenerator il, int index)
        {
            if (index < 0 || index >= short.MaxValue) throw new ArgumentNullException(nameof(index));
            switch (index)
            {
                case 0: il.Emit(OpCodes.Stloc_0); break;
                case 1: il.Emit(OpCodes.Stloc_1); break;
                case 2: il.Emit(OpCodes.Stloc_2); break;
                case 3: il.Emit(OpCodes.Stloc_3); break;
                default:
                    if (index <= 255)
                    {
                        il.Emit(OpCodes.Stloc_S, (byte)index);
                    }
                    else
                    {
                        il.Emit(OpCodes.Stloc, (short)index);
                    }
                    break;
            }
        }

        private static void LoadLocalAddress(ILGenerator il, int index)
        {
            if (index < 0 || index >= short.MaxValue) throw new ArgumentNullException(nameof(index));

            if (index <= 255)
            {
                il.Emit(OpCodes.Ldloca_S, (byte)index);
            }
            else
            {
                il.Emit(OpCodes.Ldloca, (short)index);
            }
        }

        /// <summary>
        /// Throws a data exception, only used internally
        /// </summary>
        /// <param name="ex">The exception to throw.</param>
        /// <param name="index">The index the exception occured at.</param>
        /// <param name="reader">The reader the exception occured in.</param>
        /// <param name="value">The value that caused the exception.</param>
        [Obsolete(ObsoleteInternalUsageOnly, false)]
        public static void ThrowDataException(Exception ex, int index, IDataReader reader, object value)
        {
            Exception toThrow;
            try
            {
                string name = "(n/a)", formattedValue = "(n/a)";
                if (reader != null && index >= 0 && index < reader.FieldCount)
                {
                    name = reader.GetName(index);
                    try
                    {
                        if (value == null || value is DBNull)
                        {
                            formattedValue = "<null>";
                        }
                        else
                        {
                            formattedValue = Convert.ToString(value) + " - " + TypeExtensions.GetTypeCode(value.GetType());
                        }
                    }
                    catch (Exception valEx)
                    {
                        formattedValue = valEx.Message;
                    }
                }
                toThrow = new DataException($"Error parsing column {index} ({name}={formattedValue})", ex);
            }
            catch
            { // throw the **original** exception, wrapped as DataException
                toThrow = new DataException(ex.Message, ex);
            }
            throw toThrow;
        }

        private static void EmitInt32(ILGenerator il, int value)
        {
            switch (value)
            {
                case -1: il.Emit(OpCodes.Ldc_I4_M1); break;
                case 0: il.Emit(OpCodes.Ldc_I4_0); break;
                case 1: il.Emit(OpCodes.Ldc_I4_1); break;
                case 2: il.Emit(OpCodes.Ldc_I4_2); break;
                case 3: il.Emit(OpCodes.Ldc_I4_3); break;
                case 4: il.Emit(OpCodes.Ldc_I4_4); break;
                case 5: il.Emit(OpCodes.Ldc_I4_5); break;
                case 6: il.Emit(OpCodes.Ldc_I4_6); break;
                case 7: il.Emit(OpCodes.Ldc_I4_7); break;
                case 8: il.Emit(OpCodes.Ldc_I4_8); break;
                default:
                    if (value >= -128 && value <= 127)
                    {
                        il.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldc_I4, value);
                    }
                    break;
            }
        }

        /// <summary>
        /// How should connection strings be compared for equivalence? Defaults to StringComparer.Ordinal.
        /// Providing a custom implementation can be useful for allowing multi-tenancy databases with identical
        /// schema to share strategies. Note that usual equivalence rules apply: any equivalent connection strings
        /// <b>MUST</b> yield the same hash-code.
        /// </summary>
        public static IEqualityComparer<string> ConnectionStringComparer
        {
            get { return connectionStringComparer; }
            set { connectionStringComparer = value ?? StringComparer.Ordinal; }
        }

        private static IEqualityComparer<string> connectionStringComparer = StringComparer.Ordinal;

#if !NETSTANDARD1_3
        /// <summary>
        /// Key used to indicate the type name associated with a DataTable.
        /// </summary>
        private const string DataTableTypeNameKey = "mapper:TypeName";

        /// <summary>
        /// Associate a DataTable with a type name.
        /// </summary>
        /// <param name="table">The <see cref="DataTable"/> that does with the <paramref name="typeName"/>.</param>
        /// <param name="typeName">The name of the type this table is for.</param>
        public static void SetTypeName(this DataTable table, string typeName)
        {
            if (table != null)
            {
                if (string.IsNullOrEmpty(typeName))
                    table.ExtendedProperties.Remove(DataTableTypeNameKey);
                else
                    table.ExtendedProperties[DataTableTypeNameKey] = typeName;
            }
        }

        /// <summary>
        /// Fetch the type name associated with a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="table">The <see cref="DataTable"/> that has a type name associated with it.</param>
        public static string GetTypeName(this DataTable table) =>
            table?.ExtendedProperties[DataTableTypeNameKey] as string;
#endif



        // one per thread
        [ThreadStatic]
        private static StringBuilder perThreadStringBuilderCache;
        private static StringBuilder GetStringBuilder()
        {
            var tmp = perThreadStringBuilderCache;
            if (tmp != null)
            {
                perThreadStringBuilderCache = null;
                tmp.Length = 0;
                return tmp;
            }
            return new StringBuilder();
        }

        private static string __ToStringRecycle(this StringBuilder obj)
        {
            if (obj == null) return "";
            var s = obj.ToString();
            perThreadStringBuilderCache = perThreadStringBuilderCache ?? obj;
            return s;
        }
    }
}