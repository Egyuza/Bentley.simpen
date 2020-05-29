using System;
using System.Collections.Generic;
using System.Linq;

using Bentley.Interop.TFCom;
using BCOM = Bentley.Interop.MicroStationDGN;
using TFCOM = Bentley.Interop.TFCom;

using Shared.Bentley;

#if V8i
using Bentley.Internal.MicroStation.Elements;
using BMI = Bentley.MicroStation.InteropServices;

#elif CONNECT
using Bentley.DgnPlatformNET.Elements;
using Bentley.MstnPlatformNET;
using Bentley.GeometryNET;
using Bentley.DgnPlatformNET;
using Bentley.Building.DataGroupSystem.Serialization;
using Bentley.Building.DataGroupSystem;
using Bentley.Building.Api;
using Bentley.Interop.TFCom;

using BMI = Bentley.MstnPlatformNET.InteropServices;
#endif

namespace Embedded.Penetrations.Shared
{
public static class PenetrHelper
{
        //static double 
        //    perMaster,
        //    subPerMaster,
        //    perStorage,
        //    perSub,
        //    active_perMaster,
        //    active_subPerMaster,
        //    active_perStorage,
        //    active_perSub;

        // TODO улучшить логику!
        //private static void updateUORs(PenetrTask task)
        //{
        //    BCOM.ModelReference taskModel = task.modelRef;
        //    perMaster = taskModel.UORsPerMasterUnit;
        //    subPerMaster = taskModel.SubUnitsPerMasterUnit;
        //    perStorage = taskModel.UORsPerStorageUnit;
        //    perSub = taskModel.UORsPerSubUnit;

        //    BCOM.ModelReference activeModel = App.ActiveModelReference;               

        //    active_perMaster = activeModel.UORsPerMasterUnit;
        //    active_subPerMaster = activeModel.SubUnitsPerMasterUnit;
        //    active_perStorage = activeModel.UORsPerStorageUnit;
        //    active_perSub = activeModel.UORsPerSubUnit;
        //}

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


        //    public static TFCOM.TFFrameList createFrameList(
        //        PenetrTask task, PenetrInfo penInfo)
        //    {
        //        //long diamIndex = DiameterType.Parse(task.DiameterTypeStr).number;  
        //        //PenetrInfo penInfo = penData.getPenInfo(task.FlangesType, diamIndex); 

        //        TFCOM.TFFrameList frameList =
        //            PenetrHelper.createFrameListRaw(task, penInfo, PenetrTask.LevelMain);

        //#if V8i
        //        PenetrHelper.addProjection(ref frameList, 
        //            task, penInfo, levelSymb, levelRefPoint);

        //        // TODO видимость контура перфоратора можно в конфиг. переменную
        //        PenetrHelper.addPerforator(ref frameList, task, penInfo, levelSymb, false);

        //#elif CONNECT

        //        addProjectionAndPerfoNet(
        //            ref frameList, task, penInfo, levelSymb, levelRefPoint);
        //#endif
        //        return frameList;
        //    }

    public static BCOM.Element getPenElementWithoutFlanges(PenetrTask task, PenetrInfo penInfo)
    {
        UOR uor = new UOR(task.modelRef);
        double pipeOutsideDiam = uor.convertToMaster(penInfo.pipeDiameterOutside);
        double length = uor.convertToMaster((task.LengthCm * 10.0));
        var cylinder = App.SmartSolid.CreateCylinder(null, pipeOutsideDiam / 2.0, length);
        return cylinder.transformByTask(task, shiftZ: length/2.0);
    }

