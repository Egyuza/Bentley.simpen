using System;
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

using Shared.sp3d;

namespace Shared.Penetrations
{
public class PenetrTask
{
    public const string LEVEL_NAME = "C-EMBP-PNTR";
    public const string LEVEL_SYMB_NAME = "C-EMB-ANNO";
    public const string LEVEL_POINT_NAME = "C-EMB-POINT";

    public enum TaskObjectType
    {
        Pipe,
        PipeOld,
        PipeEquipment,
        Flange
    }
    public IntPtr elemRefP { get; private set; }
    public long elemId { get; private set; }
    public IntPtr modelRefP { get; private set; }

    public string Name
    {
        get
        {
            return string.Format("T{0}-{1}-{2}",
                FlangesType, DiameterType.number, Length);
        }
    }
    public long FlangesType { get; set; }
    public int FlangesCount
    {
        get
        {
            return FlangesType == 1 ? 1 : FlangesType == 2 ? 2 : 0;
        }
    }

    //public int Diametr { get; set; }

    private DiameterType DiameterType
    {
        get { return DiameterType.Parse(DiameterTypeStr); }
    }
    public string DiameterTypeStr { get; set; }

    public int Length { get; set; } // в сантиметрах

    private BCOM.Vector3d singleFlangeSide_ = App.Vector3dZero();
    /// <summary>
    /// расположение фланца, если он один
    /// </summary>
    public BCOM.Vector3d singleFlangeSide
    {
        get { return (singleFlangeSide_ = App.Vector3dNormalize(singleFlangeSide_)); }
        set { singleFlangeSide_ = value; }
    }

    public bool isSingleFlangeFirst()
    {
        return App.Vector3dEqualTolerance(
            singleFlangeSide, App.Vector3dFromXYZ(0, 0, -1), 0.1);
    }

    public string RefPoint1 { get { return Location.ToStringEx();; } }
    public string RefPoint2 { get; private set; }
    public string RefPoint3 { get; private set; }

    //public ulong ElementId {get; private set; }
    public string Code { get; private set; }
    public string Oid { get; private set; }
    public string User { get; private set; }
    public string Path { get; private set; }
    public TaskObjectType TaskType { get; private set; }

    private BCOM.Point3d RawLocation;

    /// <summary> 
    /// корректируется по проекции на объект
    /// </summary>
    public BCOM.Point3d Location { get; private set; }
    public BCOM.Matrix3d Rotation { get; private set; }

    public string ErrorText { get; private set; }

    public bool isValid { get { return string.IsNullOrEmpty(ErrorText); } }

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

