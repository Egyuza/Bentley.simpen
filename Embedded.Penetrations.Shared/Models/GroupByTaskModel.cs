using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

using BCOM = Bentley.Interop.MicroStationDGN;
using TFCOM = Bentley.Interop.TFCom;

using Shared;
using Shared.Bentley;

using Bentley.Building.DataGroupSystem.Serialization;
using Bentley.Building.DataGroupSystem;
using System.Data;
using System.Xml.Linq;

#if V8i
using Bentley.MicroStation;
using Bentley.Internal.MicroStation;
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
public class GroupByTaskModel : NotifyPropertyChangedBase
{
    public new class NP : NotifyPropertyChangedBase.NP
    {
        public const string
            TaskSelection = "TaskSelection",
            SelectionCount = "SelectionCount";
    }

    public class PropKey
    {
        public const string 
            CODE = "Code",
            NAME = "Name";
    }

    public class FieldName
    {
        public const string 
            STATUS = "Status",
            FLANGES = "Flanges",
            DIAMETER = "Diameter",
            LENGTH = "Length(cm)",
            REF_POINT1 = "RefPoint1",
            REF_POINT2 = "RefPoint2",
            REF_POINT3 = "RefPoint3";
    }

    public DataTable TaskTable {get; private set;}

    public int SelectionCount => TaskTable.Rows.Count;

    public XDocument AttrsXDoc {get; set;}

    public PenetrVueTask GetTask(DataRow dataRow)
    {
        if (!rowsToTasks_.ContainsKey(dataRow))
            return null;

        return rowsToTasks_[dataRow];
    }

    public IList<DiameterType> getDiametersList(PenetrVueTask task)
    {
        return penData_.getDiameters(task.FlangesType);
    }

    public IEnumerable<long> getFlangeNumbersSort() {
        return penData_.getFlangeNumbersSort();
    }

    private GroupByTaskModel()
    {
        penData_ = PenetrDataSource.Instance;

        TaskTable = new DataTable();
        TaskTable.Columns.Add(FieldName.STATUS, typeof(string));
        TaskTable.Columns.Add(PropKey.CODE, typeof(string));
        TaskTable.Columns.Add(PropKey.NAME, typeof(string));
        TaskTable.Columns.Add(FieldName.FLANGES, typeof(long));
        TaskTable.Columns.Add(FieldName.DIAMETER, typeof(string));
        TaskTable.Columns.Add(FieldName.LENGTH, typeof(int));
        TaskTable.Columns.Add(FieldName.REF_POINT1, typeof(string));
        TaskTable.Columns.Add(FieldName.REF_POINT2, typeof(string));
        TaskTable.Columns.Add(FieldName.REF_POINT3, typeof(string));

        foreach(Sp3dToDataGroupMapProperty item in Sp3dToDGMapping.Instance.Items)
        {
            if (item.Key == PropKey.CODE || item.Key == PropKey.NAME)
                continue;

            if (!string.IsNullOrEmpty(item.TargetXPath))
            {
                TaskTable.Columns.Add(item.TargetName, typeof(string));
            }
        }
        TaskTable.RowChanged += TaskTable_RowChanged;
        signOnNotify(NP.SelectionCount, NP.TaskSelection);


        BCOM.Point3d zero = App.Point3dZero();
        BCOM.LineElement line = App.CreateLineElement2(null, zero, zero);       
        line.Color = 255;
        
        selectionTranCon_ = App.CreateTransientElementContainer1(
            line, 
            BCOM.MsdTransientFlags.DisplayFirst |
            BCOM.MsdTransientFlags.Overlay,
            BCOM.MsdViewMask.AllViews, 
            BCOM.MsdDrawingMode.Temporary);
        
        previewTranCon_ = App.CreateTransientElementContainer1(
            line, 
            BCOM.MsdTransientFlags.DisplayFirst |
            BCOM.MsdTransientFlags.Overlay | BCOM.MsdTransientFlags.Snappable | BCOM.MsdTransientFlags.IncludeInPlot,
            BCOM.MsdViewMask.AllViews, 
            BCOM.MsdDrawingMode.Temporary);
    }

