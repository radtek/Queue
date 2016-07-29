using System.Net.Http;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Engine.Cloud.Core.Utils
{
    public abstract class SerializableXmlObject <T>
    {
        public HttpContent Serialize()
        {
            var sb = new StringBuilder();
            var serializer = new XmlSerializer(this.GetType());

            //REMOVE NAMESPACE DO ROOT XML
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            serializer.Serialize(XmlWriter.Create(sb, new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Indent = true
            }), this, ns);

            var myContent = new StringContent(sb.ToString(), Encoding.UTF8, "application/xml");

            return myContent;
        }
    }
}
