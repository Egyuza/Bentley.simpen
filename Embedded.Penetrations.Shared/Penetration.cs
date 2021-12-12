using Bentley.Building.DataGroupSystem;
using Bentley.Building.DataGroupSystem.Serialization;
using Shared.Bentley;
using System;
using System.Collections.Generic;
using System.Text;

using BCOM = Bentley.Interop.MicroStationDGN;
using TFCOM = Bentley.Interop.TFCom;
using System.Linq;
using Bentley.Interop.TFCom;
using Embedded.Penetrations.Shared.Mapping;

#if V8i
using Bentley.MicroStation;
using Bentley.Internal.MicroStation.Elements;
using BMI = Bentley.MicroStation.InteropServices;

#elif CONNECT
using BMI = Bentley.MstnPlatformNET.InteropServices;
using Bentley.DgnPlatformNET;
using Bentley.DgnPlatformNET.Elements;
using Bentley.MstnPlatformNET;

#endif

namespace Embedded.Penetrations.Shared
{
public class Penetration : EmbeddedBase
{
    public IPenetrTask Task { get; private set; }
    public PenetrInfo PenInfo { get; private set; }

    private UOR taskUOR;

    public Penetration(IPenetrTask task)
    {
        this.Task = task;
        this.PenInfo = PenetrDataSource.Instance.getPenInfo(
            Task.FlangesType, Task.DiameterType.Number);
        taskUOR = new UOR(task.ModelRef);

        task.prepairDataGroup();

        Initialize();
    }

    public override string CellName => CELL_NAME;

    public override string LevelName => LEVEL_NAME;

    public override string CatalogTypeName => DG_CATALOG_TYPE;

    public override string CatalogInstanceName => DG_CATALOG_INSTANCE;

    public override Dictionary<Sp3dToDataGroupMapProperty, string> 
    DataGroupPropsValues => Task.DataGroupPropsValues;

    public double Length => (Task.LengthCm *10 / taskUOR.activeSubPerMaster);

    public override IEnumerable<BCOM.Element> GetBodyElements()
    {
        var elements = new List<BCOM.Element>();
        Task.scanInfo();

        var taskUOR = new UOR(Task.ModelRef);

        double pipeInsideDiam = PenInfo.pipeDiameterInside / taskUOR.activeSubPerMaster;
        double pipeOutsideDiam = PenInfo.pipeDiameterOutside / taskUOR.activeSubPerMaster;

        double flangeInsideDiam = PenInfo.flangeDiameterInside / taskUOR.activeSubPerMaster;
        double flangeOutsideDiam = PenInfo.flangeDiameterOutside / taskUOR.activeSubPerMaster;
        double flangeThick = PenInfo.flangeThick / taskUOR.activeSubPerMaster;

        double length = Length;

        var solids = App.SmartSolid;

        /*
            ! длина трубы меньше размера проходки на толщину фланца
            ! ЕСЛИ ФЛАНЕЦ ЕСТЬ
        */

        double delta = Task.FlangesCount == 0 ? 0 : 
            Task.FlangesCount * flangeThick / 2;        

        BCOM.SmartSolidElement cylindrInside =
            solids.CreateCylinder(null, pipeInsideDiam / 2, length - delta);

        BCOM.SmartSolidElement cylindrOutside =
            solids.CreateCylinder(null, pipeOutsideDiam / 2, length - delta);
        
        var cylindr = solids.SolidSubtract(cylindrOutside, cylindrInside);

        var elementsShift = new Dictionary<BCOM.Element, double>();

        {
            double shift  = Task.FlangesCount == 1 ? delta : 0;
            shift *= Task.IsSingleFlangeFirst ? 1 : -1;
            elementsShift.Add(cylindr, (length + shift)/2);
        }        

        // Фланцы:
        for (int i = 0; i < Task.FlangesCount; ++i)
        {
            BCOM.SmartSolidElement flangeCylindr = solids.SolidSubtract(
                solids.CreateCylinder(null, flangeOutsideDiam / 2, flangeThick), 
                solids.CreateCylinder(null, pipeOutsideDiam / 2, flangeThick));            
            
            double shift = 0;

            if (Task.FlangesCount == 1)
            {
                bool isNearest = App.Vector3dEqualTolerance(Task.SingleFlangeSide,
                    App.Vector3dFromXYZ(0, 0, -1), 0.1); // 0.001
                
                // 0.5 - для видимости фланцев на грани стены/плиты 
                shift = isNearest ?
                        0.0    + flangeThick / 2 - Task.FlangeWallOffset:
                        length - flangeThick / 2 + Task.FlangeWallOffset;
            }
            else
            {
                shift = i == 0 ? 0.0 : length;               
                // для самих фланцев:                
                
                shift += Math.Pow(-1, i) * (flangeThick/2 - Task.FlangeWallOffset); //0.02);
            }
            elementsShift.Add(flangeCylindr, shift);
        }
         
        BCOM.Transform3d taskTran = App.Transform3dFromMatrix3d(Task.Rotation);
        
        var angles = Task.CorrectiveAngles;

        foreach (var pair in elementsShift)
        {
            BCOM.Element elem = pair.Key;
            double shift = pair.Value;
            
            //elem.Color = 0; // TODO
            BCOM.Point3d offset = App.Point3dAddScaled(
                App.Point3dZero(), App.Point3dFromXYZ(0, 0, 1), shift);
            elem.Move(offset);

            elem.Rotate(App.Point3dZero(), angles.X, angles.Y, angles.Z);
            
            elem.Transform(taskTran);
            elem.Move(Task.Location);

            elements.Add(elem);
        }

        return elements;
    }