    private PenetrTask(Element element, Sp3dTask task)
    {
        long id;
        IntPtr elRef, modelRef;
        ElementHelper.extractFromElement(element, 
            out id, out elRef, out modelRef);
        
        elemRefP = elRef;
        modelRefP = modelRef;
        elemId = id;

        P3Dbase data = task.isEquipment() ?
            (P3Dbase)task.equipment : task.pipe;

        Oid = data.Oid;

        BCOM.ModelReference taskModel =
            App.MdlGetModelReferenceFromModelRefP((int)modelRefP);
        BCOM.Element bcomEl = taskModel.GetElementByID(elemId);

        double task_toUOR = taskModel.UORsPerMasterUnit;
        double task_subPerMaster = taskModel.SubUnitsPerMasterUnit;
        //double task_unit3 = taskModel.UORsPerStorageUnit;
        //double task_unit4 = taskModel.UORsPerSubUnit;

        BCOM.ModelReference activeModel = App.ActiveModelReference;

        double toUOR = activeModel.UORsPerMasterUnit;
        double subPerMaster = activeModel.SubUnitsPerMasterUnit;
        //double unit3 = activeModel.UORsPerStorageUnit;
        //double unit4 = activeModel.UORsPerSubUnit;

        BCOM.Matrix3d rot;
        rot.RowX = App.Point3dFromXYZ(
            data.OrientationMatrix_x0,
            data.OrientationMatrix_x1,
            data.OrientationMatrix_x2);
        rot.RowX = roundExt(rot.RowX, 5, 10, 0);

        rot.RowY = App.Point3dFromXYZ(
            data.OrientationMatrix_y0,
            data.OrientationMatrix_y1,
            data.OrientationMatrix_y2);
        rot.RowY = roundExt(rot.RowY, 5, 10, 0);

        rot.RowZ = App.Point3dFromXYZ(
            data.OrientationMatrix_z0,
            data.OrientationMatrix_z1,
            data.OrientationMatrix_z2);
        rot.RowZ = roundExt(rot.RowZ, 5, 10, 0);

        Rotation = rot;

        Code = data.Name.Trim();
        User = data.SP3D_UserLastModified.Trim();
        Path = data.SP3D_SystemPath;

        BCOM.Point3d pt = App.Point3dFromXYZ(
            data.LocationX, data.LocationY, data.LocationZ);
        RawLocation = App.Point3dScale(pt,
            taskModel.IsAttachment ? task_subPerMaster : 1);
        Location = roundExt(RawLocation);

        //RefPoint1 = Location.ToStringEx();

        this.TaskType = task.isEquipment() ? TaskObjectType.PipeEquipment :
            task.component?.Description == "PenFlange" ? TaskObjectType.Flange :
            task.component?.Description == "PntrtPlate-d" ? TaskObjectType.PipeOld :
            TaskObjectType.Pipe;

        // разбор типоразмера:
        try
        {
            string[] parameters =
                data.Description.TrimStart('T').Split('-');
            FlangesType = int.Parse(parameters[0]);

            DiameterTypeStr = new DiameterType(int.Parse(parameters[1])).ToString();
            Length = int.Parse(parameters[2]);
        }
        catch (Exception)
        {
            ErrorText = string.Format("Не удалось разобрать типоразмер \"{0}\"",
                data.Description);
        }

        FlangesGeom = new List<PenetrTaskFlange>();
        TFFormsIntersected = new List<Bentley.Interop.TFCom.TFElementList>();
        CompoundsIntersected = new List<Bentley.Interop.TFCom.TFElementList>();

        if (TaskType == TaskObjectType.Pipe)
            findFlanges();
        else
            findFlangesOld();

        scanInfo();
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

    public void getCorrectiveAngles(out double aboutX, out double aboutY, out double aboutZ)
    {   
        aboutZ = 0;

        /*        
        Ориентация тела построения solid - вдоль оси Z
            Z              _____  
            ^  Y             |
            | /'             |
            !/___> X       __*__ 
        */
        if (TaskType == PenetrTask.TaskObjectType.PipeEquipment)
        {
            /* Ориентация проходки перед применением матрицы поворота задания
             * должна быть вдоль оси  X
             * 
                Y             |         |
                ^             • ========|
                !___> X       |         |
            */

            aboutX = 0;
            aboutY = Math.PI/2;
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

            aboutX = -Math.PI/2;
            aboutY = 0;
            //rawVector = Addin.App.Point3dFromXYZ(0, 1, 0);
        }
    }

    private void transformToBase(BCOM.Element element, BCOM.Point3d origin)
    {
        double aboutX, aboutY, aboutZ;
        getCorrectiveAngles(out aboutX, out aboutY, out aboutZ);

        // инвертируем трансформацию:
        if (App.Matrix3dHasInverse(Rotation))
        {
            BCOM.Transform3d tran = App.Transform3dFromMatrix3d(Rotation);
            element.Transform(App.Transform3dInverse(tran));
        }       
        element.Rotate(App.Point3dZero(), -aboutX, -aboutY, -aboutZ);
    }


    private void findFlangesOld()
    {
        BCOM.ModelReference model =
            App.MdlGetModelReferenceFromModelRefP((int)modelRefP);

        singleFlangeSide = App.Vector3dZero();

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

        transformToBase(cell, Location);

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
            singleFlangeSide = App.Vector3dSubtractPoint3dPoint3d(
                ElementHelper.getCenter(cones[2]),
                ElementHelper.getCenter(cones[0]));
        }
        else
        {
            // базовая ориентация при построении
            // фланец совпадает с Точкой установки
            singleFlangeSide = App.Vector3dFromXYZ(0, 0, -1);
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

        transformToBase(cell, Location);

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

            transformToBase(flangeEl, Location);

            singleFlangeSide = App.Vector3dSubtractPoint3dPoint3d(
                ElementHelper.getCenter(flangeEl),
                ElementHelper.getCenter(cell));
        }
        else
        {
            singleFlangeSide = App.Vector3dFromXYZ(0, 0, -1);
        }
    }

