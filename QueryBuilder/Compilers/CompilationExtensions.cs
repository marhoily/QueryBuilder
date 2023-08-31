using System.Collections.Immutable;
using FluentAssertions;

namespace SqlKata.Compilers
{
    public static class CompilationExtensions
    {
        public static SqlResult Compile(this Compiler compiler, Query query)
        {
            var writer = new Writer(compiler.XService);
            var ctx = compiler.CompileRaw(query, writer);
            ctx.RawSql.Should().Be((string)writer);
            writer.AssertMatches(ctx);
            ctx = compiler.PrepareResult(ctx, writer);
            return ctx;
        }

        public static SqlResult Compile(this Compiler compiler, IEnumerable<Query> queries)
        {
            var writer = new Writer(compiler.XService);
            var ctx = Accumulate();

            ctx.ReplaceRaw(writer);
            ctx = compiler.PrepareResult(ctx, writer);
            return ctx;

            SqlResult Accumulate()
            {
                var sqlResult = new SqlResult();
                writer.List(";\n", queries, query =>
                {
                    var sub = compiler.CompileRaw(query, writer);
                    writer.AssertMatches(sub);
                    sqlResult.BindingsAddRange(sub.Bindings);
                });
                return sqlResult;
            }
        }

        private static SqlResult CompileRaw(this Compiler compiler, Query query, Writer writer)
        {
            var ctx = new SqlResult();
            writer.Push(ctx);
            // handle CTEs
            if (query.HasComponent("cte", compiler.EngineCode))
            {
                compiler.CompileCteQuery(query, writer);
                ctx.BindingsAddRange(writer.Bindings);
                writer.AssertMatches(ctx);
            }
            if (query.Method == "insert")
            {
                compiler.CompileInsertQuery(ctx, query, writer);
                writer.AssertMatches(ctx);

            }
            else if (query.Method == "update")
            {
                compiler.CompileUpdateQuery(ctx, query, writer);
                writer.AssertMatches(ctx);
            }
            else if (query.Method == "delete")
            {
                compiler.CompileDeleteQuery(ctx, query, writer);
                writer.AssertMatches(ctx);
            }
            else
            {
                if (query.Method == "aggregate")
                {
                    query.RemoveComponent("limit")
                        .RemoveComponent("order")
                        .RemoveComponent("group");

                    query = TransformAggregateQuery(query);
                }
              
                writer.AssertMatches(ctx);
                compiler.CompileSelectQueryInner(ctx, query, writer);
                writer.AssertMatches(ctx);
            }


            // "... WHERE `Id` in (?)" -> "... WHERE `Id` in (?,?,?)"
           // ctx.ReplaceRaw(BindingExtensions.ExpandParameters(ctx.RawSql,
           //     "?", ctx.Bindings.ToArray()));
            writer.AssertMatches(ctx);

            return ctx;
            Query TransformAggregateQuery(Query query1)
            {
                var clause = query1.GetOneComponent<AggregateClause>("aggregate", compiler.EngineCode)!;

                if (clause.Columns.Length == 1 && !query1.IsDistinct) return query1;

                if (query1.IsDistinct)
                {
                    query1.RemoveComponent("aggregate", compiler.EngineCode);
                    query1.RemoveComponent("select", compiler.EngineCode);
                    query1.Select(clause.Columns.ToArray());
                }
                else
                {
                    foreach (var column in clause.Columns) query1.WhereNotNull(column);
                }

                var outerClause = new AggregateClause
                {
                    Engine = compiler.EngineCode,
                    Component = "aggregate",
                    Columns = ImmutableArray.Create<string>().Add("*"),
                    Type = clause.Type
                };

                return new Query()
                    .AddComponent(outerClause)
                    .From(query1, $"{clause.Type}Query");
            }

        }

        private static SqlResult PrepareResult(this Compiler compiler, SqlResult ctx, Writer writer)
        {
            ctx.NamedBindings = ctx.Bindings.GenerateNamedBindings(compiler.ParameterPrefix);
            ctx.Sql = BindingExtensions.ReplaceAll(writer,
                "?", i => compiler.ParameterPrefix + i);
            return ctx;
        }


    }
}
