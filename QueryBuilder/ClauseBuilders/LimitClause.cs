namespace SqlKata
{
    public sealed class LimitClauseBuilder : AbstractClauseBuilder
    {
        public required int Limit { get; init; }
    }
}