    // todo
    private void collectSubElementsByType<T>(BCOM.ComplexElement cell, ref List<T> list)
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
                list.Add((T)iter.Current);
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
        scanInfo(activeModel);

        foreach (BCOM.Attachment attachment in activeModel.Attachments)
        {
            if (!attachment.DisplayFlag)
                continue;

            scanInfo(App.MdlGetModelReferenceFromModelRefP(
                attachment.MdlModelRefP()));
        }

        //BCOM.ModelReference refModel = 
        //    App.MdlGetModelReferenceFromModelRefP((int)modelRefP);

        //var models = new List<BCOM.ModelReference>();

        //BCOM.CellElement cell = refModel.GetElementByID(elemId).AsCellElement();
        //if (cell != null)
        //{
        //    // Поиск фланцев
        //    BCOM.ElementScanCriteria criteria = new BCOM.ElementScanCriteriaClass();
        //    BCOM.Range3d scanRange = 
        //        App.Range3dScaleAboutCenter(cell.Range, 1.0);

        //    criteria.IncludeOnlyVisible();
        //    criteria.IncludeOnlyWithinRange(scanRange);

        //    BCOM.ElementEnumerator res = activeModel.Scan(criteria);
        //    while((res?.MoveNext()).Value)
        //    {
        //        // todo использовать только один тип точек
        //        TFCOM.TFElementList tfEl = AppTF.CreateTFElement();
        //        tfEl.InitFromElement(res.Current);

        //        if (tfEl.AsTFElement != null && tfEl.AsTFElement.GetIsCompoundCellType())
        //        {
        //            isCompoundExistsInPlace = true;
        //        }
        //    }
        //}
    }

    private void scanInfo(BCOM.ModelReference model)
    {
        BCOM.CellElement cell = getTaskCell();
        if (cell == null)
            return;

        BCOM.ModelReference activeModel = App.ActiveModelReference;

        BCOM.ElementScanCriteria criteria = new BCOM.ElementScanCriteriaClass();
        criteria.ExcludeAllTypes();
        criteria.ExcludeNonGraphical();
        criteria.IncludeType(BCOM.MsdElementType.CellHeader);

        BCOM.Range3d scanRange = cell.Range;
        if (model.IsAttachment)
        {
            double k = model.UORsPerStorageUnit / activeModel.UORsPerStorageUnit;
            scanRange.High = App.Point3dScale(scanRange.High, k);
            scanRange.Low = App.Point3dScale(scanRange.Low, k);
        }
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

        projectLocationToTFForm(TFFormsIntersected);
    }

    private void projectLocationToTFForm(List<TFCOM.TFElementList> tfForms)
    {
        var projPoints = new SortedDictionary<double, BCOM.Point3d>();

        foreach (var tfform in tfForms)
        {
            projectLocationToTFForm(tfform, ref projPoints);
        }

        var iter = projPoints.GetEnumerator();
        if (iter.MoveNext())
        {
            ProjectPoint = iter.Current.Value;
            Location = iter.Current.Value;
        }       
    }

    public BCOM.Point3d ProjectPoint { get; private set; }

    private void projectLocationToTFForm(TFCOM.TFElementList tfForm, 
        ref SortedDictionary<double, BCOM.Point3d> projPoints)
    {
        BCOM.Element element;
        tfForm.AsTFElement.GetElement(out element);
        
        TFCOM.TFFormRecipeListClass recipeList = new TFCOM.TFFormRecipeListClass();
        recipeList.InitFromElement(element);
        
        TFCOM.TFBrepList brepList;
        recipeList.GetBrepList(out brepList, false, false, false, "");

        BCOM.Point3d projPoint;
        brepList.FindClosestPoint(out projPoint, Location);
        projPoints.Add(App.Point3dDistance(Location, projPoint), projPoint);
    }


