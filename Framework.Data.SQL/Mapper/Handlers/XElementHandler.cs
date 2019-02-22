using System.Xml.Linq;

namespace Framework.Data.SQL
{
    internal sealed class XElementHandler : XmlTypeHandler<XElement>
    {
        #region| Methods |

        protected override XElement Parse(string xml) => XElement.Parse(xml);
        protected override string Format(XElement xml) => xml.ToString(); 

        #endregion
    }
}