    public static TFCOM.TFFrameListClass createFrameList(
        PenetrTask task, PenetrInfo penInfo, BCOM.Level level)
    {
        task.scanInfo();
        
        var taskUOR = new UOR(task.modelRef);

        double pipeInsideDiam = penInfo.pipeDiameterInside / taskUOR.activeSubPerMaster;
        double pipeOutsideDiam = penInfo.pipeDiameterOutside / taskUOR.activeSubPerMaster;

        double flangeInsideDiam = penInfo.flangeDiameterInside / taskUOR.activeSubPerMaster;
        double flangeOutsideDiam = penInfo.flangeDiameterOutside / taskUOR.activeSubPerMaster;
        double flangeThick = penInfo.flangeThick / taskUOR.activeSubPerMaster;

        double length = task.LengthCm *10 / taskUOR.activeSubPerMaster;

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

        TFCOM.TFFrameListClass frameList = new TFCOM.TFFrameListClass();

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

        frameList.AsTFFrame.SetName(PenetrTask.CELL_NAME); // ранее было 'EmbeddedPart'
        return frameList;
    }

    public static void addProjection(
        TFCOM.TFFrameList frame, PenetrTask task, PenetrInfo penInfo)
    {
        var taskUOR = new UOR(task.modelRef);

        double pipeInsideDiam = penInfo.pipeDiameterInside / taskUOR.activeSubPerMaster;
        double pipeOutsideDiam = penInfo.pipeDiameterOutside / taskUOR.activeSubPerMaster;

        double flangeInsideDiam = penInfo.flangeDiameterInside / taskUOR.activeSubPerMaster;
        double flangeOutsideDiam = penInfo.flangeDiameterOutside / taskUOR.activeSubPerMaster;
        double flangeThick = penInfo.flangeThick / taskUOR.activeSubPerMaster;
        double length = task.LengthCm *10 / taskUOR.activeSubPerMaster;

        App.Point3dZero();
        addProjectionToFrame(frame, ElementHelper.createCrossRound(pipeInsideDiam)
            .transformByTask(task), "cross", PenetrTask.LevelSymb);
        addProjectionToFrame(frame, ElementHelper.createCircle(pipeInsideDiam)
            .transformByTask(task), "circle", PenetrTask.LevelSymb);
        addProjectionToFrame(frame, ElementHelper.createCrossRound(pipeInsideDiam)
            .transformByTask(task, shiftZ: length), "cross", PenetrTask.LevelSymb);
        addProjectionToFrame(frame, ElementHelper.createCircle(pipeInsideDiam)
            .transformByTask(task, shiftZ: length), "circle", PenetrTask.LevelSymb);
        if (task.FlangesCount > 0)
        {
            addProjectionToFrame(frame, ElementHelper.createCircle(flangeOutsideDiam).
                transformByTask(task, 0.0, 0.0, -PenetrTask.FLANGE_SHIFT), 
                "flange", PenetrTask.LevelFlangeSymb);

            if (task.FlangesCount == 2)
            {
                addProjectionToFrame(frame, ElementHelper.createCircle(flangeOutsideDiam).
                    transformByTask(task, 0.0, 0.0, length + PenetrTask.FLANGE_SHIFT), 
                    "flange", PenetrTask.LevelFlangeSymb);
            }
        }
        addProjectionToFrame(frame, ElementHelper.createPoint().transformByTask(task), 
            "refPoint", PenetrTask.LevelRefPoint);
    }

    public static void addPerforator (TFCOM.TFFrameList frameList, 
        PenetrTask task, PenetrInfo penInfo, BCOM.Level levelSymb, bool isVisible)
    {
        var taskUOR = new UOR(task.modelRef);

        double pipeInsideDiam = penInfo.pipeDiameterInside / taskUOR.activeSubPerMaster;
        double pipeOutsideDiam = penInfo.pipeDiameterOutside / taskUOR.activeSubPerMaster;

        double flangeInsideDiam = penInfo.flangeDiameterInside / taskUOR.activeSubPerMaster;
        double flangeOutsideDiam = penInfo.flangeDiameterOutside / taskUOR.activeSubPerMaster;
        double flangeThick = penInfo.flangeThick / taskUOR.activeSubPerMaster;
        double length = task.LengthCm *10 / taskUOR.activeSubPerMaster;

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
        
        {
            // ! без этого кода не срабатывает перфорация в стенке/плите
            // судя по всему инициализирует обновление объектов, с которыми
            // взаимодействует frame
            
            AppTF.ModelReferenceUpdateAutoOpeningsByFrame(
                App.ActiveModelReference, frameList.AsTFFrame, true, false, 
                TFCOM.TFdFramePerforationPolicy.tfdFramePerforationPolicyNone);
        }

    }

