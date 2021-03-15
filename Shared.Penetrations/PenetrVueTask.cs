﻿using System;
using System.Collections.Generic;

using BCOM = Bentley.Interop.MicroStationDGN;
using TFCOM = Bentley.Interop.TFCom;

#if V8i
using Bentley.Internal.MicroStation.Elements;
using BMI = Bentley.MicroStation.InteropServices;

#elif CONNECT
using Bentley.DgnPlatformNET.Elements;
using Bentley.MstnPlatformNET;
using BMI = Bentley.MstnPlatformNET.InteropServices;
#endif

using Shared.Bentley.sp3d;
using Shared.Bentley;
using Shared;
using System.Xml.Linq;
using System.Linq;
using System.Xml.XPath;
using System.Text.RegularExpressions;

namespace Embedded.Penetrations.Shared
{
public class PenetrVueTask : BentleyInteropBase, IPenetrTask
{
    public enum TaskTypeEnum
    {
        Pipe,
        PipeOld,
        PipeEquipment,
        Flange
    }
    public IntPtr elemRefP { get; private set; }
    public long elemId { get; private set; }
    private IntPtr modelRefP;
    public BCOM.ModelReference ModelRef => 
        App.MdlGetModelReferenceFromModelRefP((int)modelRefP);

    public BCOM.Attachment getAttachment() => 
        ModelRef.AsAttachment(App.ActiveModelReference);
    
    public BCOM.Element getElement() => 
        ElementHelper.getElementCOM(elemRefP, modelRefP);

    public string Name => $"T{PenCode}-{DiameterType.Number}-{LengthCm}";

    public string PenCode { get; private set; }

    public long FlangesType
    {
        get { return PenetrInfo.getFlangesType(PenCode); }
        set { PenCode = PenetrInfo.getPenCode(value); }
    }
        
    public int FlangesCount => FlangesType == 1 ? 1 : FlangesType == 2 ? 2 : 0; // TODO для "4"

    public double FlangeWallOffset => 
        1.0 / UOR.activeSubPerMaster; // TODO можно ли сделать мешьше - 0.02

    public string DiameterTypeStr {get; set;}
    public DiameterType DiameterType => DiameterType.Parse(DiameterTypeStr);
        
    /// <summary> длина в см </summary>
    public int LengthCm { get; set; }
    /// <summary> длина в мм </summary>
    public int Length => LengthCm * 10;

    private BCOM.Vector3d singleFlangeSide_ = App.Vector3dZero();
    /// <summary>
    /// расположение фланца, если он один
    /// </summary>
    public BCOM.Vector3d SingleFlangeSide
    {
        get { return (singleFlangeSide_ = App.Vector3dNormalize(singleFlangeSide_)); }
        private set { singleFlangeSide_ = value; }
    }

    public bool IsSingleFlangeFirst => App.Vector3dEqualTolerance(
        SingleFlangeSide, App.Vector3dFromXYZ(0, 0, -1), 0.1);
    
    
    public double getFlangeShift(bool first, double flangeThick)
    {
        return (first ? 1 : -1) * (flangeThick / 2 - 1);
    }

    public string RefPoint1 => Location.ToStringEx();
    public string RefPoint2 { get; private set; }
    public string RefPoint3 { get; private set; }

    //public ulong ElementId {get; private set; }
    public string Code { get; private set; }
    public string Oid { get; private set; }
    public string User { get; private set; }
    public string Path { get; private set; }
    public TaskTypeEnum TaskType { get; private set; }

    public BCOM.CellElement Cell { get; private set; }

    private BCOM.Point3d rawLocation_;
    private BCOM.Point3d? projectLocationOnForm_ = null;

    /// <summary> корректируется по проекции на объект </summary>
    public BCOM.Point3d Location => projectLocationOnForm_ ?? 
        RoundTool.roundExt(rawLocation_, /* 5 мм */ 5 / UOR.activeSubPerMaster);
    public BCOM.Matrix3d Rotation { get; private set; }

    private BCOM.Point3d? correctiveAngles_;
    public BCOM.Point3d CorrectiveAngles => correctiveAngles_.HasValue 
        ? correctiveAngles_.Value : (correctiveAngles_ = getCorrectiveAngles()).Value ;

    private UOR uor_;
    public UOR UOR => uor_ ?? (uor_ = new UOR(ModelRef));

