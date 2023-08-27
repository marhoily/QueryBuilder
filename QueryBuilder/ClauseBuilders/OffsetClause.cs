namespace SqlKata
{
    public sealed class OffsetClauseBuilder : AbstractClauseBuilder
    {
        public required long Offset { get; init; }
    }
}
