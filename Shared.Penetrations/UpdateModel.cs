using System;
using System.Collections.Generic;
using System.Text;

using Bentley.Interop.MicroStationDGN;
using Bentley.Interop.TFCom;

using Shared.Bentley;


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

    public void scanForUpdate()
    {

    /* TODO
        Поиск:
        - по имени Cell;
        - по CatalogGroupName 
        */

        ElementScanCriteria scanCriteria = new ElementScanCriteriaClass();
        scanCriteria.ExcludeNonGraphical();
        scanCriteria.IncludeOnlyVisible();        

        var changeList = new List<CellElement>();

        foreach (string cellName in new string[] 
            {PenetrTask.CELL_NAME, PenetrTask.CELL_NAME_OLD})
        {
            scanCriteria.IncludeOnlyCell(cellName);

            ElementEnumerator iter = App.ActiveModelReference.Scan(scanCriteria);
            iter.Reset();
            while (iter.MoveNext())
            {
                if (!iter.Current.IsCellElement() && !iter.Current.IsCompundCell())
                    continue;

                changeList.Add(iter.Current.AsCellElement());                
            }                 
        }

        update();
    }

    public void update()
    {
        ElementScanCriteria scanCriteria = new ElementScanCriteriaClass();
        scanCriteria.ExcludeNonGraphical();
        scanCriteria.IncludeOnlyVisible();        

        var changeList = new List<CellElement>();

        foreach (string cellName in new string[] 
            {PenetrTask.CELL_NAME, PenetrTask.CELL_NAME_OLD})
        {
            scanCriteria.IncludeOnlyCell(cellName);

            ElementEnumerator iter = App.ActiveModelReference.Scan(scanCriteria);
            iter.Reset();
            while (iter.MoveNext())
            {
                if (!iter.Current.IsCellElement() && !iter.Current.IsCompundCell())
                    continue;
            
                CellElement cell = iter.Current.AsCellElement();

                bool dirty = false;

                //if (cell.ID != 319836)
                //{
                //    //System.Windows.Forms.MessageBox.Show(cell.ID.ToString() +
                //    //    "\n" + cell.Origin.ToStringEx());
                    
                //    continue;
                //}

                TFFrameListClass frameList = new TFFrameListClass();                
                
                if (cellName.Equals(PenetrTask.CELL_NAME))
                {
                    frameList.InitFromElement(cell);
                    process(frameList, ref dirty);
                }
                else if (cellName.Equals(PenetrTask.CELL_NAME_OLD))
                {
                    // 1. Cell
                    processOld(ref cell, ref dirty);
                    // 2. Frame           
                    frameList.InitFromElement(cell);
                    processOld(frameList, ref dirty);  
                }

                if (dirty)
                {
                    AppTF.ModelReferenceRewriteFrameList(
                       App.ActiveModelReference, frameList);
                }
            }                 
        }
        scanCriteria = null;
    }

    public void process(TFFrameListClass frameList, ref bool dirty)
    {
        // smartsolids раскладываются на примитивы: окружности, линии

        TFProjectionList projList = frameList.GetProjectionList();
        string name = projList.AsTFProjection.GetName();
        
        if (!isProjectionListCorrect(projList))
        {
            dirty = true;
        }
    }

    public void processOld(TFFrameListClass frame, ref bool dirty)
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
            Point3d origin = frame.Get3DElement().AsCellElement().Origin;
            PenetrHelper.addProjectionToFrame(frame,
                ElementHelper.createPoint(origin),
                "refPoint",
                PenetrTask.LevelRefPoint);
        }
    }

    public void processOld(ref /* ref - важно */ CellElement cell, ref bool dirty)
    {
        // TODO необходимо полностью обновить старые проходки,
        // т.к. нужно назначить правильные Level всем элементам проходки

        // либо поэтапно ...

        // TODO формализовать список обновлений через Enum

        Level levelBody = PenetrTask.LevelMain;

        int change = 0;
        bool cellDirty = false;

        cell.ResetElementEnumeration();
        while (cell.MoveToNextElement(true, ref change))
        {
            Element temp = cell.CopyCurrentElement();
            if (!temp.IsGraphical || temp.Level == null)
                continue;

            if (temp.Level?.ID != levelBody.ID)
            {
                cellDirty = true;
                temp.Level = levelBody;               
            }
            ElementHelper.setSymbologyByLevel(temp, ref cellDirty);
            if (cellDirty)
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
