using FluentAssertions;
using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using Xunit;

namespace SqlKata.Tests
{
    public sealed class CoverageTests : TestSupport
    {
        [Fact]
        public void CompileCte_With_RawFromClause()
        {
            var queryCTe = new Query("prodCTE")
                .WithRaw("prodCTE", "SELECT * FROM B");

            Compile(queryCTe)[EngineCodes.SqlServer].Should()
                .Be("WITH [prodCTE] AS (SELECT * FROM B)\n" +
                    "SELECT * FROM [prodCTE]");
        }
        [Fact]
        public void From_QueryFromClause_With_Bindings()
        {
            var query = new Query("T")
                .From(new Query("S").Where("c", 1));

            Compile(query)[EngineCodes.SqlServer].Should()
                .Be("SELECT * FROM " +
                    "(SELECT * FROM [S] WHERE [c] = 1)");
        }
        [Fact]
        public void Join_QueryFromClause_With_Bindings()
        {
            var query = new Query("L")
                .Join(new Query("R").Where("c", 1),
                    join => join.On("a", "b"));

            Compile(query)[EngineCodes.SqlServer].Should()
                .Be("SELECT * FROM [L] \n" +
                    "INNER JOIN " +
                    "(SELECT * FROM [R] WHERE [c] = 1) " +
                    "ON ([a] = [b])");
        }
        [Fact]
        public void InCondition()
        {
            var query = new Query("L").WhereIn("a", 1, 2);

            Compile(query)[EngineCodes.SqlServer].Should()
                .Be("SELECT * FROM [L] WHERE [a] IN (1, 2)");
        }
        [Fact]
        public void InCondition_When_Empty_Bindings()
        {
            var query = new Query("L").WhereIn<int>("a");

            Compile(query)[EngineCodes.SqlServer].Should()
                .Be("SELECT * FROM [L] WHERE 1 = 0 /* IN [empty list] */");
        }
        [Fact]
        public void InCondition_When_Not()
        {
            var query = new Query("L").WhereNotIn("a", "blah");

            Compile(query)[EngineCodes.SqlServer].Should()
                .Be("SELECT * FROM [L] WHERE [a] NOT IN ('blah')");
        }
           
        [Fact]
        public void WhereDate_When_Unknown_DatePart()
        {
            var query = new Query("Orders")
                .Define("@d", 1996)
                .WhereDatePart("blah", "RequiredDate", "=", Expressions.Variable("@d"));
            Compile(query)[EngineCodes.SqlServer].Should()
                .Be("SELECT * FROM [Orders] WHERE DATEPART(BLAH, [RequiredDate]) = 1996");
        }

        [Fact]
        public void Compile_Multiple()
        {
            new SqlServerCompiler().Compile(new[]{
                new Query("A"), new Query("B")})
                .ToString().Should().Be(
                    "SELECT * FROM [A];\n" +
                    "SELECT * FROM [B]");
        }
    }
}
