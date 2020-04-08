//using System;
//using System.Collections.Generic;

//using Bentley.DgnPlatformNET.Elements;
//using BCOM = Bentley.Interop.MicroStationDGN;
//using TFCOM = Bentley.Interop.TFCom;
//using Bentley.MstnPlatformNET;

//using Shared;
//using Shared.sp3d;

//namespace simpen_cn
//{

//class DiameterType : IComparable<DiameterType>
//{
//    public long number {get; set;}
//    private  float diameter;
//    private  float thickness;
    
//    //public string NumberDisplay {get; set;}
//    //{
//    //    get { return this.ToString(); }
//    //}

//    public string typeSize
//    {
//        get 
//        { 
//            return string.Format("{0}x{1}", diameter , thickness);
//        }
//    }

//    public DiameterType(long number)
//    {
//        this.number = number;
//        diameter = 
//        thickness = 0;
//        //typeSize = string.Format("{0}x{1}", diameter , thickness);
//        //NumberDisplay = this.ToString();
//    }

//    public DiameterType(long number, float diameter, float thickness)
//    {
//        this.number = number;
//        this.diameter = diameter;
//        this.thickness = thickness;
//        //NumberDisplay = this.ToString();
//        //typeSize = string.Format("{0}x{1}", diameter , thickness);

//    }

//    public int CompareTo(DiameterType other)
//    {
//        return number.CompareTo(other.number);
//    }
    
//    public override bool Equals(object obj)
//    {
//        if ((obj as DiameterType) != null)
//            return this.CompareTo((DiameterType)obj) == 0;
//        return base.Equals(obj);
//    }

//    public override int GetHashCode()
//    {
//        return (int)number;
//    }

//    public override string ToString()
//    {
//        return string.Format("{0} ({1})", number, typeSize);
//    }

//    public static DiameterType Parse(string text)
//    {
//        long number = long.Parse(text.Split(' ')[0]);
//        text = text.Split(' ')[1].TrimStart('(').TrimEnd(')');

//        float diameter = float.Parse(text.Split('x')[0]);
//        float thickness = float.Parse(text.Split('x')[1]);

//        return new DiameterType(number, diameter, thickness);
//    }
//}

//class PenetrTask
//{
//    public const string LEVEL_NAME = "C-EMBP-PNTR";
//    public const string LEVEL_SYMB_NAME = "C-EMB-ANNO";
//    public const string LEVEL_POINT_NAME = "C-EMB-POINT";

//    public enum TaskObjectType
//    {
//        Pipe,
//        PipeOld,
//        Flange
//    }
//    public IntPtr elemRefP { get; private set; }
//    public long elemId { get; private set; }
//    public IntPtr modelRefP { get; private set; }

//    public string Name
//    {
//        get 
//        { 
//            return string.Format("T{0}-{1}-{2}",
//                FlangesType, DiameterType.number, Length); 
//        }
//    }
//    public long FlangesType { get; set; }
//    public int FlangesCount
//    {
//        get
//        {
//            return FlangesType == 1 ? 1 : FlangesType == 2 ? 2 : 0;
//        }
//    }

//    //public int Diametr { get; set; }
   
//    private DiameterType DiameterType
//    {
//        get { return DiameterType.Parse(DiameterTypeStr); }
//    }
//    public string DiameterTypeStr { get; set; }
        
//    public int Length { get; set; } // в сантиметрах

//    /// <summary>
//    /// расположение фланца, если он один
//    /// </summary>
//    public BCOM.Vector3d singleFlangeSide { get; private set; } = 
//        Addin.App.Vector3dZero();

//    public string RefPoint1 { get; private set; }
//    public string RefPoint2 { get; private set; }
//    public string RefPoint3 { get; private set; }

//    //public ulong ElementId {get; private set; }
//    public string Code { get; private set; }
//    public string Oid { get; private set; }
//    public string User { get; private set; }
//    public string Path { get; private set; }
//    public TaskObjectType TaskType { get; private set; }

//    private BCOM.Point3d RawLocation;
//    public BCOM.Point3d Location { get; private set; }
//    public BCOM.Matrix3d Rotation { get; private set; }

//    public string ErrorText { get; private set; }

//    public bool isValid { get { return string.IsNullOrEmpty(ErrorText); } }

//    public List<string> Warnings { get; private set; } = new List<string>();

