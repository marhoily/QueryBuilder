using System.Collections.Immutable;

namespace SqlKata
{
    public partial class QueryBuilder
    {
        public QueryBuilder Combine(string operation, bool all, QueryBuilder query)
        {
            if (Method != "select" || query.Method != "select")
                throw new InvalidOperationException("Only select queries can be combined.");

            return AddComponent(new CombineBuilder
            {
                Engine = EngineScope,
                Component = "combine",
                Query = query,
                Operation = operation,
                All = all
            });
        }

        public QueryBuilder CombineRaw(string sql, params object[] bindings)
        {
            if (Method != "select") throw new InvalidOperationException("Only select queries can be combined.");

            return AddComponent(new RawCombineBuilder
            {
                Engine = EngineScope,  
                Component = "combine",
                Expression = sql,
                Bindings = bindings.ToImmutableArray()
            });
        }

        public QueryBuilder Union(QueryBuilder query, bool all = false)
        {
            return Combine("union", all, query);
        }

        public QueryBuilder UnionAll(QueryBuilder query)
        {
            return Union(query, true);
        }

        public QueryBuilder Union(Func<QueryBuilder, QueryBuilder> callback, bool all = false)
        {
            var query = callback.Invoke(new QueryBuilder());
            return Union(query, all);
        }

        public QueryBuilder UnionAll(Func<QueryBuilder, QueryBuilder> callback)
        {
            return Union(callback, true);
        }

        public QueryBuilder UnionRaw(string sql, params object[] bindings)
        {
            return CombineRaw(sql, bindings);
        }

        public QueryBuilder Except(QueryBuilder query, bool all = false)
        {
            return Combine("except", all, query);
        }

        public QueryBuilder ExceptAll(QueryBuilder query)
        {
            return Except(query, true);
        }

        public QueryBuilder Except(Func<QueryBuilder, QueryBuilder> callback, bool all = false)
        {
            var query = callback.Invoke(new QueryBuilder());
            return Except(query, all);
        }

        public QueryBuilder ExceptAll(Func<QueryBuilder, QueryBuilder> callback)
        {
            return Except(callback, true);
        }

        public QueryBuilder ExceptRaw(string sql, params object[] bindings)
        {
            return CombineRaw(sql, bindings);
        }

        public QueryBuilder Intersect(QueryBuilder query, bool all = false)
        {
            return Combine("intersect", all, query);
        }

        public QueryBuilder IntersectAll(QueryBuilder query)
        {
            return Intersect(query, true);
        }

        public QueryBuilder Intersect(Func<QueryBuilder, QueryBuilder> callback, bool all = false)
        {
            var query = callback.Invoke(new QueryBuilder());
            return Intersect(query, all);
        }

        public QueryBuilder IntersectAll(Func<QueryBuilder, QueryBuilder> callback)
        {
            return Intersect(callback, true);
        }

        public QueryBuilder IntersectRaw(string sql, params object[] bindings)
        {
            return CombineRaw(sql, bindings);
        }
    }
}