    public override TFPerforatorList GetPerfoList()
    {
        double pipeOutsideDiam = PenInfo.pipeDiameterOutside / taskUOR.activeSubPerMaster;
        double length = Task.LengthCm *10 / taskUOR.activeSubPerMaster;

        var angles = Task.CorrectiveAngles;

        BCOM.Transform3d taskTran = App.Transform3dFromMatrix3d(Task.Rotation);

        // ПЕРФОРАТОР
        BCOM.EllipseElement perfoEl = 
            App.CreateEllipseElement2(null, App.Point3dZero(), 
                pipeOutsideDiam/2, pipeOutsideDiam/2, 
                App.Matrix3dIdentity(), BCOM.MsdFillMode.Filled);
        {
            BCOM.Point3d offset = App.Point3dAddScaled(
                App.Point3dZero(), 
                App.Point3dFromXYZ(0, 0, 1), length/2);
            perfoEl.Move(offset);
            perfoEl.Rotate(App.Point3dZero(), angles.X, angles.Y, angles.Z);
        }
        perfoEl.Level = ElementHelper.GetOrCreateLevel(LEVEL_SYMB_NAME);
        ElementHelper.setSymbologyByLevel(perfoEl);
        perfoEl.Transform(taskTran);
        perfoEl.Move(Task.Location);      

        BCOM.Point3d perfoVec = perfoEl.Normal;        

        TFCOM.TFPerforatorList perfoList = AppTF.CreateTFPerforator();
        var tranIdentity = App.Transform3dIdentity();

        perfoList.InitFromElement(perfoEl, perfoVec, length/2 * 1.01, tranIdentity);
        return perfoList;
    }

    public override double GetPerfoSenseDistance()
    {
        return Length / 2;
    }

    public override IEnumerable<ProjectionInfo> GetProjectionInfoList()
    {
        var projInfoList = new List<ProjectionInfo>();
        double pipeInsideDiam = PenInfo.pipeDiameterInside / taskUOR.activeSubPerMaster;
        double flangeOutsideDiam = PenInfo.flangeDiameterOutside / taskUOR.activeSubPerMaster;

        double length = Task.LengthCm *10 / taskUOR.activeSubPerMaster;

        foreach(double shift in new double[] { 0, length})
        {
            projInfoList.Add(new ProjectionInfo() {
                Element = ElementHelper.createCrossRound(pipeInsideDiam)
                    .transformByTask(Task, shiftZ: shift),
                LevelName = LEVEL_SYMB_NAME,
                ProjectionName = "cross"
            });
            projInfoList.Add(new ProjectionInfo() {
                Element = ElementHelper.createCircle(pipeInsideDiam)
                    .transformByTask(Task, shiftZ: shift),
                LevelName = LEVEL_SYMB_NAME,
                ProjectionName = "circle"
            });
        }

        if (Task.FlangesCount > 0)
        {
            projInfoList.Add(new ProjectionInfo() {
                Element = ElementHelper.createCircle(flangeOutsideDiam)
                    .transformByTask(Task, shiftZ: -Task.FlangeWallOffset),
                LevelName = LEVEL_FLANGE_SYMB_NAME,
                ProjectionName = "flange"
            });

            if (Task.FlangesCount == 2)
            {
                projInfoList.Add(new ProjectionInfo() {
                    Element = ElementHelper.createCircle(flangeOutsideDiam)
                        .transformByTask(Task, shiftZ: length -Task.FlangeWallOffset),
                    LevelName = LEVEL_FLANGE_SYMB_NAME,
                    ProjectionName = "flange"
                });
            }
        }

        if (Task.RefPoints != null)
        {
            foreach(BCOM.Point3d? refPoint in Task.RefPoints)
            {
                if (!refPoint.HasValue)
                    continue;

                projInfoList.Add(new ProjectionInfo() {
                    Element = ElementHelper.createPoint(refPoint.Value),
                    LevelName = LEVEL_POINT_NAME,
                    ProjectionName = "refPoint"
                });
            }
        }
        return projInfoList;
    }

    public bool SetTags()
    {
        BCOM.Element bcomElement;
        FrameList.GetElement(out bcomElement);

        bool res = false;

        foreach (TagToDataGroupMapProperty mapTag in TagsToDataGroupMapping.Instance.Items)
        {
            res |= TagsToDataGroupConverter.SetMapTagOnElement(bcomElement, mapTag);
        }
        return res;
    }

    public const string CELL_NAME = "Penetration";
    public const string CELL_NAME_OLD = "EmbeddedPart";

    public const string DG_CATALOG_TYPE = "EmbeddedPart";
    public const string DG_CATALOG_INSTANCE = "Embedded Part";
    public const string DG_SCHEMA_NAME = "EmbPart";

    private const string LEVEL_NAME = "C-EMBP-PNTR";
    private const string LEVEL_SYMB_NAME = "C-EMB-ANNO";
    private const string LEVEL_FLANGE_SYMB_NAME = "C-EMB-FLANGE";
    private const string LEVEL_POINT_NAME = "C-EMB-POINT";
}
}