//    /// <summary>
//    /// В диапазоне задания найден элемент CompoundCell
//    /// </summary>
//    public bool isCompoundExistsInPlace
//    {
//        get { return CompoundsIntersected?.Count > 0; }
//    }

//    /// <summary>
//    /// Геометрические фланцы в объекте задания
//    /// </summary>
//    public List<PenetrTaskFlange> FlangesGeom { get; private set; }
//    /// <summary>
//    /// TF-объекты, которые пересекает объект задания
//    /// </summary>
//    public List<TFCOM.TFElementList> TFFormsIntersected { get; private set; }
//    public List<TFCOM.TFElementList> CompoundsIntersected { get; private set; }

//    private PenetrTask(Element element, Sp3dTask task)
//    {
//        elemRefP = element.GetNativeElementRef();
//        modelRefP = element.GetNativeDgnModelRef();
//        elemId = element.ElementId;
//        Oid = task.pipe.Oid;

//        BCOM.ModelReference taskModel = 
//            Addin.App.MdlGetModelReferenceFromModelRefP((long)modelRefP);
//        BCOM.Element bcomEl = taskModel.GetElementByID(elemId);

//        double task_toUOR = taskModel.UORsPerMasterUnit;
//        double task_subPerMaster = taskModel.SubUnitsPerMasterUnit;
//        //double task_unit3 = taskModel.UORsPerStorageUnit;
//        //double task_unit4 = taskModel.UORsPerSubUnit;

//        BCOM.ModelReference activeModel = Addin.App.ActiveModelReference;               

//        double toUOR = activeModel.UORsPerMasterUnit;
//        double subPerMaster = activeModel.SubUnitsPerMasterUnit;
//        //double unit3 = activeModel.UORsPerStorageUnit;
//        //double unit4 = activeModel.UORsPerSubUnit;
        

//        BCOM.Matrix3d rot;
//        rot.RowX = Addin.App.Point3dFromXYZ(
//            task.pipe.OrientationMatrix_x0, 
//            task.pipe.OrientationMatrix_x1,
//            task.pipe.OrientationMatrix_x2);
//        rot.RowX = roundExt(rot.RowX, 5, 10, 0);

//        rot.RowY = Addin.App.Point3dFromXYZ(
//            task.pipe.OrientationMatrix_y0, 
//            task.pipe.OrientationMatrix_y1,
//            task.pipe.OrientationMatrix_y2);
//        rot.RowY = roundExt(rot.RowY, 5, 10, 0);

//        rot.RowZ = Addin.App.Point3dFromXYZ(
//            task.pipe.OrientationMatrix_z0, 
//            task.pipe.OrientationMatrix_z1,
//            task.pipe.OrientationMatrix_z2);
//        rot.RowZ = roundExt(rot.RowZ, 5, 10, 0);

//        Rotation = rot;

//        if (task.pipe == null || task.component == null)
//        {
//            ErrorText = "Не удалось прочитать данные задания";
//            return;
//        }

//        Code = task.pipe.Name.Trim();
//        User = task.pipe.SP3D_UserLastModified.Trim();
//        Path = task.pipe.SP3D_SystemPath;

//        BCOM.Point3d pt = Addin.App.Point3dFromXYZ(
//            task.pipe.LocationX, task.pipe.LocationY, task.pipe.LocationZ);
//        RawLocation = Addin.App.Point3dScale(pt, 
//            element.DgnModel.IsDgnAttachmentOf(Session.Instance.GetActiveDgnModel())? 
//            task_subPerMaster : 1);

//        Location = roundExt(RawLocation);

//        RefPoint1 = Location.ToStringEx();

//        this.TaskType = task.component.Description == "PenFlange" ? 
//            TaskObjectType.Flange : task.component.Description == "PntrtPlate-d" ?
//            TaskObjectType.PipeOld : TaskObjectType.Pipe;

//        // разбор типоразмера:
//        try
//        {
//            string[] parameters = 
//                task.pipe.Description.TrimStart('T').Split('-');
//            FlangesType = int.Parse(parameters[0]);

//            DiameterTypeStr = new DiameterType(int.Parse(parameters[1])).ToString();
//            Length = int.Parse(parameters[2]);
//        }
//        catch (Exception)
//        {
//            ErrorText = string.Format("Не удалось разобрать типоразмер \"{0}\"",
//                task.pipe.Description);
//        }
        
