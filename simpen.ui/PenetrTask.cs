using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Bentley.Internal.MicroStation;
using Bentley.Internal.MicroStation.Elements;
using Bentley.MicroStation.XmlInstanceApi;
using BCOM = Bentley.Interop.MicroStationDGN;

namespace simpen.ui
{
class PenetrTask
{
    public enum TaskObjectType
    {
        Pipe,
        Flange
    }
    
    public IntPtr elemRefP {get; private set; }
    public long elemId {get; private set; }
    public IntPtr modelRefP {get; private set; }

    public string Name
    {
        get { return string.Format("T{0}-{1}-{2}", FlangesType, Diametr, Length); }
    }
    public int FlangesType { get; set; }
    public int Diametr { get; set; }
    public int Length { get; set; }

    public uint RefPointIndex {get; set;}
    public string RefPoint1 {get; private set;}
    public string RefPoint2 {get; private set;}
    public string RefPoint3 {get; private set;}

    //public ulong ElementId {get; private set; }
    public string Code {get; private set; }
    public string Oid {get; private set; }
    public string User {get; private set; }
    public string Path {get; private set; }
    public TaskObjectType TaskType {get; private set; }

    public BCOM.Point3d Location {get; private set; }

    public string ErrorText { get; private set; }

    public bool isValid { get { return string.IsNullOrEmpty(ErrorText); } }

    public List<PenetrTaskFlange> Flanges { get; private set; }

    private PenetrTask(Element element, sp3d.Sp3dTask task)
    {
        elemRefP = element.ElementRef;
        modelRefP = element.ModelRef;
        elemId = element.ElementID;
        Oid = task.pipe.Oid;

        Flanges = new List<PenetrTaskFlange>();

        if (task.pipe == null || task.component == null)
        {
            ErrorText = "Не удалось прочитать данные задания";
            return;
        }

        Code = task.pipe.Name;
        User = task.pipe.SP3D_UserLastModified;
        Path = task.pipe.SP3D_SystemPath;

        this.TaskType = task.component.Description == "PenFlange" ? 
                TaskObjectType.Flange : TaskObjectType.Pipe;

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
            ErrorText = string.Format("Не удалость разобрать типоразмер \"{0}\"",
                task.pipe.Description);
        }


        BCOM.CellElement cell = Addin.App.ActiveModelReference.
                GetElementByID(elemId).AsCellElement();
        if (cell != null) 
        { 
            // Поиск фланцев
            BCOM.ElementScanCriteria criteria = new BCOM.ElementScanCriteriaClass();
            BCOM.Range3d scanRange = 
                Addin.App.Range3dScaleAboutCenter(cell.Range, 1.0);
            
            criteria.IncludeOnlyVisible();
            criteria.IncludeOnlyWithinRange(scanRange);
            
            BCOM.ModelReference modelRef =
                Addin.App.MdlGetModelReferenceFromModelRefP((int)modelRefP);

            BCOM.ElementEnumerator res = modelRef.Scan(criteria);            
            while((res?.MoveNext()).Value) 
            {
                PenetrTaskFlange flangeTask = null;
                Element curEl = Element.FromElementID((ulong)res.Current.ID, modelRefP);

                if (PenetrTaskFlange.getFromElement(curEl, out flangeTask))
                {
                    if (flangeTask.Oid.Equals(Oid))
                        Flanges.Add(flangeTask);
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
}
}
