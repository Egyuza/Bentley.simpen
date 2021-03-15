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
[XmlRoot("TagsToDataGroupMapping")]
public class TagsToDataGroupMapping
{
    [XmlArrayItem("Item")]
    public TagToDataGroupMapProperty[] Items { get; set; }

    public static TagsToDataGroupMapping Instance => 
        (instance_ ?? (instance_ = read()));
    
    private TagsToDataGroupMapping()
    {
    }

    private static TagsToDataGroupMapping instance_;

    private static TagsToDataGroupMapping read() =>
        XmlSerializerEx.FromXmlFile<TagsToDataGroupMapping>( 
            WorkspaceHelper.GetConfigVariable(CfgVariables.TAGS_DG_MAP_FILE_PATH)
        );
}
}
