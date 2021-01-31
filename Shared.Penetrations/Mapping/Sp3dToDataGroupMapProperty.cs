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
public class Sp3dToDataGroupMapProperty
{
    [XmlAttribute]
    public string Sp3dXmlPath { get; set; }
    [XmlAttribute]
    public string TargetXPath { get; set; }
    [XmlAttribute]
    public string TargetName { get; set; }
    [XmlAttribute]
    public bool Visible { get; set; }
    [XmlAttribute]
    public bool ReadOnly { get; set; }
}
}