    public void loadXmlAttrs(string uri)
    {
        AttrsXDoc = XDocument.Load(uri);
        foreach(XElement xEl in AttrsXDoc.Root.Nodes())
        {
            foreach(XElement child in xEl.Nodes().ToList())
            {
                foreach(var attr in child.Attributes())
                {
                    xEl.Add(XElement.Parse(
                        $"<{attr.Name}>{attr.Value}</{attr.Name}>"));
                }
                child.Remove();
            }
        }
    }

    private bool IsRunning_TaskTable_RowChanged_;
    private void TaskTable_RowChanged(object sender, DataRowChangeEventArgs e)
    {
        if (IsRunning_TaskTable_RowChanged_)
            return;

        IsRunning_TaskTable_RowChanged_ = true;

        try
        {
            taskTable_RowChanged_(e.Row);
        }
        finally
        {
            IsRunning_TaskTable_RowChanged_ = false;
        }
    }

    private void taskTable_RowChanged_(DataRow row)
    {
        if (!rowsToTasks_.ContainsKey(row))
            return;

        PenetrVueTask task = rowsToTasks_[row];

        // TODO
        task.Code = row.Field<string>(PropKey.CODE);
        task.DiameterTypeStr = row.Field<string>(FieldName.DIAMETER);
        task.FlangesType = row.Field<long>(FieldName.FLANGES);
        task.LengthCm = row.Field<int>(FieldName.LENGTH);

        { // RefPoints:
            task.SetRefPoint(0, row.Field<string>(FieldName.REF_POINT1));
            task.SetRefPoint(1, row.Field<string>(FieldName.REF_POINT2));
            task.SetRefPoint(2, row.Field<string>(FieldName.REF_POINT3));
        }

        foreach(Sp3dToDataGroupMapProperty dgProp in task.DataGroupPropsValues.Keys.ToList())
        {
            foreach(string key in new string[] {dgProp.Key, dgProp.TargetName})
            {
                if (dgProp.Key == PropKey.CODE)
                {
                    task.DataGroupPropsValues[dgProp] = task.Code;
                }
                else if(dgProp.Key == PropKey.NAME)
                {
                    task.DataGroupPropsValues[dgProp] = task.Name;
                }
                else if(row.Table.Columns.Contains(key))
                {
                    task.DataGroupPropsValues[dgProp]= row.Field<string>(key);
                }
            }
        }

        { // ! обновление DataRow
            row.BeginEdit();
            row.SetField(PropKey.NAME, task.Name);

            row.SetField(FieldName.REF_POINT1, task.GetRefPointCoords(0));
            row.SetField(FieldName.REF_POINT2, task.GetRefPointCoords(1));
            row.SetField(FieldName.REF_POINT3, task.GetRefPointCoords(2));

            row.EndEdit();
        }        
    }

    private AddIn.SelectionChangedEventArgs.ActionKind lastSelectionAction_;
    private uint lastFilePos_;

#if V8i
    Bentley.MicroStation.AddIn addin_;
    public GroupByTaskModel(Bentley.MicroStation.AddIn addin) : this()
    {
        addin_ = addin;
    }

    private void Addin_SelectionChangedEvent(
        AddIn sender, AddIn.SelectionChangedEventArgs eventArgs)
    {
        try
        {
            if (eventArgs.Action == lastSelectionAction_  && 
                eventArgs.FilePosition == lastFilePos_)
            {
                return;
            }

            switch (eventArgs.Action)
            {
            case AddIn.SelectionChangedEventArgs.ActionKind.SetEmpty:
                unselectAll_();
                break;
            case AddIn.SelectionChangedEventArgs.ActionKind.Remove:
            {
                Element element = ElementHelper.getElement(eventArgs);
                unselectElement_(element);
                break;
            }
            case AddIn.SelectionChangedEventArgs.ActionKind.New:
            {
                Element element = ElementHelper.getElement(eventArgs);
                selectElement_(element);
                break;
            }
            }
           
            //TaskSelection.ResetBindings();
            OnPropertyChanged(NP.TaskSelection);
        }
        catch (Exception ex)
        {
            ex.AddToMessageCenter();
        }
        finally
        {
            lastSelectionAction_ = eventArgs.Action;
            lastFilePos_ = eventArgs.FilePosition;
        }
    }

#elif CONNECT

    Bentley.MstnPlatformNET.AddIn addin_;
    public GroupByTaskModel(Bentley.MstnPlatformNET.AddIn addin) : this()
    {
        addin_ = addin;
    }

