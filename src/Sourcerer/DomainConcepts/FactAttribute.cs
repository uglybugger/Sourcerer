using System;
using System.Xml.Serialization;

namespace Sourcerer.DomainConcepts
{
    public class FactAttribute : XmlRootAttribute
    {
        public FactAttribute(string id)
        {
            Namespace = Guid.Parse(id).ToString();
        }
    }
}