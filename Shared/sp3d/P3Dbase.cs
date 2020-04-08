using System;
using System.Xml.Serialization;

namespace Shared.sp3d
{
[Serializable]
public class P3Dbase
{
    [XmlAttribute]
    public string instanceID;

    public double
        Range1X,
        Range1Y,
        Range1Z,
        Range2X,
        Range2Y,
        Range2Z,

        OrientationMatrix_x0,
        OrientationMatrix_x1,
        OrientationMatrix_x2,
        OrientationMatrix_y0,
        OrientationMatrix_y1,
        OrientationMatrix_y2,
        OrientationMatrix_z0,
        OrientationMatrix_z1,
        OrientationMatrix_z2,

        LocationX,
        LocationY,
        LocationZ;
        
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
        SP3D_UserLastModified,
        SP3D_SystemPath;
}
}