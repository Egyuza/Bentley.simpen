using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace simpen.ui.sp3d
{
[Serializable]
public class P3DHangerPipeSupport
{
    [XmlAttribute]
    public string instanceID;

    public double
        Group_EmptyWeight,

        //Global_Dry_Installed_CoG_X,
        //Global_Dry_Installed_CoG_Y,
        //Global_Dry_Installed_CoG_Z,
        
        Global_Wet_Operating_CoG_X,
        Global_Wet_Operating_CoG_Y,
        Global_Wet_Operating_CoG_Z,
        
        //Dry_Installed_Weight,
        Wet_Operating_Weight,

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
        //CatalogPartNumber,
        //ShortMaterialDescription,
        SP3D_UserCreated,
        SP3D_UserLastModified,
        SP3D_SystemPath;
}
}


/*
 <P3DHangerPipeSupport instanceID="DGNEC::1548080000::ECXA::1" xmlns="SP3DReview.04.02">
    <SP3D_DateCreated>636566091040000000</SP3D_DateCreated>
    <SP3D_DateLastModified>637079508080000000</SP3D_DateLastModified>
    <SP3D_ApprovalStatus>1</SP3D_ApprovalStatus>
    <SP3D_ApprovalReason>1</SP3D_ApprovalReason>
    <SP3D_PermissionGroup>244</SP3D_PermissionGroup>
    <OrientationMatrix_x0>4.8450284582946E-19</OrientationMatrix_x0>
    <OrientationMatrix_x1>8.69395621868164E-35</OrientationMatrix_x1>
    <OrientationMatrix_x2>1</OrientationMatrix_x2>
    <OrientationMatrix_y0>1</OrientationMatrix_y0>
    <OrientationMatrix_y1>0</OrientationMatrix_y1>
    <OrientationMatrix_y2>0</OrientationMatrix_y2>
    <OrientationMatrix_z0>1.43385737311208E-17</OrientationMatrix_z0>
    <OrientationMatrix_z1>1</OrientationMatrix_z1>
    <OrientationMatrix_z2>0</OrientationMatrix_z2>
    <Range1X>-49.8325004577637</Range1X>
    <Range1Y>-141.242492675781</Range1Y>
    <Range1Z>4.29899978637695</Range1Z>
    <Range2X>-49.3474998474121</Range2X>
    <Range2Y>-140.757507324219</Range2Y>
    <Range2Z>5.00099992752075</Range2Z>
    <LocationX>-49.59</LocationX>
    <LocationY>-141</LocationY>
    <LocationZ>4.3</LocationZ>
    <Oid>00022303-0000-0000-751e-1bcaa85aef05</Oid>
    <UID> @a=0028!!140035##427660170615266933</UID>
    <Name>10GHD02BQ2404</Name>
    <Description>T1-8-70</Description>
    <SP3D_UserCreated>SP\d.novikov</SP3D_UserCreated>
    <SP3D_UserLastModified>SP\AABeloglazov</SP3D_UserLastModified>
    <SP3D_SystemPath>HnhNPP\Task\02_Penetration Area\10USG\Piping\10GHD</SP3D_SystemPath>
</P3DHangerPipeSupport>

 */
  


 /* Старая
  * 
  <P3DHangerPipeSupport instanceID="DGNEC::154d080000::ECXA::1" xmlns="SP3DReview.04.02">
    <Global_Wet_Operating_CoG_X>-51.29195</Global_Wet_Operating_CoG_X>
    <Global_Wet_Operating_CoG_Y>-140.036550088686</Global_Wet_Operating_CoG_Y>
    <Global_Wet_Operating_CoG_Z>4.3127</Global_Wet_Operating_CoG_Z>
    <Wet_Operating_Weight>2.07470778843074E-05</Wet_Operating_Weight>
    <SP3D_DateCreated>636586039620000000</SP3D_DateCreated>
    <SP3D_DateLastModified>636813348160000000</SP3D_DateLastModified>
    <SP3D_ApprovalStatus>1</SP3D_ApprovalStatus>
    <SP3D_ApprovalReason>1</SP3D_ApprovalReason>
    <SP3D_PermissionGroup>244</SP3D_PermissionGroup>
    <OrientationMatrix_x0>1.26716128909243E-18</OrientationMatrix_x0>
    <OrientationMatrix_x1>-1.03472890231132E-34</OrientationMatrix_x1>
    <OrientationMatrix_x2>1</OrientationMatrix_x2>
    <OrientationMatrix_y0>1</OrientationMatrix_y0>
    <OrientationMatrix_y1>0</OrientationMatrix_y1>
    <OrientationMatrix_y2>0</OrientationMatrix_y2>
    <OrientationMatrix_z0>-3.16315983822646E-17</OrientationMatrix_z0>
    <OrientationMatrix_z1>1</OrientationMatrix_z1>
    <OrientationMatrix_z2>0</OrientationMatrix_z2>
    <Range1X>-51.3294982910156</Range1X>
    <Range1Y>-140.129501342773</Range1Y>
    <Range1Z>4.29899978637695</Range1Z>
    <Range2X>-51.1705017089844</Range2X>
    <Range2Y>-139.970504760742</Range2Y>
    <Range2Z>4.90100002288818</Range2Z>
    <LocationX>-51.25</LocationX>
    <LocationY>-140.050000088686</LocationY>
    <LocationZ>4.3</LocationZ>
    <Group_EmptyWeight>2.07470778843074E-05</Group_EmptyWeight>
    <Oid>00022303-0000-0000-054e-5726c75ae206</Oid>
    <UID> @a=0028!!140035##496058720352423429</UID>
    <Name>10GKC21BQ2403</Name>
    <Description>T1-1-70</Description>
    <SP3D_UserCreated>SP\d.novikov</SP3D_UserCreated>
    <SP3D_UserLastModified>SP\EVMaryksin</SP3D_UserLastModified>
    <SP3D_SystemPath>HnhNPP\Task\02_Penetration Area\10USG\Piping\10GKC</SP3D_SystemPath>
</P3DHangerPipeSupport>
  */