namespace SqlKata
{
    public sealed class JoinBuilder 
    {
        public QueryBuilder BaseQuery { get; }

        public JoinBuilder(QueryBuilder baseQuery)
        {
            BaseQuery = baseQuery;
        }

        public string? Type { get; private set; }

        public JoinBuilder Clone() => new(BaseQuery) { Type = Type };

        public JoinBuilder AsType(string type)
        {
            ArgumentNullException.ThrowIfNull(type);
            Type = type.ToUpperInvariant();
            return this;
        }

        /// <summary>
        ///     Alias for "from" operator.
        ///     Since "from" does not sound well with JoinBuilder clauses
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public JoinBuilder JoinWith(string table)
        {
            return new JoinBuilder(BaseQuery.From(table));
        }

        public JoinBuilder JoinWith(QueryBuilder QueryBuilder)
        {
            return new JoinBuilder(BaseQuery.From(QueryBuilder));
        }

        public JoinBuilder JoinWith(Func<QueryBuilder, QueryBuilder> callback)
        {
            return new JoinBuilder(BaseQuery.From(callback));
        }

        public JoinBuilder AsInner()
        {
            return AsType("inner JoinBuilder");
        }

        public JoinBuilder AsOuter()
        {
            return AsType("outer JoinBuilder");
        }

        public JoinBuilder AsLeft()
        {
            return AsType("left JoinBuilder");
        }

        public JoinBuilder AsRight()
        {
            return AsType("right JoinBuilder");
        }

        public JoinBuilder AsCross()
        {
            return AsType("cross JoinBuilder");
        }

        public JoinBuilder On(string first, string second, string op = "=")
        {
            return new JoinBuilder(BaseQuery.AddComponent(new TwoColumnsConditionBuilder
            {
                Engine = BaseQuery.EngineScope,
                Component = "where",
                First = first,
                Second = second,
                Operator = op,
                IsOr = BaseQuery.GetOr(),
                IsNot = BaseQuery.GetNot()
            }));
        }

        public JoinBuilder OrOn(string first, string second, string op = "=")
        {
            return new JoinBuilder(BaseQuery.Or()).On(first, second, op);
        }

    }
}
