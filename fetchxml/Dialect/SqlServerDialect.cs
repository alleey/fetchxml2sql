
using System;
using System.Collections.Generic;
using System.Linq;
using FetchXml.SQL;

namespace FetchXml.Dialect
{
    public class SqlServerDialect : ISqlDialect
    {
        public string EscapeIdentifier(string value) => $"[{value}]";


        public string EscapeStringValue(string value)
        {
            // spcical case for number and boolean value. Risks here, but I have no other way unless we check the table schema first.
            if (int.TryParse(value, out int number))
                return value;
            // special case for Guid value. In CRM2016 generated FetchXML, Guid value is expressed as {GUID},
            // but in sql server, Guid is not wrapped in curly braces.
            else if (System.Guid.TryParse(value, out var gx))
                return $"'{gx}'";
            
            return $"'{value}'";
        }


        public string EscapeStringValues(IEnumerable<string> values) 
        {
            var result = values.Select(x => EscapeStringValue(x)).ToArray();
            return string.Join(", ", result);
        }

        public string ConvertOperator(string lhs, string operatorId, params string[] operands)
        {
            var rhs = operands[0];
            switch(operatorId)
            {
                case "eq": return $"{lhs} = {EscapeStringValue(rhs)}";
                // ne/neq have include null values
                case "ne": return $"( {lhs} != {EscapeStringValue(rhs)} or {lhs} is null )";
                case "neq": return $"( {lhs} != {EscapeStringValue(rhs)} or {lhs} is null )";
                case "lt": return $"{lhs} < {EscapeStringValue(rhs)}";
                case "le": return $"{lhs} <= {EscapeStringValue(rhs)}";
                case "gt": return $"{lhs} > {EscapeStringValue(rhs)}";
                case "ge": return $"{lhs} >= {EscapeStringValue(rhs)}";
                case "like": return $"{lhs} like {EscapeStringValue(rhs)}";
                case "not-like": return $"{lhs} not like {EscapeStringValue(rhs)}";
                case "null": return $"{lhs} is NULL";
                case "not-null": return $"{lhs} is not NULL";

                case "in":
                {
                    var escaped = string.Join(", ", operands.Select(x => EscapeStringValue(x)).ToArray());
                    return $"{lhs} in ({escaped})";
                }
                case "not-in": 
                {
                    var escaped = string.Join(", ", operands.Select(x => EscapeStringValue(x)).ToArray());
                    return $"{lhs} not in ({escaped})";
                }

                case "between": 
                {
                    if(operands.Length != 2)
                        throw new ArgumentException("Between operator requires two operands");
                    return $"({lhs} >= {operands[0]} and {lhs} <= {operands[1]})";
                }
                case "not-between": 
                {
                    if(operands.Length != 2)
                        throw new ArgumentException("Not-Between operator requires two operands");
                    return $"({lhs} < {operands[0]} or {lhs} > {operands[1]})";
                }

                case "begins-with": 
                {
                    rhs = EscapeStringValue($"%{rhs}");
                    return $"{lhs} = {rhs}";
                }
                case "not-begin-with": 
                {
                    rhs = EscapeStringValue($"%{rhs}");
                    return $"{lhs} != {rhs}";
                }
                case "ends-with": 
                {
                    rhs = EscapeStringValue($"{rhs}%");
                    return $"{lhs} = {rhs}";
                }
                case "not-end-with": 
                {
                    rhs = EscapeStringValue($"{rhs}%");
                    return $"{lhs} != {rhs}";
                }

                case "today": return $"{lhs} = CONVERT(DATE, GETDATE())";
                case "tomorrow": return $"{lhs} = CONVERT(DATE, DATEADD(DAY, 1, GETDATE()))";
                case "yesterday": return $"{lhs} = CONVERT(DATE, DATEADD(DAY, -1, GETDATE()))";

                case "on": return $"DATEDIFF(day, {lhs}, '{rhs}') = 0";
                case "on-or-before": return $"DATEDIFF(day, {lhs}, '{rhs}') <= 0";
                case "on-or-after": return $"DATEDIFF(day, {lhs}, '{rhs}') >= 0";

                case "last-week": return $"DATEDIFF(week, {lhs}, GETDATE()) = -1";
                case "this-week": return $"DATEDIFF(week, {lhs}, GETDATE()) = 0";
                case "next-week": return $"DATEDIFF(week, {lhs}, GETDATE()) = 1";
                case "last-month": return $"DATEDIFF(month, {lhs}, GETDATE()) = -1";
                case "this-month": return $"DATEDIFF(month, {lhs}, GETDATE()) = 0";
                case "next-month": return $"DATEDIFF(month, {lhs}, GETDATE()) = 1";
                case "last-year": return $"DATEDIFF(year, {lhs}, GETDATE()) = -1";
                case "this-year": return $"DATEDIFF(year, {lhs}, GETDATE()) = 0";
                case "next-year": return $"DATEDIFF(year, {lhs}, GETDATE()) = 1";

                case "last-seven-days": return $"(DATEDIFF(day, {lhs}, GETDATE()) <= 0 and DATEDIFF(day, {lhs}, GETDATE()) > -7)";
                case "next-seven-days": return $"(DATEDIFF(day, {lhs}, GETDATE()) >= 0 and DATEDIFF(day, {lhs}, GETDATE()) < 7)";

                case "last-x-hours": return $"(DATEDIFF(hour, {lhs}, GETDATE()) <= 0 and DATEDIFF(hour, {lhs}, GETDATE()) > -{rhs})";
                case "next-x-hours": return $"(DATEDIFF(hour, {lhs}, GETDATE()) >= 0 and DATEDIFF(hour, {lhs}, GETDATE()) < {rhs})";
                case "last-x-days": return $"(DATEDIFF(day, {lhs}, GETDATE()) <= 0 and DATEDIFF(day, {lhs}, GETDATE()) > -{rhs})";
                case "next-x-days": return $"(DATEDIFF(day, {lhs}, GETDATE()) >= 0 and DATEDIFF(day, {lhs}, GETDATE()) < {rhs})";
                case "last-x-weeks": return $"(DATEDIFF(week, {lhs}, GETDATE()) <= 0 and DATEDIFF(week, {lhs}, GETDATE()) > -{rhs})";
                case "next-x-weeks": return $"(DATEDIFF(week, {lhs}, GETDATE()) >= 0 and DATEDIFF(week, {lhs}, GETDATE()) < {rhs})";
                case "last-x-months": return $"(DATEDIFF(month, {lhs}, GETDATE()) <= 0 and DATEDIFF(month, {lhs}, GETDATE()) > -{rhs})";
                case "next-x-months": return $"(DATEDIFF(month, {lhs}, GETDATE()) >= 0 and DATEDIFF(month, {lhs}, GETDATE()) < {rhs})";
                case "last-x-years": return $"(DATEDIFF(year, {lhs}, GETDATE()) <= 0 and DATEDIFF(year, {lhs}, GETDATE()) > -{rhs})";
                case "next-x-years": return $"(DATEDIFF(year, {lhs}, GETDATE()) >= 0 and DATEDIFF(year, {lhs}, GETDATE()) < {rhs})";

                case "olderthan-x-minutes": return $"DATEDIFF(minute, {lhs}, GETDATE()) < {rhs}";
                case "olderthan-x-hours": return $"DATEDIFF(hour, {lhs}, GETDATE()) < {rhs}";
                case "olderthan-x-days": return $"DATEDIFF(day, {lhs}, GETDATE()) < {rhs}";
                case "olderthan-x-weeks": return $"DATEDIFF(week, {lhs}, GETDATE()) < {rhs}";
                case "olderthan-x-months": return $"DATEDIFF(month, {lhs}, GETDATE()) < {rhs}";
                case "olderthan-x-years": return $"DATEDIFF(year, {lhs}, GETDATE()) < {rhs}";

                case "eq-userid": return $"{lhs} in (select SystemUserId from SystemUserBase where DomainName = SUSER_SNAME())";
            }
            throw new System.Exception($"Operator {operatorId} is not implemented");
        }

        public string ConvertAggregate(string operatorId, string value) => $"{operatorId}({value})";

        public string MapJoinType(SelectExpression.Join.Type joinType)
        {
            // non-standard are supported here. Fetchxml only supports inner and outer
            switch(joinType)
            {
                case SelectExpression.Join.Type.Left: return "left outer";
                case SelectExpression.Join.Type.Right: return "right outer";
                case SelectExpression.Join.Type.Outer: return "full outer";
            }
            return "inner";
        }
    }
}
