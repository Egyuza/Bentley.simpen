using System;
using System.Collections.Generic;
using System.Text;

using Bentley.Interop.MicroStationDGN;
using Bentley.Interop.TFCom;

using Shared.Bentley;
using System.Windows.Forms;

namespace Embedded.Penetrations.Shared
{
class UpdateModel : BentleyInteropBase
{
    //public UpdateModel(Bentley.MicroStation.AddIn addin)
    //{
    //    addin_ = addin;
    //        addin_.SelectionChangedEvent += Addin__SelectionChangedEvent;
    //}

    //private void Addin__SelectionChangedEvent(Bentley.MicroStation.AddIn sender, Bentley.MicroStation.AddIn.SelectionChangedEventArgs eventArgs)
    //{
    //    throw new NotImplementedException();
    //}

    private Dictionary<ModelReference, List<CellElement>> modelsCellsForUpdate_;
    private List<CellElement> checkedCellsForUpdate_;

#if V8i
    private Dictionary<int, TFFrameListClass> cellFrames_ = new Dictionary<int, TFFrameListClass>();
        
#elif CONNECT
    private Dictionary<long, TFFrameListClass> cellFrames_ = new Dictionary<long, TFFrameListClass>();
#endif


    public UpdateModel()
    {
        modelsCellsForUpdate_ = new Dictionary<ModelReference, List<CellElement>>();
        checkedCellsForUpdate_ = new List<CellElement>();
    }

    public void updateNodeDoubleClick(TreeNode node)
    {
        Element element = node.Tag as Element;
        if (element == null)
            return;
        
        App.ActiveModelReference.UnselectAllElements();
        App.ActiveModelReference.SelectElement(element);
        
        element.zoomToElement();
    }

    public void scanForUpdate(TreeView treeView) // TODO без TreeView
    {
        treeView.AfterCheck -= TreeView_AfterCheck;
        treeView.AfterCheck += TreeView_AfterCheck;

        checkedCellsForUpdate_.Clear(); 
        modelsCellsForUpdate_.Clear();
        cellFrames_.Clear();

        /* TODO
        Поиск:
        - по имени Cell;
        - по CatalogGroupName 
        */

        ElementScanCriteria scanCriteria = new ElementScanCriteriaClass();
        scanCriteria.ExcludeNonGraphical();
        scanCriteria.IncludeOnlyVisible();        

        scanRecurse(App.ActiveModelReference, scanCriteria, false);

        treeView.Nodes.Clear();

        foreach (var pair in modelsCellsForUpdate_)
        {
            ModelReference model = pair.Key;
            List<CellElement> updateList = pair.Value;

            TreeNode modelNode = 
                treeView.Nodes.Add(model.Name + $" ({updateList.Count})");

            foreach (var cell in updateList)
            {
                TFFrameListClass frame = cellFrames_[cell.MdlElementRef()];
                TreeNode cellNode = modelNode.Nodes.Add(cell.ID.ToString());
                cellNode.Tag = cell;
            }
            modelNode.Checked = true;
        }
    }

    private void TreeView_AfterCheck(object sender, TreeViewEventArgs e)
    {
        var cell = e.Node.Tag as CellElement;

        if (e.Node.Nodes.Count > 0)
        {
            foreach (TreeNode subNode in e.Node.Nodes)
            {
                subNode.Checked = e.Node.Checked;
            }   
        }
        else if (cell != null)
        {
            if (checkedCellsForUpdate_.Contains(cell))
            {
                checkedCellsForUpdate_.Remove(cell);
            }

            if (e.Node.Checked)
            {
                checkedCellsForUpdate_.Add(cell);
            }    
        }
    }

    public void runUpdate()
    {
        modelsCellsForUpdate_.Clear();
        cellFrames_.Clear();

        ElementScanCriteria scanCriteria = new ElementScanCriteriaClass();
        scanCriteria.ExcludeNonGraphical();
        scanCriteria.IncludeOnlyVisible();

        scanRecurse(App.ActiveModelReference, scanCriteria, true);

        // TODO запустить progressBar

        foreach (CellElement cell in checkedCellsForUpdate_)
        {
            var frame = cellFrames_[cell.MdlElementRef()];
            AppTF.ModelReferenceRewriteFrameList(cell.ModelReference, frame);
        }
    }