    //    /* the different types of top definitions of a form */
    // #define TF_TOP_SHAPES                        1
    // #define TF_TOP_PLANE                         2
    // #define TF_TOP_FIXED_HEIGHT                  3
    // /* the different types of TriForma elements */
    // #define TF_NO_TF_ELM                         0
    // #define TF_OLD_TYPE_EXTRUSION_FORM         999
    // #define TF_FREE_FORM_ELM                    31  /* a TF free form */
    // #define TF_LINEAR_FORM_ELM                  32  /* a TF linear form */
    // #define TF_LINEAR_ELM                       33  /* a MicroStation line, linestring, arc, curve or complex linestring */
    // #define TF_SHAPE_ELM                        34  /* a MicroStation shape, ellipse or complex shape */
    // #define TF_CELL_ELM                         35  /* a MicroStation cell or shared cell instance with a TF partname */
    // #define TF_COMPOUND_CELL_ELM                36  /* a TF compound cell */
    // #define TF_ROOM_SHAPE_ELM                   37  /* a TF shape belonging to a room */
    // #define TF_ROOM_ELM                         38  /* a TF room element */
    // #define TF_LINE_STRING_FORM_ELM             39  /* a TF linestring form */
    // #define TF_SURFACE_ELM                      40  /* a MicroStation surface element, bspline surface, cone or solid element */
    // #define TF_MS_CELL_ELM                      41  /* a MicroStation cell or shared cell instance without a TF partname */
    // #define TF_SLAB_FORM_ELM                    42  /* a TF slab form */
    // #define TF_BLOB_FORM_ELM                    43  /* a TF blob form */
    // #define TF_ARC_FORM_ELM                     44  /* a TF Arc form */
    // #define TF_SMOOTH_FORM_ELM                  45  /* a TF Smooth Free Form form */
    // #define TF_PATH_FORM_ELM                    46  /* a TF path form */
    // #define TF_MORPH_FORM_ELM                   47  /* a TF Morph form */
    // #define TF_COMPOUND_FORM_ELM                48  /* a TF compound form */
    // #define TF_PART_TAG_ELM                     50  /* a MS tag with part info attached */
    // 
    // #define TF_CC_INT_DOOR_ELM                  51  /* a TF internal door (compound cell) */
    // #define TF_CC_EXT_DOOR_ELM                  52  /* a TF external door (compound cell) */
    // #define TF_CC_INT_WINDOW_ELM                53  /* a TF internal window (compound cell) */
    // #define TF_CC_EXT_WINDOW_ELM                54  /* a TF external window (compound cell) */
    // #define TF_CC_INT_DOOR_AND_WINDOW_ELM       55  /* a TF internal door and window (compound cell) */
    // #define TF_CC_EXT_DOOR_AND_WINDOW_ELM       56  /* a TF external door and window (compound cell) */
    // #define TF_CC_STAIR_ELM                     57  /* a TF stair (compound cell) */
    // #define TF_CC_GRID_ELM                      58  /* a TF grid (compound cell) */
    // #define TF_ADFCELL_ELM                      59  /* a persistent triforma ADF model (subtypes: TF_PAZCELL_ELM, TF_RFACELL_ELM) */
    // 
    // #define TF_EBREP_ELM                        60    /* embedded breps */
    // #define TF_STRUCTURAL_ELM                   61  /* a TF structural (based on free form) */
    // #define TF_MESH_ELM                         62  /* a TF Mesh Element */
    // #define TF_STRUCTSMOOTH_ELM                 63  /* a TF structural (based on smooth form) */
    // #define TF_STRUCTPATH_ELM                   64  /* a TF structural (based on path form) */
    // #define TF_STRUCTTAPER_ELM                  65
    // #define TF_CEILING_ELM                      66  /* a TF ceiling */
    // #define TF_MECHANICAL_ELM                   67  /* a TF mechanical */
    // #define TF_FEATURE_SOLID_ELM                68  /* a MicroStation Feature Solid */
    // #define TF_STAIR_ELM                        69  /* a TriForma Stair Cell */
    // #define TF_FLIGHT_ELM                       70  /* a TriForma Stair Flight Cell */
    // #define TF_TREAD_ELM                        71  /* a TriForma Stair Tread Cell */
    // #define TF_RISER_ELM                        72  /* a TriForma Stair Riser Cell */
    // #define TF_LANDING_ELM                      73  /* a TriForma Stair Landing Cell */
    // #define TF_STRINGER_ELM                     74  /* a TriForma Stair Stringer Cell */
    // #define TF_STAIRANNOTATION_ELM              75  /* a TriForma Stair Annotation Cell */
    // #define TF_SPACE_ELM                        76  /* a Space */
    // #define TF_RAILING_ELM                      77  /* a Railing */
    // #define TF_HORIZONTALRAIL_ELM               78  /* a GuardRail (member of Railing)*/
    // #define TF_POST_ELM                         79  /* a Post (member of Railing) */
    // #define TF_BALUSTER_ELM                     80  /* a Baluster (member of Railing) */
    // #define TF_SHARED_COMPOUND_CELL_ELM         81  // SharedFrameHandler
    // #define TF_SHARED_ADFCELL_ELM               82  // SharedAdfCellHandler
    // #define TF_ROOF_ELM                         83  /* a TriForma Roof Cell */
    // #define TF_RAILINGENDS_ELM                  84  /* a Baluster (member of Railing) */
    // #define TF_GRID_SYSTEM_ELM                  85  // TFColumnGridHandler
    // #define TF_RFACELL_ELM                      86  // PA-Cell with label subtype RFACELL_ELEMENT
    // #define TF_PAZCELL_ELM                      87  // traditional PA-Cell
    // 
    // #define NUM_QUANTIFIED_TYPES                88  /* last # type of TriForma element + 1 */