    public string ErrorText { get; private set; }

    public bool isValid => string.IsNullOrEmpty(ErrorText);

    public List<string> Warnings { get; private set; } = new List<string>();

    /// <summary>
    /// В диапазоне задания найден элемент CompoundCell
    /// </summary>
    public bool isCompoundExistsInPlace
    {
        get { return CompoundsIntersected?.Count > 0; }
    }

    /// <summary>
    /// Геометрические фланцы в объекте задания
    /// </summary>
    public List<PenetrTaskFlange> FlangesGeom { get; private set; }
    /// <summary>
    /// TF-объекты, которые пересекает объект задания
    /// </summary>
    public List<TFCOM.TFElementList> TFFormsIntersected { get; private set; }
    public List<TFCOM.TFElementList> CompoundsIntersected { get; private set; }

    public Dictionary<Sp3dToDataGroupMapProperty, string>
    DataGroupPropsValues { get; set; }

    public static PenetrVueTask getByParameters(
        BCOM.Point3d location, long flangeType, int lengthCm)
    {
        return new PenetrVueTask(location, flangeType, lengthCm);
    }

    public void prepairDataGroup()
    {
    }

    public static bool getFromElement(Element element, out PenetrVueTask penTask)
    {
        Sp3dTask_Old taskOld = null;
        penTask = null;

        if (ElementHelper.isElementSp3dTask_Old(element, out taskOld) &&
            (taskOld.isPipe() || taskOld.isPipeOld() || taskOld.isEquipment()))
        {
            penTask = new PenetrVueTask(element, taskOld);
        }
        else
        {
            IEnumerable<string> sp3dXmlData = 
                ElementHelper.getSp3dXmlData(element);
            Sp3dPenTask sp3dTask = new Sp3dPenTask(sp3dXmlData);

            if (sp3dTask.Type == Sp3dPenTask.TaskType.Pipe && sp3dTask.IsValid)
            {
                penTask = new PenetrVueTask(element, sp3dTask);                
            }
        }
        
        return penTask != null ? penTask.isValid : false;
    }

    private void initialize_(Element element = null)
    {
        DataGroupPropsValues = new Dictionary<Sp3dToDataGroupMapProperty, string>();
        FlangesGeom = new List<PenetrTaskFlange>();
        TFFormsIntersected = new List<TFCOM.TFElementList>();
        CompoundsIntersected = new List<TFCOM.TFElementList>();

        if (element != null)
        {
            Cell = element.AsCellElementCOM();
            long id;
            IntPtr elRef, modelRef;
            ElementHelper.extractFromElement(element, out id, out elRef, out modelRef);

            elemRefP = elRef;
            modelRefP = modelRef;
            elemId = id;
        }
    }

    private PenetrVueTask(BCOM.Point3d location, long flangeType, int lengthCm)
    {
        initialize_();
        rawLocation_ = location;
        this.FlangesType = flangeType;
        this.LengthCm = lengthCm;
    }

    private PenetrVueTask(Element element, Sp3dPenTask task)
    {
        if (task == null)
            return;

        initialize_(element);

        if (task.Location != null)
        {
            rawLocation_ = Cell.Origin;
            rawLocation_ = App.Point3dFromXYZ(
                task.Location[0] * 1000, task.Location[1] * 1000, task.Location[2] * 1000);
        }
        else
        {
            rawLocation_ = Cell.Origin;
        }
        
        Rotation = Cell.Rotation;

        // разбор типоразмера:
        parseTypeSize(task.Name);
        Code = task.Code;

        testScan();

        //findFlangesOld();
        scanInfo();
    }
    
    private void testScan() 
    {
        if (Cell == null)
            return;

        var surfaces = new List<BCOM.Element>();
        collectSubElementsByType(Cell, ref surfaces, BCOM.MsdElementType.Surface);

        if (surfaces.Count > 0)
        {
            ;
        }
        else
        {
            var cones = new List<BCOM.ConeElement>();
            collectSubElementsByType(Cell, ref cones);
            ;
        }
    }

