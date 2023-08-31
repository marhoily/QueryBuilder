namespace SqlKata.Compilers
{
    public partial class Compiler
    {
        private static readonly string[] LikeOperators = { "starts", "ends", "contains", "like" };

        private void CompileCondition(SqlResult ctx, Query query, AbstractCondition clause, Writer writer)
        {
            switch (clause)
            {
                case BasicDateCondition basicDateCondition:
                    CompileBasicDateCondition(ctx, query, basicDateCondition, writer);
                    break;
                case BasicStringCondition basicStringCondition:
                    CompileBasicStringCondition(ctx, query, basicStringCondition, writer);
                    break;
                case BasicCondition basicCondition:
                    CompileBasicCondition(ctx, query, basicCondition, writer);
                    break;
                case BetweenCondition betweenCondition:
                    CompileBetweenCondition(ctx, query, betweenCondition, writer);
                    break;
                case BooleanCondition booleanCondition:
                    CompileBooleanCondition(booleanCondition, writer);
                    break;
                case ExistsCondition existsCondition:
                    CompileExistsCondition(ctx, existsCondition, writer);
                    break;
                case InCondition inCondition:
                    CompileInCondition(ctx, query, inCondition, writer);
                    break;
                case InQueryCondition inQueryCondition:
                    CompileInQueryCondition(ctx, inQueryCondition, writer);
                    break;
                case NestedCondition nestedCondition:
                    CompileNestedCondition(ctx, query, nestedCondition, writer);
                    break;
                case NullCondition nullCondition:
                    CompileNullCondition(nullCondition, writer);
                    break;
                case QueryCondition queryCondition:
                    CompileQueryCondition(ctx, queryCondition, writer);
                    break;
                case RawCondition rawCondition:
                    CompileRawCondition(ctx, rawCondition, writer);
                    break;
                case SubQueryCondition subQueryCondition:
                    CompileSubQueryCondition(ctx, query, subQueryCondition, writer);
                    break;
                case TwoColumnsCondition twoColumnsCondition:
                    CompileTwoColumnsCondition(twoColumnsCondition, writer);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(clause));
            }
        }

        private void CompileConditions(SqlResult ctx, Query query, List<AbstractCondition> conditions, Writer writer)
        {
            writer.List(" ", conditions, (c, i) =>
            {
                if (i != 0)
                    writer.Append(c.IsOr ? "OR " : "AND ");
                CompileCondition(ctx, query, c, writer);
            });
        }

        private void CompileRawCondition(SqlResult ctx, RawCondition x, Writer writer)
        {
            ctx.BindingsAddRange(x.Bindings);
            writer.AppendRaw(x.Expression, x.Bindings);
            writer.AssertMatches(ctx);
        }

        private void CompileQueryCondition(SqlResult ctx, QueryCondition x, Writer writer)
        {
            writer.AppendName(x.Column);
            writer.Append(" ");
            writer.Append(Operators.CheckOperator(x.Operator));
            writer.Append(" (");
            var subCtx = CompileSelectQuery(x.Query, writer);
            ctx.BindingsAddRange(subCtx);
            writer.Append(")");
            writer.AssertMatches(ctx);
        }

        private void CompileSubQueryCondition(SqlResult ctx, Query query, SubQueryCondition x, Writer writer)
        {
            writer.Append("(");
            var subCtx = CompileSelectQuery(x.Query, writer);
            ctx.BindingsAddRange(subCtx);
            writer.Append(") ");
            writer.Append(Operators.CheckOperator(x.Operator));
            writer.Append(" ");
            writer.AppendParameter(ctx, query, x.Value);
            writer.AssertMatches(ctx);
        }

        private void CompileBasicCondition(SqlResult ctx, Query query, BasicCondition x, Writer writer)
        {
            if (x.IsNot)
                writer.Append("NOT (");
            writer.AppendName(x.Column);
            writer.Append(" ");
            writer.Append(Operators.CheckOperator(x.Operator));
            writer.Append(" ");
            writer.AssertMatches();
            writer.AppendParameter(ctx, query, x.Value);
            if (x.IsNot)
                writer.Append(")");
        }

        protected virtual void CompileBasicStringCondition(SqlResult ctx, Query query, BasicStringCondition x,
            Writer writer)
        {
            var column = XService.Wrap(x.Column);

            if (Resolve(query,  x.Value) is not string value)
                throw new ArgumentException("Expecting a non nullable string");

            var method = x.Operator;

            if (LikeOperators.Contains(x.Operator))
            {
                method = "LIKE";

                switch (x.Operator)
                {
                    case "starts":
                        value = $"{value}%";
                        break;
                    case "ends":
                        value = $"%{value}";
                        break;
                    case "contains":
                        value = $"%{value}%";
                        break;
                }
            }


            if (!x.CaseSensitive)
            {
                column = $"LOWER({column})";
                value = value.ToLowerInvariant();
            }

            if (x.IsNot)
                writer.Append("NOT (");
            writer.Append(column);
            writer.Append(" ");
            writer.Append(Operators.CheckOperator(method));
            writer.Append(" ");
            writer.AppendParameter(ctx, query, value);
            if (x.EscapeCharacter is { } esc1)
            {
                writer.Append(" ESCAPE '");
                writer.Append(esc1);
                writer.Append('\'');
            }

            if (x.IsNot)
                writer.Append(")");
            writer.AssertMatches(ctx);
        }

        protected virtual void CompileBasicDateCondition(SqlResult ctx, Query query, BasicDateCondition x,
            Writer writer)
        {
            if (x.IsNot)
                writer.Append("NOT (");
            writer.Append(x.Part.ToUpperInvariant());
            writer.Append("(");
            writer.AppendName(x.Column);
            writer.Append(") ");
            writer.Append(Operators.CheckOperator(x.Operator));
            writer.Append(" ");
            writer.AppendParameter(ctx, query, x.Value);
            if (x.IsNot)
                writer.Append(")");
            writer.AssertMatches(ctx);
        }

        private void CompileNestedCondition(SqlResult ctx, Query query, NestedCondition x, Writer writer)
        {
            if (!x.Query.HasComponent("where", EngineCode) &&
                !x.Query.HasComponent("having", EngineCode))
                return;

            var clause = x.Query.HasComponent("where", EngineCode) ? "where" : "having";

            var clauses = x.Query.GetComponents<AbstractCondition>(clause, EngineCode);

            if (x.IsNot)
                writer.Append("NOT ");
            writer.Append("(");
            CompileConditions(ctx, query, clauses, writer);
            writer.Append(")");
            writer.AssertMatches(ctx);
        }

        private void CompileTwoColumnsCondition(TwoColumnsCondition x, Writer writer)
        {
            if (x.IsNot)
                writer.Append("NOT ");
            writer.AppendName(x.First);
            writer.Append(" ");
            writer.Append(Operators.CheckOperator(x.Operator));
            writer.Append(" ");
            writer.AppendName(x.Second);
            writer.AssertMatches();
        }

        private void CompileBetweenCondition(SqlResult ctx, Query query, BetweenCondition x, Writer writer)
        {
            writer.AppendName(x.Column);
            writer.Append(x.IsNot ? " NOT BETWEEN " : " BETWEEN ");
            writer.AppendParameter(ctx, query, x.Lower);
            writer.Append(" AND ");
            writer.AppendParameter(ctx, query, x.Higher);
            writer.AssertMatches(ctx);
        }

        private void CompileInCondition(SqlResult ctx, Query query, InCondition x, Writer writer)
        {
            if (!x.Values.Any())
            {
                writer.Append(x.IsNot
                    ? "1 = 1 /* NOT IN [empty list] */"
                    : "1 = 0 /* IN [empty list] */");
                return;
            }

            writer.AppendName(x.Column);
            writer.Append(x.IsNot ? " NOT IN (" : " IN (");
            writer.CommaSeparatedParameters(
                ctx, query, x.Values.OfType<object>());
            writer.Append(")");
            writer.AssertMatches(ctx);
        }

        private void CompileInQueryCondition(SqlResult ctx, InQueryCondition x, Writer writer)
        {
            writer.AppendName(x.Column);
            writer.Append(x.IsNot ? " NOT IN (" : " IN (");
            var subCtx = CompileSelectQuery(x.Query, writer);
            ctx.BindingsAddRange(subCtx);
            writer.Append(")");
            writer.AssertMatches(ctx);
        }

        private void CompileNullCondition(NullCondition x, Writer writer)
        {
            writer.AppendName(x.Column);
            writer.Append(x.IsNot ? " IS NOT NULL" : " IS NULL");
            writer.AssertMatches();
        }

        private void CompileBooleanCondition(BooleanCondition x, Writer writer)
        {
            writer.AppendName(x.Column);
            writer.Append(x.IsNot ? " != " : " = ");
            writer.Append(x.Value ? CompileTrue() : CompileFalse());
            writer.AssertMatches();
        }

        private void CompileExistsCondition(SqlResult ctx, ExistsCondition item, Writer writer)
        {
            writer.Append(item.IsNot ? "NOT EXISTS (" : "EXISTS (");

            var query = OmitSelectInsideExists
                ? item.Query.Clone().RemoveComponent("select").SelectRaw("1")
                : item.Query;

            var subCtx = CompileSelectQuery(query, writer);
            ctx.BindingsAddRange(subCtx);
            writer.Append(")");
            writer.AssertMatches(ctx);
        }
    }
}
