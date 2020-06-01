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

    private Dictionary<ModelReference, List<TFFrameListClass>> updateColl_;


    public void scanForUpdate(TreeView treeView) // TODO без TreeView
    {
        updateColl_ = updateColl_ ?? 
            new Dictionary<ModelReference, List<TFFrameListClass>>();
        updateColl_.Clear();

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

        foreach (var pair in updateColl_)
        {
            ModelReference model = pair.Key;
            List<TFFrameListClass> updateList = pair.Value;

            TreeNode node = 
                treeView.Nodes.Add(model.Name + $" ({updateList.Count})");
            foreach (TFFrameListClass frame in updateList)
            {
                Element element = frame.Get3DElement();
                node.Nodes.Add(element.ID.ToString());
            }
        }
    }

    public void update()
    {
        updateColl_.Clear();

        ElementScanCriteria scanCriteria = new ElementScanCriteriaClass();
        scanCriteria.ExcludeNonGraphical();
        scanCriteria.IncludeOnlyVisible();  

        scanRecurse(App.ActiveModelReference, scanCriteria, true);

        // TODO запустить progressBar

        foreach (var pair in updateColl_)
        {
            ModelReference model = pair.Key;
            List<TFFrameListClass> updateList = pair.Value;

            foreach (TFFrameListClass frame in updateList)
            {
                AppTF.ModelReferenceRewriteFrameList(model, frame);
            }
        }      
    }

    private void scanRecurse(ModelReference model, ElementScanCriteria criteria,
        bool updateImidiatly)
    {
        if (updateColl_.ContainsKey(model))
            return;

        var updateList = new List<TFFrameListClass>();

        foreach (string cellName in new string[] 
            {PenetrTask.CELL_NAME, PenetrTask.CELL_NAME_OLD})
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
                TFFrameListClass frameList = new TFFrameListClass();

                try
                {
                    if (cellName.Equals(PenetrTask.CELL_NAME))
                    {
                        frameList.InitFromElement(cell);
                        process(frameList, ref dirty, updateImidiatly);
                    }
                    else if (cellName.Equals(PenetrTask.CELL_NAME_OLD))
                    {
                        processOld(ref cell, ref dirty, updateImidiatly); // CELL  
                        frameList.InitFromElement(cell); // FRAME
                        processOld(frameList, ref dirty, updateImidiatly);  
                    }
                }
                catch (Exception) 
                {
                    // TODO log exception 
                }

                if (dirty)
                {
                    updateList.Add(frameList);
                    //AppTF.ModelReferenceRewriteFrameList(
                    //   App.ActiveModelReference, frameList);
                }            
            }                 
        }

        if (updateList.Count > 0)
        {
            updateColl_.Add(model, updateList);
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
                PenetrHelper.addProjectionToFrame(frame,
                    ElementHelper.createPoint(origin),
                    "refPoint",
                    PenetrTask.LevelRefPoint);
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
                requiredLevel = PenetrTask.LevelSymb;
            }
            else if (temp.Type == MsdElementType.Ellipse) /*перфоратор*/
            {
                requiredLevel = PenetrTask.LevelSymb;
            }
            else
            {
                requiredLevel = PenetrTask.LevelMain;
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
