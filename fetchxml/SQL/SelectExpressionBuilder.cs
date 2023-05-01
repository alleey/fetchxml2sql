namespace FetchXml.SQL
{
    public class SelectExpressionBuilder
    {
        private SelectExpression _expr = new SelectExpression();

        public SelectExpression Build(bool normalize)
        {
            if(normalize)
                _expr.Normalize();
            return _expr;
        }

        public SelectExpressionBuilder Distinct(bool distinct = true)
        {
            _expr.Distinct = distinct;
            return this;
        }

        public SelectExpressionBuilder Top(int top)
        {
            _expr.Top = top;
            return this;
        }

        public SelectExpressionBuilder Skip(int skip)
        {
            _expr.Skip = skip;
            return this;
        }

        public SelectExpressionBuilder Attribute(string expression, string alias = null, string name = null, string aggregate = null)
        {
            _expr.Attributes.Add(new SelectExpression.Attribute()
            {
                Expression = expression,
                Alias = alias,
                Name = name,
                Aggregate = aggregate
            });
            return this;
        }

        public SelectExpressionBuilder Attribute(SelectExpression.Attribute attribute)
        {
            _expr.Attributes.Add(attribute);
            return this;
        }

        public SelectExpressionBuilder From(string table, string alias = null)
        {
            _expr.Source = new SelectExpression.AliasName() {
                Name = table,
                Alias = alias
            };
            return this;
        }
        public SelectExpressionBuilder From(SelectExpression.AliasName aliasName)
        {
            _expr.Source = aliasName;
            return this;
        }

        public SelectExpressionBuilder Join(SelectExpression.Join join)
        {
            _expr.Joins.Add(join);
            return this;
        }

        public SelectExpressionBuilder Where(SelectExpression.Filter filter)
        {
            if(_expr.Where == null)
                _expr.Where = filter;
            else
                _expr.Where.SubFilters.Add(filter);
            return this;
        }

        public SelectExpressionBuilder GroupBy(string expression, string alias = null)
        {
            _expr.GroupBy.Add(new SelectExpression.AliasName()
            {
                Name = expression,
                Alias = alias
            });
            return this;
        }

        public SelectExpressionBuilder OrderBy(SelectExpression.Order.Type type, string column, string alias = null)
        {
            _expr.OrderBy.Add(new SelectExpression.Order() {
                Column = new SelectExpression.AliasName() { Name=column, Alias=alias },
                OrderType = type
             });
            return this;
        }

    }
}
