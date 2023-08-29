using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public sealed record QValueInsert(
        AbstractFrom From,
        InsertClause InsertClause,
        QLazyList<object?>[] Values) :Q
    {
        public override void Render(StringBuilder sb, Renderer r)
        {
            return MList(" ", MInsertStartClause());
        }

        private object MInsertStartClause()
        {
            sb.Append(IsMultiValue
                ? r.MultiInsertStartClause
                : r.SingleInsertStartClause);

        }

        private object MList(string s)
        {
            throw new NotImplementedException();
        }
    }
}
