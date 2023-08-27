using System.Collections.Immutable;

namespace SqlKata
{
    public abstract class AbstractFromBuilder : AbstractClauseBuilder
    {
        protected string? AliasField;

        /// <summary>
        ///     Try to extract the Alias for the current clause.
        /// </summary>
        /// <returns></returns>
        public virtual string? Alias
        {
            get => AliasField;
            set => AliasField = value;
        }
    }

    /// <summary>
    ///     Represents a "from" clause.
    /// </summary>
    public sealed class FromClauseBuilder : AbstractFromBuilder
    {
        public required string Table { get; init; }

        public override string Alias
        {
            get
            {
                var segments = Table.Split(" as ");
                return segments.Length > 1 ? segments[1] : Table;
            }
        }
    }

    /// <summary>
    ///     Represents a "from subQuery" clause.
    /// </summary>
    public sealed class QueryFromClauseBuilder : AbstractFromBuilder
    {
        public required QueryBuilder Query { get; init; }

        public override string? Alias => string.IsNullOrEmpty(AliasField)
            ? Query.QueryAlias : AliasField;
    }

    public sealed class RawFromClauseBuilder : AbstractFromBuilder
    {
        public required string Expression { get; init; }
        public required ImmutableArray<object> Bindings { get; init; }
    }

    /// <summary>
    ///     Represents a FROM clause that is an ad-hoc table built with predefined values.
    /// </summary>
    public sealed class AdHocTableFromClauseBuilder : AbstractFromBuilder
    {
        public ImmutableArray<string> Columns { get; init; }
        public ImmutableArray<object?> Values { get; init; }
    }
}
