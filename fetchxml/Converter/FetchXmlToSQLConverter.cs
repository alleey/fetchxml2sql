using System.Linq;
using System.Collections.ObjectModel;
using System.Text;
using FetchXml.Model;
using FetchXml.SQL;

namespace FetchXml.Converter
{
    public class FetchXmlToSQLConverter : IFetchXmlToSQLConverter
    {
        public class Options
        {
            public Options()
            {
                UseAliasOnRoot = false;
                NormalizeJoins = false;
            }

            public bool UseAliasOnRoot { get; set; }
            public bool NormalizeJoins { get; set; }
        }

        private Options _options;

        public FetchXmlToSQLConverter(Options options = null)
        {
            _options = options ?? new Options();
        }

        public SelectExpression[] Convert(FetchType fetchType)
        {
            return fetchType.Entity.Select(entity =>
            {
                var builder = new SelectExpressionBuilder();

                if (fetchType.DistinctSpecified && fetchType.Distinct)
                    builder.Distinct();
                if (fetchType.TopSpecified)
                    builder.Top(fetchType.Top);

                var entityName = new SelectExpression.AliasName()
                {
                    Name = entity.Name,
                    Alias = _options.UseAliasOnRoot ? entity.Name : null
                };
                builder.From(entityName);

                if (entity.AllAttributesSpecified)
                    builder.Attribute("*", entityName.Alias);
                else
                    Convert(entity.Attribute, entityName, builder);

                if (entity.FilterSpecified)
                    builder.Where(Convert(entity.Filter, entityName));

                if (entity.OrderSpecified)
                    Convert(entity.Order, entityName, builder);

                if (entity.LinkedEntitySpecified)
                    Convert(entity.LinkedEntity, entityName, builder);

                return builder.Build(_options.NormalizeJoins);
            }).ToArray();
        }

        private void Convert(Collection<Attribute> attributes, SelectExpression.AliasName entityName, SelectExpressionBuilder builder)
        {
            foreach (var attr in attributes)
            {
                var attribBuilder = new SelectAttributeBuilder();
                attribBuilder.Attribute(attr.Name, entityName.Alias, attr.Alias, attr.Aggregate);
                if(attr.DistinctSpecified)
                    attribBuilder.Distinct();

                builder.Attribute(attribBuilder.Build());
                if(attr.GroupbySpecified && attr.Groupby == FetchBoolType.True)
                    builder.GroupBy(attr.Name, entityName.Alias);
            }
        }

        private void Convert(Collection<LinkedEntityType> linkeds, SelectExpression.AliasName entityName, SelectExpressionBuilder builder)
        {
            foreach (var link in linkeds)
                Convert(link, entityName, builder);
        }

        private void Convert(LinkedEntityType entity, SelectExpression.AliasName sourceEntityName, SelectExpressionBuilder builder)
        {
            var targetEntityName = new SelectExpression.AliasName()
            {
                Name = entity.Name,
                Alias = entity.Alias
            };

            var jbuilder = new SelectJoinBuilder();
            jbuilder.Join(entity.Link_Type)
                .Source(sourceEntityName)
                .Target(targetEntityName)
                .On(entity.To, entity.From);

            builder.Join(jbuilder.Build());

            if (entity.AllAttributesSpecified)
            {
                builder.Attribute("*", targetEntityName.Alias);
            }
            else
            {
                Convert(entity.Attribute, targetEntityName, builder);
            }

            if (entity.FilterSpecified)
                builder.Where(Convert(entity.Filter, targetEntityName));

            if (entity.OrderSpecified)
                Convert(entity.Order, targetEntityName, builder);

            if (entity.LinkedEntitySpecified)
                Convert(entity.LinkedEntity, targetEntityName, builder);
        }

        private SelectExpression.Filter Convert(Filter filter, SelectExpression.AliasName entityName)
        {
            var builder = new SelectFilterBuilder();

            if (filter.Type == FilterType.And)
                builder.And();
            else
                builder.Or();

            if (filter.ConditionSpecified)
            {
                foreach (var cond in filter.Condition)
                {
                    string alias = !string.IsNullOrEmpty(cond.Alias) ? cond.Alias :
                        (!string.IsNullOrEmpty(cond.Entityname) ? cond.Entityname:entityName.Alias);
                    var lhs = new SelectExpression.Attribute()
                    {
                        Expression = cond.Attribute,
                        Alias = alias
                    };

                    LiteralValue rhs = null;
                    if (cond.ValueSpecified)
                    {
                        rhs = new MultiStringValue()
                        {
                            Values = cond.Value.Select(x => x.Value).ToList()
                        };
                    }
                    else
                    {
                        rhs = new StringValue()
                        {
                            Value = cond.Value_1
                        };
                    }
                    builder.Condition(lhs, cond.Operator, rhs);
                }
            }
            if (filter.FilterPropertySpecified)
            {
                builder.SubFilter(filter.FilterProperty.Select(f => Convert(f, entityName)).ToArray());
            }

            return builder.Build();
        }

        private SelectExpression.Filter Convert(Collection<Filter> filters, SelectExpression.AliasName entityName)
        {
            var filtersList = filters.Select(f => Convert(f, entityName)).ToArray();
            if (filtersList.Length == 1)
                return filtersList[0];

            var builder = new SelectFilterBuilder();
            builder.SubFilter(filtersList);
            return builder.Build();
        }

        private void Convert(Collection<FetchOrderType> orders, SelectExpression.AliasName entityName, SelectExpressionBuilder builder)
        {
            foreach (var order in orders)
            {
                string alias = !string.IsNullOrEmpty(order.Alias) ? order.Alias : entityName.Alias;
                builder.OrderBy(order.Descending ? SelectExpression.Order.Type.Descending : SelectExpression.Order.Type.Ascending,
                    order.Attribute,
                    alias
                );
            }
        }
    }
}
