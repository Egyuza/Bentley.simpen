using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace simpen.ui.sp3d
{
[Serializable]
public class P3DHangerStdComponent
{
    [XmlAttribute]
    public string instanceID;

    public double
        Global_Dry_Installed_CoG_X,
        Global_Dry_Installed_CoG_Y,
        Global_Dry_Installed_CoG_Z,

        Global_Wet_Operating_CoG_X,
        Global_Wet_Operating_CoG_Y,
        Global_Wet_Operating_CoG_Z,
        
        Dry_Installed_Weight,
        Wet_Operating_Weight,

        Range1X,
        Range1Y,
        Range1Z,
        Range2X,
        Range2Y,
        Range2Z;
        
    public ulong
        SP3D_DateCreated,
        SP3D_DateLastModified;

    public int 
        SP3D_ApprovalStatus,
        SP3D_ApprovalReason,
        SP3D_PermissionGroup;

    public string 
        Oid,
        UID,
        Name,
        Description,
        CatalogPartNumber,
        ShortMaterialDescription,
        SP3D_UserCreated,
        SP3D_UserLastModified;
}
}
