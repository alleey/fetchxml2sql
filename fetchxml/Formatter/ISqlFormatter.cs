using FetchXml.SQL;

namespace FetchXml.Formatter
{
    public interface ISqlFormatter
    {
        string Format(SelectExpression expression);
    }
}