    enum TFFormTypeEnum
    {
        TF_FREE_FORM_ELM = 31,
        TF_LINEAR_FORM_ELM = 32,
        TF_SLAB_FORM_ELM = 42,
        TF_SMOOTH_FORM_ELM = 45
    }

    private bool isAvaliableTFFromType(int tftype)
    {
        if (!Enum.IsDefined(typeof(TFFormTypeEnum), tftype))
            return false;

        switch ((TFFormTypeEnum)tftype)
        {
            case TFFormTypeEnum.TF_FREE_FORM_ELM: // TF_FREE_FORM_ELM
            case TFFormTypeEnum.TF_LINEAR_FORM_ELM: // TF_LINEAR_FORM_ELM
            case TFFormTypeEnum.TF_SLAB_FORM_ELM: // TF_SLAB_FORM_ELM
            case TFFormTypeEnum.TF_SMOOTH_FORM_ELM: // TF_SMOOTH_FORM_ELM
                return true;
        }
        return false;
    }

    public static bool getFromElement(Element element, out PenetrTask penTask)
    {
        Sp3dTask task = null;
        penTask = null;

        if (!ElementHelper.isElementSp3dTask(element, out task) ||
            !(task.isPipe() || task.isPipeOld() || task.isEquipment()))
        {
            return false;
        }

        penTask = new PenetrTask(element, task);
        return true;
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

    /// <summary>
    /// Функция кратного округления из оригинального simpen Л.Вибе
    /// </summary>
    long roundExt(double val, int digs = -1, double snap = 5, int shft = 0)
    {
        double dv;
        dv = val * Math.Pow(snap, digs);
        dv = Math.Floor(dv + 0.55555555555555 - (0.111111111111111 * shft));
        dv = dv / Math.Pow(snap, digs);
        return Convert.ToInt64(dv);
    }

    BCOM.Point3d roundExt(BCOM.Point3d pt, int digs = -1, double snap = 5, int shft = 0)
    {
        BCOM.Point3d res;
        res.X = roundExt(pt.X, digs, snap, shft);
        res.Y = roundExt(pt.Y, digs, snap, shft);
        res.Z = roundExt(pt.Z, digs, snap, shft);
        return res;
    }

    public override string ToString()
    {
        return Name;
    }

    private static BCOM.Application App
    {
        get { return BMI.Utilities.ComApp; }
    }

    private static TFCOM.TFApplication _tfApp;
    public static TFCOM.TFApplication AppTF
    {
        get
        {
            return _tfApp ?? 
                (_tfApp = new TFCOM.TFApplicationList().TFApplication);
        }
    }
}
}
