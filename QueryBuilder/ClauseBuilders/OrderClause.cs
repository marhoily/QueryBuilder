using System.Collections.Immutable;

namespace SqlKata
{
    public abstract class AbstractOrderByBuilder : AbstractClauseBuilder
    {
    }

    public sealed class OrderByBuilder : AbstractOrderByBuilder
    {
        public required string Column { get; init; }
        public required bool Ascending { get; init; }
    }

    public sealed class RawOrderByBuilder : AbstractOrderByBuilder
    {
        public required string Expression { get; init; }
        public required ImmutableArray<object> Bindings { get; init; }
    }

    public sealed class OrderByRandomBuilder : AbstractOrderByBuilder
    {
    }
}
