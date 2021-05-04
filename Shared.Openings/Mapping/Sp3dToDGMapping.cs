using System;
using System.Collections.Generic;
using System.Text;
using Shared.Bentley;

namespace Embedded.Openings.Shared.Mapping
{
    public class Sp3dToDGMapping : Sp3dToDataGroupMapping
    {
        public static Sp3dToDataGroupMapping Instance => 
            instance_ ?? (instance_ = read(
                WorkspaceHelper.GetConfigVariable(
                    CfgVariables.OPENINGS_SP3D_MAP_FILE_PATH)
            ));
    }
}
