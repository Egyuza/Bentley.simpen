using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Bentley.Internal.MicroStation;
using Bentley.Internal.MicroStation.Elements;
using Bentley.MicroStation.XmlInstanceApi;
using BCOM = Bentley.Interop.MicroStationDGN;
using TFCOM = Bentley.Interop.TFCom;

namespace simpen.ui
{
class TaskGeometry
{
    BCOM.Point3d start;
    BCOM.Point3d end;
    double radiusInside;
    double radiusOutside;
}

class FlangeTaskGeometry : TaskGeometry
{
    BCOM.Vector3d sideLocation;
}

class PenetrTask
{
    public enum TaskObjectType
    {
        Pipe,
        PipeOld,
        Flange
    }
    
    public IntPtr elemRefP { get; private set; }
    public long elemId { get; private set; }
    public IntPtr modelRefP { get; private set; }

    public string Name
    {
        get 
        { 
            return string.Format("T{0}-{1}-{2}", FlangesType, Diametr, Length); 
        }
    }
    public int FlangesType { get; set; }
    public int FlangesCount
    {
        get
        {
            return FlangesType == 1 ? 1 : FlangesType == 2 ? 2 : 0;
        }
    }

    public int Diametr { get; set; }
    // в сантиметрах
    public int Length { get; set; }

    /// <summary>
    /// расположение фланца, если он один
    /// </summary>
    public BCOM.Vector3d singleFlangeSide { get; private set; } = 
        Addin.App.Vector3dZero();

    public uint RefPointIndex { get; set; }
    public string RefPoint1 { get; private set; }
    public string RefPoint2 { get; private set; }
    public string RefPoint3 { get; private set; }

    //public ulong ElementId {get; private set; }
    public string Code { get; private set; }
    public string Oid { get; private set; }
    public string User { get; private set; }
    public string Path { get; private set; }
    public TaskObjectType TaskType { get; private set; }

    public BCOM.Point3d Location { get; private set; }
    public BCOM.Matrix3d Rotation { get; private set; }

    public string ErrorText { get; private set; }

    public bool isValid { get { return string.IsNullOrEmpty(ErrorText); } }

    public List<string> Warnings { get; private set; } = new List<string>();

    /// <summary>
    /// В диапазоне задания найден элемент CompoundCell
    /// </summary>
    public bool isCompoundExistsInPlace { get; private set; }

    public List<PenetrTaskFlange> Flanges { get; private set; }

    private PenetrTask(Element element, sp3d.Sp3dTask task)
    {
        elemRefP = element.ElementRef;
        modelRefP = element.ModelRef;
        elemId = element.ElementID;
        Oid = task.pipe.Oid;

        BCOM.ModelReference model = 
            Addin.App.MdlGetModelReferenceFromModelRefP((int)modelRefP);

        BCOM.Element bcomEl = model.GetElementByID(elemId);

        BCOM.Matrix3d rot;
        rot.RowX = Addin.App.Point3dFromXYZ(
            task.pipe.OrientationMatrix_x0, task.pipe.OrientationMatrix_x1,
            task.pipe.OrientationMatrix_x2);
        rot.RowY = Addin.App.Point3dFromXYZ(
            task.pipe.OrientationMatrix_y0, task.pipe.OrientationMatrix_y1,
            task.pipe.OrientationMatrix_y2);
        rot.RowZ = Addin.App.Point3dFromXYZ(
            task.pipe.OrientationMatrix_z0, task.pipe.OrientationMatrix_z1,
            task.pipe.OrientationMatrix_z2);
        Rotation = rot;

        if (task.pipe == null || task.component == null)
        {
            ErrorText = "Не удалось прочитать данные задания";
            return;
        }

        Code = task.pipe.Name;
        User = task.pipe.SP3D_UserLastModified;
        Path = task.pipe.SP3D_SystemPath;

        BCOM.Point3d pt = new BCOM.Point3d() ;
        pt.X = task.pipe.LocationX;
        pt.Y = task.pipe.LocationY;
        pt.Z = task.pipe.LocationZ;
        Location = pt;

        RefPoint1 = Location.ToStringEx();

        this.TaskType = task.component.Description == "PenFlange" ? 
            TaskObjectType.Flange : task.component.Description == "PntrtPlate-d" ?
            TaskObjectType.PipeOld : TaskObjectType.Pipe;

        // разбор типоразмера:
        try
        {
            string[] parameters = 
                task.pipe.Description.TrimStart('T').Split('-');
            FlangesType = int.Parse(parameters[0]);
            Diametr = int.Parse(parameters[1]);
            Length = int.Parse(parameters[2]);
        }
        catch (Exception)
        {
            ErrorText = string.Format("Не удалось разобрать типоразмер \"{0}\"",
                task.pipe.Description);
        }
        
        Flanges = new List<PenetrTaskFlange>();       
        
        // выполняем обратную трансформацию объекта,
        // проходка должна выровняться по оси Y

        if (TaskType == TaskObjectType.Pipe)
            findFlanges();
        else
            findFlangesOld();

        scanCollisions();
    }

