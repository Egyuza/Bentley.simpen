using System;
using System.Collections.Generic;
using System.Text;
using Bentley.Building.DataGroupSystem.Serialization;
using Bentley.Building.DataGroupSystem;

using BCOM = Bentley.Interop.MicroStationDGN;
using TFCOM = Bentley.Interop.TFCom;

#if V8i
using Bentley.Internal.MicroStation.Elements;

#elif CONNECT
using Bentley.Building.DataGroupSystem.Serialization;
using Bentley.Building.DataGroupSystem;
#endif


namespace Shared.Bentley
{
public class DataGroupHelper
{
    public static bool IsElementOfCatalogType(BCOM.Element comElement, 
        string catalogTypeName)
    {
        Element element = ElementHelper.getElement(comElement);
        using (var catalogEditHandle = new CatalogEditHandle(element, true, true))
        {
            if (catalogEditHandle != null && 
                catalogTypeName.Equals(catalogEditHandle.CatalogTypeName, 
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
            BCOM.Application app;

            
        return false;
    }
}
}
