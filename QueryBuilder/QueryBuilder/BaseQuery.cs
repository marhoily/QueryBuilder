using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using SqlKata.Extensions;

namespace SqlKata
{
    public partial class QueryBuilder
    {
        public QueryBuilder? Parent;
        public string? EngineScope;
        private bool _notFlag;

        private bool _orFlag;

        public List<AbstractClauseBuilder> Clauses { get; set; } = new();

        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> CacheDictionaryProperties = new();

        private string? _comment;
        public List<IncludeBuilder> Includes = new();
        public Dictionary<string, object?> Variables = new();
        public bool IsDistinct { get; set; }
        // Mandatory for CTE queries
        public string? QueryAlias { get; set; }
        public string Method { get; set; } = "select";

        
        public Query Build()
        {
            return new Query
            {
                QueryAlias = QueryAlias,
                Method = Method
            };
        }

        public QueryBuilder SetEngineScope(string? engine)
        {
            EngineScope = engine;

            return this;
        }


        public QueryBuilder SetParent(QueryBuilder? parent)
        {
            if (this == parent)
                throw new ArgumentException($"Cannot set the same {nameof(QueryBuilder)} as a parent of itself");

            Parent = parent;
            return this;
        }

        public QueryBuilder NewChild()
        {
            var newQuery = new QueryBuilder().SetParent(this);
            newQuery.EngineScope = EngineScope;
            return newQuery;
        }

        /// <summary>
        ///     Add a component clause to the QueryBuilder.
        /// </summary>
        public QueryBuilder AddComponent(AbstractClauseBuilder clause)
        {
            Debug.Assert(clause.Component != null);
            Clauses.Add(clause);

            return this;
        }

        /// <summary>
        ///     If the QueryBuilder already contains a clause for the given component
        ///     and engine, replace it with the specified clause. Otherwise, just
        ///     add the clause.
        /// </summary>
        /// <param name="clause"></param>
        /// <returns></returns>
        public QueryBuilder AddOrReplaceComponent(AbstractClauseBuilder clause)
        {
            var countRemoved = Clauses.RemoveAll(
                c => c.Component == clause.Component &&
                     c.Engine == clause.Engine);
            if (countRemoved > 1) throw
                new InvalidOperationException("AddOrReplaceComponent cannot replace a component when there is more than one component to replace!");

            return AddComponent(clause);
        }


        /// <summary>
        ///     Get the list of clauses for a component.
        /// </summary>
        /// <returns></returns>
        public List<TC> GetComponents<TC>(string component, string? engineCode = null) where TC : AbstractClause
        {
            engineCode ??= EngineScope;
            return Clauses
                .Where(x => x.Component == component)
                .Where(x => engineCode == null || x.Engine == null || engineCode == x.Engine)
                .Cast<TC>()
                .ToList();
        }

        /// <summary>
        ///     Get the list of clauses for a component.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="engineCode"></param>
        /// <returns></returns>
        public List<AbstractClause> GetComponents(string component, string? engineCode = null)
        {
            engineCode ??= EngineScope;

            return GetComponents<AbstractClause>(component, engineCode);
        }

        /// <summary>
        ///     Get a single component clause from the QueryBuilder.
        /// </summary>
        /// <returns></returns>
        public TC? GetOneComponent<TC>(string component, string? engineCode = null) where TC : AbstractClause
        {
            engineCode ??= EngineScope;

            var all = GetComponents<TC>(component, engineCode);
            return all.FirstOrDefault(c => c.Engine == engineCode) ??
                   all.FirstOrDefault(c => c.Engine == null);
        }

        /// <summary>
        ///     Get a single component clause from the QueryBuilder.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="engineCode"></param>
        /// <returns></returns>
        public AbstractClause? GetOneComponent(string component, string? engineCode = null)
        {
            engineCode ??= EngineScope;

            return GetOneComponent<AbstractClause>(component, engineCode);
        }

        /// <summary>
        ///     Return whether the QueryBuilder has clauses for a component.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="engineCode"></param>
        /// <returns></returns>
        public bool HasComponent(string component, string? engineCode = null)
        {
            engineCode ??= EngineScope;

            return GetComponents(component, engineCode).Any();
        }

        /// <summary>
        ///     Remove all clauses for a component.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="engineCode"></param>
        /// <returns></returns>
        public QueryBuilder RemoveComponent(string component, string? engineCode = null)
        {
            engineCode ??= EngineScope;

            Clauses = Clauses
                .Where(x => !(x.Component == component &&
                              (engineCode == null || x.Engine == null || engineCode == x.Engine)))
                .ToList();

            return this;
        }

        /// <summary>
        ///     Set the next boolean operator to "and" for the "where" clause.
        /// </summary>
        /// <returns></returns>
        public QueryBuilder And()
        {
            _orFlag = false;
            return this;
        }

        /// <summary>
        ///     Set the next boolean operator to "or" for the "where" clause.
        /// </summary>
        /// <returns></returns>
        public QueryBuilder Or()
        {
            _orFlag = true;
            return this;
        }

        /// <summary>
        ///     Set the next "not" operator for the "where" clause.
        /// </summary>
        /// <returns></returns>
        public QueryBuilder Not(bool flag = true)
        {
            _notFlag = flag;
            return this;
        }

        /// <summary>
        ///     Get the boolean operator and reset it to "and"
        /// </summary>
        /// <returns></returns>
        public bool GetOr()
        {
            var ret = _orFlag;

            // reset the flag
            _orFlag = false;
            return ret;
        }

        /// <summary>
        ///     Get the "not" operator and clear it
        /// </summary>
        /// <returns></returns>
        public bool GetNot()
        {
            var ret = _notFlag;

            // reset the flag
            _notFlag = false;
            return ret;
        }

        /// <summary>
        ///     Add a from Clause
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public QueryBuilder From(GTable table)
        {
            return AddOrReplaceComponent(new FromClauseBuilder
            {
                Engine = EngineScope,
                Component = "from",
                Table = table.Name
            });
        }
        public QueryBuilder From(string table)
        {
            return AddOrReplaceComponent(new FromClauseBuilder
            {
                Engine = EngineScope,
                Component = "from",
                Table = table
            });
        }

        public QueryBuilder From(QueryBuilder QueryBuilder, string? alias = null)
        {
            QueryBuilder = QueryBuilder.Clone();
            QueryBuilder.SetParent(this);

            if (alias != null) QueryBuilder.As(alias);


            return AddOrReplaceComponent(new QueryFromClauseBuilder
            {
                Engine = EngineScope,
                Component = "from",
                Query = QueryBuilder
            });
        }

        public QueryBuilder FromRaw(string sql, params object[] bindings)
        {
            ArgumentNullException.ThrowIfNull(sql);
            ArgumentNullException.ThrowIfNull(bindings);
            return AddOrReplaceComponent(new RawFromClauseBuilder
            {
                Engine = EngineScope,
                Component = "from",
                Expression = sql,
                Bindings = bindings.ToImmutableArray()
            });
        }

        public QueryBuilder From(Func<QueryBuilder, QueryBuilder> callback, string? alias = null)
        {
            var queryBuilder = new QueryBuilder();

            queryBuilder.SetParent(this);

            return From(callback.Invoke(queryBuilder), alias);
        }
    }
}
