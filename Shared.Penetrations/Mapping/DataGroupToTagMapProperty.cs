using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using Shared;

namespace Embedded.Penetrations.Shared
{
[Serializable]
public class DataGroupToTagMapProperty
{
    [XmlAttribute]
    public string DataGroupCatalogType { get; set; }
    [XmlAttribute]
    public string DataGroupInstance { get; set; }
    [XmlAttribute]
    public string DataGroupXPath { get; set; }
    [XmlAttribute]
    public string DataGroupName { get; set; }
    [XmlAttribute]
    public string TagSetName { get; set; }
    [XmlAttribute]
    public string TagName { get; set; }
    [XmlAttribute]
    public string TagType { get; set; }
}
}
