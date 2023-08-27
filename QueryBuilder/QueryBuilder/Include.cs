namespace SqlKata
{
    public sealed class IncludeBuilder
    {
        public required string Name { get; init; }
        public required QueryBuilder Query { get; init; }
        public required string LocalKey { get; init; }
        public required bool IsMany { get; init; }

        public required string? ForeignKey { get; set; }
    }
}
