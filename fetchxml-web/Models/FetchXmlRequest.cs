using System;

namespace FetchXml.Web.Models
{
    public class FetchXmlRequest
    {
        public string Text { get; set; }

        public bool Pretty { get; set; }

        public int TabLength { get; set; }

        public string SpaceChar { get; set; }

        public string Comma { get; set; }
    }
}