    private PenetrVueTask(Element element, Sp3dTask_Old task) 
    {
        initialize_(element);

        P3Dbase taskData = task.isEquipment() ? (P3Dbase)task.equipment : task.pipe;

        Oid = taskData.Oid;

        BCOM.ModelReference taskModel =
            App.MdlGetModelReferenceFromModelRefP((int)modelRefP);
        //BCOM.Element bcomEl = taskModel.GetElementByID(elemId);

        Rotation = taskData.getRotation();

        Code = taskData.Name.Trim();
        User = taskData.SP3D_UserLastModified.Trim();
        Path = taskData.SP3D_SystemPath;

        //BCOM.Point3d pt = App.Point3dFromXYZ(
        //    taskData.LocationX, taskData.LocationY, taskData.LocationZ);

        rawLocation_ = App.Point3dScale(taskData.getLocation(),
            taskModel.IsAttachmentOf(App.ActiveModelReference) ?
                UOR.subPerMaster : 1);

//#if V8i
//        rawLocation_ = App.Point3dScale(pt,
//            taskModel.IsAttachment ? UOR.subPerMaster : 1);
//#elif CONNECT
//        var actDgnModel = Session.Instance.GetActiveDgnModel();
//        rawLocation_ = App.Point3dScale(pt, 
//                element.DgnModel.IsDgnAttachmentOf(actDgnModel) ? 
//            UOR.subPerMaster : 1);
//#endif

        this.TaskType = task.isEquipment() ? TaskTypeEnum.PipeEquipment :
            task.component?.Description == "PenFlange" ? TaskTypeEnum.Flange :
            task.component?.Description == "PntrtPlate-d" ? TaskTypeEnum.PipeOld :
            TaskTypeEnum.Pipe;

        // разбор типоразмера:
        parseTypeSize(taskData.Description);

        // Свойства DataGroup
        // task.XmlDoc.Root

        foreach (var propMap in Sp3dToDataGroupMapping.Instance.Items)
        {  
            if (propMap.TargetXPath == null)
                continue;
            
            foreach (string path in propMap.Sp3dXmlPaths)
            {
                string propName;
                var xEl = task.XmlDoc.Root.getChildByRegexPath(path, out propName);
                //var xEl = xDoc.Root.XPathSelectElement(path);
                if (xEl != null)
                {
                    string value = propMap.getMapValue(xEl.Value);
                    DataGroupPropsValues.Add(propMap, value);
                }
            }
        }

        if (TaskType == TaskTypeEnum.Pipe)
            findFlanges();
        else
            findFlangesOld();

        scanInfo();
    }


    /// <summary>
    /// Разбор типоразмера
    /// </summary>
    private void parseTypeSize(string description)
    {
        try
        {
            string[] parameters = description.TrimStart('T').Split('-');            
            PenCode = parameters[0]; // до ввода фиброцементного типа проходок FlangesType = int.Parse(parameters[0]);
            DiameterTypeStr = new DiameterType(int.Parse(parameters[1])).ToString();
            LengthCm = int.Parse(parameters[2]);
        }
        catch (Exception)
        {
            ErrorText = $"Не удалось разобрать типоразмер \"{description}\"";
        }
    }

    private BCOM.CellElement getTaskCell()
    {
        BCOM.ModelReference taskModel = // TODO 
            App.MdlGetModelReferenceFromModelRefP((int)modelRefP);

        BCOM.ModelReference activeModel = App.ActiveModelReference;

        bool isAttachment = taskModel.IsAttachment;

        BCOM.CellElement cell = taskModel.IsAttachment ?
            taskModel.AsAttachment.GetElementByID(elemId).AsCellElement() :
            taskModel.GetElementByID(elemId).AsCellElement();

        return cell;
    }

    private BCOM.Point3d getCorrectiveAngles()
    {   
        BCOM.Point3d angles;

        angles.Z = 0;

        /*        
        Ориентация тела построения solid - вдоль оси Z
            Z              _____  
            ^  Y             |
            | /'             |
            !/___> X       __*__ 
        */
        if (TaskType == PenetrVueTask.TaskTypeEnum.PipeEquipment)
        {
            /* Ориентация проходки перед применением матрицы поворота задания
             * должна быть вдоль оси  X
             * 
                Y             |         |
                ^             • ========|
                !___> X       |         |
            */

            angles.X = 0;
            angles.Y = Math.PI/2;
            //rawVector = Addin.App.Point3dFromXYZ(1, 0, 0);
        }
        else
        {
            /* Ориентация проходки перед применением матрицы поворота задания
             * должна быть вдоль оси Y
             *                 _____
                Y                |
                ^                |
                !___> X        __*__
            */

            angles.X = -Math.PI/2;
            angles.Y = 0;
            //rawVector = Addin.App.Point3dFromXYZ(0, 1, 0);
        }
        return angles;
    }

