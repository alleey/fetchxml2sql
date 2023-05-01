
using System.Collections.Generic;
using FetchXml.SQL;

namespace FetchXml.Dialect
{
    public interface ISqlDialect
    {
        string EscapeIdentifier(string value);
        string EscapeStringValue(string value);
        string EscapeStringValues(IEnumerable<string> values); 
        string ConvertOperator(string lhs, string operatorId, params string[] operands);
        string ConvertAggregate(string operatorId, string value);
        string MapJoinType(SelectExpression.Join.Type joinType);
    }
}
