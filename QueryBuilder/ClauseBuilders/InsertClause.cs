using System.Collections.Immutable;

namespace SqlKata
{
    public abstract class AbstractInsertClauseBuilder : AbstractClauseBuilder
    {
    }

    public class InsertClauseBuilder : AbstractInsertClauseBuilder
    {
        public required ImmutableArray<string> Columns { get; init; }
        public required ImmutableArray<object?> Values { get; init; }
        public required bool ReturnId { get; init; }
    }

    public sealed class InsertQueryClauseBuilder : AbstractInsertClauseBuilder
    {
        public required ImmutableArray<string> Columns { get; init; }
        public required QueryBuilder Query { get; init; }
    }
}
