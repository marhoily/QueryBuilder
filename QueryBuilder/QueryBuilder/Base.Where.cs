using System.Collections.Immutable;
using System.Reflection;

namespace SqlKata
{
    public partial class QueryBuilder
    {
        public QueryBuilder Where(string column, string op, object? value)
        {
            // If the value is "null", we will just assume the developer wants to add a
            // where null clause to the QueryBuilder. So, we will allow a short-cut here to
            // that method for convenience so the developer doesn't have to check.
            if (value == null) return Not(op != "=").WhereNull(column);

            if (value is bool boolValue)
            {
                if (op != "=") Not();

                return boolValue ? WhereTrue(column) : WhereFalse(column);
            }

            return AddComponent(new BasicConditionBuilder
            {
                Engine = EngineScope,
                Component = "where",
                Column = column,
                Operator = op,
                Value = value,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public QueryBuilder WhereNot(string column, string op, object? value)
        {
            return Not().Where(column, op, value);
        }

        public QueryBuilder OrWhere(string column, string op, object? value)
        {
            return Or().Where(column, op, value);
        }

        public QueryBuilder OrWhereNot(string column, string op, object? value)
        {
            return Or().Not().Where(column, op, value);
        }

        public QueryBuilder Where(string column, object? value)
        {
            return Where(column, "=", value);
        }

        public QueryBuilder WhereNot(string column, object? value)
        {
            return WhereNot(column, "=", value);
        }

        public QueryBuilder OrWhere(string column, object? value)
        {
            return OrWhere(column, "=", value);
        }

        public QueryBuilder OrWhereNot(string column, object? value)
        {
            return OrWhereNot(column, "=", value);
        }

        /// <summary>
        ///     Perform a where constraint
        /// </summary>
        /// <param name="constraints"></param>
        /// <returns></returns>
        public QueryBuilder Where(object constraints)
        {
            var dictionary = new Dictionary<string, object?>();

            foreach (var item in constraints.GetType().GetRuntimeProperties())
                dictionary.Add(item.Name, item.GetValue(constraints));

            return Where(dictionary);
        }

        public QueryBuilder Where(IEnumerable<KeyValuePair<string, object?>> values)
        {
            var QueryBuilder = this;
            var orFlag = GetOr();
            var notFlag = GetNot();

            foreach (var tuple in values)
            {
                if (orFlag)
                    QueryBuilder.Or();
                else
                    QueryBuilder.And();

                QueryBuilder = Not(notFlag).Where(tuple.Key, tuple.Value);
            }

            return QueryBuilder;
        }

        public QueryBuilder WhereRaw(string sql, params object[] bindings)
        {
            return AddComponent(new RawConditionBuilder
            {
                Engine = EngineScope,
                Component = "where",
                Expression = sql,
                Bindings = bindings,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public QueryBuilder OrWhereRaw(string sql, params object[] bindings)
        {
            return Or().WhereRaw(sql, bindings);
        }

        /// <summary>
        ///     Apply a nested where clause
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public QueryBuilder Where(Func<QueryBuilder, QueryBuilder> callback)
        {
            var QueryBuilder = callback.Invoke(NewChild());

            // omit empty queries
            if (!QueryBuilder.Clauses.Any(x => x.Component == "where")) return this;

            return AddComponent(new NestedConditionBuilder
            {
                Engine = EngineScope,
                Component = "where",
                Query = QueryBuilder,
                IsNot = GetNot(),
                IsOr = GetOr()
            });
        }

        public QueryBuilder WhereNot(Func<QueryBuilder, QueryBuilder> callback)
        {
            return Not().Where(callback);
        }

        public QueryBuilder OrWhere(Func<QueryBuilder, QueryBuilder> callback)
        {
            return Or().Where(callback);
        }

        public QueryBuilder OrWhereNot(Func<QueryBuilder, QueryBuilder> callback)
        {
            return Not().Or().Where(callback);
        }

        public QueryBuilder WhereColumns(string first, string op, string second)
        {
            return AddComponent(new TwoColumnsConditionBuilder
            {
                Engine = EngineScope,
                Component = "where",
                First = first,
                Second = second,
                Operator = op,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public QueryBuilder OrWhereColumns(string first, string op, string second)
        {
            return Or().WhereColumns(first, op, second);
        }

        public QueryBuilder WhereNull(string column)
        {
            return AddComponent(new NullConditionBuilder
            {
                Engine = EngineScope,
                Component = "where",
                Column = column,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public QueryBuilder WhereNotNull(string column)
        {
            return Not().WhereNull(column);
        }

        public QueryBuilder OrWhereNull(string column)
        {
            return Or().WhereNull(column);
        }

        public QueryBuilder OrWhereNotNull(string column)
        {
            return Or().Not().WhereNull(column);
        }

        public QueryBuilder WhereTrue(string column)
        {
            return AddComponent(new BooleanConditionBuilder
            {
                Engine = EngineScope,
                Component = "where",
                Column = column,
                Value = true,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public QueryBuilder OrWhereTrue(string column)
        {
            return Or().WhereTrue(column);
        }

        public QueryBuilder WhereFalse(string column)
        {
            return AddComponent(new BooleanConditionBuilder
            {
                Engine = EngineScope,
                Component = "where",
                Column = column,
                Value = false,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public QueryBuilder OrWhereFalse(string column)
        {
            return Or().WhereFalse(column);
        }

        public QueryBuilder WhereLike(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        {
            return AddComponent(new BasicStringConditionBuilder
            {
                Engine = EngineScope,
                Component = "where",
                Operator = "like",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                EscapeCharacter = escapeCharacter,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public QueryBuilder WhereNotLike(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        {
            return Not().WhereLike(column, value, caseSensitive, escapeCharacter);
        }

        public QueryBuilder OrWhereLike(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        {
            return Or().WhereLike(column, value, caseSensitive, escapeCharacter);
        }

        public QueryBuilder OrWhereNotLike(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        {
            return Or().Not().WhereLike(column, value, caseSensitive, escapeCharacter);
        }

        public QueryBuilder WhereStarts(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        {
            return AddComponent(new BasicStringConditionBuilder
            {
                Engine = EngineScope,
                Component = "where",
                Operator = "starts",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                EscapeCharacter = escapeCharacter,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public QueryBuilder WhereNotStarts(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        {
            return Not().WhereStarts(column, value, caseSensitive, escapeCharacter);
        }

        public QueryBuilder OrWhereStarts(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        {
            return Or().WhereStarts(column, value, caseSensitive, escapeCharacter);
        }

        public QueryBuilder OrWhereNotStarts(string column, object value, bool caseSensitive = false,
            string? escapeCharacter = null)
        {
            return Or().Not().WhereStarts(column, value, caseSensitive, escapeCharacter);
        }

        public QueryBuilder WhereEnds(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        {
            return AddComponent(new BasicStringConditionBuilder
            {
                Engine = EngineScope,
                Component = "where",
                Operator = "ends",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                EscapeCharacter = escapeCharacter,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public QueryBuilder WhereNotEnds(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        {
            return Not().WhereEnds(column, value, caseSensitive, escapeCharacter);
        }

        public QueryBuilder OrWhereEnds(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        {
            return Or().WhereEnds(column, value, caseSensitive, escapeCharacter);
        }

        public QueryBuilder OrWhereNotEnds(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        {
            return Or().Not().WhereEnds(column, value, caseSensitive, escapeCharacter);
        }

        public QueryBuilder WhereContains(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        {
            return AddComponent(new BasicStringConditionBuilder
            {
                Engine = EngineScope,
                Component = "where",
                Operator = "contains",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                EscapeCharacter = escapeCharacter,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public QueryBuilder WhereNotContains(string column, object value, bool caseSensitive = false,
            string? escapeCharacter = null)
        {
            return Not().WhereContains(column, value, caseSensitive, escapeCharacter);
        }

        public QueryBuilder OrWhereContains(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        {
            return Or().WhereContains(column, value, caseSensitive, escapeCharacter);
        }

        public QueryBuilder OrWhereNotContains(string column, object value, bool caseSensitive = false,
            string? escapeCharacter = null)
        {
            return Or().Not().WhereContains(column, value, caseSensitive, escapeCharacter);
        }

        public QueryBuilder WhereBetween<T>(string column, T lower, T higher)
        {
            return AddComponent(new BetweenConditionBuilder<T>
            {
                Engine = EngineScope,
                Component = "where",
                Column = column,
                IsOr = GetOr(),
                IsNot = GetNot(),
                Lower = lower,
                Higher = higher
            });
        }

        public QueryBuilder OrWhereBetween<T>(string column, T lower, T higher)
        {
            return Or().WhereBetween(column, lower, higher);
        }

        public QueryBuilder WhereNotBetween<T>(string column, T lower, T higher)
        {
            return Not().WhereBetween(column, lower, higher);
        }

        public QueryBuilder OrWhereNotBetween<T>(string column, T lower, T higher)
        {
            return Or().Not().WhereBetween(column, lower, higher);
        }

        public QueryBuilder WhereIn<T>(string column, IEnumerable<T> values)
        {
            // If the developer has passed a string they most likely want a List<string>
            // since string is considered as List<char>
            if (values is string val)
            {
                return AddComponent(new InConditionBuilder<string>
                {
                    Engine = EngineScope,
                    Component = "where",
                    Column = column,
                    IsOr = GetOr(),
                    IsNot = GetNot(),
                    Values = ImmutableArray.Create(val)
                });
            }

            return AddComponent(new InConditionBuilder<T>
            {
                Engine = EngineScope,
                Component = "where",
                Column = column,
                IsOr = GetOr(),
                IsNot = GetNot(),
                Values = values.Distinct().ToImmutableArray()
            });
        }

        public QueryBuilder OrWhereIn<T>(string column, IEnumerable<T> values)
        {
            return Or().WhereIn(column, values);
        }

        public QueryBuilder WhereNotIn<T>(string column, IEnumerable<T> values)
        {
            return Not().WhereIn(column, values);
        }

        public QueryBuilder OrWhereNotIn<T>(string column, IEnumerable<T> values)
        {
            return Or().Not().WhereIn(column, values);
        }


        public QueryBuilder WhereIn(string column, QueryBuilder QueryBuilder)
        {
            return AddComponent(new InQueryConditionBuilder
            {
                Engine = EngineScope,
                Component = "where",
                Column = column,
                IsOr = GetOr(),
                IsNot = GetNot(),
                Query = QueryBuilder
            });
        }

        public QueryBuilder WhereIn(string column, Func<QueryBuilder, QueryBuilder> callback)
        {
            var QueryBuilder = callback.Invoke(new QueryBuilder().SetParent(this));

            return WhereIn(column, QueryBuilder);
        }

        public QueryBuilder OrWhereIn(string column, QueryBuilder QueryBuilder)
        {
            return Or().WhereIn(column, QueryBuilder);
        }

        public QueryBuilder OrWhereIn(string column, Func<QueryBuilder, QueryBuilder> callback)
        {
            return Or().WhereIn(column, callback);
        }

        public QueryBuilder WhereNotIn(string column, QueryBuilder QueryBuilder)
        {
            return Not().WhereIn(column, QueryBuilder);
        }

        public QueryBuilder WhereNotIn(string column, Func<QueryBuilder, QueryBuilder> callback)
        {
            return Not().WhereIn(column, callback);
        }

        public QueryBuilder OrWhereNotIn(string column, QueryBuilder QueryBuilder)
        {
            return Or().Not().WhereIn(column, QueryBuilder);
        }

        public QueryBuilder OrWhereNotIn(string column, Func<QueryBuilder, QueryBuilder> callback)
        {
            return Or().Not().WhereIn(column, callback);
        }


        /// <summary>
        ///     Perform a sub QueryBuilder where clause
        /// </summary>
        /// <param name="column"></param>
        /// <param name="op"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public QueryBuilder Where(string column, string op, Func<QueryBuilder, QueryBuilder> callback)
        {
            var QueryBuilder = callback.Invoke(NewChild());

            return Where(column, op, QueryBuilder);
        }

        public QueryBuilder Where(string column, string op, QueryBuilder QueryBuilder)
        {
            return AddComponent(new QueryConditionBuilder
            {
                Engine = EngineScope,
                Component = "where",
                Column = column,
                Operator = op,
                Query = QueryBuilder,
                IsNot = GetNot(),
                IsOr = GetOr()
            });
        }

        public QueryBuilder WhereSub(QueryBuilder QueryBuilder, object value)
        {
            return WhereSub(QueryBuilder, "=", value);
        }

        public QueryBuilder WhereSub(QueryBuilder QueryBuilder, string op, object value)
        {
            return AddComponent(new SubQueryConditionBuilder
            {
                Engine = EngineScope,
                Component = "where",
                Value = value,
                Operator = op,
                Query = QueryBuilder,
                IsNot = GetNot(),
                IsOr = GetOr()
            });
        }

        public QueryBuilder OrWhereSub(QueryBuilder QueryBuilder, object value)
        {
            return Or().WhereSub(QueryBuilder, value);
        }

        public QueryBuilder OrWhereSub(QueryBuilder QueryBuilder, string op, object value)
        {
            return Or().WhereSub(QueryBuilder, op, value);
        }

        public QueryBuilder OrWhere(string column, string op, QueryBuilder QueryBuilder)
        {
            return Or().Where(column, op, QueryBuilder);
        }

        public QueryBuilder OrWhere(string column, string op, Func<QueryBuilder, QueryBuilder> callback)
        {
            return Or().Where(column, op, callback);
        }

        public QueryBuilder WhereExists(QueryBuilder QueryBuilder)
        {
            if (!QueryBuilder.HasComponent("from"))
                throw new ArgumentException(
                    $"'{nameof(FromClause)}' cannot be empty if used inside a '{nameof(WhereExists)}' condition");

            return AddComponent(new ExistsConditionBuilder
            {
                Engine = EngineScope,
                Component = "where",
                Query = QueryBuilder,
                IsNot = GetNot(),
                IsOr = GetOr()
            });
        }

        public QueryBuilder WhereExists(Func<QueryBuilder, QueryBuilder> callback)
        {
            var childQuery = new QueryBuilder().SetParent(this);
            return WhereExists(callback.Invoke(childQuery));
        }

        public QueryBuilder WhereNotExists(QueryBuilder QueryBuilder)
        {
            return Not().WhereExists(QueryBuilder);
        }

        public QueryBuilder WhereNotExists(Func<QueryBuilder, QueryBuilder> callback)
        {
            return Not().WhereExists(callback);
        }

        public QueryBuilder OrWhereExists(QueryBuilder QueryBuilder)
        {
            return Or().WhereExists(QueryBuilder);
        }

        public QueryBuilder OrWhereExists(Func<QueryBuilder, QueryBuilder> callback)
        {
            return Or().WhereExists(callback);
        }

        public QueryBuilder OrWhereNotExists(QueryBuilder QueryBuilder)
        {
            return Or().Not().WhereExists(QueryBuilder);
        }

        public QueryBuilder OrWhereNotExists(Func<QueryBuilder, QueryBuilder> callback)
        {
            return Or().Not().WhereExists(callback);
        }

        #region date

        public QueryBuilder WhereDatePart(string part, string column, string op, object value)
        {
            return AddComponent(new BasicDateConditionBuilder
            {
                Engine = EngineScope,
                Component = "where",
                Operator = op,
                Column = column,
                Value = value,
                Part = part.ToLowerInvariant(),
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public QueryBuilder WhereNotDatePart(string part, string column, string op, object value)
        {
            return Not().WhereDatePart(part, column, op, value);
        }

        public QueryBuilder OrWhereDatePart(string part, string column, string op, object value)
        {
            return Or().WhereDatePart(part, column, op, value);
        }

        public QueryBuilder OrWhereNotDatePart(string part, string column, string op, object value)
        {
            return Or().Not().WhereDatePart(part, column, op, value);
        }

        public QueryBuilder WhereDate(string column, string op, object value)
        {
            return WhereDatePart("date", column, op, value);
        }

        public QueryBuilder WhereNotDate(string column, string op, object value)
        {
            return Not().WhereDate(column, op, value);
        }

        public QueryBuilder OrWhereDate(string column, string op, object value)
        {
            return Or().WhereDate(column, op, value);
        }

        public QueryBuilder OrWhereNotDate(string column, string op, object value)
        {
            return Or().Not().WhereDate(column, op, value);
        }

        public QueryBuilder WhereTime(string column, string op, object value)
        {
            return WhereDatePart("time", column, op, value);
        }

        public QueryBuilder WhereNotTime(string column, string op, object value)
        {
            return Not().WhereTime(column, op, value);
        }

        public QueryBuilder OrWhereTime(string column, string op, object value)
        {
            return Or().WhereTime(column, op, value);
        }

        public QueryBuilder OrWhereNotTime(string column, string op, object value)
        {
            return Or().Not().WhereTime(column, op, value);
        }

        public QueryBuilder WhereDatePart(string part, string column, object value)
        {
            return WhereDatePart(part, column, "=", value);
        }

        public QueryBuilder WhereNotDatePart(string part, string column, object value)
        {
            return WhereNotDatePart(part, column, "=", value);
        }

        public QueryBuilder OrWhereDatePart(string part, string column, object value)
        {
            return OrWhereDatePart(part, column, "=", value);
        }

        public QueryBuilder OrWhereNotDatePart(string part, string column, object value)
        {
            return OrWhereNotDatePart(part, column, "=", value);
        }

        public QueryBuilder WhereDate(string column, object value)
        {
            return WhereDate(column, "=", value);
        }

        public QueryBuilder WhereNotDate(string column, object value)
        {
            return WhereNotDate(column, "=", value);
        }

        public QueryBuilder OrWhereDate(string column, object value)
        {
            return OrWhereDate(column, "=", value);
        }

        public QueryBuilder OrWhereNotDate(string column, object value)
        {
            return OrWhereNotDate(column, "=", value);
        }

        public QueryBuilder WhereTime(string column, object value)
        {
            return WhereTime(column, "=", value);
        }

        public QueryBuilder WhereNotTime(string column, object value)
        {
            return WhereNotTime(column, "=", value);
        }

        public QueryBuilder OrWhereTime(string column, object value)
        {
            return OrWhereTime(column, "=", value);
        }

        public QueryBuilder OrWhereNotTime(string column, object value)
        {
            return OrWhereNotTime(column, "=", value);
        }

        #endregion
    }
}