    /// <summary>
    /// без этого кода не срабатывает перфорация в стенке/плите
    /// судя по всему инициализирует обновление объектов, с которыми
    /// взаимодействует frame
    /// </summary>
    public static void applyPerforatorInModel(TFCOM.TFFrameList frameList)
    {
        AppTF.ModelReferenceUpdateAutoOpeningsByFrame(
            App.ActiveModelReference, frameList.AsTFFrame, true, false, 
            TFCOM.TFdFramePerforationPolicy.tfdFramePerforationPolicyNone);
    }

    public static void addToModel(TFCOM.TFFrameList frameList)
    {
        AppTF.ModelReferenceAddFrameList(App.ActiveModelReference, frameList);
    }

    public static void addProjectionToFrame(TFCOM.TFFrameList frameList, 
        BCOM.Element element, string projectionName, BCOM.Level level)
    {
        TFCOM.TFProjectionList tfProjection = AppTF.CreateTFProjection((string)null);
        tfProjection.Init();
        element.Level = level;

        ElementHelper.setSymbologyByLevel(element);
        tfProjection.AsTFProjection.SetDefinitionName(projectionName);
        tfProjection.AsTFProjection.SetEmbeddedElement(element);
        tfProjection.AsTFProjection.SetIsDoubleSided(true);

        TFCOM.TFProjectionList projectionList = frameList.AsTFFrame.GetProjectionList();
        if (projectionList == null)
        {
            frameList.AsTFFrame.SetProjectionList(tfProjection);
        }
        else
        {
            projectionList.Append(tfProjection);
        }
    }

