using Embedded.Penetrations.Shared;
using Shared;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml.Linq;

namespace Embedded.Penetrations.Shared.sp3d
{
    public class Sp3dPenTask
    {
        public enum TaskType
        {
            Unknown,
            Pipe,
            Flange
        }

        public string TypeName { get; set; } // тип объекта:
        public TaskType Type => string.IsNullOrEmpty(TypeName) ? TaskType.Unknown :
            (TypeName.ToLower().Contains("penflange") ? TaskType.Flange : 
            (TypeName.ToLower().ContainsAny("penpipe", "penround", "pntrt", "penplate") ? TaskType.Pipe : 
                    TaskType.Unknown));

        public string Oid { get; set; }
        public string Code { get; set; }
        public string Name{ get; set; }

        public int Length { get; set; }
        public int PipeThick { get; set; }
        public int PipeDiametr { get; set; }

        public int FlangeThick { get; set; }
        public int FlangeDiametr { get; set; }
        public int FlangesNum { get; set; }
        public int FlangesPos { get; set; }

        public double[] Location { get; set; }

        public bool IsValid
        {
            get 
            {
                return !string.IsNullOrEmpty(Code);
            }
        }

        public Sp3dPenTask(IEnumerable<string> sp3dXmlData)
        {
            if (sp3dXmlData == null)
                return;

            var xDoc = XDocument.Parse("<Root></Root>");
            foreach (string xmlText in sp3dXmlData)
            {
                xDoc.Root.Add(XElement.Parse(xmlText));
            }

            foreach (var propMap in Sp3dToDGMapping.Instance.Items)
            {                
                foreach (string path in propMap.Sp3dXmlPaths)
                {
                    string propName, value;
                    var xEl = xDoc.Root.GetChildByRegexPath(path, out propName);
                    //var xEl = xDoc.Root.XPathSelectElement(path);
                    if (xEl != null)
                    {
                        value = xEl.Value;
                        switch (propMap.Key)
                        {
                        case "Type": this.TypeName = value; break;
                        case "Oid": this.Oid = value; break;
                        case "Code": this.Code = value; break;
                        case "Name": this.Name = value; break;
                        case "Length": this.Length = int.Parse(value); break;
                        case "PipeDiametr": this.PipeDiametr = int.Parse(value); break;
                        case "PipeThick": this.PipeThick = int.Parse(value); break;
                        case "FlangeDiametr": this.FlangeDiametr = int.Parse(value); break;
                        case "FlangeThick": this.FlangeThick = int.Parse(value); break;
                        case "FlangesNum": this.FlangesNum = int.Parse(value); break;
                        case "FlangesPos": this.FlangesPos = int.Parse(value); break;
                        case "Location": 
                        {
                            Location = Location ?? new double[3];
                            string sprtr = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                            string strValueCorrect = value.Replace(".", sprtr).Replace(",", sprtr);
                            double valueDbl = double.Parse(strValueCorrect);
                            string propNameUpper = propName.ToUpper();

                            if (propNameUpper.EndsWith("X"))
                                Location[0] = valueDbl;
                            else if (propNameUpper.EndsWith("Y"))
                                Location[1] = valueDbl;
                            else if (propNameUpper.EndsWith("Z"))
                                Location[2] = valueDbl;
                            break;
                        }
                        }
                    }
                }
            }
        }
    }
}


