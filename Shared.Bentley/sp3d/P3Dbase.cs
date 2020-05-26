using System;
using System.Xml.Serialization;

using BCOM = Bentley.Interop.MicroStationDGN;

namespace Shared.Bentley.sp3d
{
[Serializable]
public class P3Dbase : BentleyInteropBase
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

    
    public BCOM.Point3d getLocation()
    {
        return App.Point3dFromXYZ(LocationX, LocationY, LocationZ);
    }

    public BCOM.Matrix3d getRotation()
    {
        BCOM.Matrix3d rot;
        rot.RowX = App.Point3dFromXYZ(
            OrientationMatrix_x0,
            OrientationMatrix_x1,
            OrientationMatrix_x2);
        rot.RowX = RoundTool.roundExt(rot.RowX, 5, 10, 0);

        rot.RowY = App.Point3dFromXYZ(
            OrientationMatrix_y0,
            OrientationMatrix_y1,
            OrientationMatrix_y2);
        rot.RowY = RoundTool.roundExt(rot.RowY, 5, 10, 0);

        rot.RowZ = App.Point3dFromXYZ(
            OrientationMatrix_z0,
            OrientationMatrix_z1,
            OrientationMatrix_z2);
        rot.RowZ = RoundTool.roundExt(rot.RowZ, 5, 10, 0);

        return rot;
    }
}
}