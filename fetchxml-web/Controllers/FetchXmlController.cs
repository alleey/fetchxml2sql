using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using FetchXml.Converter;
using FetchXml.Formatter;
using FetchXml.Model;
using FetchXml.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FetchXml.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FetchXmlController : ControllerBase
    {
        private readonly IFetchXmlToSQLConverter _converter;
        private readonly ISqlFormatter _formatter;

        public FetchXmlController(IFetchXmlToSQLConverter converter, ISqlFormatter formatter)
        {
            _converter = converter ?? throw new ArgumentNullException(nameof(converter));
            _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
        }

        [HttpPost]
        [Route("Convert")]
        public IActionResult Convert(FetchXmlRequest request)
        {
            var serializer = new XmlSerializer(typeof(FetchType));
            var fetchXml = (FetchType)serializer.Deserialize(new MemoryStream(Encoding.UTF8.GetBytes(request.Text)));
            var selectExprs = _converter.Convert(fetchXml);
            return Content(_formatter.Format(selectExprs[0]), "application/sql");;
        }
    }
}