    private void scanRecurse(ModelReference model, ElementScanCriteria criteria,
        bool updateImidiatly)
    {
        if (modelsCellsForUpdate_.ContainsKey(model))
            return;

        var cellsToUpdateList = new List<CellElement>();

        foreach (string cellName in new string[] 
            {PenetrTaskBase.CELL_NAME, PenetrTaskBase.CELL_NAME_OLD})
        {
            criteria.IncludeOnlyCell(cellName);

            ElementEnumerator iter = App.ActiveModelReference.Scan(criteria);
            iter.Reset();
            while (iter.MoveNext())
            {
                if (!iter.Current.IsCellElement() && !iter.Current.IsCompundCell())
                    continue;

                bool dirty = false;

                CellElement cell = iter.Current.AsCellElement();     

                if (updateImidiatly && null == checkedCellsForUpdate_.Find(
                        x => x.MdlElementRef() == cell.MdlElementRef()))
                {
                    continue;
                }

                TFFrameListClass frameList = new TFFrameListClass();
                try
                {
                    if (cellName.Equals(PenetrTaskBase.CELL_NAME))
                    {
                        //frameList.InitFromElement(cell);
                        //process(frameList, ref dirty, updateImidiatly);
                    }
                    else if (cellName.Equals(PenetrTaskBase.CELL_NAME_OLD))
                    {
                        processOld(ref cell, ref dirty, updateImidiatly); // CELL  
                        frameList.InitFromElement(cell); // FRAME
                        processOld(frameList, ref dirty, updateImidiatly);  
                    }
                }
                catch (Exception ex) 
                {
                    // TODO log exception 
                    continue;
                }

                if (dirty)
                {
                    cellFrames_.Add(cell.MdlElementRef(), frameList);
                    cellsToUpdateList.Add(cell);
                }            
            }                 
        }

        if (cellsToUpdateList.Count > 0)
        {
            modelsCellsForUpdate_.Add(model, cellsToUpdateList);
        }

        foreach (Attachment attachment in model.Attachments)
        {
            if (!attachment.IsActive || !attachment.IsMissingFile || !attachment.IsMissingModel)
                return;

            ModelReference modelRef = 
                App.MdlGetModelReferenceFromModelRefP(attachment.MdlModelRefP());
            scanRecurse(modelRef, criteria, updateImidiatly);
        }
    }



    public void process(TFFrameListClass frameList, ref bool dirty, bool updateImidiatly)
    {
        // smartsolids раскладываются на примитивы: окружности, линии

        TFProjectionList projList = frameList.GetProjectionList();
        string name = projList.AsTFProjection.GetName();
        
        if (!isProjectionListCorrect(projList))
        {
            dirty = true;
        }
    }

    public void processOld(TFFrameListClass frame, ref bool dirty, bool updateImidiatly)
    {
        // TODO необходимо полностью обновить старые проходки,
        // т.к. нужно назначить правильные Level всем элементам проходки

        // либо поэтапно ...

        // TODO формализовать список обновлений через Enum

        TFProjectionList projList = frame.GetProjectionList();
        string name = projList.AsTFProjection.GetName();

       // bool isCorrect = isProjectionListCorrect(projList);
        if (!hasRefPoint(projList))
        {
            dirty = true;

            if (updateImidiatly)
            {
                Point3d origin = frame.Get3DElement().AsCellElement().Origin;
                frame.AddProjection(
                    ElementHelper.createPoint(origin),
                    "refPoint",
                    PenetrTaskBase.LevelRefPoint
                );
            }
        }
    }

    public void processOld(ref /* ref - важно */ CellElement cell, ref bool dirty, bool updateImidiatly)
    {
        // TODO необходимо полностью обновить старые проходки,
        // т.к. нужно назначить правильные Level всем элементам проходки

        // либо поэтапно ...

        // TODO формализовать список обновлений через Enum

        int change = 0;
        bool cellDirty = false;

        cell.ResetElementEnumeration();
        while (cell.MoveToNextElement(true, ref change))
        {
            Element temp = cell.CopyCurrentElement();
            if (!temp.IsGraphical || temp.Level == null)
                continue;

            Level requiredLevel;

            if (temp.IsLineElement() && temp.AsLineElement().SegmentsCount > 1)
            {
                requiredLevel = PenetrTaskBase.LevelSymb;
            }
            else if (temp.Type == MsdElementType.Ellipse) /*перфоратор*/
            {
                requiredLevel = PenetrTaskBase.LevelSymb;
            }
            else
            {
                requiredLevel = PenetrTaskBase.LevelMain;
            }

            if (temp.Level?.ID != requiredLevel.ID)
            {
                cellDirty = true;
                temp.Level = requiredLevel;               
            }
            ElementHelper.setSymbologyByLevel(temp, ref cellDirty);
            if (updateImidiatly && cellDirty)
            {
                temp.Rewrite(); // ! важно
                cell.ReplaceCurrentElement(temp);
            }
        }
    }

    public bool hasRefPoint(TFProjectionList projList)
    {
        TFProjectionList iter = projList;
        while (iter != null)
        {
            Element element;
            iter.AsTFProjection.GetElement(out element);

            if (element.IsLineElement() && element.AsLineElement().IsPoint())
                return true;

            iter = iter.GetNext();
        }
        return false;
    }

    public bool isProjectionListCorrect(TFProjectionList projList)
    {
        var arcs = new List<ArcElement>();
        var lines = new List<LineElement>();
        var points = new List<LineElement>();

        TFProjectionList iter = projList;
        while (iter != null)
        {
            Element element;
            iter.AsTFProjection.GetElement(out element);

            if (element.IsArcElement())
                arcs.Add(element.AsArcElement());
            else if (element.IsLineElement())
            {
                if (element.AsLineElement().IsPoint())
                    points.Add(element.AsLineElement());
                else
                    lines.Add(element.AsLineElement());                
            }          
            iter = iter.GetNext();
        }
        return arcs.Count == 4 && lines.Count != 2 && points.Count != 1;
    }


}
}
