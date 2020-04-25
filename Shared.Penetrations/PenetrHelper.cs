using System;
using System.Collections.Generic;

using BCOM = Bentley.Interop.MicroStationDGN;
using TFCOM = Bentley.Interop.TFCom;

using Shared.Bentley;

#if V8i
using Bentley.Internal.MicroStation.Elements;
using BMI = Bentley.MicroStation.InteropServices;
#elif CONNECT
using Bentley.DgnPlatformNET.Elements;
using Bentley.MstnPlatformNET;
using BMI = Bentley.MstnPlatformNET.InteropServices;
#endif

namespace Embedded.Penetrations.Shared
{
public class PenetrHelper : BentleyInteropBase
{
    static double 
        task_toUOR,
        task_subPerMaster,
        task_unit3,
        task_unit4,            
        toUOR,
        subPerMaster,
        unit3,
        unit4;

    // TODO улучшить логику!
    private static void updateUORs(PenetrTask task)
    {
        BCOM.ModelReference taskModel = 
            App.MdlGetModelReferenceFromModelRefP((int)task.modelRefP);
            taskModel.MdlModelRefP();
        task_toUOR = taskModel.UORsPerMasterUnit;
        task_subPerMaster = taskModel.SubUnitsPerMasterUnit;
        task_unit3 = taskModel.UORsPerStorageUnit;
        task_unit4 = taskModel.UORsPerSubUnit;

        BCOM.ModelReference activeModel = App.ActiveModelReference;               

        toUOR = activeModel.UORsPerMasterUnit;
        subPerMaster = activeModel.SubUnitsPerMasterUnit;
        unit3 = activeModel.UORsPerStorageUnit;
        unit4 = activeModel.UORsPerSubUnit;
    }

    //**********************************************************************
    //{ // ПОСТРОЕНИЕ ЧЕРЕЗ ПРОФИЛЬ И ПУТЬ
    // *********************************************************************
    //    BCOM.LineElement line = App.CreateLineElement2(null, 
    //        App.Point3dZero(), App.Point3dFromXYZ(0, 1, 1));

    //    BCOM.EllipseElement circle = App.CreateEllipseElement2(null, 
    //        App.Point3dZero(), pipeOutsideDiam/2, pipeOutsideDiam/2,
    //        App.Matrix3dIdentity());

    //    elements.Clear();
    //    elements.Add(solids.SweepProfileAlongPath(circle, line),task.Location);
    //}    


    public static TFCOM.TFFrameList createFrameList(PenetrTask task, PenetrInfo penInfo)
    {
        BCOM.Level level = ElementHelper.getOrCreateLevel(PenetrTask.LEVEL_NAME);
        BCOM.Level levelSymb = 
            ElementHelper.getOrCreateLevel(PenetrTask.LEVEL_SYMB_NAME);
        BCOM.Level levelRefPoint = 
            ElementHelper.getOrCreateLevel(PenetrTask.LEVEL_POINT_NAME);

        long diamIndex = DiameterType.Parse(task.DiameterTypeStr).number;  
        //PenetrInfo penInfo = penData.getPenInfo(task.FlangesType, diamIndex); 

        TFCOM.TFFrameList frameList =
            PenetrHelper.createFrameList(task, penInfo, level);

        PenetrHelper.addProjection(ref frameList, 
            task, penInfo, levelSymb, levelRefPoint);

        // TODO видимость контура перфоратора можно в конфиг. переменную
        PenetrHelper.addPerforator(ref frameList, task, penInfo, levelSymb, false);

        return frameList;
    }