/* 
 <Root>
  <CUHASPenParts_EN_10219O instanceID="DGNEC::15d5080000::ECXA::1">
    <DateLastModified>637510708714070000</DateLastModified>
    <PermissionGroup>244</PermissionGroup>
    <ApprovalStatus>1</ApprovalStatus>
    <ApprovalReason>1</ApprovalReason>
    <IJDObject_DateCreated>637413834173500000</IJDObject_DateCreated>
    <Index>0</Index>
    <Locked>False</Locked>
    <DryProperties>256</DryProperties>
    <WetProperties>256</WetProperties>
    <PenLen>750</PenLen>
    <PipeDia>323</PipeDia>
    <PipeThk>8</PipeThk>
    <FlngDia>80</FlngDia>
    <FlngThk>8</FlngThk>
    <PipeMat>100</PipeMat>
    <FlngMat>100</FlngMat>
    <FlngNum>1</FlngNum>
    <FlngPos>1</FlngPos>
    <OffsetX>0</OffsetX>
    <OffsetY>0</OffsetY>
    <Oid>00022307-0000-0000-ebbe-a3ffb55f7b04</Oid>
    <OriginalType>CUHASPenParts_EN_10219O</OriginalType>
    <OriginalTypeName>HASPenParts EN 10219O</OriginalTypeName>
    <UserCreated>SP\AAAndrienko</UserCreated>
    <UserLastModified>SP\RVGizbrekht</UserLastModified>
    <BOMdescription>PenPipe</BOMdescription>
    <SystemPath>HnhNPP\1\10JND\10UKD10JND10H\10JND10BQ2403\PenPipe_EN_10219-1-C42451</SystemPath>
    <Name>PenPipe_EN_10219-1-C42451</Name>
  </CUHASPenParts_EN_10219O>
  
  <CUHASD08T1_EN_10219O instanceID="DGNEC::1596070000::ECXA::1">
    <DateLastModified>637510708729300000</DateLastModified>
    <PermissionGroup>244</PermissionGroup>
    <ApprovalStatus>1</ApprovalStatus>
    <ApprovalReason>1</ApprovalReason>
    <IJDObject_DateCreated>637413834173500000</IJDObject_DateCreated>
    <Index>0</Index>
    <Locked>False</Locked>
    <MaxLoad>0</MaxLoad>
    <TypeSelectionRule>False</TypeSelectionRule>
    <OrigSuppingCount>0</OrigSuppingCount>
    <OrigSuppedCount>1</OrigSuppedCount>
    <AutoFaceConnect>1</AutoFaceConnect>
    <SupportDiscipline>10</SupportDiscipline>
    <SupportType>10001</SupportType>
    <Water_x0020_resistance>10000</Water_x0020_resistance>
    <Tightness>10000</Tightness>
    <Radiation_x0020_protection>10000</Radiation_x0020_protection>
    <PenLen>750</PenLen>
    <PipeDia>323</PipeDia>
    <PipeThk>8</PipeThk>
    <FlngDia>80</FlngDia>
    <FlngThk>8</FlngThk>
    <PipeMat>100</PipeMat>
    <FlngMat>100</FlngMat>
    <FlngNum>1</FlngNum>
    <FlngPos>1</FlngPos>
    <OffsetX>0</OffsetX>
    <OffsetY>0</OffsetY>
    <Oid>00022303-0000-0000-e7be-a3ffb55f7b04</Oid>
    <OriginalType>CUHASD08T1_EN_10219O</OriginalType>
    <OriginalTypeName>HASD08T1 EN 10219O</OriginalTypeName>
    <UserCreated>SP\AAAndrienko</UserCreated>
    <UserLastModified>SP\RVGizbrekht</UserLastModified>
    <AssmSelectionRule>UNKNOWN</AssmSelectionRule>
    <PrimaryRunName>10JND10BR002</PrimaryRunName>
    <BOMdescription>T1-8-75</BOMdescription>
    <SystemPath>HnhNPP\1\10JND\10UKD10JND10H\10JND10BQ2403</SystemPath>
    <Name>10JND10BQ2403</Name>
    <RMKKSLocationCodeFullString>10UKD99R210</RMKKSLocationCodeFullString>
    <RMRoomName>10UKD99R210</RMRoomName>
  </CUHASD08T1_EN_10219O>
  
  <CUHASPenParts_EN_10219 instanceID="DGNEC::1585090000::ECXA::1">
    <DateLastModified>636655950749270000</DateLastModified>
    <PermissionGroup>1</PermissionGroup>
    <ApprovalStatus>1</ApprovalStatus>
    <ApprovalReason>1</ApprovalReason>
    <IJDObject_DateCreated>636655950749270000</IJDObject_DateCreated>
    <IsCreateFlavor>True</IsCreateFlavor>
    <MirrorBehaviorOption>15</MirrorBehaviorOption>
    <IJHgrPart_PartType>0</IJHgrPart_PartType>
    <PenLen>500</PenLen>
    <PipeDia>219.1</PipeDia>
    <PipeThk>6</PipeThk>
    <FlngDia>80</FlngDia>
    <FlngThk>6</FlngThk>
    <PipeMat>-1</PipeMat>
    <FlngMat>-1</FlngMat>
    <FlngNum>1</FlngNum>
    <FlngPos>1</FlngPos>
    <OffsetX>0</OffsetX>
    <OffsetY>0</OffsetY>
    <NDFrom>5</NDFrom>
    <NDTo>1200</NDTo>
    <Oid>0000eae8-0000-0000-1c00-8ac1315b1d04</Oid>
    <OriginalType>CUHASPenParts_EN_10219</OriginalType>
    <OriginalTypeName>Penetration Parts (EN 10219-1) (Catalog)</OriginalTypeName>
    <UserCreated>SP\a_burdin</UserCreated>
    <UserLastModified>SP\a_burdin</UserLastModified>
    <ProgId>HSPenEN_Parts.CPartPipe</ProgId>
    <DisplayProgId>HSPenEN_Parts.CPartPipe</DisplayProgId>
    <PartNumber>PenPipe_EN_10219</PartNumber>
    <PartDescription>PenPipe</PartDescription>
    <NDUnitType>mm</NDUnitType>
  </CUHASPenParts_EN_10219>
</Root>
 
 
 
 */