    private void transformToBase(BCOM.Element element, BCOM.Point3d origin)
    {
        // инвертируем трансформацию:
        if (App.Matrix3dHasInverse(Rotation))
        {
            BCOM.Transform3d tran = App.Transform3dFromMatrix3d(Rotation);
            element.Transform(App.Transform3dInverse(tran));
        }
        var angles = CorrectiveAngles;
        element.Rotate(App.Point3dZero(), -angles.X, -angles.Y, -angles.Z);
    }

    /// <summary>
    /// Поиск фланцев на объекте задания старого типа
    /// </summary>
    private void findFlangesOld()
    {
        BCOM.ModelReference model =
            App.MdlGetModelReferenceFromModelRefP((int)modelRefP);

        SingleFlangeSide = App.Vector3dZero();

        // Todo из правильной модели
        BCOM.CellElement cell = model.GetElementByID(elemId).AsCellElement();
        if (cell == null)
            return;

        //var tran = App.Transform3dFromMatrix3d(Rotation);
        //if (App.Matrix3dHasInverse(Rotation))
        //{
        //    tran = App.Transform3dInverse(tran);
        //}
        //var tran = App.Transform3dInverse(
        //    App.Transform3dFromMatrix3d(Rotation));        
        //cell.Transform(tran);

        // TODO проверить корректность transform отн. rawLocation_
        transformToBase(cell, rawLocation_);

        var cones = new List<BCOM.ConeElement>();
        collectSubElementsByType(cell, ref cones);

        //var iter = bcomEl.AsCellElement().GetSubElements();
        //while (iter.MoveNext())
        //{
        //    var iterSub = iter.Current.AsCellElement().GetSubElements();
        //    while (iterSub.MoveNext())
        //    {
        //        if (iterSub.Current.IsConeElement())                
        //            cones.Add(iterSub.Current.AsConeElement());
        //    }
        //}

        cones.Sort((BCOM.ConeElement lhs, BCOM.ConeElement rhs) =>
            {
                return -1 * ElementHelper.getHeight(lhs).CompareTo(
                    ElementHelper.getHeight(rhs));
            });

        /* cones.Count ==
            * 2: без фланцев
            * 4: 1 фланец, определяем вектор с какой стороны фланец
            * 6: 2 фланца         
        */

        int geomFlangesCount = cones.Count / 2 - 1;

        if (geomFlangesCount == 1)
        { // 1 фланец, определяем вектор с какой стороны фланец относительно центра
            SingleFlangeSide = App.Vector3dSubtractPoint3dPoint3d(
                ElementHelper.getCenter(cones[2]),
                ElementHelper.getCenter(cones[0]));
        }
        else
        {
            // базовая ориентация при построении
            // фланец совпадает с Точкой установки
            SingleFlangeSide = App.Vector3dFromXYZ(0, 0, -1);
        }

        if (FlangesCount != geomFlangesCount)
        {
            Warnings.Add(string.Format("Несоответствие количества фланцев, " +
                "указанных в атрибутах - {0} и заданных геометрически - {1}",
                FlangesCount, geomFlangesCount));
        }
    }