//        FlangesGeom = new List<PenetrTaskFlange>();
//        TFFormsIntersected = new List<Bentley.Interop.TFCom.TFElementList>();
//        CompoundsIntersected = new List<Bentley.Interop.TFCom.TFElementList>();
        
//        if (TaskType == TaskObjectType.Pipe)
//            findFlanges();
//        else
//            findFlangesOld();

//        scanInfo();
//    }

//    private BCOM.CellElement getTaskCell()
//    {
//        BCOM.ModelReference taskModel = // TODO 
//            Addin.App.MdlGetModelReferenceFromModelRefP((long)modelRefP);

//        BCOM.ModelReference activeModel = Addin.App.ActiveModelReference;
        
//        bool isAttachment = taskModel.IsAttachment;

//        BCOM.CellElement cell = taskModel.IsAttachment ? 
//            taskModel.AsAttachment.GetElementByID(elemId).AsCellElement() :
//            taskModel.GetElementByID(elemId).AsCellElement();

//        return cell;
//    }

//    private void findFlangesOld()
//    {
//        BCOM.ModelReference model = 
//            Addin.App.MdlGetModelReferenceFromModelRefP((long)modelRefP);

//        singleFlangeSide = Addin.App.Vector3dZero();

//        // Todo из правильной модели
//        BCOM.CellElement cell = model.GetElementByID(elemId).AsCellElement();
//        if (cell == null)
//            return;

//        var tran = Addin.App.Transform3dInverse(
//            Addin.App.Transform3dFromMatrix3d(Rotation));        
//        cell.Transform(tran);
        
//        var cones = new List<BCOM.ConeElement>();
//        collectSubElementsByType(cell, ref cones);

//        //var iter = bcomEl.AsCellElement().GetSubElements();
//        //while (iter.MoveNext())
//        //{
//        //    var iterSub = iter.Current.AsCellElement().GetSubElements();
//        //    while (iterSub.MoveNext())
//        //    {
//        //        if (iterSub.Current.IsConeElement())                
//        //            cones.Add(iterSub.Current.AsConeElement());
//        //    }
//        //}

//        cones.Sort((BCOM.ConeElement lhs, BCOM.ConeElement rhs) =>
//            {  
//                return -1 * ElementHelper.getHeight(lhs).CompareTo(
//                    ElementHelper.getHeight(rhs));
//            });

//        /* cones.Count ==
//         * 2: без фланцев
//         * 4: 1 фланец, определяем вектор с какой стороны фланец
//         * 6: 2 фланца         
//        */

//        int geomFlangesCount = cones.Count/2 - 1;

//        if (geomFlangesCount == 1) 
//        { // 1 фланец, определяем вектор с какой стороны фланец
//            singleFlangeSide = Addin.App.Vector3dSubtractPoint3dPoint3d(
//                ElementHelper.getCenter(cones[0]), 
//                ElementHelper.getCenter(cones[2]));
//        }
//        else
//        {
//            // базовая ориентация при построении
//            // фланец совпадает с Точкой установки
//            singleFlangeSide = Addin.App.Vector3dFromXY(0, 1);
//        }
//        singleFlangeSide = Addin.App.Vector3dNormalize(singleFlangeSide);

//        if (FlangesCount != geomFlangesCount) 
//        {
//            Warnings.Add(string.Format("Несоответствие количества фланцев, " + 
//                "указанных в атрибутах - {0} и заданных геометрически - {1}", 
//                FlangesCount, geomFlangesCount));
//        }
//    }

//    private void findFlanges()
//    {
//        BCOM.CellElement cell = getTaskCell();        
//        if (cell == null)
//            return;

//        BCOM.ModelReference taskModel = // TODO 
//            Addin.App.MdlGetModelReferenceFromModelRefP((long)modelRefP);
//        BCOM.ModelReference activeModel = Addin.App.ActiveModelReference;
        
//        FlangesGeom.Clear();

//        // Поиск фланцев
//        BCOM.ElementScanCriteria criteria = new BCOM.ElementScanCriteriaClass();
//        //criteria.IncludeOnlyVisible();
//        criteria.ExcludeAllTypes();
//        criteria.ExcludeNonGraphical();
//        criteria.IncludeType(BCOM.MsdElementType.CellHeader);

