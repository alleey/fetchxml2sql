using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FetchXml.Dialect;
using FetchXml.SQL;

namespace FetchXml.Formatter
{
    public class SqlServerSqlFormatter : ISqlFormatter
    {
        public class Options
        {
            public Options()
            {
                Pretty = true;
                SpaceChar = ' ';
                Comma = ", ";
                TabLength = 3;
            }

            public bool Pretty { get; set; }
            public int TabLength { get; set; }
            public char SpaceChar { get; set; }
            public string Comma { get; set; }
            public string Space
            {
                get
                {
                    return $"{SpaceChar}";
                }
            }
        }

        class FormatterContext
        {
            private Options _options = null;
            private int _level = 0;
            public FormatterContext(Options options, int level = 0)
            {
                _options = options;
                _level = level;
            }

            public string Space => _options.Space;
            public string Comma => _options.Comma;
            public string Indent => !_options.Pretty ? _options.Space : $"\n".PadRight(_level * _options.TabLength, _options.SpaceChar);

            public IDisposable NewIndent(int level = 1) 
            {
                _level += level;
                return new OnDisposeAction(() => _level-=level );
            }
            class OnDisposeAction : IDisposable
            {
                Action _finalizer;
                public OnDisposeAction(Action act) => _finalizer = act;
                void IDisposable.Dispose() => _finalizer();
            };
        }

        private Options _options;
        private ISqlDialect _dialect;

        public SqlServerSqlFormatter(ISqlDialect dialect, Options options = null)
        {
            _dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
            _options = options ?? new Options();
        }

        public string Format(SelectExpression expression)
        {
            var sb = new StringBuilder();
            var ctx = new FormatterContext(_options);

            sb.Append("select");
            if(expression.Distinct)
                sb.Append(ctx.Space).Append("distinct");
            if(expression.Top.HasValue)
                sb.Append(ctx.Space).Append($"top").Append(ctx.Space).Append(expression.Top.Value);

            var entityAlias = expression.Source.Alias;
            if(string.IsNullOrEmpty(entityAlias) && expression.HasJoins())
                entityAlias = expression.Source.Name;
            var attributes = expression.Attributes.Select(attr => Format(attr, entityAlias, ctx)).ToArray();

            using (ctx.NewIndent())
            {
                sb.Append(ctx.Indent);
                sb.Append(string.Join(ctx.Comma + ctx.Indent, attributes));
            }

            sb.Append(ctx.Indent).Append("from").Append(ctx.Space);
            sb.Append(_dialect.EscapeIdentifier(expression.Source.Name));
            if(string.IsNullOrEmpty(entityAlias) && expression.HasJoins())
                sb.Append(ctx.Space).Append($"as").Append(ctx.Space).Append(entityAlias);

            if(expression.HasJoins())
            {
                using (ctx.NewIndent())
                {
                    var joins = expression.Joins.Select(join => Format(join, entityAlias, ctx)).ToArray();
                    sb.Append(ctx.Indent);
                    sb.Append(string.Join(ctx.Indent, joins));
                }
            }

            if(expression.HasFilter())
            {
                sb.Append(ctx.Indent).Append($"where");
                using (ctx.NewIndent())
                {
                    sb.Append(ctx.Indent);
                    sb.Append(Format(expression.Where, entityAlias, ctx));
                }
            }

            if(expression.HasOrderBy())
            {
                sb.Append(ctx.Indent).Append($"order by");
                using (ctx.NewIndent())
                {
                    sb.Append(ctx.Indent);
                    sb.Append(Format(expression.OrderBy, entityAlias, ctx));
                }
            }

            if(expression.HasGroupBy())
            {
                var groups = expression.GroupBy.Select(group => Format(group, entityAlias, ctx)).ToArray();
                sb.Append(ctx.Indent).Append($"group by");
                using (ctx.NewIndent())
                {
                    sb.Append(ctx.Indent);
                    sb.Append(string.Join(ctx.Comma, groups));
                }
            }

            return sb.ToString();
        }

