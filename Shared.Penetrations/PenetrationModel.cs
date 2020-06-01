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
public class PenetrationModel : NotifyPropertyChangedBase
{
    public new class NP : NotifyPropertyChangedBase.NP
    {
        public const string
            TaskSelection = "TaskSelection",
            SelectionCount = "SelectionCount";
    }

    public BindingList<PenetrTask> TaskSelection {get; private set;} 
    public int SelectionCount => TaskSelection.Count;

    public IList<DiameterType> getDiametersList(PenetrTask task)
    {
        return penData_.getDiameters(task.FlangesType);
    }

    public IEnumerable<long> getFlangeNumbersSort() {
        return penData_.getFlangeNumbersSort();
    }

    private PenetrationModel()
    {
        penData_ = new PenetrDataSource();
        TaskSelection = new BindingList<PenetrTask>();
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

#if V8i
    Bentley.MicroStation.AddIn addin_;
    public PenetrationModel(Bentley.MicroStation.AddIn addin) : this()
    {
        addin_ = addin;
        addin_.SelectionChangedEvent += Addin_SelectionChangedEvent;
    }

    private void Addin_SelectionChangedEvent(
        AddIn sender, AddIn.SelectionChangedEventArgs eventArgs)
    {
        try
        {
            switch (eventArgs.Action)
            {
            case AddIn.SelectionChangedEventArgs.ActionKind.SetEmpty:
                tasks_.Clear();
                previewTranCon_.Reset();
                TaskSelection.Clear();
                break;
            case AddIn.SelectionChangedEventArgs.ActionKind.Remove:
            {
                Element element = ElementHelper.getElement(eventArgs);
                if (tasks_.ContainsKey(element.ElementRef))
                {
                    TaskSelection.Remove(tasks_[element.ElementRef]);
                    tasks_.Remove(element.ElementRef);
                }
                break;
            }
            case AddIn.SelectionChangedEventArgs.ActionKind.New:
            {
                Element element = ElementHelper.getElement(eventArgs);


        #if DEBUG
                BCOM.Element comEl = ElementHelper.getElementCOM(element);;

                if (comEl.IsCompundCell())
                {
                    var cell = comEl.AsCellElement();
                    var pointEl = ElementHelper.createPoint(cell.Origin);
                    pointEl.Level.ElementLineWeight = 10;

                    previewTranCon_.AppendCopyOfElement(pointEl);
                }
        #endif

                PenetrTask task;
                if (PenetrTask.getFromElement(element, out task))
                {
                    if (tasks_.ContainsKey(element.ElementRef))
                    {
                        TaskSelection.Remove(tasks_[element.ElementRef]);
                        tasks_.Remove(element.ElementRef);
                    }
                    tasks_.Add(element.ElementRef, task);
                    TaskSelection.Add(task);
                }
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
    }

#elif CONNECT
    Bentley.MstnPlatformNET.AddIn addin_;
    public PenetrationModel(Bentley.MstnPlatformNET.AddIn addin) : this()
    {
        addin_ = addin;
        addin_.SelectionChangedEvent += Addin_SelectionChangedEvent;
    }

    private void Addin_SelectionChangedEvent(
        AddIn sender, AddIn.SelectionChangedEventArgs eventArgs)
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
                foreach (Element element in selectionSet.Values)
                {

        #if DEBUG
                BCOM.Element comEl = ElementHelper.getElementCOM(element);

                if (comEl.IsCompundCell())
                {
                    var cell = comEl.AsCellElement();

                    var cross = ElementHelper.createCrossRound(10, cell.Origin);
                    var pointEl = ElementHelper.createCircle(10, cell.Origin);

                    previewTranCon_.AppendCopyOfElement(pointEl);
                    previewTranCon_.AppendCopyOfElement(cross);
                }
        #endif

                    IntPtr elementRef = element.GetNativeElementRef();
                    PenetrTask task;
                    if (PenetrTask.getFromElement(element, out task) &&
                        !tasks_.ContainsKey(elementRef))
                    {
                        tasks_.Add(elementRef, task);
                        TaskSelection.Add(task);
                    }
                }
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
                foreach (Element element in selectionSet.Values)
                {
                    PenetrTask task;
                    if (PenetrTask.getFromElement(element, out task))
                    {
                        tasks_.Add(element.GetNativeElementRef(), task);
                        TaskSelection.Add(task);
                    }
                }
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
    }
#endif

    public bool isProjectDefined => penData_.ProjectId != 0;


    public void changeSelection(IEnumerable<PenetrTask> selection)
    {
        selectionTranCon_?.Reset();

        foreach (PenetrTask task in selection)
        {
            BCOM.ModelReference modelRef = task.modelRef;
            BCOM.View view = ViewHelper.getActiveView();

            var taskUOR = new UOR(task.modelRef);
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
                temp.Transform(attachment.GetReferenceToMasterTransform());
            #endif

                selectionTranCon_.AppendCopyOfElement(temp);
            }
        }
    }

    public void focusToTaskElement(PenetrTask task)
    {
        ViewHelper.zoomToElement(task.getElement());
    }

    public void preview()
    {
        previewTranCon_.Reset();

        try
        {
            foreach (PenetrTask task in TaskSelection)
            {
                PenetrInfo penInfo = penData_.getPenInfo(
                    task.FlangesType, task.DiameterType.Number);
                
        //#if DEBUG
        //        var cylinder = 
        //            PenetrHelper.getPenElementWithoutFlanges(task, penInfo);

        //        previewTranCon_.AppendCopyOfElement(cylinder);
        //        continue;
        //#endif

                TFCOM.TFFrameList frameList = 
                    PenetrHelper.createFrameList(task, penInfo, PenetrTask.LevelMain);
                
                previewTranCon_.AppendCopyOfElement(
                        frameList.AsTFFrame.Get3DElement());

                var projList = frameList.AsTFFrame.GetProjectionList();
                
                if (projList == null) 
                    continue;

                do
                {
                    try
                    {
                        BCOM.Element projEl = null;
                        projList.AsTFProjection.GetElement(out projEl);
                        if(projEl != null)
                            previewTranCon_.AppendCopyOfElement(projEl);
                    }
                    catch (Exception) { /* !не требует обработки  */ }               
                } while ((projList = projList.GetNext()) != null);
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

        var activeSets = App.ActiveSettings;

        BCOM.Level activeLevel = activeSets.Level;
        BCOM.LineStyle activeLineStyle = activeSets.LineStyle;
        int activeLineWeight = activeSets.LineWeight;
        int activeColor = activeSets.Color;

        var activeModel = App.ActiveModelReference;
        try
        {
            foreach (PenetrTask task in TaskSelection)
            {
                PenetrInfo penInfo = penData_.getPenInfo(
                    task.FlangesType, task.DiameterType.Number);

                
                if (!checkForIntersects(task, penInfo)) // ! ВАЖНО
                {   // TODO ПРОВЕРКА НА ПЕРЕСЕЧЕНИЕ!

                    task.Warnings.Add("Пересечение с другими закладными");
                    System.Windows.Forms.MessageBox.Show("Пересечение с другими закладными");
                    continue;
                }

                TFCOM.TFFrameListClass frameList = 
                    PenetrHelper.createFrameList(task, penInfo, PenetrTask.LevelMain);
                
                PenetrHelper.addProjection(frameList, task, penInfo);

                // TODO видимость контура перфоратора можно в конфиг. переменную
                PenetrHelper.addPerforator(frameList, 
                    task, penInfo, PenetrTask.LevelSymb, false);

                PenetrHelper.applyPerforatorInModel(frameList);

                PenetrHelper.addToModel(frameList);
                
                BCOM.Element bcomElem;
                frameList.GetElement(out bcomElem);   

                setDataGroupInstance(bcomElem, task);
            }
        }
        catch (Exception ex)
        {
            ex.ShowMessage();
        }
        finally
        {
            activeSets.Level = activeLevel;
            activeSets.LineStyle = activeLineStyle;
            activeSets.LineWeight = activeLineWeight;
            activeSets.Color = activeColor;
        }
    }


    /// <summary>
    /// Проверка на пересечения с другими закладными элементами.
    /// TRUE - если проверка пройдена
    /// </summary>
    private bool checkForIntersects(PenetrTask task, PenetrInfo penInfo)
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

        tasks_.Clear();
        TaskSelection.Clear();
        TaskSelection.ResetBindings();
        OnPropertyChanged(NP.TaskSelection);
    }


    private static void setDataGroupInstance(
        BCOM.Element bcomElement, PenetrTask task)
    {
        Element element = ElementHelper.getElement(bcomElement);
        if (element == null)
            return;
        
        var schemas = DataGroupDocument.Instance.CatalogSchemas.Schemas;

        using (var catalogEditHandle = new CatalogEditHandle(element, true, true))
        {
            if (catalogEditHandle == null || 
                catalogEditHandle.CatalogInstanceName != null)
            {
                return;
            }

            catalogEditHandle.InsertDataGroupCatalogInstance("EmbeddedPart", "Embedded Part");
            catalogEditHandle.UpdateInstanceDataDefaults();
            
            DataGroupProperty code = null;
            DataGroupProperty name = null;

            foreach (DataGroupProperty property in catalogEditHandle.GetProperties())
            {
                if (property?.Xpath == "EmbPart/@PartCode") 
                    code = property;
                else if (property?.Xpath == "EmbPart/@CatalogName")
                    name = property;
            }

            if (code != null)
                catalogEditHandle.SetValue(code, task.Code);
            else {
                code = new DataGroupProperty("PartCode", task.Code, false, true);
                //code.SchemaName = "EmbPart";
                code.Xpath = "EmbPart/@PartCode";
                catalogEditHandle.Properties.Add(code);
            }
            catalogEditHandle.SetValue(code, task.Code);

            if (name != null)
                catalogEditHandle.SetValue(name, task.Name);
            else {
                name = new DataGroupProperty("CatalogName", task.Name, false, true);
                //name.SchemaName = "EmbPart";
                name.Xpath = "EmbPart/@CatalogName";
                catalogEditHandle.Properties.Add(name);
            }
            catalogEditHandle.SetValue(name, task.Name);
            catalogEditHandle.Rewrite((int)BCOM.MsdDrawingMode.Normal);

            // TODO решить проблему вылета при команде Modify DataGroup Instance
        }
    }

    private PenetrDataSource penData_; // TODO переименовать

    private Dictionary<IntPtr, PenetrTask> tasks_ = 
        new Dictionary<IntPtr, PenetrTask>();

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
