using System;

namespace Shared.sp3d
{
[Serializable]
public class P3DEquipment : P3Dbase
{
    public int
        ConstructionStatus,
        ConstructionStatus2,
        FabricationType,
        FabricationRequirement,
        MTO_ReportingRequirements,
        MTO_ReportingType;
}
}


/*
<P3DEquipment instanceID="DGNEC::15150a0000::ECXA::1" xmlns="SP3DReview.04.02">
    <ConstructionStatus>2</ConstructionStatus>
    <ConstructionStatus2>2</ConstructionStatus2>
    <EqType0>2780</EqType0>
    <FabricationType>7</FabricationType>
    <FabricationRequirement>10</FabricationRequirement>
    <LocationX>46.5</LocationX>
    <LocationY>47.8</LocationY>
    <LocationZ>3.3</LocationZ>
    <MTO_ReportingRequirements>5</MTO_ReportingRequirements>
    <MTO_ReportingType>5</MTO_ReportingType>
    <SP3D_DateCreated>637152878320000000</SP3D_DateCreated>
    <SP3D_DateLastModified>637153741430000000</SP3D_DateLastModified>
    <SP3D_ApprovalStatus>1</SP3D_ApprovalStatus>
    <SP3D_ApprovalReason>1</SP3D_ApprovalReason>
    <SP3D_PermissionGroup>133</SP3D_PermissionGroup>
    <OrientationMatrix_x0>-1</OrientationMatrix_x0>
    <OrientationMatrix_x1>-1.732563541929E-16</OrientationMatrix_x1>
    <OrientationMatrix_x2>0</OrientationMatrix_x2>
    <OrientationMatrix_y0>0</OrientationMatrix_y0>
    <OrientationMatrix_y1>1</OrientationMatrix_y1>
    <OrientationMatrix_y2>0</OrientationMatrix_y2>
    <OrientationMatrix_z0>0</OrientationMatrix_z0>
    <OrientationMatrix_z1>4.18425538850424E-17</OrientationMatrix_z1>
    <OrientationMatrix_z2>-1</OrientationMatrix_z2>
    <Range1X>46.0989990234375</Range1X>
    <Range1Y>47.4939994812012</Range1Y>
    <Range1Z>2.99399995803833</Range1Z>
    <Range2X>46.5009994506836</Range2X>
    <Range2Y>48.1059989929199</Range2Y>
    <Range2Z>3.60599994659424</Range2Z>
    <Oid>00004e2e-0000-0000-659d-ede0275e1905</Oid>
    <UID> @a=0027!!20014##367428369977810277</UID>
    <Name>50KLE10BQ2610</Name>
    <Description>T3-12-40</Description>
    <CatalogPartNumber>PenRound-t3</CatalogPartNumber>
    <ShortMaterialDescription>HVAC Penetration</ShortMaterialDescription>
    <SP3D_SystemPath>PaksNPP\5\50KLE\HVAC</SP3D_SystemPath>
    <SP3D_UserCreated>SP\YMBukalina</SP3D_UserCreated>
    <SP3D_UserLastModified>SP\YMBukalina</SP3D_UserLastModified>
</P3DEquipment>
 */