//        BCOM.Range3d scanRange = cell.Range;
//            // Addin.App.Range3dScaleAboutCenter(cell.Range, 1.01);
       
//        if (taskModel.IsAttachment)
//        {
//            double k = taskModel.UORsPerStorageUnit / activeModel.UORsPerStorageUnit;
//            scanRange.High = Addin.App.Point3dScale(scanRange.High, k);
//            scanRange.Low = Addin.App.Point3dScale(scanRange.Low, k);
//        }

//        criteria.IncludeOnlyWithinRange(scanRange);       
       
//        BCOM.ElementEnumerator res =  taskModel.Scan(criteria);
        
//        foreach (BCOM.Element current in res?.BuildArrayFromContents())
//        {
//            // todo использовать только один тип точек
//            PenetrTaskFlange flangeTask = null;
//            //Element curEl = Element.FromElementID((ulong)current.ID, 
//            //    (IntPtr)current.ModelReference.MdlModelRefP());

//            if (PenetrTaskFlange.getFromElement(current, out flangeTask))
//            {
//                BCOM.Range3d range = current.Range;

//                if (flangeTask.Oid.Equals(Oid))
//                    FlangesGeom.Add(flangeTask);
//            }
//        }
        
//        BCOM.Transform3d tran = Addin.App.Transform3dInverse(
//            Addin.App.Transform3dFromMatrix3d(Rotation));

//        cell.Transform(tran);

//        if (FlangesCount != FlangesGeom.Count)
//        {
//            Warnings.Add(string.Format("Несоответствие количества фланцев, " + 
//                "указанных в атрибутах - {0} и заданных геометрически - {1}", 
//                FlangesCount, FlangesGeom.Count));
//        }

//        if (FlangesCount == 1 && FlangesGeom.Count > 0)
//        {
//            BCOM.Element flangeEl = taskModel.GetElementByID(FlangesGeom[0].elemId);
            
//            flangeEl.Transform(tran);

//            singleFlangeSide = Addin.App.Vector3dSubtractPoint3dPoint3d(
//                ElementHelper.getCenter(cell), 
//                ElementHelper.getCenter(flangeEl));
            
//        }
//        else
//        {
//            singleFlangeSide = Addin.App.Vector3dFromXY(0, 1);
//        }
//        singleFlangeSide = Addin.App.Vector3dNormalize(singleFlangeSide);
//    }

//    // todo
//    private void collectSubElementsByType<T>(BCOM.ComplexElement cell, ref List<T> list) 
//        where T : BCOM.Element
//    {
//        list = list ?? new List<T>();
        
//        var iter = cell.GetSubElements();
//        while (iter.MoveNext())
//        {
//            var type = iter.Current.Type;

//            if (type == BCOM.MsdElementType.Surface) {
//            }

//            if (type == BCOM.MsdElementType.Surface) {
//                type = type;                               

//                //ComplexElement surface = (ComplexElement)Element.FromElementID(
//                //    (ulong)iter.Current.ID, modelRefP);

//                Element el = Element.GetFromElementRefAndModelRef(elemRefP, modelRefP);
//                    // Element.FromElementID((ulong)iter.Current.ID, modelRefP);      
//            }
            
//            if (iter.Current is T)
//            {
//                list.Add((T)iter.Current);
//            }
//            else if (iter.Current is BCOM.ComplexElement)
//            {
//                collectSubElementsByType(iter.Current.AsComplexElement(), ref list);
//            }
//        }
//    }

//    /// <summary>
//    /// Поиск коолизий и пересечений
//    /// </summary>
//    public void scanInfo()
//    { 
//        (TFFormsIntersected ?? 
//            (TFFormsIntersected = new List<TFCOM.TFElementList>())).Clear();
//        (CompoundsIntersected ?? 
//            (CompoundsIntersected = new List<TFCOM.TFElementList>())).Clear();

//        BCOM.ModelReference activeModel = Addin.App.ActiveModelReference;        
//        scanInfo(activeModel);

//        foreach (BCOM.Attachment attachment in activeModel.Attachments)
//        {
//            if (!attachment.DisplayFlag)
//                continue;

//            long mref = attachment.MdlModelRefP();
            
//            scanInfo(Addin.App.MdlGetModelReferenceFromModelRefP(
//                attachment.MdlModelRefP()));
//        }

