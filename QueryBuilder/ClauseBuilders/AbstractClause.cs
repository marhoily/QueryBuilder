namespace SqlKata
{
    public abstract class AbstractClauseBuilder
    {
        public required string? Engine { get; init; }
        public required string Component { get; init; }
    }
}
