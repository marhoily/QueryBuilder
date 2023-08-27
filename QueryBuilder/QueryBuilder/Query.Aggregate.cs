using System.Collections.Immutable;

namespace SqlKata
{
    public partial class QueryBuilder
    {
        public QueryBuilder AsAggregate(string type, string[]? columns = null)
        {
            Method = "aggregate";

            RemoveComponent("aggregate")
                .AddComponent(new AggregateClauseBuilder
                {
                    Engine = EngineScope,
                    Component = "aggregate",
                    Type = type,
                    Columns = columns?.ToImmutableArray() ?? ImmutableArray<string>.Empty
                });

            return this;
        }

        public QueryBuilder AsCount(string[]? columns = null)
        {
            var cols = columns?.ToList() ?? new List<string>();

            if (!cols.Any()) cols.Add("*");

            return AsAggregate("count", cols.ToArray());
        }

        public QueryBuilder AsAvg(string column)
        {
            return AsAggregate("avg", new[] { column });
        }

        public QueryBuilder AsAverage(string column)
        {
            return AsAvg(column);
        }

        public QueryBuilder AsSum(string column)
        {
            return AsAggregate("sum", new[] { column });
        }

        public QueryBuilder AsMax(string column)
        {
            return AsAggregate("max", new[] { column });
        }

        public QueryBuilder AsMin(string column)
        {
            return AsAggregate("min", new[] { column });
        }
    }
}
