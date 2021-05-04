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
using Embedded.Openings.Shared.Mapping;
using System.Xml.Linq;
using Bentley.Internal.MicroStation;

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

namespace Embedded.Openings.Shared.Models
{
public class GroupByTaskModel : NotifyPropertyChangedBase
{
    public new class NP : NotifyPropertyChangedBase.NP
    {
        public const string
            TaskSelection = "TaskSelection",
            SelectionCount = "SelectionCount";
    }

    public class FieldName
    {
        public const string CODE = "Code";
        public const string STATUS = "Status";
        public const string HEIGHT = "Height";
        public const string WIDTH = "Width";
        public const string DEPTH = "Depth";
    }

    public DataTable TaskTable {get; private set;}

    public int SelectionCount => TaskTable.Rows.Count;

    public bool IsDatasourceRefreshRequired {get; set;}

    private XDocument AttrsXDoc_;

    private GroupByTaskModel()
    {
        TaskTable = new DataTable();
        TaskTable.Columns.Add(FieldName.STATUS, typeof(string));
        TaskTable.Columns.Add(FieldName.CODE, typeof(string));
        TaskTable.Columns.Add(FieldName.HEIGHT, typeof(double));
        TaskTable.Columns.Add(FieldName.WIDTH, typeof(double));
        TaskTable.Columns.Add(FieldName.DEPTH, typeof(double));

        foreach(Sp3dToDataGroupMapProperty item in Sp3dToDGMapping.Instance.Items)
        {
            if (!string.IsNullOrEmpty(item.TargetXPath) && item.Key != FieldName.CODE)
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
        AttrsXDoc_ = XDocument.Load(uri);
        foreach(XElement xEl in AttrsXDoc_.Root.Nodes())
        {
            foreach(XElement child in xEl.Nodes().ToList())
            {
                foreach(var attr in child.Attributes())
                {
                    xEl.Add(
                        XElement.Parse($"<{attr.Name}>{attr.Value}</{attr.Name}>"));
                }
                child.Remove();
            }
        }
    }

    private void TaskTable_RowChanged(object sender, DataRowChangeEventArgs e)
    {
        if (!rowsToTasks_.ContainsKey(e.Row))
            return;

        OpeningTask task = rowsToTasks_[e.Row];
        task.Height = e.Row.Field<double>(FieldName.HEIGHT);
        task.Width = e.Row.Field<double>(FieldName.WIDTH);
        task.Depth = e.Row.Field<double>(FieldName.DEPTH);

        foreach(Sp3dToDataGroupMapProperty dgProp in task.DataGroupPropsValues.Keys.ToList())
        {
            foreach(string key in new string[] {dgProp.Key, dgProp.TargetName})
            {
                if(e.Row.Table.Columns.Contains(key))
                {
                    task.DataGroupPropsValues[dgProp]= e.Row.Field<string>(key);
                }
            }
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
                previewTranCon_.Reset();
                TaskTable.Clear();
                taskElemsToRows_.Clear();
                rowsToTasks_.Clear();
                break;
            case AddIn.SelectionChangedEventArgs.ActionKind.Remove:
            {
                Element element = ElementHelper.getElement(eventArgs);
                if (taskElemsToRows_.ContainsKey(element))
                {
                    DataRow row = taskElemsToRows_[element];
                    rowsToTasks_.Remove(row);
                    TaskTable.Rows.Remove(row);
                    taskElemsToRows_.Remove(element);
                }
                break;
            }
            case AddIn.SelectionChangedEventArgs.ActionKind.New:
            {
                Element element = ElementHelper.getElement(eventArgs);

                OpeningTask task;
                if (OpeningHelper.getFromElement(element, AttrsXDoc_, out task) &&
                    !taskElemsToRows_.ContainsKey(element))
                {
                    DataRow row = TaskTable.Rows.Add();
                    
                    row.SetField(FieldName.HEIGHT, task.Height);
                    row.SetField(FieldName.WIDTH, task.Width);
                    row.SetField(FieldName.DEPTH, task.Depth);  
                    
                    foreach(var pair in task.DataGroupPropsValues)
                    {
                        Sp3dToDataGroupMapProperty dgProp = pair.Key;
                        if (dgProp.Key == FieldName.CODE)
                        {
                            row.SetField(FieldName.CODE, pair.Value);
                        }
                        else if (TaskTable.Columns.Contains(dgProp.TargetName))
                        {
                            row.SetField(dgProp.TargetName, pair.Value);
                        }
                    }

                    taskElemsToRows_.Add(element, row);
                    rowsToTasks_.Add(row, task);
                }
                break;
            }
            case AddIn.SelectionChangedEventArgs.ActionKind.SetChanged:
            {
                if (lastSelectionAction_ != 
                    AddIn.SelectionChangedEventArgs.ActionKind.New)
                {
                    break;
                }
                
                //TaskSelection.RaiseListChangedEvents = false;
                //foreach (PenetrVueTask task in tasksBuf_.Values)
                //{
                //    //Logger.Log.Info($"Выбор объекта заадния {task.ToString()}");
                //    TaskSelection.Add(task);
                //}
                //tasksBuf_.Clear();
                //TaskSelection.RaiseListChangedEvents = true;
                break;
            }
            }
           
            //TaskSelection.ResetBindings();
            OnPropertyChanged(NP.TaskSelection);
        }
        catch (Exception ex)
        {
            // todo обработать
            ex.ShowMessage();
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
        addin_.SelectionChangedEvent += Addin_SelectionChangedEvent;
    }

    private void Addin_SelectionChangedEvent(
        AddIn sender, AddIn.SelectionChangedEventArgs eventArgs)
    {
        if (eventArgs.Action == lastSelectionAction_  && 
            eventArgs.FilePosition == lastFilePos_)
        {
            return;
        }

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

        try
        {
            switch ((int)eventArgs.Action)
            {
            case (int)AddIn.SelectionChangedEventArgs.ActionKind.SetEmpty:
                tasks_.Clear();
                TaskSelection.Clear();
                previewTranCon_.Reset();
                break;
            case (int)AddIn.SelectionChangedEventArgs.ActionKind.SetChanged:
            {
                // remove unselected
                foreach (IntPtr ptr in tasks_.Keys)
                {
                    if (!selectionSet.ContainsKey(ptr))
                    {
                        tasks_.Remove(ptr);
                    }
                }

                // add new
                TaskSelection.RaiseListChangedEvents = false;
                foreach (Element element in selectionSet.Values)
                {
                
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

                    IntPtr elementRef = element.GetNativeElementRef();
                    PenetrVueTask task;
                    if (PenetrVueTask.getFromElement(element, out task) &&
                        !tasks_.ContainsKey(elementRef))
                    {
                        Logger.Log.Info($"Выбор объекта заадния {task.ToString()}");
                        tasks_.Add(elementRef, task);
                        TaskSelection.Add(task);
                    }
                }
                
                TaskSelection.RaiseListChangedEvents = true;

                break;
            }
            case 7: // ActionKind.Remove
            {
                foreach (IntPtr ptr in tasks_.Keys)
                {
                    if (tasks_.ContainsKey(ptr))
                    {
                        TaskSelection.Remove(tasks_[ptr]);
                        tasks_.Remove(ptr);
                    }
                }
                break;
            }
            case 5: // ActionKind.New:
            {
                TaskSelection.RaiseListChangedEvents = false;
                foreach (Element element in selectionSet.Values)
                {
                    PenetrVueTask task;
                    if (PenetrVueTask.getFromElement(element, out task))
                    {
                        tasks_.Add(element.GetNativeElementRef(), task);
                        TaskSelection.Add(task);
                    }
                }                
                TaskSelection.RaiseListChangedEvents = true;
                break;
            }
            }

            TaskSelection.ResetBindings();
            OnPropertyChanged(NP.TaskSelection);
        }
        catch (Exception ex)
        {
            // todo обработать
            ex.ShowMessage();
        }
        finally
        {
            lastSelectionAction_ = eventArgs.Action;
            lastFilePos_ = eventArgs.FilePosition;
        }
    }
#endif

    public void focusTaskElement(DataRow row)
    {
        Element element = taskElemsToRows_.FirstOrDefault(x => x.Value == row).Key;        
        ViewHelper.zoomToElement(element.ToElementCOM());
    }


    public void changeSelection(IEnumerable<DataRow> selection)
    {
        selectionTranCon_?.Reset();

        foreach (DataRow taskRow in selection)
        {
            Element element = taskElemsToRows_.FirstOrDefault(x => 
                x.Value == taskRow).Key;   

            if (element == null)
                continue;

            selectionTranCon_.AppendCopyOfElement(element.ToElementCOM());

            //BCOM.ModelReference modelRef = task.ModelRef;
            //BCOM.View view = ViewHelper.getActiveView();

            //var taskUOR = new UOR(task.ModelRef);
            //var activeUOR = new UOR(App.ActiveModelReference);

            //List<long> itemsIds = new List<long> {task.elemId};
            //// добавляем фланцы:
            //foreach (PenetrTaskFlange flangeTask in task.FlangesGeom) 
            //{
            //    itemsIds.Add(flangeTask.elemId);
            //}

            //foreach (long id in itemsIds)
            //{
            //    BCOM.Element temp = modelRef.GetElementByID(id).Clone();
            //    temp.Color = 2; // зелёный
            //    temp.LineWeight = 5;

            //#if CONNECT
            //    // для версии CONNECT требуется поправка
            //    // в V8i возмоно она производится автоматически
            //    BCOM.Attachment attachment = task.getAttachment();
            //    if (attachment != null)
            //    {
            //        temp.Transform(attachment.GetReferenceToMasterTransform());
            //    }
            //#endif

            //    selectionTranCon_.AppendCopyOfElement(temp);
            //}
        }
    }

    public void rowsAdded(IEnumerable<DataRow> rows)
    {
        foreach (DataRow row in rows)
        {
            if (!row.HasErrors)
            {
                row.SetField(FieldName.STATUS, "OK");
            }
            else
            {
                row.SetField(FieldName.STATUS, "ERROR");
            }
        }
    }

    public void preview()
    {
        previewTranCon_.Reset();

        try
        {
            foreach (DataRow row in TaskTable.Rows)
            {
                OpeningTask task  = rowsToTasks_[row];

                var opening = new Opening(task);
                opening.AddProjection();

                previewTranCon_.AppendCopyOfElement(opening.GetElement());

                foreach(BCOM.Element projElement in opening.GetProjections())
                {
                    previewTranCon_.AppendCopyOfElement(projElement);
                }
            }
        }
        catch (Exception ex) // TODO
        {
            // ex.ShowMessage();
        }
    }
    
    public void addToModel()
    {
        previewTranCon_?.Reset();

        Session.Instance.StartUndoGroup();

        ElementHelper.RunByRecovertingSettings(() => {

            foreach (DataRow row in TaskTable.Rows)
            {
                Session.Instance.SetUndoMark();

                OpeningTask task  = rowsToTasks_[row];
                try
                {
                    Opening opening = new Opening(task);
                    opening.AddPerforation();
                    opening.AddProjection();
                    opening.AddToModel(false);

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
    private bool checkForIntersects(OpeningTask task/*, PenetrInfo penInfo*/)
    {
        //task.scanInfo();

        //BCOM.Element penElement = 
        //    PenetrHelper.getPenElementWithoutFlanges(task, penInfo);

        //IEnumerable<BCOM.Element> intersects =
        //    ElementHelper.scanIntersectsInElementRange(penElement,
        //        App.ActiveModelReference);
                    
        //foreach (BCOM.Element intersection in intersects)
        //{
        //    if (intersection.IsPenetrationCell())
        //    {
        //        var body = getBodyWithOutFlanges(intersection.AsCellElement());

        //        var contrIntersects = 
        //            ElementHelper.scanIntersectsInElementRange( body, 
        //                App.ActiveModelReference);

        //        BCOM.Range3d res = App.Range3dInit();
        //        if (App.Range3dIntersect2(ref res, body.Range, penElement.Range))
        //        {
        //            return false;
        //        }                
        //    }
        //    else if (intersection.IsCompundCell())
        //    {
        //        return false;
        //    }
        //}
        return true;
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

    private Dictionary<DataRow, OpeningTask> rowsToTasks_ = 
        new Dictionary<DataRow, OpeningTask>();

    private Dictionary<Element, DataRow> taskElemsToRows_ = 
        new Dictionary<Element, DataRow>();    

    private BCOM.TransientElementContainer selectionTranCon_;
    private BCOM.TransientElementContainer previewTranCon_;

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
