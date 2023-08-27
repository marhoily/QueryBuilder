using SqlKata.Compilers;

namespace SqlKata.Extensions
{
    public static class QueryForExtensions
    {
        public static JoinBuilder ForFirebird(this JoinBuilder src, Func<JoinBuilder, JoinBuilder> fn)
        {
            return src.For(EngineCodes.Firebird, fn);
        }

        public static JoinBuilder ForMySql(this JoinBuilder src, Func<JoinBuilder, JoinBuilder> fn)
        {
            return src.For(EngineCodes.MySql, fn);
        }

        public static JoinBuilder ForOracle(this JoinBuilder src, Func<JoinBuilder, JoinBuilder> fn)
        {
            return src.For(EngineCodes.Oracle, fn);
        }

        public static JoinBuilder ForPostgreSql(this JoinBuilder src, Func<JoinBuilder, JoinBuilder> fn)
        {
            return src.For(EngineCodes.PostgreSql, fn);
        }

        public static JoinBuilder ForSqlite(this JoinBuilder src, Func<JoinBuilder, JoinBuilder> fn)
        {
            return src.For(EngineCodes.Sqlite, fn);
        }

        public static JoinBuilder ForSqlServer(this JoinBuilder src, Func<JoinBuilder, JoinBuilder> fn)
        {
            return src.For(EngineCodes.SqlServer, fn);
        }
    }
}
