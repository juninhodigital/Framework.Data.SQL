using System.Xml.Linq;

namespace Framework.Data.SQL
{
    internal sealed class XDocumentHandler : XmlTypeHandler<XDocument>
    {
        #region| Methods |

        protected override XDocument Parse(string xml) => XDocument.Parse(xml);
        protected override string Format(XDocument xml) => xml.ToString(); 

        #endregion
    }
}