    private static TFCOM.TFFrameList createFrameList(
        PenetrTask task, PenetrInfo penInfo, BCOM.Level level)
    {
        task.scanInfo();
        
        // TODO отключено временно, до решения по алгоритму пересечений фланцев:
        //if (!Keyins.Penetrations.DEBUG_MODE) {
        //    if (task.isCompoundExistsInPlace || task.TFFormsIntersected.Count == 0) 
        //        return null;
        //}
        
        updateUORs(task);

        double pipeInsideDiam = penInfo.pipeDiameterInside / subPerMaster;
        double pipeOutsideDiam = penInfo.pipeDiameterOutside / subPerMaster;

        double flangeInsideDiam = penInfo.flangeDiameterInside / subPerMaster;
        double flangeOutsideDiam = penInfo.flangeDiameterOutside / subPerMaster;
        double flangeThick = penInfo.flangeThick / subPerMaster;

        double length = task.Length *10 / subPerMaster;

        var solids = App.SmartSolid;

        /*
            ! длина трубы меньше размера проходки на толщину фланца
            ! ЕСЛИ ФЛАНЕЦ ЕСТЬ
        */

        double delta = task.FlangesCount == 0 ? 0 : 
            task.FlangesCount * flangeThick / 2;        

        BCOM.SmartSolidElement cylindrInside =
            solids.CreateCylinder(null, pipeInsideDiam / 2, length - delta);

        BCOM.SmartSolidElement cylindrOutside =
            solids.CreateCylinder(null, pipeOutsideDiam / 2, length - delta);
        
        var cylindr = solids.SolidSubtract(cylindrOutside, cylindrInside);

        var elements = new Dictionary<BCOM.Element, double>();

        {
            double shift  = task.FlangesCount == 1 ? delta : 0;
            shift *= task.isSingleFlangeFirst() ? 1 : -1;
            elements.Add(cylindr, (length + shift)/2);
        }        

        // Фланцы:
        for (int i = 0; i < task.FlangesCount; ++i)
        {
            BCOM.SmartSolidElement flangeCylindr = solids.SolidSubtract(
                solids.CreateCylinder(null, flangeOutsideDiam / 2, flangeThick), 
                solids.CreateCylinder(null, pipeOutsideDiam / 2, flangeThick));            
            
            double shift = 0;
            if (task.FlangesCount == 1)
            {
                bool isNearest = App.Vector3dEqualTolerance(task.singleFlangeSide,
                    App.Vector3dFromXYZ(0, 0, -1), 0.1); // 0.001
                
                // 0.5 - для видимости фланцев на грани стены/плиты 
                shift = isNearest ?
                        0.0    + flangeThick / 2 - 1: // 0.02:
                        length - flangeThick / 2 + 1; // 0.02;
            }
            else
            {
                shift = i == 0 ? 0.0 : length;               
                // для самих фланцев:
                // 0.5 - для видимости фланцев на грани стены/плиты 
                shift += Math.Pow(-1, i) * (flangeThick/2 - 1); //0.02);
            }
            elements.Add(flangeCylindr, shift);
        }
         
        BCOM.Transform3d taskTran = App.Transform3dFromMatrix3d(task.Rotation);
        
        double aboutX, aboutY, aboutZ;
        task.getCorrectiveAngles(out aboutX, out aboutY, out aboutZ);

        TFCOM.TFFrameList frameList = AppTF.CreateTFFrame();

        foreach (var pair in elements)
        {
            BCOM.Element elem = pair.Key;
            double shift = pair.Value;
            
            elem.Color = 0; // TODO
            BCOM.Point3d offset = App.Point3dAddScaled(
                App.Point3dZero(), App.Point3dFromXYZ(0, 0, 1), shift);
            elem.Move(offset);

            elem.Rotate(App.Point3dZero(), aboutX, aboutY, aboutZ);
            
            elem.Transform(taskTran);
            elem.Move(task.Location);

            elem.Level = level;
            ElementHelper.setSymbologyByLevel(elem);

            frameList.AsTFFrame.Add3DElement(elem);
        }

        frameList.AsTFFrame.SetName("Penetration"); // ранее было 'EmbeddedPart'
        return frameList;
    }


