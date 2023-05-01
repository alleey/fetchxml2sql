using FetchXml.Model;
using FetchXml.SQL;

namespace FetchXml.Converter
{
    public interface IFetchXmlToSQLConverter
    {
        SelectExpression[] Convert(FetchType fetchType);
    }
}
