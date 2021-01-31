using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using Shared;

using Shared.Bentley;

namespace Embedded.Penetrations.Shared
{
[Serializable]
[XmlRoot("DataGroupToTagsMapping")]
public class DataGroupToTagsMapping
{
    [XmlArrayItem("Item")]
    public DataGroupToTagMapProperty[] Items { get; set; }

    public static DataGroupToTagsMapping Instance => 
        (instance_ ?? (instance_ = read()));
    
    private DataGroupToTagsMapping()
    {
    }

    private static DataGroupToTagsMapping instance_;

    private static DataGroupToTagsMapping read() =>
        XmlSerializerEx.FromXmlFile<DataGroupToTagsMapping>(            
            Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.TrimStart("file:///".ToCharArray())),
                "Mapping/DataGroupToTagsMapping.xml")
        );
}
}