    private void Addin_SelectionChangedEvent(
        AddIn sender, AddIn.SelectionChangedEventArgs eventArgs)
    {
        //if (eventArgs.Action == lastSelectionAction_  && 
        //    eventArgs.FilePosition == lastFilePos_)
        //{
        //    return;
        //}

        try
        {
            switch ((int)eventArgs.Action)
            {
            case (int)AddIn.SelectionChangedEventArgs.ActionKind.SetEmpty:
                unselectAll_();
                break;
            case (int)AddIn.SelectionChangedEventArgs.ActionKind.SetChanged:
            case 7: // ActionKind.Remove
            case 5: // ActionKind.New:
            case (int)AddIn.SelectionChangedEventArgs.ActionKind.DragNew:
            {
                refreshSelection_();
                break;
            }
            }

            OnPropertyChanged(NP.TaskSelection);
        }
        catch (Exception ex)
        {
            // todo обработать
            ex.AddToMessageCenter();
        }
        finally
        {
            lastSelectionAction_ = eventArgs.Action;
            lastFilePos_ = eventArgs.FilePosition;
        }
    }

    private void refreshSelection_()
    {
        Dictionary<IntPtr, Element> selectionSet = new Dictionary<IntPtr, Element>();
        uint nums = SelectionSetManager.NumSelected();
        for (uint i = 0; i < nums; ++i)
        {
            Element element = null;
            DgnModelRef modelRef = null;

            if (StatusInt.Success ==
                SelectionSetManager.GetElement(i, ref element, ref modelRef) &&
                element.ElementType == MSElementType.CellHeader &&
                !selectionSet.ContainsKey(element.GetNativeElementRef()))
            {
                selectionSet.Add(element.GetNativeElementRef(), element);
            }
        }

        if (selectionSet.Count == 0)
        {
            unselectAll_();
        }
        else
        {
            foreach (Element element in taskElemsToRows_.Keys)
            {
                if (!selectionSet.ContainsKey(element.GetNativeElementRef()))
                {
                    unselectElement_(element);
                }
            }

            foreach (Element el in selectionSet.Values)
            {
                selectElement_(el);
            }
        }
    }
#endif

    private void unselectAll_()
    {
        previewTranCon_.Reset();
        TaskTable.Clear();
        taskElemsToRows_.Clear();
        rowsToTasks_.Clear();
    }

    private void selectElement_(Element element)
    {
        PenetrVueTask task;
        if (PenetrVueTask.getFromElement(element, AttrsXDoc, out task) &&
            !taskElemsToRows_.ContainsKey(element) && 
            null == rowsToTasks_.Values.FirstOrDefault(x => x.Oid == task.Oid)) // ?
        {
            TaskTable.BeginLoadData();
            DataRow row = TaskTable.Rows.Add();

            row.SetField(PropKey.CODE, task.Code);
            row.SetField(PropKey.NAME, task.Name);
            row.SetField(FieldName.FLANGES, task.FlangesType);
            row.SetField(FieldName.DIAMETER, task.DiameterTypeStr);
            row.SetField(FieldName.LENGTH, task.LengthCm);

            row.SetField<string>(FieldName.REF_POINT1, task.GetRefPointCoords(0));
            row.SetField<string>(FieldName.REF_POINT2, task.GetRefPointCoords(1));
            row.SetField<string>(FieldName.REF_POINT3, task.GetRefPointCoords(2));
                
            foreach(var pair in task.DataGroupPropsValues)
            {
                Sp3dToDataGroupMapProperty dgProp = pair.Key;
                if (TaskTable.Columns.Contains(dgProp.TargetName))
                {
                    row.SetField(dgProp.TargetName, pair.Value);
                }
            }

            taskElemsToRows_.Add(element, row);
            rowsToTasks_.Add(row, task);                    
            TaskTable.AcceptChanges();
            TaskTable.EndLoadData();
            
        #if DEBUG
                //BCOM.Element comEl = ElementHelper.getElementCOM(element);

                //if (comEl.IsCompundCell())
                //{
                //    var cell = comEl.AsCellElement();

                //    var cross = ElementHelper.createCrossRound(10, cell.Origin);
                //    var pointEl = ElementHelper.createCircle(10, cell.Origin);

                //    previewTranCon_.AppendCopyOfElement(pointEl);
                //    previewTranCon_.AppendCopyOfElement(cross);
                //}
        #endif
        }
    }

