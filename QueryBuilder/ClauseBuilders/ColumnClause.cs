using System.Collections.Immutable;

namespace SqlKata
{
    public abstract class AbstractColumnBuilder : AbstractClauseBuilder
    {
    }

    /// <summary>
    ///     Represents "column" or "column as alias" clause.
    /// </summary>
    /// <seealso cref="AbstractColumnBuilder" />
    public sealed class ColumnBuilder : AbstractColumnBuilder
    {
        /// <summary>
        ///     Gets or sets the column name. Can be "columnName" or "columnName as columnAlias".
        /// </summary>
        /// <value>
        ///     The column name.
        /// </value>
        public required string Name { get; init; }
    }

    /// <summary>
    ///     Represents column clause calculated using query.
    /// </summary>
    /// <seealso cref="AbstractColumnBuilder" />
    public sealed class QueryColumnBuilder : AbstractColumnBuilder
    {
        /// <summary>
        ///     Gets or sets the query that will be used for column value calculation.
        /// </summary>
        /// <value>
        ///     The query for column value calculation.
        /// </value>
        public required QueryBuilder Query { get; init; }
    }

    public sealed class RawColumnBuilder : AbstractColumnBuilder
    {
        /// <summary>
        ///     Gets or sets the RAW expression.
        /// </summary>
        /// <value>
        ///     The RAW expression.
        /// </value>
        public required string Expression { get; init; }

        public required ImmutableArray<object?> Bindings { get; init; }
    }

    /// <summary>
    ///     Represents an aggregated column clause with an optional filter
    /// </summary>
    /// <seealso cref="AbstractColumnBuilder" />
    public sealed class AggregatedColumnBuilder : AbstractColumnBuilder
    {
        /// <summary>
        ///     Gets or sets the a query that used to filter the data,
        ///     the compiler will consider only the `Where` clause.
        /// </summary>
        /// <value>
        ///     The filter query.
        /// </value>
        public required QueryBuilder? Filter { get; init; }

        public required string Aggregate { get; init; }
        public required AbstractColumnBuilder Column { get; init; }
    }
}
