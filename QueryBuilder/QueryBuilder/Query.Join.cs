namespace SqlKata
{
    public partial class QueryBuilder
    {
        private QueryBuilder JoinBuilder(Func<JoinBuilder, JoinBuilder> callback)
        {
            var join = callback.Invoke(new JoinBuilder(new QueryBuilder()).AsInner());

            return AddComponent(new BaseJoinBuilder
            {
                    Engine = EngineScope,
                Component = "join", 
                Join = join
            });
        }

        public QueryBuilder JoinBuilder(
            string table,
            string first,
            string second,
            string op = "=",
            string type = "inner join"
        )
        {
            return JoinBuilder(j =>
            {
                var join = new JoinBuilder(j.JoinWith(table).BaseQuery);
                join.BaseQuery.WhereColumns(first, op, second);
                return join.AsType(type);
            });
        }

        public QueryBuilder JoinBuilder(string table, Func<JoinBuilder, JoinBuilder> callback, string type = "inner join")
        {
            return JoinBuilder(j =>
            {
                var join = new JoinBuilder(j.JoinWith(table).BaseQuery);
                join.BaseQuery.Where(q => callback(new JoinBuilder(q)).BaseQuery);
                return join.AsType(type);
            });
        }

        public QueryBuilder JoinBuilder(QueryBuilder query, Func<JoinBuilder, JoinBuilder> onCallback, string type = "inner join")
        {
            return JoinBuilder(j =>
            {
                var join = new JoinBuilder(j.JoinWith(query).BaseQuery);
                join.BaseQuery.Where(q => onCallback(new JoinBuilder(q)).BaseQuery);
                return join.AsType(type);
            });
        }

        public QueryBuilder LeftJoin(string table, string first, string second, string op = "=")
        {
            return JoinBuilder(table, first, second, op, "left join");
        }

        public QueryBuilder LeftJoin(string table, Func<JoinBuilder, JoinBuilder> callback)
        {
            return JoinBuilder(table, callback, "left join");
        }

        public QueryBuilder LeftJoin(QueryBuilder query, Func<JoinBuilder, JoinBuilder> onCallback)
        {
            return JoinBuilder(query, onCallback, "left join");
        }

        public QueryBuilder RightJoin(string table, string first, string second, string op = "=")
        {
            return JoinBuilder(table, first, second, op, "right join");
        }

        public QueryBuilder RightJoin(string table, Func<JoinBuilder, JoinBuilder> callback)
        {
            return JoinBuilder(table, callback, "right join");
        }

        public QueryBuilder RightJoin(QueryBuilder query, Func<JoinBuilder, JoinBuilder> onCallback)
        {
            return JoinBuilder(query, onCallback, "right join");
        }

        public QueryBuilder CrossJoin(string table)
        {
            return JoinBuilder(j => j.JoinWith(table).AsCross());
        }
    }
}
