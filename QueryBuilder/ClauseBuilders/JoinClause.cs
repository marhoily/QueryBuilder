namespace SqlKata
{
    public abstract class AbstractJoinBuilder : AbstractClauseBuilder
    {
    }

    public sealed class BaseJoinBuilder : AbstractJoinBuilder
    {
        public required JoinBuilder Join { get; init; }
    }

    public sealed class DeepJoinBuilder : AbstractJoinBuilder
    {
        public required string Type { get; init; }
        public required string Expression { get; init; }
        public required string SourceKeySuffix { get; init; }
        public required string TargetKey { get; init; }
        public required Func<string, string> SourceKeyGenerator { get; init; }
        public required Func<string, string> TargetKeyGenerator { get; init; }
    }
}
