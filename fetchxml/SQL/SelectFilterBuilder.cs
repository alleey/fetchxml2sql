namespace FetchXml.SQL
{
    public class SelectFilterBuilder
    {
        private SelectExpression.Filter _filter = new SelectExpression.Filter();

        public SelectExpression.Filter Build() => _filter;

        public SelectFilterBuilder And() {
            _filter.FilterType = SelectExpression.Filter.Type.And;
            return this;
        }

        public SelectFilterBuilder Or() {
            _filter.FilterType = SelectExpression.Filter.Type.Or;
            return this;
        }

        public SelectFilterBuilder Condition(SelectExpression.Attribute lhs, string oper, LiteralValue rhs) {
            _filter.Conditions.Add(new SelectExpression.Condition() {
                Lhs = lhs,
                Operator = oper,
                Rhs = rhs
            });
            return this;
        }

        public SelectFilterBuilder SubFilter(params SelectExpression.Filter[] subfilter) {
            foreach(var filter in subfilter)
                _filter.SubFilters.Add(filter);
            return this;
        }
    }
}
