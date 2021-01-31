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
[XmlRoot("Sp3dToDataGroupMapping")]
public class Sp3dToDataGroupMapping
{
    [XmlArrayItem("Item")]
    public Sp3dToDataGroupMapProperty[] Items { get; set; }

    public static Sp3dToDataGroupMapping Instance => 
        (instance_ ?? (instance_ = read()));

    private Sp3dToDataGroupMapping() 
    {
    }

    private static Sp3dToDataGroupMapping instance_;

    private static Sp3dToDataGroupMapping read() =>
        XmlSerializerEx.FromXmlFile<Sp3dToDataGroupMapping>(            
            Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.TrimStart("file:///".ToCharArray())),
                "Mapping/Sp3dToDataGroupMapping.xml")
        );
    
}
}
