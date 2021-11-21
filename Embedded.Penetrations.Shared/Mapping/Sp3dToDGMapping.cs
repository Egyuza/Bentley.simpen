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
public class Sp3dToDGMapping : Sp3dToDataGroupMapping
{
    public static Sp3dToDataGroupMapping Instance => 
        instance_ ?? (instance_ = read(WorkspaceHelper.GetConfigVariable(
            CfgVariables.PEN_SP3D_MAP_FILE_PATH)));
}
}
