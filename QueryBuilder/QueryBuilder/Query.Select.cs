using System.Collections.Immutable;

namespace SqlKata
{
    public partial class QueryBuilder
    {
        public QueryBuilder Select(params string[] columns)
        {
            return Select(columns.AsEnumerable());
        }

        public QueryBuilder Select(IEnumerable<string> columns)
        {
            Method = "select";

            columns = columns
                .Select(Helper.ExpandExpression)
                .SelectMany(x => x)
                .ToArray();


            foreach (var column in columns)
                AddComponent(new ColumnBuilder
                {
                    Engine = EngineScope,
                    Component = "select",
                    Name = column
                });

            return this;
        }

        /// <summary>
        ///     Add a new "raw" select expression to the query.
        /// </summary>
        /// <returns></returns>
        public QueryBuilder SelectRaw(string sql, params object?[] bindings)
        {
            Method = "select";

            AddComponent(new RawColumnBuilder
            {
                Engine = EngineScope,
                Component = "select",
                Expression = sql,
                Bindings = bindings.ToImmutableArray()
            });

            return this;
        }

        public QueryBuilder Select(QueryBuilder query, string alias)
        {
            Method = "select";

            query = query.Clone();

            AddComponent(new QueryColumnBuilder
            {
                Engine = EngineScope,
                Component = "select",
                Query = query.As(alias)
            });

            return this;
        }

        public QueryBuilder Select(Func<QueryBuilder, QueryBuilder> callback, string alias)
        {
            return Select(callback.Invoke(NewChild()), alias);
        }

        public QueryBuilder SelectAggregate(string aggregate, string column, QueryBuilder? filter = null)
        {
            Method = "select";

            AddComponent(new AggregatedColumnBuilder
            {
                Engine = EngineScope,
                Component = "select",
                Column = new ColumnBuilder
                {
                    Engine = EngineScope,
                    Component = "select",
                    Name = column
                },
                Aggregate = aggregate,
                Filter = filter
            });

            return this;
        }

        public QueryBuilder SelectAggregate(string aggregate, string column, Func<QueryBuilder, QueryBuilder>? filter)
        {
            if (filter == null) return SelectAggregate(aggregate, column);

            return SelectAggregate(aggregate, column, filter.Invoke(NewChild()));
        }

        public QueryBuilder SelectSum(string column, Func<QueryBuilder, QueryBuilder>? filter = null)
        {
            return SelectAggregate("sum", column, filter);
        }

        public QueryBuilder SelectCount(string column, Func<QueryBuilder, QueryBuilder>? filter = null)
        {
            return SelectAggregate("count", column, filter);
        }

        public QueryBuilder SelectAvg(string column, Func<QueryBuilder, QueryBuilder>? filter = null)
        {
            return SelectAggregate("avg", column, filter);
        }

        public QueryBuilder SelectMin(string column, Func<QueryBuilder, QueryBuilder>? filter = null)
        {
            return SelectAggregate("min", column, filter);
        }

        public QueryBuilder SelectMax(string column, Func<QueryBuilder, QueryBuilder>? filter = null)
        {
            return SelectAggregate("max", column, filter);
        }
    }
}
