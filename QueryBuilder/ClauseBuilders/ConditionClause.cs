using System.Collections.Immutable;

namespace SqlKata
{
    public abstract class AbstractConditionBuilder : AbstractClauseBuilder
    {
        public required bool IsOr { get; init; }
        public required bool IsNot { get; init; }
    }

    /// <summary>
    ///     Represents a comparison between a column and a value.
    /// </summary>
    public class BasicConditionBuilder : AbstractConditionBuilder
    {
        public required string Column { get; init; }
        public required string Operator { get; init; }
        public required object Value { get; init; }
    }

    public class BasicStringConditionBuilder : BasicConditionBuilder
    {
        public required bool CaseSensitive { get; init; }

        private readonly string? _escapeCharacter;
        public required string? EscapeCharacter
        {
            get => _escapeCharacter;
            init
            {
                if (string.IsNullOrWhiteSpace(value))
                    value = null;
                else if (value.Length > 1)
                    throw new ArgumentOutOfRangeException(
                        $"The {nameof(EscapeCharacter)} can only contain a single character!");
                _escapeCharacter = value;
            }
        }
    }

    public sealed class BasicDateConditionBuilder : BasicConditionBuilder
    {
        public required string Part { get; init; }
    }

    /// <summary>
    ///     Represents a comparison between two columns.
    /// </summary>
    public sealed class TwoColumnsConditionBuilder : AbstractConditionBuilder
    {
        public required string First { get; init; }
        public required string Operator { get; init; }
        public required string Second { get; init; }
    }

    /// <summary>
    ///     Represents a comparison between a column and a full "subQuery".
    /// </summary>
    public sealed class QueryConditionBuilder : AbstractConditionBuilder
    {
        public required string Column { get; init; }
        public required string Operator { get; init; }
        public required QueryBuilder Query { get; init; }
    }

    /// <summary>
    ///     Represents a comparison between a full "subQuery" and a value.
    /// </summary>
    public class SubQueryConditionBuilder : AbstractConditionBuilder
    {
        public required object Value { get; init; }
        public required string Operator { get; init; }
        public required QueryBuilder Query { get; init; }
    }

    /// <summary>
    ///     Represents a "is in" condition.
    /// </summary>
    public class InConditionBuilder<T> : AbstractConditionBuilder
    {
        public required string Column { get; init; }
        public required ImmutableArray<T> Values { get; init; }
    }

    /// <summary>
    ///     Represents a "is in subQuery" condition.
    /// </summary>
    public class InQueryConditionBuilder : AbstractConditionBuilder
    {
        public required QueryBuilder Query { get; init; }
        public required string Column { get; init; }
    }

    /// <summary>
    ///     Represents a "is between" condition.
    /// </summary>
    public class BetweenConditionBuilder<T> : AbstractConditionBuilder
    {
        public required string Column { get; init; }
        public required T Higher { get; init; }
        public required T Lower { get; init; }
    }

    /// <summary>
    ///     Represents an "is null" condition.
    /// </summary>
    public class NullConditionBuilder : AbstractConditionBuilder
    {
        public required string Column { get; init; }
    }

    /// <summary>
    ///     Represents a boolean (true/false) condition.
    /// </summary>
    public class BooleanConditionBuilder : AbstractConditionBuilder
    {
        public required string Column { get; init; }
        public required bool Value { get; init; }
    }

    /// <summary>
    ///     Represents a "nested" clause condition.
    ///     i.e OR (myColumn = "A")
    /// </summary>
    public class NestedConditionBuilder : AbstractConditionBuilder
    {
        public required QueryBuilder Query { get; init; }
    }

    /// <summary>
    ///     Represents an "exists sub query" clause condition.
    /// </summary>
    public class ExistsConditionBuilder : AbstractConditionBuilder
    {
        public required QueryBuilder Query { get; init; }
    }

    public class RawConditionBuilder : AbstractConditionBuilder
    {
        public required string Expression { get; init; }
        public required object[] Bindings { get; init; }
    }
}