    private void findFlangesOld()
    {
        BCOM.ModelReference model = 
            Addin.App.MdlGetModelReferenceFromModelRefP((int)modelRefP);

        singleFlangeSide = Addin.App.Vector3dZero();

        // Todo из правильной модели
        BCOM.CellElement cell = model.GetElementByID(elemId).AsCellElement();
        if (cell == null)
            return;

        var tran = Addin.App.Transform3dInverse(
            Addin.App.Transform3dFromMatrix3d(Rotation));        
        cell.Transform(tran);
        
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

        int geomFlangesCount = cones.Count/2 - 1;

        if (geomFlangesCount == 1) 
        { // 1 фланец, определяем вектор с какой стороны фланец
            singleFlangeSide = Addin.App.Vector3dSubtractPoint3dPoint3d(
                ElementHelper.getCenter(cones[0]), 
                ElementHelper.getCenter(cones[2]));

            singleFlangeSide = Addin.App.Vector3dNormalize(singleFlangeSide);
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
        BCOM.ModelReference taskModel = // TODO 
            Addin.App.MdlGetModelReferenceFromModelRefP((int)modelRefP);

        BCOM.ModelReference activeModel = Addin.App.ActiveModelReference;
        
        bool isAttachment = taskModel.IsAttachment;

        BCOM.CellElement cell = taskModel.IsAttachment ? 
            taskModel.AsAttachment.GetElementByID(elemId).AsCellElement() :
            taskModel.GetElementByID(elemId).AsCellElement();
        
        if (cell == null)
            return;
        
        Flanges.Clear();

        // Поиск фланцев
        BCOM.ElementScanCriteria criteria = new BCOM.ElementScanCriteriaClass();
        //criteria.IncludeOnlyVisible();
        criteria.ExcludeAllTypes();
        criteria.ExcludeNonGraphical();
        criteria.IncludeType(BCOM.MsdElementType.CellHeader);

        BCOM.Range3d scanRange = cell.Range;
            // Addin.App.Range3dScaleAboutCenter(cell.Range, 1.01);
       
        if (taskModel.IsAttachment)
        {
            double k = taskModel.UORsPerStorageUnit / activeModel.UORsPerStorageUnit;
            scanRange.High = Addin.App.Point3dScale(scanRange.High, k);
            scanRange.Low = Addin.App.Point3dScale(scanRange.Low, k);
        }

        criteria.IncludeOnlyWithinRange(scanRange);
       
        //var arcs = new List<BCOM.ArcElement>();
        //collectSubElementsByType(cell, ref arcs);        
        
        BCOM.ElementEnumerator res =  taskModel.Scan(criteria);
        //taskModel.IsAttachment ?
        //    taskModel.AsAttachment.Scan(criteria) :
        //    taskModel.Scan(criteria);
        
        while((res?.MoveNext()).Value)
        {
            // todo использовать только один тип точек
            PenetrTaskFlange flangeTask = null;
            //Element curEl = Element.FromElementID((ulong)res.Current.ID, 
            //    (IntPtr)res.Current.ModelReference.MdlModelRefP());

            if (PenetrTaskFlange.getFromElement(res.Current, out flangeTask))
            {
                BCOM.Range3d range = res.Current.Range;

                if (flangeTask.Oid.Equals(Oid))
                    Flanges.Add(flangeTask);
            }
        }
        
        BCOM.Transform3d tran = Addin.App.Transform3dInverse(
            Addin.App.Transform3dFromMatrix3d(Rotation));

        cell.Transform(tran);

        if (FlangesCount != Flanges.Count)
        {
            Warnings.Add(string.Format("Несоответствие количества фланцев, " + 
                "указанных в атрибутах - {0} и заданных геометрически - {1}", 
                FlangesCount, Flanges.Count));
        }

        if (FlangesCount == 1 && Flanges.Count > 0)
        {           
            BCOM.Element flangeEl = taskModel.GetElementByID(Flanges[0].elemId);
         
            flangeEl.Transform(tran);

            singleFlangeSide = Addin.App.Vector3dSubtractPoint3dPoint3d(
                ElementHelper.getCenter(cell), 
                ElementHelper.getCenter(flangeEl));

            singleFlangeSide = Addin.App.Vector3dNormalize(singleFlangeSide);
        }

        activeModel.Activate();
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

            if (type == BCOM.MsdElementType.Surface) {
            }

            if (type == BCOM.MsdElementType.Surface) {
                type = type;                               

                //ComplexElement surface = (ComplexElement)Element.FromElementID(
                //    (ulong)iter.Current.ID, modelRefP);

                Element el = Element.ElementFactory(elemRefP, modelRefP);
                    // Element.FromElementID((ulong)iter.Current.ID, modelRefP);      
            }
            
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
    /// Поиск уже ранее созданных объектов в диапазоне техн. задания на проходку
    /// </summary>
    public void scanCollisions()
    { 
        // TODO проверить через референсы:
        // TODO нужно ли искать коллизии в референсах?? ... по идеи да
        // учитывать перевод scanRange для референса

        BCOM.ModelReference refModel = 
            Addin.App.MdlGetModelReferenceFromModelRefP((int)modelRefP);
        BCOM.ModelReference activeModel = Addin.App.ActiveModelReference;        
        
        BCOM.CellElement cell = refModel.GetElementByID(elemId).AsCellElement();
        if (cell != null)
        {
            // Поиск фланцев
            BCOM.ElementScanCriteria criteria = new BCOM.ElementScanCriteriaClass();
            BCOM.Range3d scanRange = 
                Addin.App.Range3dScaleAboutCenter(cell.Range, 1.0);
            
            criteria.IncludeOnlyVisible();
            criteria.IncludeOnlyWithinRange(scanRange);

            BCOM.ElementEnumerator res = activeModel.Scan(criteria);
            while((res?.MoveNext()).Value)
            {
                // todo использовать только один тип точек
                BCOM.Element bcomEl = activeModel.GetElementByID(res.Current.ID);
                
                TFCOM.TFElementList tfEl = Addin.AppTF.CreateTFElement();
                tfEl.InitFromElement(bcomEl);

                if (tfEl.AsTFElement != null && tfEl.AsTFElement.GetIsCompoundCellType())
                {
                    isCompoundExistsInPlace = true;
                }
            }
        }
    }

    public static bool getFromElement(Element element, out PenetrTask penTask)
    {
        sp3d.Sp3dTask task = null;
        penTask = null;

        if (!ElementHelper.isElementSp3dTask(
            element.ElementID, element.ModelRef, out task) || 
            !(task.isPipe() || task.isPipeOld()))
        {
            return false;
        }

        penTask = new PenetrTask(element, task);
        return true;        
    }

    public override string ToString()
    {
        return Name;
    }
}
}
