using System;
namespace FetchXml.SQL
{
    public class SelectJoinBuilder
    {
        private SelectExpression.Join _join = new SelectExpression.Join();

        public SelectExpression.Join Build() => _join;

        public SelectJoinBuilder Join(string type) {
            _join.JoinType = SelectJoinBuilderHelper.Convert(type);
            return this;
        }

        public SelectJoinBuilder Source(string entityName, string alias = null) {
            _join.Source = new SelectExpression.AliasName() {
                Name = entityName,
                Alias = string.IsNullOrEmpty(entityName) ? entityName:alias
            };
            return this;
        }
        public SelectJoinBuilder Source(SelectExpression.AliasName aliasName) {
            _join.Source = aliasName;
            return this;
        }

        public SelectJoinBuilder Target(string entityName, string alias = null) {
            _join.Target = new SelectExpression.AliasName() {
                Name = entityName,
                Alias = string.IsNullOrEmpty(entityName) ? entityName:alias
            };
            return this;
        }
        public SelectJoinBuilder Target(SelectExpression.AliasName aliasName) {
            _join.Target = aliasName;
            return this;
        } 

        public SelectJoinBuilder On(string lhs, string rhs) {
            _join.Conditions.Add(new SelectExpression.JoinCondition() {
                Lhs = lhs, Rhs = rhs
            });
            return this;
        }
    }

    class SelectJoinBuilderHelper
    {
        public static SelectExpression.Join.Type Convert(string joinType)
        {
            switch(joinType)
            {
                case "inner": return SelectExpression.Join.Type.Inner;
                case "left": return SelectExpression.Join.Type.Left;
                case "left-outer": return SelectExpression.Join.Type.Left;
                case "right": return SelectExpression.Join.Type.Right;
                case "right-outer": return SelectExpression.Join.Type.Right;
                case "outer": return SelectExpression.Join.Type.Left;  // outer in fetchxml is left outer
                case "full-outer": return SelectExpression.Join.Type.Outer;
            }
            throw new ArgumentException($"Unrecognized join type {joinType}");
        }
    }
}
