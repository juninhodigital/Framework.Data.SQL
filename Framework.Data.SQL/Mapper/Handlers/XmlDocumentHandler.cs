using System.Xml;

namespace Framework.Data.SQL
{
    internal sealed class XmlDocumentHandler : XmlTypeHandler<XmlDocument>
    {
        #region| Methods |

        protected override XmlDocument Parse(string xml)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            return doc;
        }

        protected override string Format(XmlDocument xml) => xml.OuterXml; 

        #endregion
    }
}
