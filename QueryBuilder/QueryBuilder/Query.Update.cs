using System.Collections.Immutable;

namespace SqlKata
{
    public partial class QueryBuilder
    {
        public QueryBuilder AsUpdate(object data)
        {
            var dictionary = BuildKeyValuePairsFromObject(data, true);

            return AsUpdate(dictionary);
        }

        public QueryBuilder AsUpdate(IEnumerable<string> columns, IEnumerable<object?> values)
        {
            var columnsCache = columns is ImmutableArray<string> c ? c : columns.ToImmutableArray();
            var valuesCache = values is ImmutableArray<object?> v ? v : values.ToImmutableArray();
            if (!columnsCache.Any() || !valuesCache.Any())
                throw new InvalidOperationException($"{columnsCache} and {valuesCache} cannot be null or empty");

            if (columnsCache.Length != valuesCache.Length)
                throw new InvalidOperationException($"{columnsCache} count should be equal to {valuesCache} count");

            Method = "update";

            RemoveComponent("update").AddComponent(new InsertClauseBuilder
            {
                Engine = EngineScope,
                Component = "update",
                Columns = columnsCache,
                Values = valuesCache,
                ReturnId = false
            });

            return this;
        }

        public QueryBuilder AsUpdate(IEnumerable<KeyValuePair<string, object?>> values)
        {
            var valuesCached = values is IReadOnlyDictionary<string, object?> d
                ? d
                : values.ToDictionary(x => x.Key, x => x.Value);
            if (valuesCached == null || valuesCached.Any() == false)
                throw new InvalidOperationException($"{valuesCached} cannot be null or empty");

            Method = "update";

            RemoveComponent("update").AddComponent(new InsertClauseBuilder
            {
                Engine = EngineScope,
                Component = "update",
                Columns = valuesCached.Select(x => x.Key).ToImmutableArray(),
                Values = valuesCached.Select(x => x.Value).ToImmutableArray(),
                ReturnId = false
            });

            return this;
        }

        public QueryBuilder AsIncrement(string column, int value = 1)
        {
            Method = "update";
            AddOrReplaceComponent(new IncrementClauseBuilder
            {
                Engine = EngineScope,
                Component = "update",
                Column = column,
                Value = value,
                Columns = ImmutableArray<string>.Empty,
                Values = ImmutableArray<object?>.Empty,
                ReturnId = false
            });

            return this;
        }

        public QueryBuilder AsDecrement(string column, int value = 1)
        {
            return AsIncrement(column, -value);
        }
    }
}