    private void findFlanges()
    {
        BCOM.CellElement cell = getTaskCell();
        if (cell == null)
            return;

        BCOM.ModelReference taskModel = // TODO 
            App.MdlGetModelReferenceFromModelRefP((int)modelRefP);
        BCOM.ModelReference activeModel = App.ActiveModelReference;

        FlangesGeom.Clear();

        // Поиск фланцев
        BCOM.ElementScanCriteria criteria = new BCOM.ElementScanCriteriaClass();
        //criteria.IncludeOnlyVisible();
        criteria.ExcludeAllTypes();
        criteria.ExcludeNonGraphical();
        criteria.IncludeType(BCOM.MsdElementType.CellHeader);

        BCOM.Range3d scanRange = cell.Range;
        // App.Range3dScaleAboutCenter(cell.Range, 1.01);

        if (taskModel.IsAttachment)
        {
            double k = taskModel.UORsPerStorageUnit / activeModel.UORsPerStorageUnit;
            scanRange.High = App.Point3dScale(scanRange.High, k);
            scanRange.Low = App.Point3dScale(scanRange.Low, k);
        }

        criteria.IncludeOnlyWithinRange(scanRange);

        BCOM.ElementEnumerator res = taskModel.Scan(criteria);

        foreach (BCOM.Element current in res?.BuildArrayFromContents())
        {
            // todo использовать только один тип точек
            PenetrTaskFlange flangeTask = null;
            //Element curEl = Element.FromElementID((ulong)current.ID, 
            //    (IntPtr)current.ModelReference.MdlModelRefP());

            if (PenetrTaskFlange.getFromElement(current, out flangeTask))
            {
                BCOM.Range3d range = current.Range;

                if (flangeTask.Oid.Equals(Oid))
                    FlangesGeom.Add(flangeTask);
            }
        }

        //BCOM.Transform3d tran = App.Transform3dInverse(
        //    App.Transform3dFromMatrix3d(Rotation));
        //cell.Transform(tran);

        // TODO проверить корректность transform отн. rawLocation_
        transformToBase(cell, rawLocation_);

        if (FlangesCount != FlangesGeom.Count)
        {
            Warnings.Add(string.Format("Несоответствие количества фланцев, " +
                "указанных в атрибутах - {0} и заданных геометрически - {1}",
                FlangesCount, FlangesGeom.Count));
        }

        if (FlangesCount == 1 && FlangesGeom.Count > 0)
        {
            BCOM.Element flangeEl = taskModel.GetElementByID(FlangesGeom[0].elemId);

            //flangeEl.Transform(tran);

            // TODO проверить корректность transform отн. rawLocation_
            transformToBase(flangeEl, rawLocation_);

            SingleFlangeSide = App.Vector3dSubtractPoint3dPoint3d(
                ElementHelper.getCenter(flangeEl),
                ElementHelper.getCenter(cell));
        }
        else
        {
            SingleFlangeSide = App.Vector3dFromXYZ(0, 0, -1);
        }
    }

    // todo
    private void collectSubElementsByType<T>(BCOM.ComplexElement cell, ref List<T> list, 
            BCOM.MsdElementType? specifyType = null)
        where T : BCOM.Element
    {
        list = list ?? new List<T>();

        var iter = cell.GetSubElements();
        while (iter.MoveNext())
        {
            var type = iter.Current.Type;
            //if (type == BCOM.MsdElementType.Surface)
            //{
            //    Element el = ElementHelper.getElement(elemRefP, modelRefP);
            //}

            if (iter.Current is T)
            {
                if (specifyType != null && iter.Current.Type == specifyType.Value)
                {
                    list.Add((T)iter.Current);
                }
                else
                {
                    list.Add((T)iter.Current);
                }
            }
            else if (iter.Current is BCOM.ComplexElement)
            {
                collectSubElementsByType(iter.Current.AsComplexElement(), ref list);
            }
        }
    }

    /// <summary>
    /// Поиск коолизий и пересечений
    /// </summary>
    public void scanInfo()
    {
        (TFFormsIntersected ??
            (TFFormsIntersected = new List<TFCOM.TFElementList>())).Clear();
        (CompoundsIntersected ??
            (CompoundsIntersected = new List<TFCOM.TFElementList>())).Clear();

        BCOM.ModelReference activeModel = App.ActiveModelReference;
        scanInfoPerModel(activeModel);

        // TODO удостовериться в корректности алгоритма относительно входящих
        // референсов
        foreach (BCOM.Attachment attachment in activeModel.Attachments)
        {
            if (!attachment.DisplayFlag)
                continue;

            scanInfoPerModel(App.MdlGetModelReferenceFromModelRefP(
                attachment.MdlModelRefP()));
        }
    }

