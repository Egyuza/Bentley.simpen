using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

using BCOM = Bentley.Interop.MicroStationDGN;

using Shared;
using Shared.Bentley;

#if V8i
using Bentley.MicroStation;
using Bentley.Internal.MicroStation.Elements;
using BMI = Bentley.MicroStation.InteropServices;

#elif CONNECT
using Bentley.DgnPlatformNET;
using Bentley.DgnPlatformNET.Elements;
using Bentley.MstnPlatformNET;
using BMI = Bentley.MstnPlatformNET.InteropServices;

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
    public PenetrationModel(Bentley.MicroStation.AddIn addin) : this()
    {
        addin.SelectionChangedEvent += Addin_SelectionChangedEvent;
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
    public PenetrationModel(Bentley.MstnPlatformNET.AddIn addin) : this()
    {
        addin.SelectionChangedEvent += Addin_SelectionChangedEvent;
    }

    private void Addin_SelectionChangedEvent(
        AddIn sender, AddIn.SelectionChangedEventArgs eventArgs)
    {
 // Element element = null;
        // Session.Instance.StartUndoGroup();
        // Session.Instance.EndUndoGroup();
        
        Dictionary<IntPtr, Element> selectionSet = new Dictionary<IntPtr, Element>();
        uint nums = SelectionSetManager.NumSelected();
        for (uint i = 0; i < nums; ++i)
        {
            Element element = null;
            DgnModelRef modelRef = null;

            if (StatusInt.Success ==
                SelectionSetManager.GetElement(i, ref element, ref modelRef) &&
                element.ElementType == MSElementType.CellHeader)
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

    public void preview()
    {
        foreach (PenetrTask task in TaskSelection)
        {
            System.Windows.Forms.MessageBox.Show(task.ToString());
        }
    }

    public void create()
    {
        System.Windows.Forms.MessageBox.Show("TODO create");
    }


    private PenetrDataSource penData_;

    private Dictionary<IntPtr, PenetrTask> tasks_ = 
        new Dictionary<IntPtr, PenetrTask>();

    private BCOM.TransientElementContainer selectionTranCon_;
    private BCOM.TransientElementContainer previewTranCon_;

    private static BCOM.Application App => BMI.Utilities.ComApp;
}
}
