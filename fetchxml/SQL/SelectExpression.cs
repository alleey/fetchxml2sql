using System;
using System.Collections.Generic;

namespace FetchXml.SQL
{
    public interface LiteralValue {}

    [Serializable]
    public class StringValue : LiteralValue
    {
        public string Value { set; get; }
    }

    public class MultiStringValue : LiteralValue
    {
        public MultiStringValue()
        {
            Values = new List<string>();
        }
        public List<string> Values { set; get; }
    }

    [Serializable]
    public class SelectExpression
    {
        [Serializable]
        public class AliasName
        {
            public string Name { set; get; }
            public string Alias { set; get; }
        }

        [Serializable]
        public class Attribute : AliasName
        {
            public string Expression { set; get; }
            public string Aggregate { set; get; }
            public bool Distinct { set; get; }
        }

        [Serializable]
        public class Condition
        {
            public Attribute Lhs { set; get; }
            public string Operator { set; get; }
            public LiteralValue Rhs { set; get; }
        }

        [Serializable]
        public class Order
        {
            public enum Type
            {
                Ascending,
                Descending
            }
            public Type OrderType { set; get; }
            public AliasName Column { set; get; }
        }

        [Serializable]
        public class Filter
        {
            public enum Type
            {
                And,
                Or
            }
            public Filter()
            {
                FilterType = Type.And;
                Conditions = new List<Condition>();
                SubFilters = new List<Filter>();
            }
            public Type FilterType { set; get; }
            public List<Condition> Conditions { private set; get; }
            public List<Filter> SubFilters { private set; get; }
        }

        [Serializable]
        public class JoinCondition
        {
            public string Lhs { set; get; }
            public string Rhs { set; get; }
        }

        [Serializable]
        public class Join
        {
            public enum Type
            {
                Inner = 0,
                Left = 1,
                Right = 2,
                Outer = 3
            }
            public Join()
            {
                Conditions = new List<JoinCondition>();
            }

            public Type JoinType { set; get; }
            public AliasName Source { set; get; }
            public AliasName Target { set; get; }
            public List<JoinCondition> Conditions { private set; get; }
        }

        public SelectExpression()
        {
            Attributes = new List<Attribute>();
            Joins = new List<Join>();
            OrderBy = new List<Order>();
            GroupBy = new List<AliasName>();
        }

        public AliasName Source { set; get; }
        public int? Top { set; get; }
        public int? Skip { set; get; }
        public bool Distinct { set; get; }
        public bool Aggregate { set; get; }

        public List<Attribute> Attributes { private set; get; }
        public Filter Where { set; get; }
        public List<Join> Joins { private set; get; }
        public List<Order> OrderBy { private set; get; }
        public List<AliasName> GroupBy { private set; get; }
        public Filter Having { set; get; }

        public void Normalize()
        {
            Joins.Sort((x, y) => x.JoinType.CompareTo(y.JoinType));
        } 
    }

    public static class SelectExpressionExtensions
    {
        static public bool HasAlias(this SelectExpression expression) => !string.IsNullOrEmpty(expression.Source.Alias);
        static public bool HasOrderBy(this SelectExpression expression) => expression.OrderBy.Count > 0;
        static public bool HasJoins(this SelectExpression expression) => expression.Joins.Count > 0;
        static public bool HasFilter(this SelectExpression expression) => expression.Where != null && expression.Where.IsSpecified();
        static public bool HasGroupBy(this SelectExpression expression) => expression.GroupBy.Count > 0;

        static public bool HasAlias(this SelectExpression.AliasName expression) => !string.IsNullOrEmpty(expression.Alias);
        static public bool HasName(this SelectExpression.AliasName expression) => !string.IsNullOrEmpty(expression.Name);
        static public bool IsWildcard(this SelectExpression.Attribute expression) => expression.Expression == "*";
        static public bool IsAggregate(this SelectExpression.Attribute expression) => !string.IsNullOrEmpty(expression.Aggregate);

        static public bool HasConditions(this SelectExpression.Filter expression) => expression.Conditions.Count > 0;
        static public bool HasSubfilters(this SelectExpression.Filter expression) => expression.SubFilters.Count > 0;
        static public bool IsSpecified(this SelectExpression.Filter expression) => expression.HasConditions() || expression.HasSubfilters();
    }
}