//        //BCOM.ModelReference refModel = 
//        //    Addin.App.MdlGetModelReferenceFromModelRefP((long)modelRefP);
        
//        //var models = new List<BCOM.ModelReference>();
        
//        //BCOM.CellElement cell = refModel.GetElementByID(elemId).AsCellElement();
//        //if (cell != null)
//        //{
//        //    // Поиск фланцев
//        //    BCOM.ElementScanCriteria criteria = new BCOM.ElementScanCriteriaClass();
//        //    BCOM.Range3d scanRange = 
//        //        Addin.App.Range3dScaleAboutCenter(cell.Range, 1.0);
            
//        //    criteria.IncludeOnlyVisible();
//        //    criteria.IncludeOnlyWithinRange(scanRange);

//        //    BCOM.ElementEnumerator res = activeModel.Scan(criteria);
//        //    while((res?.MoveNext()).Value)
//        //    {
//        //        // todo использовать только один тип точек
//        //        TFCOM.TFElementList tfEl = Addin.AppTF.CreateTFElement();
//        //        tfEl.InitFromElement(res.Current);

//        //        if (tfEl.AsTFElement != null && tfEl.AsTFElement.GetIsCompoundCellType())
//        //        {
//        //            isCompoundExistsInPlace = true;
//        //        }
//        //    }
//        //}
//    }

//    private void scanInfo(BCOM.ModelReference model)
//    {
//        BCOM.CellElement cell = getTaskCell();        
//        if (cell == null)
//            return;
       
//        BCOM.ModelReference activeModel = Addin.App.ActiveModelReference;
        
//        BCOM.ElementScanCriteria criteria = new BCOM.ElementScanCriteriaClass();
//        criteria.ExcludeAllTypes();
//        criteria.ExcludeNonGraphical();
//        criteria.IncludeType(BCOM.MsdElementType.CellHeader);

//        BCOM.Range3d scanRange = cell.Range;
//        if (!model.IsAttachment) // НЕ ТРЕБУЕТСЯ ДЛЯ CONNECT
//        {
//            // здесь есть различия с V8i:
//            double k = model.UORsPerStorageUnit / activeModel.UORsPerStorageUnit;
//            scanRange.High = Addin.App.Point3dScale(scanRange.High, 
//                activeModel.UORsPerStorageUnit/cell.ModelReference.UORsPerStorageUnit);
//            scanRange.Low = Addin.App.Point3dScale(scanRange.Low, 
//                activeModel.UORsPerStorageUnit/cell.ModelReference.UORsPerStorageUnit);
//        }
//            criteria.IncludeOnlyWithinRange(scanRange);

//        BCOM.ElementEnumerator res =  model.Scan(criteria);
        
//        foreach (BCOM.Element current in res?.BuildArrayFromContents())
//        {
//            TFCOM.TFElementList tfList = Addin.AppTF.CreateTFElement();
//            tfList.InitFromElement(current);

//            if (tfList.AsTFElement == null)
//                continue;

//            int tfType = tfList.AsTFElement.GetApplicationType();

//            if (tfList.AsTFElement.GetIsCompoundCellType())
//            {
//                CompoundsIntersected.Add(tfList);
//            }
//            else if (tfList.AsTFElement.GetIsFormType())
//            {
//                //int tfType = tfEl.AsTFElement.GetApplicationType();
//                bool coorrectType = isAvaliableTFFromType(tfType);

//                if (isAvaliableTFFromType(tfType))
//                {
//                    TFFormsIntersected.Add(tfList);
//                }                
//            }
//        }
//    }

