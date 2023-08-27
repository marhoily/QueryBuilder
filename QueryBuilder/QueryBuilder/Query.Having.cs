using System.Collections.Immutable;
using System.Reflection;

namespace SqlKata
{
    public partial class QueryBuilder
    {
        public QueryBuilder Having(string column, string op, object? value)
        {
            // If the value is "null", we will just assume the developer wants to add a
            // Having null clause to the query. So, we will allow a short-cut here to
            // that method for convenience so the developer doesn't have to check.
            if (value == null) return Not(op != "=").HavingNull(column);

            return AddComponent(new BasicConditionBuilder
            {
                Engine = EngineScope,
                Component = "having",
                Column = column,
                Operator = op,
                Value = value,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public QueryBuilder HavingNot(string column, string op, object value)
        {
            return Not().Having(column, op, value);
        }

        public QueryBuilder OrHaving(string column, string op, object value)
        {
            return Or().Having(column, op, value);
        }

        public QueryBuilder OrHavingNot(string column, string op, object value)
        {
            return Or().Not().Having(column, op, value);
        }

        public QueryBuilder Having(string column, object? value)
        {
            return Having(column, "=", value);
        }

        public QueryBuilder HavingNot(string column, object value)
        {
            return HavingNot(column, "=", value);
        }

        public QueryBuilder OrHaving(string column, object value)
        {
            return OrHaving(column, "=", value);
        }

        public QueryBuilder OrHavingNot(string column, object value)
        {
            return OrHavingNot(column, "=", value);
        }

        /// <summary>
        ///     Perform a Having constraint
        /// </summary>
        /// <param name="constraints"></param>
        /// <returns></returns>
        public QueryBuilder Having(object constraints)
        {
            var dictionary = new Dictionary<string, object?>();

            foreach (var item in constraints.GetType().GetRuntimeProperties())
                dictionary.Add(item.Name, item.GetValue(constraints));

            return Having(dictionary);
        }

        public QueryBuilder Having(IEnumerable<KeyValuePair<string, object?>> values)
        {
            var query = this;
            var orFlag = GetOr();
            var notFlag = GetNot();

            foreach (var tuple in values)
            {
                if (orFlag)
                    query.Or();
                else
                    query.And();

                query = Not(notFlag).Having(tuple.Key, tuple.Value);
            }

            return query;
        }

        public QueryBuilder HavingRaw(string sql, params object[] bindings)
        {
            return AddComponent(new RawConditionBuilder
            {
                Engine = EngineScope,
                Component = "having",
                Expression = sql,
                Bindings = bindings,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public QueryBuilder OrHavingRaw(string sql, params object[] bindings)
        {
            return Or().HavingRaw(sql, bindings);
        }

        /// <summary>
        ///     Apply a nested Having clause
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public QueryBuilder Having(Func<QueryBuilder, QueryBuilder> callback)
        {
            var query = callback.Invoke(NewChild());

            return AddComponent(new NestedConditionBuilder
            {
                Engine = EngineScope,
                Component = "having",
                Query = query,
                IsNot = GetNot(),
                IsOr = GetOr()
            });
        }

        public QueryBuilder HavingNot(Func<QueryBuilder, QueryBuilder> callback)
        {
            return Not().Having(callback);
        }

        public QueryBuilder OrHaving(Func<QueryBuilder, QueryBuilder> callback)
        {
            return Or().Having(callback);
        }

        public QueryBuilder OrHavingNot(Func<QueryBuilder, QueryBuilder> callback)
        {
            return Not().Or().Having(callback);
        }

        public QueryBuilder HavingColumns(string first, string op, string second)
        {
            return AddComponent(new TwoColumnsConditionBuilder
            {
                Engine = EngineScope,
                Component = "having",
                First = first,
                Second = second,
                Operator = op,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public QueryBuilder OrHavingColumns(string first, string op, string second)
        {
            return Or().HavingColumns(first, op, second);
        }

        public QueryBuilder HavingNull(string column)
        {
            return AddComponent(new NullConditionBuilder
            {
                Engine = EngineScope,
                Component = "having",
                Column = column,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public QueryBuilder HavingNotNull(string column)
        {
            return Not().HavingNull(column);
        }

        public QueryBuilder OrHavingNull(string column)
        {
            return Or().HavingNull(column);
        }

        public QueryBuilder OrHavingNotNull(string column)
        {
            return Or().Not().HavingNull(column);
        }

        public QueryBuilder HavingTrue(string column)
        {
            return AddComponent(new BooleanConditionBuilder
            {
                IsOr = false,
                IsNot = false,
                Engine = EngineScope,
                Component = "having",
                Column = column,
                Value = true
            });
        }

        public QueryBuilder OrHavingTrue(string column)
        {
            return Or().HavingTrue(column);
        }

        public QueryBuilder HavingFalse(string column)
        {
            return AddComponent(new BooleanConditionBuilder
            {
                IsOr = false,
                IsNot = false,
                Engine = EngineScope,
                Component = "having",
                Column = column,
                Value = false
            });
        }

        public QueryBuilder OrHavingFalse(string column)
        {
            return Or().HavingFalse(column);
        }

        public QueryBuilder HavingLike(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        {
            return AddComponent(new BasicStringConditionBuilder
            {
                Engine = EngineScope,
                Component = "having",
                Operator = "like",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                EscapeCharacter = escapeCharacter,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public QueryBuilder HavingNotLike(string column, object value, bool caseSensitive = false,
            string? escapeCharacter = null)
        {
            return Not().HavingLike(column, value, caseSensitive, escapeCharacter);
        }

        public QueryBuilder OrHavingLike(string column, object value, bool caseSensitive = false,
            string? escapeCharacter = null)
        {
            return Or().HavingLike(column, value, caseSensitive, escapeCharacter);
        }

        public QueryBuilder OrHavingNotLike(string column, object value, bool caseSensitive = false,
            string? escapeCharacter = null)
        {
            return Or().Not().HavingLike(column, value, caseSensitive, escapeCharacter);
        }

        public QueryBuilder HavingStarts(string column, object value, bool caseSensitive = false,
            string? escapeCharacter = null)
        {
            return AddComponent(new BasicStringConditionBuilder
            {
                Engine = EngineScope,
                Component = "having",
                Operator = "starts",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                EscapeCharacter = escapeCharacter,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public QueryBuilder HavingNotStarts(string column, object value, bool caseSensitive = false,
            string? escapeCharacter = null)
        {
            return Not().HavingStarts(column, value, caseSensitive, escapeCharacter);
        }

        public QueryBuilder OrHavingStarts(string column, object value, bool caseSensitive = false,
            string? escapeCharacter = null)
        {
            return Or().HavingStarts(column, value, caseSensitive, escapeCharacter);
        }

        public QueryBuilder OrHavingNotStarts(string column, object value, bool caseSensitive = false,
            string? escapeCharacter = null)
        {
            return Or().Not().HavingStarts(column, value, caseSensitive, escapeCharacter);
        }

        public QueryBuilder HavingEnds(string column, object value, bool caseSensitive = false, string? escapeCharacter = null)
        {
            return AddComponent(new BasicStringConditionBuilder
            {
                Engine = EngineScope,
                Component = "having",
                Operator = "ends",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                EscapeCharacter = escapeCharacter,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public QueryBuilder HavingNotEnds(string column, object value, bool caseSensitive = false,
            string? escapeCharacter = null)
        {
            return Not().HavingEnds(column, value, caseSensitive, escapeCharacter);
        }

        public QueryBuilder OrHavingEnds(string column, object value, bool caseSensitive = false,
            string? escapeCharacter = null)
        {
            return Or().HavingEnds(column, value, caseSensitive, escapeCharacter);
        }

        public QueryBuilder OrHavingNotEnds(string column, object value, bool caseSensitive = false,
            string? escapeCharacter = null)
        {
            return Or().Not().HavingEnds(column, value, caseSensitive, escapeCharacter);
        }

        public QueryBuilder HavingContains(string column, object value, bool caseSensitive = false,
            string? escapeCharacter = null)
        {
            return AddComponent(new BasicStringConditionBuilder
            {
                Engine = EngineScope,
                Component = "having",
                Operator = "contains",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                EscapeCharacter = escapeCharacter,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public QueryBuilder HavingNotContains(string column, object value, bool caseSensitive = false,
            string? escapeCharacter = null)
        {
            return Not().HavingContains(column, value, caseSensitive, escapeCharacter);
        }

        public QueryBuilder OrHavingContains(string column, object value, bool caseSensitive = false,
            string? escapeCharacter = null)
        {
            return Or().HavingContains(column, value, caseSensitive, escapeCharacter);
        }

        public QueryBuilder OrHavingNotContains(string column, object value, bool caseSensitive = false,
            string? escapeCharacter = null)
        {
            return Or().Not().HavingContains(column, value, caseSensitive, escapeCharacter);
        }

        public QueryBuilder HavingBetween<T>(string column, T lower, T higher)
        {
            return AddComponent(new BetweenConditionBuilder<T>
            {
                Engine = EngineScope,
                Component = "having",
                Column = column,
                IsOr = GetOr(),
                IsNot = GetNot(),
                Lower = lower,
                Higher = higher
            });
        }

        public QueryBuilder OrHavingBetween<T>(string column, T lower, T higher)
        {
            return Or().HavingBetween(column, lower, higher);
        }

        public QueryBuilder HavingNotBetween<T>(string column, T lower, T higher)
        {
            return Not().HavingBetween(column, lower, higher);
        }

        public QueryBuilder OrHavingNotBetween<T>(string column, T lower, T higher)
        {
            return Or().Not().HavingBetween(column, lower, higher);
        }

        public QueryBuilder HavingIn<T>(string column, IEnumerable<T> values)
        {
            // If the developer has passed a string they most likely want a List<string>
            // since string is considered as List<char>
            if (values is string val)
            {
                return AddComponent(new InConditionBuilder<string>
                {
                    Engine = EngineScope,
                    Component = "having",
                    Column = column,
                    IsOr = GetOr(),
                    IsNot = GetNot(),
                    Values = ImmutableArray.Create(val)
                });
            }

            return AddComponent(new InConditionBuilder<T>
            {
                Engine = EngineScope,
                Component = "having",
                Column = column,
                IsOr = GetOr(),
                IsNot = GetNot(),
                Values = values.Distinct().ToImmutableArray()
            });
        }

        public QueryBuilder OrHavingIn<T>(string column, IEnumerable<T> values)
        {
            return Or().HavingIn(column, values);
        }

        public QueryBuilder HavingNotIn<T>(string column, IEnumerable<T> values)
        {
            return Not().HavingIn(column, values);
        }

        public QueryBuilder OrHavingNotIn<T>(string column, IEnumerable<T> values)
        {
            return Or().Not().HavingIn(column, values);
        }


        public QueryBuilder HavingIn(string column, QueryBuilder query)
        {
            return AddComponent(new InQueryConditionBuilder
            {
                Engine = EngineScope,
                Component = "having",
                Column = column,
                IsOr = GetOr(),
                IsNot = GetNot(),
                Query = query
            });
        }

        public QueryBuilder HavingIn(string column, Func<QueryBuilder, QueryBuilder> callback)
        {
            var query = callback.Invoke(new QueryBuilder());

            return HavingIn(column, query);
        }

        public QueryBuilder OrHavingIn(string column, QueryBuilder query)
        {
            return Or().HavingIn(column, query);
        }

        public QueryBuilder OrHavingIn(string column, Func<QueryBuilder, QueryBuilder> callback)
        {
            return Or().HavingIn(column, callback);
        }

        public QueryBuilder HavingNotIn(string column, QueryBuilder query)
        {
            return Not().HavingIn(column, query);
        }

        public QueryBuilder HavingNotIn(string column, Func<QueryBuilder, QueryBuilder> callback)
        {
            return Not().HavingIn(column, callback);
        }

        public QueryBuilder OrHavingNotIn(string column, QueryBuilder query)
        {
            return Or().Not().HavingIn(column, query);
        }

        public QueryBuilder OrHavingNotIn(string column, Func<QueryBuilder, QueryBuilder> callback)
        {
            return Or().Not().HavingIn(column, callback);
        }


        /// <summary>
        ///     Perform a sub query Having clause
        /// </summary>
        /// <param name="column"></param>
        /// <param name="op"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public QueryBuilder Having(string column, string op, Func<QueryBuilder, QueryBuilder> callback)
        {
            var query = callback.Invoke(NewChild());

            return Having(column, op, query);
        }

        public QueryBuilder Having(string column, string op, QueryBuilder query)
        {
            return AddComponent(new QueryConditionBuilder
            {
                Engine = EngineScope,
                Component = "having",
                Column = column,
                Operator = op,
                Query = query,
                IsNot = GetNot(),
                IsOr = GetOr()
            });
        }

        public QueryBuilder OrHaving(string column, string op, QueryBuilder query)
        {
            return Or().Having(column, op, query);
        }

        public QueryBuilder OrHaving(string column, string op, Func<QueryBuilder, QueryBuilder> callback)
        {
            return Or().Having(column, op, callback);
        }

        public QueryBuilder HavingExists(QueryBuilder query)
        {
            if (!query.HasComponent("from"))
                throw new ArgumentException(
                    $"{nameof(FromClause)} cannot be empty if used inside a {nameof(HavingExists)} condition");

            // simplify the query as much as possible
            query = query.Clone().RemoveComponent("select")
                .SelectRaw("1")
                .Limit(1);

            return AddComponent(new ExistsConditionBuilder
            {
                Engine = EngineScope,
                Component = "having",
                Query = query,
                IsNot = GetNot(),
                IsOr = GetOr()
            });
        }

        public QueryBuilder HavingExists(Func<QueryBuilder, QueryBuilder> callback)
        {
            var childQuery = new QueryBuilder().SetParent(this);
            return HavingExists(callback.Invoke(childQuery));
        }

        public QueryBuilder HavingNotExists(QueryBuilder query)
        {
            return Not().HavingExists(query);
        }

        public QueryBuilder HavingNotExists(Func<QueryBuilder, QueryBuilder> callback)
        {
            return Not().HavingExists(callback);
        }

        public QueryBuilder OrHavingExists(QueryBuilder query)
        {
            return Or().HavingExists(query);
        }

        public QueryBuilder OrHavingExists(Func<QueryBuilder, QueryBuilder> callback)
        {
            return Or().HavingExists(callback);
        }

        public QueryBuilder OrHavingNotExists(QueryBuilder query)
        {
            return Or().Not().HavingExists(query);
        }

        public QueryBuilder OrHavingNotExists(Func<QueryBuilder, QueryBuilder> callback)
        {
            return Or().Not().HavingExists(callback);
        }

        #region date

        public QueryBuilder HavingDatePart(string part, string column, string op, object value)
        {
            return AddComponent(new BasicDateConditionBuilder
            {
                Engine = EngineScope,
                Component = "having",
                Operator = op,
                Column = column,
                Value = value,
                Part = part,
                IsOr = GetOr(),
                IsNot = GetNot()
            });
        }

        public QueryBuilder HavingNotDatePart(string part, string column, string op, object value)
        {
            return Not().HavingDatePart(part, column, op, value);
        }

        public QueryBuilder OrHavingDatePart(string part, string column, string op, object value)
        {
            return Or().HavingDatePart(part, column, op, value);
        }

        public QueryBuilder OrHavingNotDatePart(string part, string column, string op, object value)
        {
            return Or().Not().HavingDatePart(part, column, op, value);
        }

        public QueryBuilder HavingDate(string column, string op, object value)
        {
            return HavingDatePart("date", column, op, value);
        }

        public QueryBuilder HavingNotDate(string column, string op, object value)
        {
            return Not().HavingDate(column, op, value);
        }

        public QueryBuilder OrHavingDate(string column, string op, object value)
        {
            return Or().HavingDate(column, op, value);
        }

        public QueryBuilder OrHavingNotDate(string column, string op, object value)
        {
            return Or().Not().HavingDate(column, op, value);
        }

        public QueryBuilder HavingTime(string column, string op, object value)
        {
            return HavingDatePart("time", column, op, value);
        }

        public QueryBuilder HavingNotTime(string column, string op, object value)
        {
            return Not().HavingTime(column, op, value);
        }

        public QueryBuilder OrHavingTime(string column, string op, object value)
        {
            return Or().HavingTime(column, op, value);
        }

        public QueryBuilder OrHavingNotTime(string column, string op, object value)
        {
            return Or().Not().HavingTime(column, op, value);
        }

        public QueryBuilder HavingDatePart(string part, string column, object value)
        {
            return HavingDatePart(part, column, "=", value);
        }

        public QueryBuilder HavingNotDatePart(string part, string column, object value)
        {
            return HavingNotDatePart(part, column, "=", value);
        }

        public QueryBuilder OrHavingDatePart(string part, string column, object value)
        {
            return OrHavingDatePart(part, column, "=", value);
        }

        public QueryBuilder OrHavingNotDatePart(string part, string column, object value)
        {
            return OrHavingNotDatePart(part, column, "=", value);
        }

        public QueryBuilder HavingDate(string column, object value)
        {
            return HavingDate(column, "=", value);
        }

        public QueryBuilder HavingNotDate(string column, object value)
        {
            return HavingNotDate(column, "=", value);
        }

        public QueryBuilder OrHavingDate(string column, object value)
        {
            return OrHavingDate(column, "=", value);
        }

        public QueryBuilder OrHavingNotDate(string column, object value)
        {
            return OrHavingNotDate(column, "=", value);
        }

        public QueryBuilder HavingTime(string column, object value)
        {
            return HavingTime(column, "=", value);
        }

        public QueryBuilder HavingNotTime(string column, object value)
        {
            return HavingNotTime(column, "=", value);
        }

        public QueryBuilder OrHavingTime(string column, object value)
        {
            return OrHavingTime(column, "=", value);
        }

        public QueryBuilder OrHavingNotTime(string column, object value)
        {
            return OrHavingNotTime(column, "=", value);
        }

        #endregion
    }
}
