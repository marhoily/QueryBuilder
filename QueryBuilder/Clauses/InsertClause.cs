namespace SqlKata
{
    public abstract class AbstractInsertClause : AbstractClause
    {
    }

    public class InsertClause : AbstractInsertClause
    {
        public List<string> Columns { get; set; }
        public List<object> Values { get; set; }
        public bool ReturnId { get; set; }
    }

    public class InsertQueryClause : AbstractInsertClause
    {
        public List<string> Columns { get; set; }
        public Query Query { get; set; }
    }
}