    public static T transformByTask<T>(this T element, PenetrTask task,
            double shiftX = 0.0, double shiftY = 0.0, double shiftZ = 0.0)
        where T : BCOM.Element
    {
        BCOM.Point3d shiftPoint = App.Point3dFromXYZ(shiftX, shiftY, shiftZ);
        element.Move(ref shiftPoint);

        double aboutX;
        double aboutY;
        double aboutZ;
        task.getCorrectiveAngles(out aboutX, out aboutY, out aboutZ);
        BCOM.Point3d zero = App.Point3dZero();
        element.Rotate(ref zero, aboutX, aboutY, aboutZ);

        BCOM.Matrix3d rotation = task.Rotation;
        BCOM.Transform3d transform3d = App.Transform3dFromMatrix3d(ref rotation);
        element.Transform(ref transform3d);

        BCOM.Point3d location = task.Location;
        element.Move(ref location);
        return element;
    }

#if V8i

#elif CONNECT_test
    public static void addToModelWithProjAndPerfo (ref TFCOM.TFFrameList frameList, 
        PenetrTask task, PenetrInfo penInfo, 
        BCOM.Level levelSymb, BCOM.Level levelRefPoint)
    {
        var taskUOR = new UOR(task.modelRef);

        double pipeInsideDiam = penInfo.pipeDiameterInside / taskUOR.active_subPerMaster;
        double pipeOutsideDiam = penInfo.pipeDiameterOutside / taskUOR.active_subPerMaster;

        double flangeInsideDiam = penInfo.flangeDiameterInside / taskUOR.active_subPerMaster;
        double flangeOutsideDiam = penInfo.flangeDiameterOutside / taskUOR.active_subPerMaster;
        double flangeThick = penInfo.flangeThick / taskUOR.active_subPerMaster;

        double length = task.LengthCm *10 / taskUOR.active_subPerMaster;

        BCOM.ModelReference activeModel = App.ActiveModelReference;
        //AppTF.ModelReferenceAddFrameList(activeModel, ref frameList);

        frameList.Synchronize();
        var frameListClass = frameList as TFCOM.TFFrameListClass;

        BCOM.Element bcomElem;
        frameListClass.GetElement(out bcomElem);          
                         
        var tfApi = new Bentley.Building.Api.TFApplicationList().AsTFApplication;
        var modelRef = Session.Instance.GetActiveDgnModelRef();
        var model = Session.Instance.GetActiveDgnModel();

        Element ielement = Element.GetFromElementRef((IntPtr)bcomElem.MdlElementRef());
        modelRef.GetFromElementRef((IntPtr)bcomElem.MdlElementRef());

        ITFFrameList iframeList;
        tfApi.CreateTFFrame(0, out iframeList);
        iframeList.InitFromElement(ielement, "");
        iframeList.Synchronize("");

        DPoint3d origin = task.Location.ToDPoint();
        origin.ScaleInPlace(taskUOR.active_perMaster);

        DMatrix3d matrix = DMatrix3d.FromRows(
            task.Rotation.RowX.ToDVector(), task.Rotation.RowY.ToDVector(),
            task.Rotation.RowZ.ToDVector());

        DTransform3d dTran = DTransform3d.FromMatrixAndTranslation(matrix, origin);
        TransformInfo tranInfo = new TransformInfo(dTran);

        double pipeInsideRadius = pipeOutsideDiam/2 * taskUOR.active_perMaster;
        double dgnLength = length * taskUOR.active_perMaster;

        var ellips = new EllipseElement(model, null, 
            DEllipse3d.FromCenterRadiusNormal(DPoint3d.Zero, pipeInsideRadius, 
            DVector3d.FromXY(0, 1)));        
            
        ellips.ApplyTransform(tranInfo);
        
        
     


        //{  // ПЕРФОРАТОР:
        //    ITFPerforatorList perfoList;
        //    tfApi.CreateTFPerforator(0, out perfoList);
        //    var dir = DVector3d.FromXY(1, 0);
        //    var tran = DTransform3d.Identity;
        //    //perfoList.InitFromElement(ellips, ref dir, length*toUOR, ref tran, "");
        //    perfoList.InitFromElement2(ellips, length*taskUOR.active_perMaster, "");
        //    perfoList.AsTFPerforator.SetIsVisible(false, 0);
        //    perfoList.SetSweepMode(Bentley.Building.Api.TFdPerforatorSweepMode.tfdPerforatorSweepModeBi, "");
        //    perfoList.SetPolicy(Bentley.Building.Api.TFdPerforatorPolicy.tfdPerforatorPolicyThroughHoleWithinSenseDist, "");
        
        //    (iframeList as Bentley.Building.Api.TFFrameList).SetPerforatorList(ref perfoList, 0);

        //   // iframeList.AsTFFrame.SetPerforatorList(ref perfoList, 0);
        //    iframeList.AsTFFrame.SetSenseDistance2(length, 0);
        //    iframeList.AsTFFrame.SetPerforatorsAreActive(true, 0);
            //        var frame = iframeList.AsTFFrame;
            //    tfApi.ModelReferenceUpdateAutoOpeningsByFrame(modelRef,
            //ref frame, true, false, Bentley.Building.Api.TFdFramePerforationPolicy.tfdFramePerforationPolicyStrict, 0);

        //}



       // tfApi.ModelReferenceRewriteFrameInstance(modelRef, iframeList.AsTFFrame, 0);
        //iframeList.AsTFFrame.Synchronize(0);
                
        { // ПРОЕКЦИОННАЯ ГЕОМЕТРИЯ
            ITFProjectionList projList, projList1, projList2, projList3;
            tfApi.CreateTFProjection(0, out projList);
            tfApi.CreateTFProjection(0, out projList1);
            tfApi.CreateTFProjection(0, out projList2);
            tfApi.CreateTFProjection(0, out projList3);

            var zero = DPoint3d.Zero;
            DPoint3d[] verts = { zero, zero, zero, zero, zero };
            double k = pipeInsideRadius * Math.Cos(Math.PI / 4);
            verts[0].X = -k;
            verts[0].Z = -k;
            verts[1].X = k;
            verts[1].Z = k;
            verts[3] = verts[0];
            verts[3].Z *= -1;
            verts[4] = verts[1];
            verts[4].Z *= -1;

            LineStringElement cross1 = new LineStringElement(model, null, verts);
            for (int i = 0; i < verts.Count(); ++i)
            {
                verts[i].Y = dgnLength;
            }
            LineStringElement cross2 = new LineStringElement(model, null, verts);

            cross1.ApplyTransform(tranInfo);
            cross2.ApplyTransform(tranInfo);

            projList1.AsTFProjection.SetEmbeddedElement(cross1, 0);
            projList2.AsTFProjection.SetEmbeddedElement(cross2, 0);

            LineElement refPoint =
                new LineElement(model, null, new DSegment3d(zero, zero));

            refPoint.ApplyTransform(tranInfo);
            ElementPropertiesSetter setter = new ElementPropertiesSetter();
            setter.SetWeight(7);
            setter.Apply(refPoint);

            projList3.AsTFProjection.SetEmbeddedElement(refPoint, 0);

            projList.Append(projList1, "");
            projList.Append(projList2, "");
            projList.Append(projList3, "");
            iframeList.AsTFFrame.SetProjectionList(projList, 0);
            //iframeList.AsTFFrame.Synchronize(0);
            //iframeList.Synchronize(string.Empty);
        }

       tfApi.ModelReferenceRewriteFrameInstance(modelRef, iframeList.AsTFFrame, 0);

      //  int stat = tfApi.ModelReferenceRewriteFrameList(modelRef, iframeList, 0);
            //tfApi.ModelReferenceAddFrameList(modelRef, ref iframeList, 0); 


    //    tfApi.ModelReferenceRewriteFrameList(modelRef, iframeList, 0);



        //frameListClass = frameList as TFCOM.TFFrameListClass;
        //frameListClass.GetElement(out bcomElem);     

        // setDataGroupInstance(bcomElem, task);
    }

