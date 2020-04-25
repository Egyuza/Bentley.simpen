using System;
using System.Xml.Serialization;

namespace Shared.Bentley.sp3d
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

/* Новая прохока
 * 
 <P3DHangerStdComponent instanceID="DGNEC::1506070000::ECXA::1" xmlns="SP3DReview.04.02">
    <SP3D_DateCreated>637079508080000000</SP3D_DateCreated>
    <SP3D_DateLastModified>637079508080000000</SP3D_DateLastModified>
    <SP3D_ApprovalStatus>1</SP3D_ApprovalStatus>
    <SP3D_ApprovalReason>1</SP3D_ApprovalReason>
    <SP3D_PermissionGroup>244</SP3D_PermissionGroup>
    <Range1X>-49.752498626709</Range1X>
    <Range1Y>-141.162506103516</Range1Y>
    <Range1Z>4.29899978637695</Range1Z>
    <Range2X>-49.4275016784668</Range2X>
    <Range2Y>-140.837493896484</Range2Y>
    <Range2Z>5.00099992752075</Range2Z>
    <Oid>00022307-0000-0000-a143-2bc4b75d3a04</Oid>
    <UID> @a=0028!!140039##304658968652497825</UID>
    <Name>PenPipe_EN_10219-1-C21324</Name>
    <Description>PenPipe</Description>
    <CatalogPartNumber>PenPipe_EN_10219</CatalogPartNumber>
    <ShortMaterialDescription>PenPipe</ShortMaterialDescription>
    <SP3D_UserCreated>SP\AABeloglazov</SP3D_UserCreated>
    <SP3D_UserLastModified>SP\AABeloglazov</SP3D_UserLastModified>
</P3DHangerStdComponent>
 
 */
 
 /* Фланец новой проходки
  * 
  <P3DHangerStdComponent instanceID="DGNEC::1544070000::ECXA::1" xmlns="SP3DReview.04.02">
    <SP3D_DateCreated>637079508080000000</SP3D_DateCreated>
    <SP3D_DateLastModified>637079508080000000</SP3D_DateLastModified>
    <SP3D_ApprovalStatus>1</SP3D_ApprovalStatus>
    <SP3D_ApprovalReason>1</SP3D_ApprovalReason>
    <SP3D_PermissionGroup>244</SP3D_PermissionGroup>
    <Range1X>-49.8325004577637</Range1X>
    <Range1Y>-141.242492675781</Range1Y>
    <Range1Z>4.29899978637695</Range1Z>
    <Range2X>-49.3474998474121</Range2X>
    <Range2Y>-140.757507324219</Range2Y>
    <Range2Z>4.309</Range2Z>
    <Oid>00022307-0000-0000-a443-2bc4b75d3a04</Oid>
    <UID> @a=0028!!140039##304658968652497828</UID>
    <Name>PenFlng_EN_10219-1-C24977</Name>
    <Description>PenFlange</Description>
    <CatalogPartNumber>PenFlng_EN_10219</CatalogPartNumber>
    <ShortMaterialDescription>PenFlange</ShortMaterialDescription>
    <SP3D_UserCreated>SP\AABeloglazov</SP3D_UserCreated>
    <SP3D_UserLastModified>SP\AABeloglazov</SP3D_UserLastModified>
</P3DHangerStdComponent>
  */


 /* Сарая
  * 
  <P3DHangerStdComponent instanceID="DGNEC::150f070000::ECXA::1" xmlns="SP3DReview.04.02">
    <Global_Dry_Installed_CoG_X>-51.29195</Global_Dry_Installed_CoG_X>
    <Global_Dry_Installed_CoG_Y>-140.036550088686</Global_Dry_Installed_CoG_Y>
    <Global_Dry_Installed_CoG_Z>4.3127</Global_Dry_Installed_CoG_Z>
    <Global_Wet_Operating_CoG_X>-51.29195</Global_Wet_Operating_CoG_X>
    <Global_Wet_Operating_CoG_Y>-140.036550088686</Global_Wet_Operating_CoG_Y>
    <Global_Wet_Operating_CoG_Z>4.3127</Global_Wet_Operating_CoG_Z>
    <Dry_Installed_Weight>2.07470778843074E-05</Dry_Installed_Weight>
    <Wet_Operating_Weight>2.07470778843074E-05</Wet_Operating_Weight>
    <SP3D_DateCreated>636586039620000000</SP3D_DateCreated>
    <SP3D_DateLastModified>636702608160000000</SP3D_DateLastModified>
    <SP3D_ApprovalStatus>1</SP3D_ApprovalStatus>
    <SP3D_ApprovalReason>1</SP3D_ApprovalReason>
    <SP3D_PermissionGroup>244</SP3D_PermissionGroup>
    <Range1X>-51.3294982910156</Range1X>
    <Range1Y>-140.129501342773</Range1Y>
    <Range1Z>4.29899978637695</Range1Z>
    <Range2X>-51.1705017089844</Range2X>
    <Range2Y>-139.970504760742</Range2Y>
    <Range2Z>4.90100002288818</Range2Z>
    <Oid>00022307-0000-0000-0b4e-5726c75ae206</Oid>
    <UID> @a=0028!!140039##496058720352423435</UID>
    <Name>PntrtPlate-d-1-C32473</Name>
    <Description>PntrtPlate-d</Description>
    <CatalogPartNumber>PntrtPlate-d</CatalogPartNumber>
    <ShortMaterialDescription>PntrtPlate-d</ShortMaterialDescription>
    <SP3D_UserCreated>SP\d.novikov</SP3D_UserCreated>
    <SP3D_UserLastModified>SP\ORKuritsyna</SP3D_UserLastModified>
</P3DHangerStdComponent>  
  */