    private void unselectElement_(Element element)
    {
        if (taskElemsToRows_.ContainsKey(element))
        {
            DataRow row = taskElemsToRows_[element];
            rowsToTasks_.Remove(row);
            TaskTable.Rows.Remove(row);
            taskElemsToRows_.Remove(element);
        }
    }

    public bool isProjectDefined => penData_.ProjectId != 0;

    public long ProjectId => penData_.ProjectId;

    public void changeSelection(IEnumerable<DataRow> selection)
    {
        selectionTranCon_?.Reset();

        foreach (DataRow taskRow in selection)
        {
            PenetrVueTask task  = rowsToTasks_[taskRow];

            BCOM.ModelReference modelRef = task.ModelRef;
            BCOM.View view = ViewHelper.getActiveView();

            var taskUOR = new UOR(task.ModelRef);
            var activeUOR = new UOR(App.ActiveModelReference);

            List<long> itemsIds = new List<long> {task.elemId};
            // добавляем фланцы:
            foreach (PenetrTaskFlange flangeTask in task.FlangesGeom) 
            {
                itemsIds.Add(flangeTask.elemId);
            }

            foreach (long id in itemsIds)
            {
                BCOM.Element temp = modelRef.GetElementByID(id).Clone();
                temp.Color = 2; // зелёный
                temp.LineWeight = 5;

            #if CONNECT
                // для версии CONNECT требуется поправка
                // в V8i возмоно она производится автоматически
                BCOM.Attachment attachment = task.getAttachment();
                if (attachment != null)
                {
                    temp.Transform(attachment.GetReferenceToMasterTransform());
                }
            #endif

                selectionTranCon_.AppendCopyOfElement(temp);
            }
        }
    }

    public void focusTaskElement(DataRow row)
    {
        Element element = taskElemsToRows_.FirstOrDefault(x => x.Value == row).Key;       
        ViewHelper.zoomToElement(element.ToElementCOM());
    }

    public void preview()
    {
        previewTranCon_.Reset();

        try
        {
            foreach (DataRow row in TaskTable.Rows)
            {
                PenetrVueTask task  = rowsToTasks_[row];

                var penetration = new Penetration(task);
                penetration.AddProjection();

                previewTranCon_.AppendCopyOfElement(penetration.GetElement());

                foreach(BCOM.Element projElement in penetration.GetProjections())
                {
                    previewTranCon_.AppendCopyOfElement(projElement);
                }
            }
        }
        catch (Exception ex) // TODO
        {
            ex.AddToMessageCenter();
        }


        //try
        //{
        //    //foreach (PenetrVueTask task in TaskSelection)
        //    //{
        //    //    PenetrInfo penInfo = penData_.getPenInfo(
        //    //        task.FlangesType, task.DiameterType.Number);                

        //    //    TFCOM.TFFrameList frameList = 
        //    //        PenetrHelper.createFrameList(task, penInfo, PenetrTaskBase.LevelMain);
                
        //    //    previewTranCon_.AppendCopyOfElement(
        //    //            frameList.AsTFFrame.Get3DElement());

        //    //    var projList = frameList.AsTFFrame.GetProjectionList();
                
        //    //    if (projList == null) 
        //    //        continue;

        //    //    do
        //    //    {
        //    //        try
        //    //        {
        //    //            BCOM.Element projEl = null;
        //    //            projList.AsTFProjection.GetElement(out projEl);
        //    //            if(projEl != null)
        //    //                previewTranCon_.AppendCopyOfElement(projEl);
        //    //        }
        //    //        catch (Exception) { /* !не требует обработки  */ }               
        //    //    } while ((projList = projList.GetNext()) != null);
        //    //}

        //}
        //catch (Exception ex) // TODO
        //{
        //    ex.AddToMessageCenter();
        //}
    }
    