    public static ITFFrameList createFrameCN
        (PenetrTask task, PenetrInfo penInfo)
    {
        task.scanInfo();

        // TODO отключено временно, до решения по алгоритму пересечений фланцев:
        //if (!Keyins.Penetrations.DEBUG_MODE) {
        //    if (task.isCompoundExistsInPlace || task.TFFormsIntersected.Count == 0) 
        //        return null;
        //}
        
        var taskUOR = task.UOR;

        double pipeInsideDiam = penInfo.pipeDiameterInside * taskUOR.active_perSub;
        double pipeOutsideDiam = penInfo.pipeDiameterOutside * taskUOR.active_perSub;

        double flangeInsideDiam = penInfo.flangeDiameterInside * taskUOR.active_perSub;
        double flangeOutsideDiam = penInfo.flangeDiameterOutside * taskUOR.active_perSub;
        double flangeThick = penInfo.flangeThick * taskUOR.active_perSub;

        double length = task.Length * taskUOR.active_perSub;

        double pipeInRadius = pipeInsideDiam / 2;
        double pipeOutRadius = pipeOutsideDiam / 2;


        var tfApi = new Bentley.Building.Api.TFApplicationList().AsTFApplication;

        BCOM.ModelReference activeModel = App.ActiveModelReference;
        
        //ITFBrepList brepList;
        //tfApi.CreateTFBrep(0, out brepList);

        DPoint3d origin = task.Location.ToDPoint();
        origin.ScaleInPlace(taskUOR.active_perMaster);

        DMatrix3d rot = task.Rotation.ToDMatrix3d();
        var dTran = DTransform3d.FromMatrixAndTranslation(rot, origin);
        var tranInfo = new TransformInfo(dTran);
        // DMatrix3d.FromColumns(
        //rot.RowX.ToDVector(), rot.RowX.ToDVector(), rot.RowX.ToDVector());

        //ITFFormRecipeList recipeList;
        
        var model = Session.Instance.GetActiveDgnModel();
        var modelRef = Session.Instance.GetActiveDgnModelRef();

        //CellHeaderElement taskEl = Element.GetFromElementRef(task.elemRefP) as CellHeaderElement;
        //DPoint3d taskOrigin;
        //taskEl.GetSnapOrigin(out taskOrigin);

        var ellips = new EllipseElement(model, null, 
            DEllipse3d.FromCenterRadiusNormal(DPoint3d.Zero, pipeOutRadius, DVector3d.FromXY(0, 1)));
        ellips.ApplyTransform(tranInfo);

        var cone = new ConeElement(model, null, pipeOutRadius, pipeOutRadius,
            DPoint3d.FromXYZ(0, length, 0), DPoint3d.Zero, 
            DMatrix3d.Rotation(DVector3d.UnitX, Angle.FromDegrees(-90)), true);
        cone.ApplyTransform(tranInfo);

        var ellips2 = new EllipseElement(model, null, DEllipse3d.FromCenterRadiusNormal(
                DPoint3d.Zero, pipeInRadius, DVector3d.FromXY(0, 1)));
        ellips2.ApplyTransform(tranInfo);

        var cone2 = new ConeElement(model, null, pipeInRadius, pipeInRadius,
            DPoint3d.Zero, DPoint3d.FromXYZ(0, length, 0), DMatrix3d.Zero, false);
        cone2.ApplyTransform(tranInfo);

        
        


        //int status = brepList.InitCylinder(pipeInsideRadius*task_subPerMaster, 
        //    (length - flangeThick)*task_subPerMaster, ref origin,
        //    ref matrix, "");
            
        //ITFElementList elemList;
        //tfApi.CreateTFElement(0, out elemList);
        // Bentley.GeometryNET.Common.CircularCylinder

        

        //ITFBrepList coneBrepList, cone2BrepList, resBrepList;
        //tfApi.CreateTFBrep(0, out coneBrepList);
        //tfApi.CreateTFBrep(0, out cone2BrepList);
        //coneBrepList.InitFromElement(cone, modelRef, "");
        //cone2BrepList.InitFromElement(cone2, modelRef, "");
        
        //coneBrepList.AsTFBrep.InitCylinder(pipeInsideRadius, length, ref origin,
        //    ref matrix, 0);

        ITFItemList itemList;
        tfApi.CreateTFItem(0, out itemList);        

        //var sweepDir = DVector3d.FromXY(1, 0);
        //coneBrepList.AsTFBrep.Drop(out resBrepList, cone2BrepList, 0);
        //sweepDir.NegateInPlace();
       // coneBrepList.AsTFBrep.Cut(out resBrepList, cone2BrepList,  ref sweepDir, length + 150, false, 0);
        //coneBrepList.AsTFBrep.SweepByVector3(ref sweepDir, length + 300, 
        //    pipeOutsideRadius - pipeInsideRadius, 0, 0);

        //Array arr = new System.Collections.ArrayList().ToArray();

        //coneBrepList.AsTFBrep.Cut2(out resBrepList, cone2BrepList.AsTFBrep, ref sweepDir,
        //Bentley.Building.Api.TFdBrepCutMethod.tfdBrepCutMethod_Outside,
        //Bentley.Building.Api.TFdBrepCutDirection.tfdBrepCutDirection_Both,
        //Bentley.Building.Api.TFdBrepCutDepth.tfdBrepCutDepth_UpToSolid, length,
        //arr, 0, false, Bentley.Building.Api.TFdBrepCutDepth.tfdBrepCutDepth_UpToSolid, length,
        //arr, 0, false, 0, 0, 0.00005, 0);

        //lement resElement;
        //resBrepList.GetElement(out resElement, 0, "");
        //coneBrepList.GetElement(out resElement, 0, "");

        ITFFrameList frameList;
        tfApi.CreateTFFrame(0, out frameList);
        frameList.AsTFFrame.Add3DElement(cone, 0);
        //frameList.AsTFFrame.Add3DElement(cone2, 0);
        //frameList.AsTFFrame.Add3DElement(resElement, 0);    
        
        //ITFFrameList openingFrameList;
        //tfApi.CreateTFFrame(0, out openingFrameList);
        //openingFrameList.AsTFFrame.Add3DElement(cone2, 0);

        //ITFFormRecipeList openRecipeList;
        ////tfApi.CreateTFFormRecipeArc
        //ITFFormRecipe openRecipe;
        //openingFrameList.AsTFFrame.GetFormRecipeList(0, out openRecipeList);
        //openRecipe = openRecipeList.AsTFFormRecipe;

        //ITFItemList featureList;
        //frameList.AsTFFrame.AddOpeningsToForm(out featureList, ref openRecipe, "", 0);


        ITFPerforatorList perfoList;
        tfApi.CreateTFPerforator(0, out perfoList);
        var dir = DVector3d.FromXY(1, 0);
        var tran = DTransform3d.Identity;
        perfoList.InitFromElement(ellips, ref dir, length, ref tran, "");
        perfoList.AsTFPerforator.SetIsVisible(false, 0);
        perfoList.SetSweepMode(Bentley.Building.Api.TFdPerforatorSweepMode.tfdPerforatorSweepModeBi, "");
        perfoList.SetPolicy(Bentley.Building.Api.TFdPerforatorPolicy.tfdPerforatorPolicyThroughHoleWithinSenseDist, "");
        

        frameList.AsTFFrame.SetPerforatorList(ref perfoList, 0);
        frameList.AsTFFrame.SetSenseDistance2(length/100, 0);
        frameList.AsTFFrame.SetPerforatorsAreActive(true, 0);

                
        int stat; // = tfApi.ModelReferenceAddFrameList(modelRef, ref frameList, 0); 
        var frame = frameList.AsTFFrame;

        stat = tfApi.ModelReferenceUpdateAutoOpeningsByFrame(modelRef,
            ref frame, true, false, Bentley.Building.Api.TFdFramePerforationPolicy.tfdFramePerforationPolicyNone, 0); 


        //Element cylindr;
        //brepList.GetElement(out cylindr, 0, "");        

        //cylindr.AddToModel();

        //Element tfElement;
        //Element perfo = null;
        //Element dp_2d = null;
        //int value;
        //tfApi.ModelReferenceAddElement(ref cylindr, Session.Instance.GetActiveDgnModel(),
        //    0, 0, out value);        

        //tfApi.ModelReferenceConstructFrameElement(Session.Instance.GetActiveDgnModel(),
        //    0, ref cylindr, ref perfo, ref origin, ref dp_2d, "name", null,
        //    1, 0, false, null, false, task.Length, 0.00005, 0, out tfElement);

        //frameList.InitFromElement(cylindr, "");

       // frameList.AsTFFrame.Add3DElement(cylindr, 0);
                

        return frameList;
    }

#endif

    public static bool IsPenetrationCell(this BCOM.Element element)
    {
        if (!element.IsCompundCell())
            return false;

        var cell = element.AsCellElement();
        return cell.Name == PenetrTask.CELL_NAME || 
            cell.Name == PenetrTask.CELL_NAME_OLD;
    }

    static BCOM.Application App
    {
        get { return BMI.Utilities.ComApp; }
    }

    static TFCOM.TFApplication _tfApp;
    static TFCOM.TFApplication AppTF
    {
        get
        {
            return _tfApp ?? 
                (_tfApp = new TFCOM.TFApplicationList().TFApplication);
        }
    } 
}
}