        string Format(SelectExpression.Join join, string entityAlias, FormatterContext ctx)
        {
            var joinType = _dialect.MapJoinType(join.JoinType);
            var targetName = _dialect.EscapeIdentifier(join.Target.Name);
            var targetAlias = string.IsNullOrEmpty(join.Target.Alias) ? join.Target.Name:join.Target.Alias;

            var conditions = string.Join(
                $"{ctx.Space}and{ctx.Space}",
                join.Conditions.Select(cond => {
                    var sourceAlias = string.IsNullOrEmpty(join.Source.Alias) ? join.Source.Name:join.Source.Alias;
                    return $"{sourceAlias}.{_dialect.EscapeIdentifier(cond.Lhs)} = {targetAlias}.{_dialect.EscapeIdentifier(cond.Rhs)}";
                })
            );
            using (ctx.NewIndent())
            {
                return $"{joinType}{ctx.Space}join{ctx.Space}{targetName}{ctx.Space}{targetAlias}{ctx.Space}on{ctx.Indent}({conditions})";
            }
        }

        string Format(SelectExpression.Filter filter, string entityAlias, FormatterContext ctx)
        {
            var conditions = new List<string>();

            if(filter.HasConditions())
                conditions.AddRange(
                    filter.Conditions.Select(cond => {
                        var attrName = Format(cond.Lhs, entityAlias, ctx);
                        return !(cond.Rhs is MultiStringValue) ? 
                            _dialect.ConvertOperator(attrName, cond.Operator, ((StringValue)cond.Rhs).Value):
                            _dialect.ConvertOperator(attrName, cond.Operator, ((MultiStringValue)cond.Rhs).Values.ToArray());
                    })
                );

            if(filter.HasSubfilters())
                conditions.AddRange(
                    filter.SubFilters.Select(filter => Format(filter, entityAlias, ctx))
                );

            var sb = new StringBuilder();
            sb.Append($"(");
            sb.Append(string.Join(
                filter.FilterType == SelectExpression.Filter.Type.And ? $"{ctx.Space}and{ctx.Space}":$"{ctx.Space}or{ctx.Space}",
                conditions
            ));
            sb.Append($")");
            return sb.ToString();
        }

        string Format(SelectExpression.AliasName aname, string entityAlias, FormatterContext ctx)
        {
            var alias = aname.HasAlias() ? aname.Alias : entityAlias;
            var name = _dialect.EscapeIdentifier(aname.Name);
            return string.IsNullOrEmpty(alias) ? name : $"{alias}.{name}";
        }

        string Format(SelectExpression.Attribute attr, string entityAlias, FormatterContext ctx)
        {
            var alias = attr.HasAlias() ? attr.Alias : entityAlias;
            var expr = attr.IsWildcard() ? attr.Expression : _dialect.EscapeIdentifier(attr.Expression);

            if(!string.IsNullOrEmpty(alias))
                expr = $"{alias}.{expr}";
            if(attr.Distinct)
                expr = $"distinct{ctx.Space}{expr}";
            if(attr.IsAggregate())
                expr = _dialect.ConvertAggregate(attr.Aggregate, expr);

            var name = attr.HasName() ? $"{ctx.Space}as{ctx.Space}{_dialect.EscapeStringValue(attr.Name)}" : string.Empty;
            return $"{expr}{name}";
        }

        string Format(List<SelectExpression.Order> orders, string entityAlias, FormatterContext ctx)
        {
            var sorts = orders.Select(order =>
            {
                var attr = Format(order.Column, entityAlias, ctx);
                var sort = order.OrderType == SelectExpression.Order.Type.Ascending ? "asc" : "desc";
                return $"{attr}{ctx.Space}{sort}";
            })
            .ToArray();
            return string.Join(ctx.Comma, sorts);
        }
    }
}