////    /* the different types of top definitions of a form */
//// #define TF_TOP_SHAPES                        1
//// #define TF_TOP_PLANE                         2
//// #define TF_TOP_FIXED_HEIGHT                  3
//// /* the different types of TriForma elements */
//// #define TF_NO_TF_ELM                         0
//// #define TF_OLD_TYPE_EXTRUSION_FORM         999
//// #define TF_FREE_FORM_ELM                    31  /* a TF free form */
//// #define TF_LINEAR_FORM_ELM                  32  /* a TF linear form */
//// #define TF_LINEAR_ELM                       33  /* a MicroStation line, linestring, arc, curve or complex linestring */
//// #define TF_SHAPE_ELM                        34  /* a MicroStation shape, ellipse or complex shape */
//// #define TF_CELL_ELM                         35  /* a MicroStation cell or shared cell instance with a TF partname */
//// #define TF_COMPOUND_CELL_ELM                36  /* a TF compound cell */
//// #define TF_ROOM_SHAPE_ELM                   37  /* a TF shape belonging to a room */
//// #define TF_ROOM_ELM                         38  /* a TF room element */
//// #define TF_LINE_STRING_FORM_ELM             39  /* a TF linestring form */
//// #define TF_SURFACE_ELM                      40  /* a MicroStation surface element, bspline surface, cone or solid element */
//// #define TF_MS_CELL_ELM                      41  /* a MicroStation cell or shared cell instance without a TF partname */
//// #define TF_SLAB_FORM_ELM                    42  /* a TF slab form */
//// #define TF_BLOB_FORM_ELM                    43  /* a TF blob form */
//// #define TF_ARC_FORM_ELM                     44  /* a TF Arc form */
//// #define TF_SMOOTH_FORM_ELM                  45  /* a TF Smooth Free Form form */
//// #define TF_PATH_FORM_ELM                    46  /* a TF path form */
//// #define TF_MORPH_FORM_ELM                   47  /* a TF Morph form */
//// #define TF_COMPOUND_FORM_ELM                48  /* a TF compound form */
//// #define TF_PART_TAG_ELM                     50  /* a MS tag with part info attached */
//// 
//// #define TF_CC_INT_DOOR_ELM                  51  /* a TF internal door (compound cell) */
//// #define TF_CC_EXT_DOOR_ELM                  52  /* a TF external door (compound cell) */
//// #define TF_CC_INT_WINDOW_ELM                53  /* a TF internal window (compound cell) */
//// #define TF_CC_EXT_WINDOW_ELM                54  /* a TF external window (compound cell) */
//// #define TF_CC_INT_DOOR_AND_WINDOW_ELM       55  /* a TF internal door and window (compound cell) */
//// #define TF_CC_EXT_DOOR_AND_WINDOW_ELM       56  /* a TF external door and window (compound cell) */
//// #define TF_CC_STAIR_ELM                     57  /* a TF stair (compound cell) */
//// #define TF_CC_GRID_ELM                      58  /* a TF grid (compound cell) */
//// #define TF_ADFCELL_ELM                      59  /* a persistent triforma ADF model (subtypes: TF_PAZCELL_ELM, TF_RFACELL_ELM) */
//// 
//// #define TF_EBREP_ELM                        60    /* embedded breps */
//// #define TF_STRUCTURAL_ELM                   61  /* a TF structural (based on free form) */
//// #define TF_MESH_ELM                         62  /* a TF Mesh Element */
//// #define TF_STRUCTSMOOTH_ELM                 63  /* a TF structural (based on smooth form) */
//// #define TF_STRUCTPATH_ELM                   64  /* a TF structural (based on path form) */
//// #define TF_STRUCTTAPER_ELM                  65
//// #define TF_CEILING_ELM                      66  /* a TF ceiling */
//// #define TF_MECHANICAL_ELM                   67  /* a TF mechanical */
//// #define TF_FEATURE_SOLID_ELM                68  /* a MicroStation Feature Solid */
//// #define TF_STAIR_ELM                        69  /* a TriForma Stair Cell */
//// #define TF_FLIGHT_ELM                       70  /* a TriForma Stair Flight Cell */
//// #define TF_TREAD_ELM                        71  /* a TriForma Stair Tread Cell */
//// #define TF_RISER_ELM                        72  /* a TriForma Stair Riser Cell */
//// #define TF_LANDING_ELM                      73  /* a TriForma Stair Landing Cell */
//// #define TF_STRINGER_ELM                     74  /* a TriForma Stair Stringer Cell */
//// #define TF_STAIRANNOTATION_ELM              75  /* a TriForma Stair Annotation Cell */
//// #define TF_SPACE_ELM                        76  /* a Space */
//// #define TF_RAILING_ELM                      77  /* a Railing */
//// #define TF_HORIZONTALRAIL_ELM               78  /* a GuardRail (member of Railing)*/
//// #define TF_POST_ELM                         79  /* a Post (member of Railing) */
//// #define TF_BALUSTER_ELM                     80  /* a Baluster (member of Railing) */
//// #define TF_SHARED_COMPOUND_CELL_ELM         81  // SharedFrameHandler
//// #define TF_SHARED_ADFCELL_ELM               82  // SharedAdfCellHandler
//// #define TF_ROOF_ELM                         83  /* a TriForma Roof Cell */
//// #define TF_RAILINGENDS_ELM                  84  /* a Baluster (member of Railing) */
//// #define TF_GRID_SYSTEM_ELM                  85  // TFColumnGridHandler
//// #define TF_RFACELL_ELM                      86  // PA-Cell with label subtype RFACELL_ELEMENT
//// #define TF_PAZCELL_ELM                      87  // traditional PA-Cell
//// 
//// #define NUM_QUANTIFIED_TYPES                88  /* last # type of TriForma element + 1 */

