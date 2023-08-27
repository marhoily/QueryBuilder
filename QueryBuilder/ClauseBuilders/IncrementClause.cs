namespace SqlKata
{
    public sealed class IncrementClauseBuilder : InsertClauseBuilder
    {
        public required string Column { get; init; }
        public required int Value { get; init; } 
    }
}