    public void addToModel()
    {
        previewTranCon_?.Reset();

        Session.Instance.StartUndoGroup();

        ElementHelper.RunByRecovertingSettings(() => {

            foreach (DataRow row in TaskTable.Rows)
            {
                Session.Instance.SetUndoMark();

                PenetrVueTask task  = rowsToTasks_[row];
                try
                {
                    var penetr = new Penetration(task);
                    penetr.AddProjection();
                    penetr.AddPerforation();
                    penetr.AddToModel(false);
                    penetr.SetTags();

                    row.SetField(FieldName.STATUS, "DONE");
                    // TODO статус о выполнении
                }
                catch (Exception ex)
                {
                    // TODO статус о выполнении
                    row.SetField(FieldName.STATUS, "ERROR");
                    Session.Instance.Keyin("undo");
                    var last = App.ActiveModelReference.GetLastValidGraphicalElement();
                    last?.Rewrite();
                }                
            }
        });

        Session.Instance.EndUndoGroup();
    }
    
    /// <summary>
    /// Проверка на пересечения с другими закладными элементами.
    /// TRUE - если проверка пройдена
    /// </summary>
    private bool checkForIntersects(PenetrVueTask task, PenetrInfo penInfo)
    {
        task.scanInfo();

        BCOM.Element penElement = 
            PenetrHelper.getPenElementWithoutFlanges(task, penInfo);

        IEnumerable<BCOM.Element> intersects =
            ElementHelper.scanIntersectsInElementRange(penElement,
                App.ActiveModelReference);
                    
        foreach (BCOM.Element intersection in intersects)
        {
            if (intersection.IsPenetrationCell())
            {
                var body = getBodyWithOutFlanges(intersection.AsCellElement());

                var contrIntersects = 
                    ElementHelper.scanIntersectsInElementRange( body, 
                        App.ActiveModelReference);

                BCOM.Range3d res = App.Range3dInit();
                if (App.Range3dIntersect2(ref res, body.Range, penElement.Range))
                {
                    return false;
                }                
            }
            else if (intersection.IsCompundCell())
            {
                return false;
            }
        }
        return true;
    }

    private BCOM.SmartSolidElement getBodyWithOutFlanges(BCOM.CellElement penCell)
    {
        BCOM.SmartSolidElement body = null;
        
        double maxVolume = 0.0;
        foreach(var solid in penCell.getSubElementsRecurse<BCOM.SmartSolidElement>())
        {
            double volume = solid.ComputeVolume();
            if (volume > maxVolume)
            {
                body = solid;
                maxVolume = volume;
            }
        }
        return body;
    }

    public void loadContext()
    {
        addin_.SelectionChangedEvent += Addin_SelectionChangedEvent;
        previewTranCon_?.Reset();
        selectionTranCon_?.Reset();
    }

    public void clearContext()
    {
        addin_.SelectionChangedEvent -= Addin_SelectionChangedEvent;
        previewTranCon_?.Reset();
        selectionTranCon_?.Reset();

        taskElemsToRows_.Clear();
        TaskTable.Clear();
        OnPropertyChanged(NP.TaskSelection);
    }

    private PenetrDataSource penData_; // TODO переименовать

    private Dictionary<DataRow, PenetrVueTask> rowsToTasks_ = 
        new Dictionary<DataRow, PenetrVueTask>();

    private Dictionary<Element, DataRow> taskElemsToRows_ = 
        new Dictionary<Element, DataRow>();

    private BCOM.TransientElementContainer selectionTranCon_;
    private BCOM.TransientElementContainer previewTranCon_;

        //private Dictionary<IntPtr, PenetrVueTask> tasks_ = 
        //    new Dictionary<IntPtr, PenetrVueTask>();
        //private Dictionary<IntPtr, PenetrVueTask> tasksBuf_ = 
        //    new Dictionary<IntPtr, PenetrVueTask>();

    private static BCOM.Application App => BMI.Utilities.ComApp;

    private static TFCOM.TFApplication appTF_;
    private static TFCOM.TFApplication AppTF => 
        appTF_ ?? (appTF_ = new TFCOM.TFApplicationList().TFApplication);

    // TODO для ОТЛАДКИ:
    //XmlInstanceSchemaManager modelSchema =
    //        new XmlInstanceSchemaManager((IntPtr)newElement.ModelReference.MdlModelRefP());
        
    //    XmlInstanceApi api = XmlInstanceApi.CreateApi(modelSchema);
    //    IList<string> instances = api.ReadInstances((IntPtr)newElement.MdlElementRef());

    //    foreach (string inst in instances)
    //    {
    //        string instId = XmlInstanceApi.GetInstanceIdFromXmlInstance(inst);   
    //    }
}
}