    public static void addProjection (ref TFCOM.TFFrameList frameList, 
        PenetrTask task, PenetrInfo penInfo, 
        BCOM.Level levelSymb, BCOM.Level levelRefPoint)
    {
        updateUORs(task);

        double pipeInsideDiam = penInfo.pipeDiameterInside / subPerMaster;
        double pipeOutsideDiam = penInfo.pipeDiameterOutside / subPerMaster;

        double flangeInsideDiam = penInfo.flangeDiameterInside / subPerMaster;
        double flangeOutsideDiam = penInfo.flangeDiameterOutside / subPerMaster;
        double flangeThick = penInfo.flangeThick / subPerMaster;
        double length = task.Length *10 / subPerMaster;

        var projections = new Dictionary<BCOM.Element, double>();

        { // Перекрестия: всегда в плоскости стены
            var zero = App.Point3dZero();
            projections.Add(
                ElementHelper.createCrossRound(ref zero, pipeInsideDiam), 
                0.0); 
            projections.Add(ElementHelper.createCircle(ref zero, pipeInsideDiam),
                0.0);         
            projections.Add(
                ElementHelper.createCrossRound(ref zero, pipeInsideDiam), 
                length);
            projections.Add(ElementHelper.createCircle(ref zero, pipeInsideDiam),
                length);
        }

        { // Точка установки:
            var pt = App.Point3dZero();
            BCOM.Element refPoint = 
                App.CreateLineElement2(null, pt, pt);
            
            projections.Add(refPoint, 0.0);
        }

        TFCOM.TFProjectionList projList = AppTF.CreateTFProjection();
        projList.Init();

        double aboutX, aboutY, aboutZ;
        task.getCorrectiveAngles(out aboutX, out aboutY, out aboutZ);

        BCOM.Transform3d taskTran = App.Transform3dFromMatrix3d(task.Rotation);
        
        foreach (var pair in projections)
        {
            BCOM.Element elem = pair.Key;
            double shift = pair.Value;

            BCOM.Point3d offset = App.Point3dAddScaled(
                App.Point3dZero(), App.Point3dFromXYZ(0, 0, 1), shift);
            elem.Move(offset);

            elem.Rotate(App.Point3dZero(), aboutX, aboutY, aboutZ);
            
            elem.Transform(taskTran);
            elem.Move(task.Location);

            elem.Level = (elem.Type == BCOM.MsdElementType.Line) ?
                levelRefPoint : levelSymb;
            ElementHelper.setSymbologyByLevel(elem);

            if (elem.Type == BCOM.MsdElementType.Line) {
                // точка вставки - линия с нулевой длинной           
                elem.Level = levelRefPoint;
            }

            var elemProjList = AppTF.CreateTFProjection();
            elemProjList.AsTFProjection.SetEmbeddedElement(elem);
            projList.Append(elemProjList);
        }
        
        frameList.AsTFFrame.SetProjectionList(projList);
    }

    public static void addPerforator (ref TFCOM.TFFrameList frameList, 
        PenetrTask task, PenetrInfo penInfo, BCOM.Level levelSymb, bool isVisible)
    {
        updateUORs(task);

        double pipeInsideDiam = penInfo.pipeDiameterInside / subPerMaster;
        double pipeOutsideDiam = penInfo.pipeDiameterOutside / subPerMaster;

        double flangeInsideDiam = penInfo.flangeDiameterInside / subPerMaster;
        double flangeOutsideDiam = penInfo.flangeDiameterOutside / subPerMaster;
        double flangeThick = penInfo.flangeThick / subPerMaster;
        double length = task.Length *10 / subPerMaster;

        double aboutX, aboutY, aboutZ;
        task.getCorrectiveAngles(out aboutX, out aboutY, out aboutZ);

        BCOM.Transform3d taskTran = App.Transform3dFromMatrix3d(task.Rotation);

        // ПЕРФОРАТОР
        BCOM.EllipseElement perfoEl = 
            App.CreateEllipseElement2(null, App.Point3dZero(), 
                pipeInsideDiam/2, pipeInsideDiam/2, 
                App.Matrix3dIdentity(), BCOM.MsdFillMode.Filled);
        {
            BCOM.Point3d offset = App.Point3dAddScaled(
                App.Point3dZero(), 
                App.Point3dFromXYZ(0, 0, 1), length/2);
            perfoEl.Move(offset);
            perfoEl.Rotate(App.Point3dZero(), aboutX, aboutY, aboutZ);
        }
        perfoEl.Level = levelSymb;
        ElementHelper.setSymbologyByLevel(perfoEl);
        perfoEl.Transform(taskTran);
        perfoEl.Move(task.Location);      

        BCOM.Point3d perfoVec = perfoEl.Normal;        

        TFCOM.TFPerforatorList perfoList = AppTF.CreateTFPerforator();
        var tranIdentity = App.Transform3dIdentity();

        perfoList.InitFromElement(perfoEl, perfoVec, length/2 * 1.01, tranIdentity);
        perfoList.SetSweepMode(
            TFCOM.TFdPerforatorSweepMode.tfdPerforatorSweepModeBi);
        //perfoList.SetSenseDist(1.01 * length / 2);
        perfoList.SetPolicy(
            TFCOM.TFdPerforatorPolicy.tfdPerforatorPolicyThroughHoleWithinSenseDist);
        perfoList.SetIsVisible(isVisible);

        frameList.AsTFFrame.SetPerforatorList(perfoList);
        frameList.AsTFFrame.SetSenseDistance2(length/2);        
        frameList.AsTFFrame.SetPerforatorsAreActive(true);
        frameList.Synchronize();
        
    }
}
}
