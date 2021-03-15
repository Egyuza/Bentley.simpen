using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using Shared;

using BCOM = Bentley.Interop.MicroStationDGN;
using Shared.Bentley;
#if V8i
using BMI = Bentley.MicroStation.InteropServices;
#elif CONNECT
using BMI = Bentley.MstnPlatformNET.InteropServices;
#endif

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

    private static Sp3dToDataGroupMapping read() =>
        XmlSerializerEx.FromXmlFile<Sp3dToDataGroupMapping>(
            WorkspaceHelper.GetConfigVariable(CfgVariables.SP3D_MAP_FILE_PATH));
    
    private static Sp3dToDataGroupMapping instance_;
}
}