//    enum TFFormTypeEnum
//    {
//        TF_FREE_FORM_ELM = 31,
//        TF_LINEAR_FORM_ELM = 32,
//        TF_SLAB_FORM_ELM = 42,
//        TF_SMOOTH_FORM_ELM = 45
//    }


//    private bool isAvaliableTFFromType(int tftype)
//    {
//        if (!Enum.IsDefined(typeof(TFFormTypeEnum), tftype))
//            return false;
             
//        switch ((TFFormTypeEnum)tftype) 
//        {
//        case TFFormTypeEnum.TF_FREE_FORM_ELM: // TF_FREE_FORM_ELM
//        case TFFormTypeEnum.TF_LINEAR_FORM_ELM: // TF_LINEAR_FORM_ELM 
//        case TFFormTypeEnum.TF_SLAB_FORM_ELM: // TF_SLAB_FORM_ELM
//        case TFFormTypeEnum.TF_SMOOTH_FORM_ELM: // TF_SMOOTH_FORM_ELM
//            return true;
//        }
//        return false;
//    }

//    public static bool getFromElement(Element element, out PenetrTask penTask)
//    {
//        Sp3dTask task = null;
//        penTask = null;

//        if (!ElementHelper.isElementSp3dTask(element, out task) || 
//            !(task.isPipe() || task.isPipeOld()))
//        {
//            return false;
//        }

//        penTask = new PenetrTask(element, task);
//        return true;        
//    }

//    public bool getTFFormThickness(out double thickness)
//    {
//        thickness = 0.0;

//        if (TFFormsIntersected == null || TFFormsIntersected.Count == 0)
//            return false;

//        TFCOM.TFElementList tfList = TFFormsIntersected[0];

//        int type = tfList.AsTFElement.GetApplicationType();

//        BCOM.Element element;
//        tfList.AsTFElement.GetElement(out element);

//        TFCOM._TFFormRecipeList list;
//        tfList.AsTFElement.GetFormRecipeList(out list);

//        if (type == (int)TFFormTypeEnum.TF_SLAB_FORM_ELM)
//        {
//            TFCOM.TFFormRecipeSlabList slab = (TFCOM.TFFormRecipeSlabList)list; 
//            slab.AsTFFormRecipeSlab.GetThickness(out thickness);
//            return true;
//        }
//        else if (type == (int)TFFormTypeEnum.TF_LINEAR_FORM_ELM)
//        {
//            TFCOM.TFFormRecipeLinearList wall = (TFCOM.TFFormRecipeLinearList)list;
//            wall.AsTFFormRecipeLinear.GetThickness(out thickness);
//            return true;
//        }       
        
//        return false;
//    }


//    /// <summary>
//    /// Функция кратного округления из оригинального simpen Л.Вибе
//    /// </summary>
//    long roundExt(double val, int digs = -1, double snap = 5, int shft = 0) 
//    {
//        double dv;

//        dv = val * Math.Pow(snap, digs);

//        dv = Math.Floor(dv + 0.55555555555555 - (0.111111111111111 * shft));

//        dv = dv / Math.Pow(snap, digs);

//        return (long)dv;
//    }

//    BCOM.Point3d roundExt(BCOM.Point3d pt, int digs = -1, double snap = 5, int shft = 0)
//    {
//        BCOM.Point3d res;
//        res.X = roundExt(pt.X, digs, snap, shft);
//        res.Y = roundExt(pt.Y, digs, snap, shft);
//        res.Z = roundExt(pt.Z, digs, snap, shft);
//        return res;
//    }


//    public override string ToString()
//    {
//        return Name;
//    }
//}
//}
