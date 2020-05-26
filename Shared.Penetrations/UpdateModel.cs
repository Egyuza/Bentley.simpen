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

/*
 Поиск:
 - по имени Cell;
 - по CatalogGroupName 
 */


    public void scanForUpdate()
    {
        ElementScanCriteria scanCriteria = new ElementScanCriteriaClass();
        scanCriteria.ExcludeNonGraphical();
        //scanCriteria.IncludeOnlySolid();
        scanCriteria.IncludeOnlyVisible();        


        foreach (string cellName in new string[] 
            {PenetrTask.CELL_NAME, PenetrTask.CELL_NAME_OLD})
        {
            scanCriteria.IncludeOnlyCell(cellName);

            ElementEnumerator iter = App.ActiveModelReference.Scan(scanCriteria);
            while (iter.MoveNext())
            {
                if (!iter.Current.IsCellElement() && !iter.Current.IsCompundCell())
                    continue;
            
                CellElement cell = iter.Current.AsCellElement();
                bool dirty = false;

                //if (iter.Current.ID == 392921)
                //{
                //    System.Windows.Forms.MessageBox.Show(cell.ID.ToString() +
                //        "\n" + cell.Origin.ToStringEx());
                //}

                TFFrameListClass frameList = new TFFrameListClass();
                frameList.InitFromElement(cell);
                
                if (cellName.Equals(PenetrTask.CELL_NAME))
                {
                    process(frameList, ref dirty);                    
                }
                else if (cellName.Equals(PenetrTask.CELL_NAME_OLD))
                {
                    processOld(frameList, ref dirty);  
                }

                if (dirty)
                {   // замена
                    AppTF.ModelReferenceRewriteFrameList(
                        App.ActiveModelReference, frameList);
                }
            }            
        }
    }

    public void process(TFFrameListClass frameList, ref bool dirty)
    {
        TFProjectionList projList = frameList.GetProjectionList();
        string name = projList.AsTFProjection.GetName();
        
        if (!isProjectionListCorrect(projList))
        {
            dirty = true;
        }
    }

    public void processOld(TFFrameListClass frameList, ref bool dirty)
    {
        // TODO необходимо полностью обновить старые проходки,
        // т.к. нужно назначить правильные Level всем элементам проходки

        // либо поэтапно ...

        // TODO формализовать список обновлений через Enum

        TFProjectionList projList = frameList.GetProjectionList();
        string name = projList.AsTFProjection.GetName();
        
        bool isCorrect = isProjectionListCorrect(projList);

        if (!hasRefPoint(projList))
        {
            dirty = true;


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

    public void update()
    {
        
    }

}
}