    private void scanInfoPerModel(BCOM.ModelReference model)
    {
        BCOM.CellElement cell = getTaskCell();
        if (cell == null)
            return;

        //BCOM.ModelReference activeModel = App.ActiveModelReference;

        BCOM.ElementScanCriteria criteria = new BCOM.ElementScanCriteriaClass();
        criteria.ExcludeAllTypes();
        criteria.ExcludeNonGraphical();
        criteria.IncludeType(BCOM.MsdElementType.CellHeader);

        BCOM.Range3d scanRange = cell.Range;

#if CONNECT
        // корректировака для версии CONNECT
        if (cell.ModelReference.IsAttachmentOf(model)) // TODO ПРОВЕРИТЬ 
        {
            // здесь есть различия с V8i:
            double k = model.UORsPerStorageUnit / cell.ModelReference.UORsPerStorageUnit;
            scanRange.High = App.Point3dScale(scanRange.High, k);            
            scanRange.Low = App.Point3dScale(scanRange.Low, k);
        }
#endif

        criteria.IncludeOnlyWithinRange(scanRange);

        BCOM.ElementEnumerator res = model.Scan(criteria);

        foreach (BCOM.Element current in res?.BuildArrayFromContents())
        {
            TFCOM.TFElementList tfList = AppTF.CreateTFElement();
            tfList.InitFromElement(current);

            if (tfList.AsTFElement == null)
                continue;

            int tfType = tfList.AsTFElement.GetApplicationType();

            if (tfList.AsTFElement.GetIsCompoundCellType())
            {
                CompoundsIntersected.Add(tfList);
            }
            else if (tfList.AsTFElement.GetIsFormType())
            {
                bool coorrectType = isAvaliableTFFromType(tfType);

                if (isAvaliableTFFromType(tfType))
                {
                    TFFormsIntersected.Add(tfList);
                }
            }
        }

        projectLocationPointOnTFForm(TFFormsIntersected);
    }

    private void projectLocationPointOnTFForm(List<TFCOM.TFElementList> tfForms)
    {
        var projPoints = new SortedDictionary<double, BCOM.Point3d>();

        foreach (var tfform in tfForms)
        {
            projectLocationToTFForm(tfform, ref projPoints);
        }

        var iter = projPoints.GetEnumerator();
        if (iter.MoveNext())
        {
            projectLocationOnForm_ = iter.Current.Value;
        }
    }

    private void projectLocationToTFForm(TFCOM.TFElementList tfForm, 
        ref SortedDictionary<double, BCOM.Point3d> projPoints)
    {
        BCOM.Element element;
        tfForm.AsTFElement.GetElement(out element);
        
        var recipeList = new TFCOM.TFFormRecipeListClass();
        recipeList.InitFromElement(element);
        
        TFCOM.TFBrepList brepList;
        recipeList.GetBrepList(out brepList, false, false, false, "");

        BCOM.Point3d projPoint;
        brepList.FindClosestPoint(out projPoint, Location);
        projPoints.Add(App.Point3dDistance(Location, projPoint), projPoint);
    }

    private bool isAvaliableTFFromType(int tftype)
    {
        if (!Enum.IsDefined(typeof(TFFormTypeEnum), tftype))
            return false;

        switch ((TFFormTypeEnum)tftype)
        {
            case TFFormTypeEnum.TF_FREE_FORM_ELM:
            case TFFormTypeEnum.TF_LINEAR_FORM_ELM:
            case TFFormTypeEnum.TF_SLAB_FORM_ELM:
            case TFFormTypeEnum.TF_SMOOTH_FORM_ELM:
                return true;
        }
        return false;
    }

    public bool getTFFormThickness(out double thickness)
    {
        thickness = 0.0;

        if (TFFormsIntersected == null || TFFormsIntersected.Count == 0)
            return false;

        TFCOM.TFElementList tfList = TFFormsIntersected[0];

        int type = tfList.AsTFElement.GetApplicationType();

        BCOM.Element element;
        tfList.AsTFElement.GetElement(out element);

        TFCOM._TFFormRecipeList list;
        tfList.AsTFElement.GetFormRecipeList(out list);

        if (type == (int)TFFormTypeEnum.TF_SLAB_FORM_ELM)
        {
            TFCOM.TFFormRecipeSlabList slab = (TFCOM.TFFormRecipeSlabList)list;
            slab.AsTFFormRecipeSlab.GetThickness(out thickness);
            return true;
        }
        else if (type == (int)TFFormTypeEnum.TF_LINEAR_FORM_ELM)
        {
            TFCOM.TFFormRecipeLinearList wall = (TFCOM.TFFormRecipeLinearList)list;
            wall.AsTFFormRecipeLinear.GetThickness(out thickness);
            return true;
        }

        return false;
    }


    public override string ToString() => Name ?? base.ToString();

}
}
