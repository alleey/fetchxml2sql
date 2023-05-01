namespace FetchXml.SQL
{
    public class SelectAttributeBuilder
    {
        private SelectExpression.Attribute _attr = new SelectExpression.Attribute();

        public SelectExpression.Attribute Build() => _attr;

        public SelectAttributeBuilder Attribute(string expression, string alias = null, string name = null, string aggregate = null) {
            _attr.Expression = expression;
            _attr.Alias = alias;
            _attr.Name = name;
            _attr.Aggregate = aggregate;
            return this;
        }

        public SelectAttributeBuilder Expression(string expression = null) {
            _attr.Expression = expression;
            return this;
        }

        public SelectAttributeBuilder Alias(string alias = null) {
            _attr.Alias = alias;
            return this;
        }

        public SelectAttributeBuilder Name(string name = null) {
            _attr.Name = name;
            return this;
        }

        public SelectAttributeBuilder Aggregate(string aggregate = null) {
            _attr.Aggregate = aggregate;
            return this;
        }

        public SelectAttributeBuilder Distinct(bool distinct = true) {
            _attr.Distinct = distinct;
            return this;
        }
    }
}
