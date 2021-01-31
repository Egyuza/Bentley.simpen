using Bentley.Interop.MicroStationDGN;
using Shared.Bentley;
using System;
using System.Collections.Generic;
using System.Text;

using BCOM = Bentley.Interop.MicroStationDGN;

#if V8i
using Bentley.Internal.MicroStation.Elements;
using BMI = Bentley.MicroStation.InteropServices;

#elif CONNECT
using Bentley.DgnPlatformNET.Elements;
using Bentley.MstnPlatformNET;
using BMI = Bentley.MstnPlatformNET.InteropServices;
#endif

namespace Embedded.Penetrations.Shared.Mapping
{
public class DataGroupToTagsConverter : BentleyInteropBase
{
    public void Run() 
    {
        ElementScanCriteria scanCriteria = new ElementScanCriteriaClass();
        scanCriteria.ExcludeAllTypes();
        scanCriteria.ExcludeNonGraphical();
        scanCriteria.IncludeType(MsdElementType.CellHeader);


    }

    private void scanRecurse(ModelReference model, ElementScanCriteria criteria)
    {
        
    }

    private bool IsElementMathesMapping(BCOM.Element comElement)
    {
        foreach (DataGroupToTagMapProperty item in DataGroupToTagsMapping.Instance.Items)
        {
            if (DataGroupHelper.IsElementOfCatalogType(comElement, item.DataGroupCatalogType))
        }
    }


    protected static BCOM.Application App
    {
        get { return BMI.Utilities.ComApp; }
    }
}
}
