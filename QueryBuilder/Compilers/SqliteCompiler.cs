namespace SqlKata.Compilers
{
    public sealed class SqliteCompiler : Compiler
    {
        private static readonly HashSet<string> Keywords = new()
        {
            "ABORT",
            "ACTION",
            "ADD",
            "AFTER",
            "ALL",
            "ALTER",
            "ALWAYS",
            "ANALYZE",
            "AND",
            "AS",
            "ASC",
            "ATTACH",
            "AUTOINCREMENT",
            "BEFORE",
            "BEGIN",
            "BETWEEN",
            "BY",
            "CASCADE",
            "CASE",
            "CAST",
            "CHECK",
            "COLLATE",
            "COLUMN",
            "COMMIT",
            "CONFLICT",
            "CONSTRAINT",
            "CREATE",
            "CROSS",
            "CURRENT",
            "CURRENT_DATE",
            "CURRENT_TIME",
            "CURRENT_TIMESTAMP",
            "DATABASE",
            "DEFAULT",
            "DEFERRABLE",
            "DEFERRED",
            "DELETE",
            "DESC",
            "DETACH",
            "DISTINCT",
            "DO",
            "DROP",
            "EACH",
            "ELSE",
            "END",
            "ESCAPE",
            "EXCEPT",
            "EXCLUDE",
            "EXCLUSIVE",
            "EXISTS",
            "EXPLAIN",
            "FAIL",
            "FILTER",
            "FIRST",
            "FOLLOWING",
            "FOR",
            "FOREIGN",
            "FROM",
            "FULL",
            "GENERATED",
            "GLOB",
            "GROUP",
            "GROUPS",
            "HAVING",
            "IF",
            "IGNORE",
            "IMMEDIATE",
            "IN",
            "INDEX",
            "INDEXED",
            "INITIALLY",
            "INNER",
            "INSERT",
            "INSTEAD",
            "INTERSECT",
            "INTO",
            "IS",
            "ISNULL",
            "JOIN",
            "KEY",
            "LAST",
            "LEFT",
            "LIKE",
            "LIMIT",
            "MATCH",
            "MATERIALIZED",
            "NATURAL",
            "NO",
            "NOT",
            "NOTHING",
            "NOTNULL",
            "NULL",
            "NULLS",
            "OF",
            "OFFSET",
            "ON",
            "OR",
            "ORDER",
            "OTHERS",
            "OUTER",
            "OVER",
            "PARTITION",
            "PLAN",
            "PRAGMA",
            "PRECEDING",
            "PRIMARY",
            "QUERY",
            "RAISE",
            "RANGE",
            "RECURSIVE",
            "REFERENCES",
            "REGEXP",
            "REINDEX",
            "RELEASE",
            "RENAME",
            "REPLACE",
            "RESTRICT",
            "RETURNING",
            "RIGHT",
            "ROLLBACK",
            "ROW",
            "ROWS",
            "SAVEPOINT",
            "SELECT",
            "SET",
            "TABLE",
            "TEMP",
            "TEMPORARY",
            "THEN",
            "TIES",
            "TO",
            "TRANSACTION",
            "TRIGGER",
            "UNBOUNDED",
            "UNION",
            "UNIQUE",
            "UPDATE",
            "USING",
            "VACUUM",
            "VALUES",
            "VIEW",
            "VIRTUAL",
            "WHEN",
            "WHERE",
            "WINDOW",
            "WITH",
            "WITHOUT"
        };
        public SqliteCompiler()
        {
            OpeningIdentifier = ClosingIdentifier = "\"";
            LastId = "select last_insert_rowid() as id";
            EngineCode = EngineCodes.Sqlite;
            SupportsFilterClause = true;
        }

        public string WrapValue(string value)
        {
            if (Keywords.Contains(value.ToUpperInvariant()))
                return base.WrapValue(value);
            return value;
        }

        public override string CompileTrue()
        {
            return "1";
        }

        public override string CompileFalse()
        {
            return "0";
        }

        public override string? CompileLimit(SqlResult ctx)
        {
            var limit = ctx.Query.GetLimit(EngineCode);
            var offset = ctx.Query.GetOffset(EngineCode);

            if (limit == 0 && offset > 0)
            {
                ctx.Bindings.Add(offset);
                return $"LIMIT -1 OFFSET {ParameterPlaceholder}";
            }

            return base.CompileLimit(ctx);
        }

        protected override string CompileBasicDateCondition(SqlResult ctx, BasicDateCondition condition)
        {
            var column = Wrap(condition.Column);
            var value = Parameter(ctx, condition.Value);

            var formatMap = new Dictionary<string, string>
            {
                { "date", "%Y-%m-%d" },
                { "time", "%H:%M:%S" },
                { "year", "%Y" },
                { "month", "%m" },
                { "day", "%d" },
                { "hour", "%H" },
                { "minute", "%M" }
            };

            if (!formatMap.ContainsKey(condition.Part)) return $"{column} {condition.Operator} {value}";

            var sql = $"strftime('{formatMap[condition.Part]}', {column}) {condition.Operator} cast({value} as text)";

            if (condition.IsNot) return $"NOT ({sql})";

            return sql;
        }
    }
}
