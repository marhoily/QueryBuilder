namespace SqlKata
{
    public partial class QueryBuilder
    {
        public QueryBuilder AsDelete()
        {
            Method = "delete";
            return this;
        }
    }
}
