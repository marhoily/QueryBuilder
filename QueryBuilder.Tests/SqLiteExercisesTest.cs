using FluentAssertions;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using SqlKata.Compilers;
using SqlKata.Execution;
using Xunit;

namespace SqlKata.Tests;

public sealed class SqLiteExercisesTest
{
    private readonly QueryFactory _db;
    private string? _lastQuery;

    public SqLiteExercisesTest()
    {
        // DB can be downloaded here: https://www.w3resource.com/sqlite-exercises/
        var cs = "Data Source=C:\\Users\\marho\\Downloads\\hr;Cache=Shared";
        var connection = new SqliteConnection(cs);
        _db = new QueryFactory(connection, new SqliteCompiler());
        _db.Logger = r => _lastQuery = r.Sql;
    }

    [Fact]
    public void Alias()
    {
        var result = _db.Query("employees").Select(
            "first_name as First Name",
            "last_name as Last Name").Limit(3).Get();
        JsonConvert.SerializeObject(result, Formatting.Indented).Should()
            .Be("""
                [
                  {
                    "First Name": "Steven",
                    "Last Name": "King"
                  },
                  {
                    "First Name": "Neena",
                    "Last Name": "Kochhar"
                  },
                  {
                    "First Name": "Lex",
                    "Last Name": "De Haan"
                  }
                ]
                """);
    }

    [Fact]
    public void Unique()
    {
        _db.Query("employees").Select("department_id")
            .Distinct().Get<int>().Should()
            .Equal(90, 60, 100, 30, 50, 80, 0, 10, 20, 40, 70, 110);
        _lastQuery.Should()
            .Be("""
                SELECT DISTINCT "department_id" FROM "employees"
                """);
    }

    [Fact]
    public void Sorting()
    {
        // get all employee details from the employee
        // table order by first name, descending.
        _db.Query("employees")
            .OrderByDesc("first_name").Limit(3)
            .Get().Select(e => (int)e.employee_id)
            .Should().Equal(180, 171, 206);
        _lastQuery.Should()
            .Be("""
                SELECT * FROM "employees" ORDER BY "first_name" DESC LIMIT @p0
                """);
    }

   
}
